using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PetCare.Application.Appointments.DTOs;
using PetCare.Domain.Appointments;
using PetCare.Domain.Pets;
using PetCare.Infrastructure.Auth;
using PetCare.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace PetCare.Integration.Tests.Appointments;

public class AppointmentsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AppointmentsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PetCareDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<PetCareDbContext>(options =>
                {
                    options.UseInMemoryDatabase("AppointmentTestDb_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();
        
        // Ensure database is created
        await dbContext.Database.EnsureCreatedAsync();

        // Add test users
        var owner = new ApplicationUser
        {
            Id = "owner-123",
            UserName = "owner@test.com",
            Email = "owner@test.com",
            FullName = "John Doe",
            EmailConfirmed = true
        };

        var vet = new ApplicationUser
        {
            Id = "vet-456",
            UserName = "vet@test.com",
            Email = "vet@test.com",
            FullName = "Dr. Smith",
            EmailConfirmed = true
        };

        var admin = new ApplicationUser
        {
            Id = "admin-789",
            UserName = "admin@test.com",
            Email = "admin@test.com",
            FullName = "Admin User",
            EmailConfirmed = true
        };

        if (!await dbContext.Users.AnyAsync())
        {
            dbContext.Users.AddRange(owner, vet, admin);
        }

        // Add test pet
        var pet = new Pet
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Buddy",
            Species = Species.Dog,
            Breed = "Golden Retriever",
            DateOfBirth = DateTime.Now.AddYears(-3),
            Color = "Golden",
            Weight = 25.5m,
            IsActive = true,
            OwnerUserId = "owner-123",
            CreatedAt = DateTime.UtcNow
        };

        if (!await dbContext.Pets.AnyAsync())
        {
            dbContext.Pets.Add(pet);
        }

        await dbContext.SaveChangesAsync();
    }

    private void SetupAuthHeaders(string userId, string role)
    {
        // Simple bearer token simulation for testing
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{role}"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    #region GET /api/appointments Tests

    [Fact]
    public async Task GetAppointments_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyAppointments_AsOwner_ShouldReturnOwnerAppointments()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("owner-123", "OWNER");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            OwnerUserId = "owner-123",
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Routine checkup",
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/appointments/my");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonConvert.DeserializeObject<AppointmentDto[]>(content);
        appointments.Should().HaveCount(1);
        appointments![0].OwnerUserId.Should().Be("owner-123");
    }

    #endregion

    #region POST /api/appointments Tests

    [Fact]
    public async Task CreateAppointment_WithValidData_ShouldCreateAppointment()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("owner-123", "OWNER");

        var request = new CreateAppointmentRequest
        {
            PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Annual checkup",
            Notes = "Pet seems healthy"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/appointments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonConvert.DeserializeObject<AppointmentDto>(responseContent);
        
        appointment.Should().NotBeNull();
        appointment!.PetId.Should().Be(request.PetId);
        appointment.OwnerUserId.Should().Be("owner-123");
        appointment.ReasonForVisit.Should().Be(request.ReasonForVisit);
        appointment.Status.Should().Be(AppointmentStatus.Pending);

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();
        var dbAppointment = await dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointment.Id);
        dbAppointment.Should().NotBeNull();
        dbAppointment!.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public async Task CreateAppointment_WithPastDate_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("owner-123", "OWNER");

        var request = new CreateAppointmentRequest
        {
            PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            RequestedDateTime = DateTime.Now.AddDays(-1), // Past date
            ReasonForVisit = "Checkup"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/appointments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("future date and time");
    }

    [Fact]
    public async Task CreateAppointment_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("owner-123", "OWNER");

        var request = new CreateAppointmentRequest
        {
            PetId = Guid.Empty, // Invalid pet ID
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "" // Empty reason
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/appointments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/appointments/{id}/status Tests

    [Fact]
    public async Task UpdateAppointmentStatus_AsAdmin_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("admin-789", "ADMIN");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            OwnerUserId = "owner-123",
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Routine checkup",
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync();

        var updateRequest = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved,
            AdminNotes = "Approved by admin",
            VetUserId = "vet-456"
        };

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{appointment.Id}/status", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedAppointment = JsonConvert.DeserializeObject<AppointmentDto>(responseContent);
        
        updatedAppointment.Should().NotBeNull();
        updatedAppointment!.Status.Should().Be(AppointmentStatus.Approved);
        updatedAppointment.VetUserId.Should().Be("vet-456");
        updatedAppointment.AdminNotes.Should().Be("Approved by admin");

        // Verify in database
        var dbAppointment = await dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointment.Id);
        dbAppointment.Should().NotBeNull();
        dbAppointment!.Status.Should().Be(AppointmentStatus.Approved);
        dbAppointment.VetUserId.Should().Be("vet-456");
    }

    [Fact]
    public async Task UpdateAppointmentStatus_AsOwner_ShouldReturnForbidden()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("owner-123", "OWNER");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            OwnerUserId = "owner-123",
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Routine checkup",
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync();

        var updateRequest = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved
        };

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{appointment.Id}/status", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_NonExistentAppointment_ShouldReturnNotFound()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("admin-789", "ADMIN");

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved
        };

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{nonExistentId}/status", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/appointments/assigned Tests

    [Fact]
    public async Task GetMyAssignedAppointments_AsVet_ShouldReturnAssignedAppointments()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("vet-456", "VET");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            OwnerUserId = "owner-123",
            VetUserId = "vet-456",
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Routine checkup",
            Status = AppointmentStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/appointments/assigned");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonConvert.DeserializeObject<AppointmentDto[]>(content);
        appointments.Should().HaveCount(1);
        appointments![0].VetUserId.Should().Be("vet-456");
    }

    [Fact]
    public async Task GetMyAssignedAppointments_AsOwner_ShouldReturnForbidden()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("owner-123", "OWNER");

        // Act
        var response = await _client.GetAsync("/api/appointments/assigned");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GET /api/appointments/pending Tests

    [Fact]
    public async Task GetPendingAppointments_AsAdmin_ShouldReturnPendingAppointments()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("admin-789", "ADMIN");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();

        var appointments = new[]
        {
            new Appointment
            {
                Id = Guid.NewGuid(),
                PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                OwnerUserId = "owner-123",
                RequestedDateTime = DateTime.Now.AddDays(1),
                ReasonForVisit = "Checkup 1",
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            },
            new Appointment
            {
                Id = Guid.NewGuid(),
                PetId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                OwnerUserId = "owner-123",
                RequestedDateTime = DateTime.Now.AddDays(2),
                ReasonForVisit = "Checkup 2",
                Status = AppointmentStatus.Approved, // Should not be returned
                CreatedAt = DateTime.UtcNow
            }
        };

        dbContext.Appointments.AddRange(appointments);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/appointments/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var pendingAppointments = JsonConvert.DeserializeObject<AppointmentDto[]>(content);
        pendingAppointments.Should().HaveCount(1);
        pendingAppointments![0].Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public async Task GetPendingAppointments_AsOwner_ShouldReturnForbidden()
    {
        // Arrange
        await SeedTestDataAsync();
        SetupAuthHeaders("owner-123", "OWNER");

        // Act
        var response = await _client.GetAsync("/api/appointments/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}