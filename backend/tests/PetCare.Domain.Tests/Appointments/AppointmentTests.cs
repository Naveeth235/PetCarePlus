using FluentAssertions;
using PetCare.Domain.Appointments;

namespace PetCare.Domain.Tests.Appointments;

public class AppointmentTests
{
    #region Constructor Tests

    [Fact]
    public void Appointment_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var appointment = new Appointment();

        // Assert
        appointment.Id.Should().NotBeEmpty();
        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.UpdatedAt.Should().BeNull();
        appointment.ActualDateTime.Should().BeNull();
        appointment.VetUserId.Should().BeNull();
        appointment.AdminNotes.Should().BeNull();
        appointment.UpdatedByUserId.Should().BeNull();
    }

    #endregion

    #region StatusDisplayName Tests

    [Theory]
    [InlineData(AppointmentStatus.Pending, "Pending")]
    [InlineData(AppointmentStatus.Approved, "Approved")]
    [InlineData(AppointmentStatus.Cancelled, "Cancelled")]
    [InlineData(AppointmentStatus.Completed, "Completed")]
    [InlineData(AppointmentStatus.NoShow, "NoShow")]
    public void StatusDisplayName_ShouldReturnCorrectDisplayName(AppointmentStatus status, string expectedDisplayName)
    {
        // Arrange
        var appointment = new Appointment { Status = status };

        // Act & Assert
        appointment.StatusDisplayName.Should().Be(expectedDisplayName);
    }

    #endregion

    #region CanBeCancelled Tests

    [Theory]
    [InlineData(AppointmentStatus.Pending, true)]
    [InlineData(AppointmentStatus.Approved, true)]
    [InlineData(AppointmentStatus.Cancelled, false)]
    [InlineData(AppointmentStatus.Completed, false)]
    [InlineData(AppointmentStatus.NoShow, false)]
    public void CanBeCancelled_ShouldReturnCorrectValue(AppointmentStatus status, bool expectedCanBeCancelled)
    {
        // Arrange
        var appointment = new Appointment { Status = status };

        // Act & Assert
        appointment.CanBeCancelled.Should().Be(expectedCanBeCancelled);
    }

    #endregion

    #region RequiresAction Tests

    [Theory]
    [InlineData(AppointmentStatus.Pending, true)]
    [InlineData(AppointmentStatus.Approved, false)]
    [InlineData(AppointmentStatus.Cancelled, false)]
    [InlineData(AppointmentStatus.Completed, false)]
    [InlineData(AppointmentStatus.NoShow, false)]
    public void RequiresAction_ShouldReturnCorrectValue(AppointmentStatus status, bool expectedRequiresAction)
    {
        // Arrange
        var appointment = new Appointment { Status = status };

        // Act & Assert
        appointment.RequiresAction.Should().Be(expectedRequiresAction);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Appointment_WhenPropertiesAssigned_ShouldRetainValues()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var ownerUserId = "owner-123";
        var vetUserId = "vet-456";
        var requestedDateTime = DateTime.Now.AddDays(1);
        var actualDateTime = DateTime.Now.AddDays(1).AddMinutes(15);
        var reasonForVisit = "Annual checkup";
        var notes = "Pet has been healthy";
        var adminNotes = "Approved for routine care";
        var updatedByUserId = "admin-789";
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var appointment = new Appointment
        {
            Id = appointmentId,
            PetId = petId,
            OwnerUserId = ownerUserId,
            VetUserId = vetUserId,
            RequestedDateTime = requestedDateTime,
            ActualDateTime = actualDateTime,
            ReasonForVisit = reasonForVisit,
            Notes = notes,
            AdminNotes = adminNotes,
            Status = AppointmentStatus.Approved,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        // Assert
        appointment.Id.Should().Be(appointmentId);
        appointment.PetId.Should().Be(petId);
        appointment.OwnerUserId.Should().Be(ownerUserId);
        appointment.VetUserId.Should().Be(vetUserId);
        appointment.RequestedDateTime.Should().Be(requestedDateTime);
        appointment.ActualDateTime.Should().Be(actualDateTime);
        appointment.ReasonForVisit.Should().Be(reasonForVisit);
        appointment.Notes.Should().Be(notes);
        appointment.AdminNotes.Should().Be(adminNotes);
        appointment.Status.Should().Be(AppointmentStatus.Approved);
        appointment.CreatedAt.Should().Be(createdAt);
        appointment.UpdatedAt.Should().Be(updatedAt);
        appointment.UpdatedByUserId.Should().Be(updatedByUserId);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public void Appointment_WhenPending_ShouldAllowStatusUpdate()
    {
        // Arrange
        var appointment = new Appointment
        {
            Status = AppointmentStatus.Pending,
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Checkup"
        };

        // Act
        appointment.Status = AppointmentStatus.Approved;
        appointment.VetUserId = "vet-123";
        appointment.ActualDateTime = appointment.RequestedDateTime;
        appointment.AdminNotes = "Approved by admin";
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedByUserId = "admin-456";

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Approved);
        appointment.VetUserId.Should().Be("vet-123");
        appointment.ActualDateTime.Should().NotBeNull();
        appointment.AdminNotes.Should().Be("Approved by admin");
        appointment.CanBeCancelled.Should().BeTrue();
        appointment.RequiresAction.Should().BeFalse();
    }

    [Fact]
    public void Appointment_WhenCancelled_ShouldNotAllowFurtherUpdates()
    {
        // Arrange
        var appointment = new Appointment
        {
            Status = AppointmentStatus.Cancelled,
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Checkup",
            AdminNotes = "Cancelled by owner request"
        };

        // Assert
        appointment.CanBeCancelled.Should().BeFalse();
        appointment.RequiresAction.Should().BeFalse();
    }

    [Fact]
    public void Appointment_WhenCompleted_ShouldHaveFinalState()
    {
        // Arrange
        var appointment = new Appointment
        {
            Status = AppointmentStatus.Completed,
            RequestedDateTime = DateTime.Now.AddDays(-1),
            ActualDateTime = DateTime.Now.AddDays(-1),
            ReasonForVisit = "Checkup",
            VetUserId = "vet-123"
        };

        // Assert
        appointment.CanBeCancelled.Should().BeFalse();
        appointment.RequiresAction.Should().BeFalse();
        appointment.ActualDateTime.Should().NotBeNull();
        appointment.VetUserId.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Appointment_WithNullOptionalProperties_ShouldHandleGracefully()
    {
        // Arrange & Act
        var appointment = new Appointment
        {
            PetId = Guid.NewGuid(),
            OwnerUserId = "owner-123",
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Checkup",
            VetUserId = null,
            Notes = null,
            AdminNotes = null,
            ActualDateTime = null,
            UpdatedAt = null,
            UpdatedByUserId = null
        };

        // Assert
        appointment.VetUserId.Should().BeNull();
        appointment.Notes.Should().BeNull();
        appointment.AdminNotes.Should().BeNull();
        appointment.ActualDateTime.Should().BeNull();
        appointment.UpdatedAt.Should().BeNull();
        appointment.UpdatedByUserId.Should().BeNull();
        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.CanBeCancelled.Should().BeTrue();
        appointment.RequiresAction.Should().BeTrue();
    }

    [Fact]
    public void Appointment_WithEmptyStringProperties_ShouldHandleGracefully()
    {
        // Arrange & Act
        var appointment = new Appointment
        {
            PetId = Guid.NewGuid(),
            OwnerUserId = "owner-123",
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Checkup",
            VetUserId = "",
            Notes = "",
            AdminNotes = "",
            UpdatedByUserId = ""
        };

        // Assert
        appointment.VetUserId.Should().Be("");
        appointment.Notes.Should().Be("");
        appointment.AdminNotes.Should().Be("");
        appointment.UpdatedByUserId.Should().Be("");
    }

    #endregion
}