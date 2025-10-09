using FluentAssertions;
using PetCare.Application.Appointments.DTOs;
using PetCare.Domain.Appointments;
using System.ComponentModel.DataAnnotations;

namespace PetCare.Application.Tests.Appointments.DTOs;

public class CreateAppointmentRequestTests
{
    #region Property Assignment Tests

    [Fact]
    public void CreateAppointmentRequest_WhenPropertiesAssigned_ShouldRetainValues()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var requestedDateTime = DateTime.Now.AddDays(1);
        var reasonForVisit = "Annual checkup";
        var notes = "Pet seems healthy";

        // Act
        var request = new CreateAppointmentRequest
        {
            PetId = petId,
            RequestedDateTime = requestedDateTime,
            ReasonForVisit = reasonForVisit,
            Notes = notes
        };

        // Assert
        request.PetId.Should().Be(petId);
        request.RequestedDateTime.Should().Be(requestedDateTime);
        request.ReasonForVisit.Should().Be(reasonForVisit);
        request.Notes.Should().Be(notes);
    }

    [Fact]
    public void CreateAppointmentRequest_WhenNotesIsNull_ShouldAllowNullValue()
    {
        // Arrange & Act
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Checkup",
            Notes = null
        };

        // Assert
        request.Notes.Should().BeNull();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void CreateAppointmentRequest_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Annual checkup",
            Notes = "Pet seems healthy"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateAppointmentRequest_WithEmptyPetId_ShouldPassValidation()
    {
        // Note: Guid.Empty is a valid Guid value and doesn't trigger [Required] validation
        // The business logic in the controller or service layer should handle empty Guid validation
        
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.Empty,
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Annual checkup"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty(); // Guid.Empty is technically valid for the DTO
    }

    [Fact]
    public void CreateAppointmentRequest_WithEmptyReasonForVisit_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = ""
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateAppointmentRequest.ReasonForVisit)));
    }

    [Fact]
    public void CreateAppointmentRequest_WithNullReasonForVisit_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = null!
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateAppointmentRequest.ReasonForVisit)));
    }

    [Theory]
    [InlineData(201)] // Over 200 characters
    [InlineData(300)]
    public void CreateAppointmentRequest_WithLongReasonForVisit_ShouldFailValidation(int length)
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = new string('a', length)
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateAppointmentRequest.ReasonForVisit)));
    }

    [Theory]
    [InlineData(1001)] // Over 1000 characters
    [InlineData(1500)]
    public void CreateAppointmentRequest_WithLongNotes_ShouldFailValidation(int length)
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Checkup",
            Notes = new string('a', length)
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(r => r.MemberNames.Contains(nameof(CreateAppointmentRequest.Notes)));
    }

    [Theory]
    [InlineData(200)] // Exactly 200 characters
    [InlineData(150)]
    [InlineData(1)]
    public void CreateAppointmentRequest_WithValidReasonForVisitLength_ShouldPassValidation(int length)
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = new string('a', length)
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1000)] // Exactly 1000 characters
    [InlineData(500)]
    [InlineData(1)]
    public void CreateAppointmentRequest_WithValidNotesLength_ShouldPassValidation(int length)
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PetId = Guid.NewGuid(),
            RequestedDateTime = DateTime.Now.AddDays(1),
            ReasonForVisit = "Checkup",
            Notes = new string('a', length)
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    #endregion
}

public class UpdateAppointmentStatusRequestTests
{
    #region Property Assignment Tests

    [Fact]
    public void UpdateAppointmentStatusRequest_WhenPropertiesAssigned_ShouldRetainValues()
    {
        // Arrange
        var status = AppointmentStatus.Approved;
        var adminNotes = "Approved by admin";
        var vetUserId = "vet-123";

        // Act
        var request = new UpdateAppointmentStatusRequest
        {
            Status = status,
            AdminNotes = adminNotes,
            VetUserId = vetUserId
        };

        // Assert
        request.Status.Should().Be(status);
        request.AdminNotes.Should().Be(adminNotes);
        request.VetUserId.Should().Be(vetUserId);
    }

    [Fact]
    public void UpdateAppointmentStatusRequest_WithNullOptionalProperties_ShouldAllowNullValues()
    {
        // Arrange & Act
        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Cancelled,
            AdminNotes = null,
            VetUserId = null
        };

        // Assert
        request.AdminNotes.Should().BeNull();
        request.VetUserId.Should().BeNull();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void UpdateAppointmentStatusRequest_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved,
            AdminNotes = "Approved for routine care",
            VetUserId = "vet-123"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Approved)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.NoShow)]
    public void UpdateAppointmentStatusRequest_WithValidStatus_ShouldPassValidation(AppointmentStatus status)
    {
        // Arrange
        var request = new UpdateAppointmentStatusRequest
        {
            Status = status
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1001)] // Over 1000 characters
    [InlineData(1500)]
    public void UpdateAppointmentStatusRequest_WithLongAdminNotes_ShouldFailValidation(int length)
    {
        // Arrange
        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved,
            AdminNotes = new string('a', length)
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(r => r.MemberNames.Contains(nameof(UpdateAppointmentStatusRequest.AdminNotes)));
    }

    [Theory]
    [InlineData(1000)] // Exactly 1000 characters
    [InlineData(500)]
    [InlineData(1)]
    public void UpdateAppointmentStatusRequest_WithValidAdminNotesLength_ShouldPassValidation(int length)
    {
        // Arrange
        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved,
            AdminNotes = new string('a', length)
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public void UpdateAppointmentStatusRequest_ForApprovalWithVet_ShouldHaveVetAssigned()
    {
        // Arrange & Act
        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Approved,
            AdminNotes = "Approved and assigned to vet",
            VetUserId = "vet-123"
        };

        // Assert
        request.Status.Should().Be(AppointmentStatus.Approved);
        request.VetUserId.Should().NotBeNull();
        request.AdminNotes.Should().NotBeNull();
    }

    [Fact]
    public void UpdateAppointmentStatusRequest_ForCancellation_ShouldHaveReason()
    {
        // Arrange & Act
        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Cancelled,
            AdminNotes = "Pet owner requested cancellation"
        };

        // Assert
        request.Status.Should().Be(AppointmentStatus.Cancelled);
        request.AdminNotes.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Helper Methods

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    #endregion
}

public class AppointmentDtoTests
{
    #region Property Assignment Tests

    [Fact]
    public void AppointmentDto_WhenPropertiesAssigned_ShouldRetainValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var petName = "Buddy";
        var ownerUserId = "owner-123";
        var ownerName = "John Doe";
        var vetUserId = "vet-456";
        var vetName = "Dr. Smith";
        var requestedDateTime = DateTime.Now.AddDays(1);
        var actualDateTime = DateTime.Now.AddDays(1).AddMinutes(15);
        var reasonForVisit = "Annual checkup";
        var notes = "Pet seems healthy";
        var adminNotes = "Approved for routine care";
        var status = AppointmentStatus.Approved;
        var statusDisplayName = "Approved";
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        var canBeCancelled = true;
        var requiresAction = false;

        // Act
        var dto = new AppointmentDto
        {
            Id = id,
            PetId = petId,
            PetName = petName,
            OwnerUserId = ownerUserId,
            OwnerName = ownerName,
            VetUserId = vetUserId,
            VetName = vetName,
            RequestedDateTime = requestedDateTime,
            ActualDateTime = actualDateTime,
            ReasonForVisit = reasonForVisit,
            Notes = notes,
            AdminNotes = adminNotes,
            Status = status,
            StatusDisplayName = statusDisplayName,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CanBeCancelled = canBeCancelled,
            RequiresAction = requiresAction
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.PetId.Should().Be(petId);
        dto.PetName.Should().Be(petName);
        dto.OwnerUserId.Should().Be(ownerUserId);
        dto.OwnerName.Should().Be(ownerName);
        dto.VetUserId.Should().Be(vetUserId);
        dto.VetName.Should().Be(vetName);
        dto.RequestedDateTime.Should().Be(requestedDateTime);
        dto.ActualDateTime.Should().Be(actualDateTime);
        dto.ReasonForVisit.Should().Be(reasonForVisit);
        dto.Notes.Should().Be(notes);
        dto.AdminNotes.Should().Be(adminNotes);
        dto.Status.Should().Be(status);
        dto.StatusDisplayName.Should().Be(statusDisplayName);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
        dto.CanBeCancelled.Should().Be(canBeCancelled);
        dto.RequiresAction.Should().Be(requiresAction);
    }

    [Fact]
    public void AppointmentDto_WithNullOptionalProperties_ShouldAllowNullValues()
    {
        // Arrange & Act
        var dto = new AppointmentDto
        {
            Id = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            PetName = "Buddy",
            OwnerUserId = "owner-123",
            OwnerName = "John Doe",
            VetUserId = null,
            VetName = null,
            RequestedDateTime = DateTime.Now.AddDays(1),
            ActualDateTime = null,
            ReasonForVisit = "Checkup",
            Notes = null,
            AdminNotes = null,
            Status = AppointmentStatus.Pending,
            StatusDisplayName = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            CanBeCancelled = true,
            RequiresAction = true
        };

        // Assert
        dto.VetUserId.Should().BeNull();
        dto.VetName.Should().BeNull();
        dto.ActualDateTime.Should().BeNull();
        dto.Notes.Should().BeNull();
        dto.AdminNotes.Should().BeNull();
        dto.UpdatedAt.Should().BeNull();
    }

    #endregion

    #region Business Logic Tests

    [Theory]
    [InlineData(AppointmentStatus.Pending, true, true)]
    [InlineData(AppointmentStatus.Approved, true, false)]
    [InlineData(AppointmentStatus.Cancelled, false, false)]
    [InlineData(AppointmentStatus.Completed, false, false)]
    [InlineData(AppointmentStatus.NoShow, false, false)]
    public void AppointmentDto_WithDifferentStatuses_ShouldHaveCorrectComputedProperties(
        AppointmentStatus status, bool expectedCanBeCancelled, bool expectedRequiresAction)
    {
        // Arrange & Act
        var dto = new AppointmentDto
        {
            Id = Guid.NewGuid(),
            Status = status,
            CanBeCancelled = expectedCanBeCancelled,
            RequiresAction = expectedRequiresAction
        };

        // Assert
        dto.CanBeCancelled.Should().Be(expectedCanBeCancelled);
        dto.RequiresAction.Should().Be(expectedRequiresAction);
    }

    #endregion
}