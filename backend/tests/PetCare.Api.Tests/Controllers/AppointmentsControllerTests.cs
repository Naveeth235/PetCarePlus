using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PetCare.Api.Controllers;
using PetCare.Application.Appointments.DTOs;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Appointments;
using PetCare.Infrastructure.Auth;
using System.Security.Claims;

namespace PetCare.Api.Tests.Controllers;

public class AppointmentsControllerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly AppointmentsController _controller;

    public AppointmentsControllerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _userManagerMock = CreateMockUserManager();
        _controller = new AppointmentsController(
            _appointmentRepositoryMock.Object,
            _notificationServiceMock.Object,
            _userManagerMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            optionsAccessor.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);
    }

    private void SetupUserClaims(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role.ToUpperInvariant())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    private Appointment CreateSampleAppointment(
        Guid? id = null,
        string ownerUserId = "owner-123",
        string? vetUserId = null,
        AppointmentStatus status = AppointmentStatus.Pending)
    {
        return new Appointment
        {
            Id = id ?? Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            VetUserId = vetUserId,
            RequestedDateTime = DateTime.Now.AddDays(1),
            ActualDateTime = status == AppointmentStatus.Approved ? DateTime.Now.AddDays(1) : null,
            ReasonForVisit = "Routine checkup",
            Notes = "Pet seems healthy",
            AdminNotes = status != AppointmentStatus.Pending ? "Approved by admin" : null,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            Pet = new Domain.Pets.Pet { Name = "Buddy" }
        };
    }

    #region CreateAppointment Tests

    [Fact]
    public async Task CreateAppointment_ValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var userId = "owner-123";
        SetupUserClaims(userId, "OWNER");
        
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Routine checkup",
            Notes = "Pet seems healthy"
        };

        var createdAppointment = CreateSampleAppointment(ownerUserId: userId);
        _appointmentRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(createdAppointment);

        var owner = new ApplicationUser { Id = userId, FullName = "John Doe" };
        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(owner);

        // Act
        var result = await _controller.CreateAppointment(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be("GetAppointment");
        createdResult.RouteValues!["id"].Should().Be(createdAppointment.Id);

        _appointmentRepositoryMock.Verify(x => x.CreateAsync(It.Is<Appointment>(a =>
            a.PetId == request.PetId &&
            a.OwnerUserId == userId &&
            a.RequestedDateTime == request.RequestedDateTime &&
            a.ReasonForVisit == request.ReasonForVisit &&
            a.Notes == request.Notes &&
            a.Status == AppointmentStatus.Pending
        )), Times.Once);
    }

    [Fact]
    public async Task CreateAppointment_PastDateTime_ShouldReturnBadRequest()
    {
        // Arrange
        SetupUserClaims("owner-123", "OWNER");
        
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(-1), // Past date
            ReasonForVisit = "Routine checkup"
        };

        // Act
        var result = await _controller.CreateAppointment(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Appointment must be scheduled for a future date and time.");

        _appointmentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreateAppointment_AsAdmin_ShouldCreateAppointment()
    {
        // Arrange
        var adminId = "admin-123";
        SetupUserClaims(adminId, "ADMIN");
        
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Emergency checkup"
        };

        var createdAppointment = CreateSampleAppointment(ownerUserId: adminId);
        _appointmentRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(createdAppointment);

        var admin = new ApplicationUser { Id = adminId, FullName = "Admin User" };
        _userManagerMock
            .Setup(x => x.FindByIdAsync(adminId))
            .ReturnsAsync(admin);

        // Act
        var result = await _controller.CreateAppointment(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _appointmentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Appointment>()), Times.Once);
    }

    #endregion

    #region GetAppointment Tests

    [Fact]
    public async Task GetAppointment_AsOwner_AccessingOwnAppointment_ShouldReturnOk()
    {
        // Arrange
        var userId = "owner-123";
        var appointmentId = Guid.NewGuid();
        SetupUserClaims(userId, "OWNER");
        
        var appointment = CreateSampleAppointment(appointmentId, ownerUserId: userId);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        var owner = new ApplicationUser { Id = userId, FullName = "John Doe" };
        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(owner);

        // Act
        var result = await _controller.GetAppointment(appointmentId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var appointmentDto = okResult.Value.Should().BeOfType<AppointmentDto>().Subject;
        appointmentDto.Id.Should().Be(appointmentId);
        appointmentDto.OwnerUserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetAppointment_AsOwner_AccessingOthersAppointment_ShouldReturnForbid()
    {
        // Arrange
        var userId = "owner-123";
        var otherUserId = "other-owner";
        var appointmentId = Guid.NewGuid();
        SetupUserClaims(userId, "OWNER");
        
        var appointment = CreateSampleAppointment(appointmentId, ownerUserId: otherUserId);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _controller.GetAppointment(appointmentId);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetAppointment_AsAdmin_ShouldReturnAnyAppointment()
    {
        // Arrange
        var adminId = "admin-123";
        var appointmentId = Guid.NewGuid();
        SetupUserClaims(adminId, "ADMIN");
        
        var appointment = CreateSampleAppointment(appointmentId, ownerUserId: "other-owner");
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        var owner = new ApplicationUser { Id = "other-owner", FullName = "Other Owner" };
        _userManagerMock
            .Setup(x => x.FindByIdAsync("other-owner"))
            .ReturnsAsync(owner);

        // Act
        var result = await _controller.GetAppointment(appointmentId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var appointmentDto = okResult.Value.Should().BeOfType<AppointmentDto>().Subject;
        appointmentDto.Id.Should().Be(appointmentId);
    }

    [Fact]
    public async Task GetAppointment_AsVet_ShouldReturnAnyAppointment()
    {
        // Arrange
        var vetId = "vet-123";
        var appointmentId = Guid.NewGuid();
        SetupUserClaims(vetId, "VET");
        
        var appointment = CreateSampleAppointment(appointmentId, ownerUserId: "other-owner");
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        var owner = new ApplicationUser { Id = "other-owner", FullName = "Other Owner" };
        _userManagerMock
            .Setup(x => x.FindByIdAsync("other-owner"))
            .ReturnsAsync(owner);

        // Act
        var result = await _controller.GetAppointment(appointmentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAppointment_NotFound_ShouldReturnNotFound()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        SetupUserClaims("admin-123", "ADMIN");
        
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _controller.GetAppointment(appointmentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetMyAppointments Tests

    [Fact]
    public async Task GetMyAppointments_AsOwner_ShouldReturnOwnerAppointments()
    {
        // Arrange
        var userId = "owner-123";
        SetupUserClaims(userId, "OWNER");
        
        var appointments = new[]
        {
            CreateSampleAppointment(ownerUserId: userId),
            CreateSampleAppointment(ownerUserId: userId, status: AppointmentStatus.Approved)
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByOwnerUserIdAsync(userId))
            .ReturnsAsync(appointments);

        var owner = new ApplicationUser { Id = userId, FullName = "John Doe" };
        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(owner);

        // Act
        var result = await _controller.GetMyAppointments();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var appointmentDtos = okResult.Value.Should().BeAssignableTo<AppointmentDto[]>().Subject;
        appointmentDtos.Should().HaveCount(2);
        appointmentDtos.All(a => a.OwnerUserId == userId).Should().BeTrue();
    }

    [Fact]
    public async Task GetMyAppointments_AsAdmin_ShouldReturnAdminAppointments()
    {
        // Arrange
        var adminId = "admin-123";
        SetupUserClaims(adminId, "ADMIN");
        
        var appointments = new[] { CreateSampleAppointment(ownerUserId: adminId) };
        _appointmentRepositoryMock
            .Setup(x => x.GetByOwnerUserIdAsync(adminId))
            .ReturnsAsync(appointments);

        var admin = new ApplicationUser { Id = adminId, FullName = "Admin User" };
        _userManagerMock
            .Setup(x => x.FindByIdAsync(adminId))
            .ReturnsAsync(admin);

        // Act
        var result = await _controller.GetMyAppointments();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region GetAllAppointments Tests

    [Fact]
    public async Task GetAllAppointments_AsAdmin_ShouldReturnAllAppointments()
    {
        // Arrange
        SetupUserClaims("admin-123", "ADMIN");
        
        var appointments = new[]
        {
            CreateSampleAppointment(ownerUserId: "owner-1"),
            CreateSampleAppointment(ownerUserId: "owner-2"),
            CreateSampleAppointment(ownerUserId: "owner-3")
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(appointments);

        foreach (var appointment in appointments)
        {
            var owner = new ApplicationUser { Id = appointment.OwnerUserId, FullName = $"Owner {appointment.OwnerUserId}" };
            _userManagerMock
                .Setup(x => x.FindByIdAsync(appointment.OwnerUserId))
                .ReturnsAsync(owner);
        }

        // Act
        var result = await _controller.GetAllAppointments();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var appointmentDtos = okResult.Value.Should().BeAssignableTo<AppointmentDto[]>().Subject;
        appointmentDtos.Should().HaveCount(3);
    }

    #endregion

    #region GetPendingAppointments Tests

    [Fact]
    public async Task GetPendingAppointments_AsAdmin_ShouldReturnPendingAppointments()
    {
        // Arrange
        SetupUserClaims("admin-123", "ADMIN");
        
        var pendingAppointments = new[]
        {
            CreateSampleAppointment(ownerUserId: "owner-1", status: AppointmentStatus.Pending),
            CreateSampleAppointment(ownerUserId: "owner-2", status: AppointmentStatus.Pending)
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetPendingAsync())
            .ReturnsAsync(pendingAppointments);

        foreach (var appointment in pendingAppointments)
        {
            var owner = new ApplicationUser { Id = appointment.OwnerUserId, FullName = $"Owner {appointment.OwnerUserId}" };
            _userManagerMock
                .Setup(x => x.FindByIdAsync(appointment.OwnerUserId))
                .ReturnsAsync(owner);
        }

        // Act
        var result = await _controller.GetPendingAppointments();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var appointmentDtos = okResult.Value.Should().BeAssignableTo<AppointmentDto[]>().Subject;
        appointmentDtos.Should().HaveCount(2);
        appointmentDtos.All(a => a.Status == AppointmentStatus.Pending).Should().BeTrue();
    }

    #endregion

    #region UpdateAppointmentStatus Tests

    [Fact]
    public async Task UpdateAppointmentStatus_ApproveWithVet_ShouldUpdateAndSendNotifications()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var vetId = "vet-123";
        SetupUserClaims("admin-123", "ADMIN");
        
        var appointment = CreateSampleAppointment(appointmentId, status: AppointmentStatus.Pending);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        
        _appointmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved,
            AdminNotes = "Approved by admin",
            VetUserId = vetId
        };

        var owner = new ApplicationUser { Id = appointment.OwnerUserId, FullName = "John Doe" };
        var vet = new ApplicationUser { Id = vetId, FullName = "Dr. Smith" };
        
        _userManagerMock.Setup(x => x.FindByIdAsync(appointment.OwnerUserId)).ReturnsAsync(owner);
        _userManagerMock.Setup(x => x.FindByIdAsync(vetId)).ReturnsAsync(vet);

        // Act
        var result = await _controller.UpdateAppointmentStatus(appointmentId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var appointmentDto = okResult.Value.Should().BeOfType<AppointmentDto>().Subject;
        
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Appointment>(a =>
            a.Status == AppointmentStatus.Approved &&
            a.VetUserId == vetId &&
            a.AdminNotes == "Approved by admin"
        )), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyVetAssignedAsync(appointment, vetId), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyAppointmentApprovedAsync(appointment, "Dr. Smith"), Times.Once);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_Cancel_ShouldUpdateAndSendCancellationNotification()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        SetupUserClaims("admin-123", "ADMIN");
        
        var appointment = CreateSampleAppointment(appointmentId, status: AppointmentStatus.Pending);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        
        _appointmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Cancelled,
            AdminNotes = "Pet owner requested cancellation"
        };

        var owner = new ApplicationUser { Id = appointment.OwnerUserId, FullName = "John Doe" };
        _userManagerMock.Setup(x => x.FindByIdAsync(appointment.OwnerUserId)).ReturnsAsync(owner);

        // Act
        var result = await _controller.UpdateAppointmentStatus(appointmentId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Appointment>(a =>
            a.Status == AppointmentStatus.Cancelled &&
            a.AdminNotes == "Pet owner requested cancellation"
        )), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyAppointmentCancelledAsync(
            appointment, 
            "Pet owner requested cancellation"), 
            Times.Once);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_NotFound_ShouldReturnNotFound()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        SetupUserClaims("admin-123", "ADMIN");
        
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved
        };

        // Act
        var result = await _controller.UpdateAppointmentStatus(appointmentId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        _notificationServiceMock.Verify(x => x.NotifyAppointmentApprovedAsync(It.IsAny<Appointment>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region GetMyAssignedAppointments Tests

    [Fact]
    public async Task GetMyAssignedAppointments_AsVet_ShouldReturnAssignedAppointments()
    {
        // Arrange
        var vetId = "vet-123";
        SetupUserClaims(vetId, "VET");
        
        var assignedAppointments = new[]
        {
            CreateSampleAppointment(vetUserId: vetId, status: AppointmentStatus.Approved),
            CreateSampleAppointment(vetUserId: vetId, status: AppointmentStatus.Approved)
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByVetUserIdAsync(vetId))
            .ReturnsAsync(assignedAppointments);

        foreach (var appointment in assignedAppointments)
        {
            var owner = new ApplicationUser { Id = appointment.OwnerUserId, FullName = $"Owner {appointment.OwnerUserId}" };
            _userManagerMock.Setup(x => x.FindByIdAsync(appointment.OwnerUserId)).ReturnsAsync(owner);
        }

        var vet = new ApplicationUser { Id = vetId, FullName = "Dr. Smith" };
        _userManagerMock.Setup(x => x.FindByIdAsync(vetId)).ReturnsAsync(vet);

        // Act
        var result = await _controller.GetMyAssignedAppointments();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var appointmentDtos = okResult.Value.Should().BeAssignableTo<AppointmentDto[]>().Subject;
        appointmentDtos.Should().HaveCount(2);
        appointmentDtos.All(a => a.VetUserId == vetId).Should().BeTrue();
    }

    [Fact]
    public async Task GetMyAssignedAppointments_AsAdmin_ShouldReturnAssignedAppointments()
    {
        // Arrange
        var adminId = "admin-123";
        SetupUserClaims(adminId, "ADMIN");
        
        var assignedAppointments = new[]
        {
            CreateSampleAppointment(vetUserId: adminId, status: AppointmentStatus.Approved)
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByVetUserIdAsync(adminId))
            .ReturnsAsync(assignedAppointments);

        var appointment = assignedAppointments[0];
        var owner = new ApplicationUser { Id = appointment.OwnerUserId, FullName = "Owner Name" };
        _userManagerMock.Setup(x => x.FindByIdAsync(appointment.OwnerUserId)).ReturnsAsync(owner);

        var admin = new ApplicationUser { Id = adminId, FullName = "Admin User" };
        _userManagerMock.Setup(x => x.FindByIdAsync(adminId)).ReturnsAsync(admin);

        // Act
        var result = await _controller.GetMyAssignedAppointments();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}