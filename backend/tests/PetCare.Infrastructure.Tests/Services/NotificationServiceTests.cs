using FluentAssertions;
using Moq;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Appointments;
using PetCare.Domain.Notifications;
using PetCare.Domain.Pets;
using PetCare.Infrastructure.Services;
using System.Text.Json;

namespace PetCare.Infrastructure.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _notificationService = new NotificationService(_notificationRepositoryMock.Object);
    }

    private void SetupRepositoryMock(out Notification capturedNotification)
    {
        var captured = (Notification)null!;
        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => captured = notification)
            .ReturnsAsync((Notification n) => n);
        capturedNotification = captured;
    }

    private Appointment CreateSampleAppointment(
        Guid? id = null,
        string ownerUserId = "owner-123",
        string? vetUserId = null,
        string petName = "Buddy")
    {
        return new Appointment
        {
            Id = id ?? Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            VetUserId = vetUserId,
            RequestedDateTime = new DateTime(2024, 12, 15, 14, 30, 0),
            ReasonForVisit = "Routine checkup",
            Pet = new Pet { Name = petName }
        };
    }

    #region NotifyAppointmentApprovedAsync Tests

    [Fact]
    public async Task NotifyAppointmentApprovedAsync_WithVetName_ShouldCreateCorrectNotification()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        var vetName = "Dr. Smith";
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyAppointmentApprovedAsync(appointment, vetName);

        // Assert
        _notificationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Notification>()), Times.Once);

        capturedNotification.Should().NotBeNull();
        capturedNotification.UserId.Should().Be(appointment.OwnerUserId);
        capturedNotification.Type.Should().Be(NotificationType.AppointmentApproved);
        capturedNotification.Title.Should().Be("Appointment Approved ✅");
        capturedNotification.Message.Should().Contain("Buddy");
        capturedNotification.Message.Should().Contain("Dec 15, 2024 at 2:30 PM");
        capturedNotification.Message.Should().Contain("Dr. Smith");
        capturedNotification.Data.Should().NotBeNull();

        // Verify JSON data structure
        var data = JsonSerializer.Deserialize<JsonElement>(capturedNotification.Data!);
        data.GetProperty("appointmentId").GetGuid().Should().Be(appointment.Id);
        data.GetProperty("petId").GetGuid().Should().Be(appointment.PetId);
        data.GetProperty("petName").GetString().Should().Be("Buddy");
        data.GetProperty("vetName").GetString().Should().Be(vetName);
    }

    [Fact]
    public async Task NotifyAppointmentApprovedAsync_WithoutVetName_ShouldCreateNotificationWithoutVetInfo()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyAppointmentApprovedAsync(appointment, null);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification.UserId.Should().Be(appointment.OwnerUserId);
        capturedNotification.Type.Should().Be(NotificationType.AppointmentApproved);
        capturedNotification.Title.Should().Be("Appointment Approved ✅");
        capturedNotification.Message.Should().Contain("Buddy");
        capturedNotification.Message.Should().Contain("Dec 15, 2024 at 2:30 PM");
        capturedNotification.Message.Should().NotContain("Assigned veterinarian:");

        // Verify JSON data structure
        var data = JsonSerializer.Deserialize<JsonElement>(capturedNotification.Data!);
        data.GetProperty("vetName").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task NotifyAppointmentApprovedAsync_WithEmptyVetName_ShouldCreateNotificationWithoutVetInfo()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyAppointmentApprovedAsync(appointment, "");

        // Assert
        capturedNotification.Message.Should().NotContain("Assigned veterinarian:");
    }

    #endregion

    #region NotifyAppointmentCancelledAsync Tests

    [Fact]
    public async Task NotifyAppointmentCancelledAsync_WithReason_ShouldCreateCorrectNotification()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        var reason = "Pet owner requested cancellation";
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyAppointmentCancelledAsync(appointment, reason);

        // Assert
        _notificationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Notification>()), Times.Once);

        capturedNotification.Should().NotBeNull();
        capturedNotification.UserId.Should().Be(appointment.OwnerUserId);
        capturedNotification.Type.Should().Be(NotificationType.AppointmentCancelled);
        capturedNotification.Title.Should().Be("Appointment Cancelled ❌");
        capturedNotification.Message.Should().Contain("Buddy");
        capturedNotification.Message.Should().Contain("Dec 15, 2024 at 2:30 PM");
        capturedNotification.Message.Should().Contain("Pet owner requested cancellation");

        // Verify JSON data structure
        var data = JsonSerializer.Deserialize<JsonElement>(capturedNotification.Data!);
        data.GetProperty("appointmentId").GetGuid().Should().Be(appointment.Id);
        data.GetProperty("petId").GetGuid().Should().Be(appointment.PetId);
        data.GetProperty("petName").GetString().Should().Be("Buddy");
        data.GetProperty("reason").GetString().Should().Be(reason);
    }

    [Fact]
    public async Task NotifyAppointmentCancelledAsync_WithEmptyReason_ShouldCreateNotificationWithoutReason()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyAppointmentCancelledAsync(appointment, "");

        // Assert
        capturedNotification.Message.Should().NotContain("Reason:");
    }

    [Fact]
    public async Task NotifyAppointmentCancelledAsync_WithNullReason_ShouldCreateNotificationWithoutReason()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyAppointmentCancelledAsync(appointment, null!);

        // Assert
        capturedNotification.Message.Should().NotContain("Reason:");
    }

    #endregion

    #region NotifyVetAssignedAsync Tests

    [Fact]
    public async Task NotifyVetAssignedAsync_ShouldCreateCorrectNotification()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        var vetUserId = "vet-456";
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyVetAssignedAsync(appointment, vetUserId);

        // Assert
        _notificationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Notification>()), Times.Once);

        capturedNotification.Should().NotBeNull();
        capturedNotification.UserId.Should().Be(vetUserId);
        capturedNotification.Type.Should().Be(NotificationType.AppointmentAssigned);
        capturedNotification.Title.Should().Be("New Appointment Assigned");
        capturedNotification.Message.Should().Contain("Buddy");
        capturedNotification.Message.Should().Contain("Dec 15, 2024 at 2:30 PM");

        // Verify JSON data structure
        var data = JsonSerializer.Deserialize<JsonElement>(capturedNotification.Data!);
        data.GetProperty("AppointmentId").GetGuid().Should().Be(appointment.Id);
        data.GetProperty("PetName").GetString().Should().Be("Buddy");
        data.GetProperty("ReasonForVisit").GetString().Should().Be("Routine checkup");
    }

    [Fact]
    public async Task NotifyVetAssignedAsync_WithNullPet_ShouldHandleGracefully()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        appointment.Pet = null; // Simulate null pet
        var vetUserId = "vet-456";
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.NotifyVetAssignedAsync(appointment, vetUserId);

        // Assert
        capturedNotification.Message.Should().Contain("a pet");
        capturedNotification.Message.Should().NotContain("null");

        var data = JsonSerializer.Deserialize<JsonElement>(capturedNotification.Data!);
        data.GetProperty("PetName").GetString().Should().Be("Unknown Pet");
    }

    #endregion

    #region CreateNotificationAsync Tests

    [Fact]
    public async Task CreateNotificationAsync_WithData_ShouldCreateCorrectNotification()
    {
        // Arrange
        var userId = "user-123";
        var title = "Test Notification";
        var message = "This is a test message";
        var data = new { TestProperty = "TestValue", Number = 42 };
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.CreateNotificationAsync(userId, title, message, data);

        // Assert
        _notificationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Notification>()), Times.Once);

        capturedNotification.Should().NotBeNull();
        capturedNotification.UserId.Should().Be(userId);
        capturedNotification.Type.Should().Be(NotificationType.General);
        capturedNotification.Title.Should().Be(title);
        capturedNotification.Message.Should().Be(message);
        capturedNotification.Data.Should().NotBeNull();

        var jsonData = JsonSerializer.Deserialize<JsonElement>(capturedNotification.Data!);
        jsonData.GetProperty("TestProperty").GetString().Should().Be("TestValue");
        jsonData.GetProperty("Number").GetInt32().Should().Be(42);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithoutData_ShouldCreateNotificationWithNullData()
    {
        // Arrange
        var userId = "user-123";
        var title = "Test Notification";
        var message = "This is a test message";
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.CreateNotificationAsync(userId, title, message);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification.UserId.Should().Be(userId);
        capturedNotification.Type.Should().Be(NotificationType.General);
        capturedNotification.Title.Should().Be(title);
        capturedNotification.Message.Should().Be(message);
        capturedNotification.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullData_ShouldCreateNotificationWithNullData()
    {
        // Arrange
        var userId = "user-123";
        var title = "Test Notification";
        var message = "This is a test message";
        Notification capturedNotification = null!;

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .Callback<Notification>(notification => capturedNotification = notification)
            .ReturnsAsync((Notification n) => n);

        // Act
        await _notificationService.CreateNotificationAsync(userId, title, message, null);

        // Assert
        capturedNotification.Data.Should().BeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task NotifyAppointmentApprovedAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        var expectedException = new Exception("Database error");

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _notificationService.NotifyAppointmentApprovedAsync(appointment, "Dr. Smith"));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task NotifyVetAssignedAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var appointment = CreateSampleAppointment();
        var expectedException = new InvalidOperationException("Repository error");

        _notificationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _notificationService.NotifyVetAssignedAsync(appointment, "vet-123"));
        
        exception.Should().Be(expectedException);
    }

    #endregion
}
