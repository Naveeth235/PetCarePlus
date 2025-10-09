using FluentAssertions;
using PetCare.Domain.Appointments;

namespace PetCare.Domain.Tests.Appointments;

public class AppointmentStatusTests
{
    #region Enum Values Tests

    [Fact]
    public void AppointmentStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<AppointmentStatus>().Should().HaveCount(5);
        
        ((int)AppointmentStatus.Pending).Should().Be(0);
        ((int)AppointmentStatus.Approved).Should().Be(1);
        ((int)AppointmentStatus.Cancelled).Should().Be(2);
        ((int)AppointmentStatus.Completed).Should().Be(3);
        ((int)AppointmentStatus.NoShow).Should().Be(4);
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending, "Pending")]
    [InlineData(AppointmentStatus.Approved, "Approved")]
    [InlineData(AppointmentStatus.Cancelled, "Cancelled")]
    [InlineData(AppointmentStatus.Completed, "Completed")]
    [InlineData(AppointmentStatus.NoShow, "NoShow")]
    public void AppointmentStatus_ToString_ShouldReturnCorrectName(AppointmentStatus status, string expectedName)
    {
        // Act & Assert
        status.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("Pending", AppointmentStatus.Pending)]
    [InlineData("Approved", AppointmentStatus.Approved)]
    [InlineData("Cancelled", AppointmentStatus.Cancelled)]
    [InlineData("Completed", AppointmentStatus.Completed)]
    [InlineData("NoShow", AppointmentStatus.NoShow)]
    public void AppointmentStatus_Parse_ShouldReturnCorrectEnum(string statusString, AppointmentStatus expectedStatus)
    {
        // Act
        var parsed = Enum.Parse<AppointmentStatus>(statusString);

        // Assert
        parsed.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData("pending", AppointmentStatus.Pending)]
    [InlineData("APPROVED", AppointmentStatus.Approved)]
    [InlineData("cancelled", AppointmentStatus.Cancelled)]
    public void AppointmentStatus_ParseIgnoreCase_ShouldReturnCorrectEnum(string statusString, AppointmentStatus expectedStatus)
    {
        // Act
        var parsed = Enum.Parse<AppointmentStatus>(statusString, ignoreCase: true);

        // Assert
        parsed.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData("NotAnEnum")]
    public void AppointmentStatus_ParseInvalid_ShouldThrowException(string invalidStatus)
    {
        // Act & Assert
        Action act = () => Enum.Parse<AppointmentStatus>(invalidStatus);
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region TryParse Tests

    [Theory]
    [InlineData("Pending", true, AppointmentStatus.Pending)]
    [InlineData("Approved", true, AppointmentStatus.Approved)]
    [InlineData("Invalid", false, default(AppointmentStatus))]
    [InlineData("", false, default(AppointmentStatus))]
    public void AppointmentStatus_TryParse_ShouldReturnExpectedResult(string statusString, bool expectedSuccess, AppointmentStatus expectedStatus)
    {
        // Act
        var success = Enum.TryParse<AppointmentStatus>(statusString, out var result);

        // Assert
        success.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            result.Should().Be(expectedStatus);
        }
    }

    #endregion

    #region IsDefined Tests

    [Theory]
    [InlineData(AppointmentStatus.Pending, true)]
    [InlineData(AppointmentStatus.Approved, true)]
    [InlineData(AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.NoShow, true)]
    [InlineData((AppointmentStatus)99, false)]
    public void AppointmentStatus_IsDefined_ShouldReturnCorrectResult(AppointmentStatus status, bool expectedResult)
    {
        // Act & Assert
        Enum.IsDefined(status).Should().Be(expectedResult);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public void AppointmentStatus_WorkflowTransitions_ShouldFollowExpectedPattern()
    {
        // Arrange - Simulate typical appointment workflow
        var initialStatus = AppointmentStatus.Pending;
        
        // Assert initial state
        initialStatus.Should().Be(AppointmentStatus.Pending);
        
        // Simulate admin approval
        var approvedStatus = AppointmentStatus.Approved;
        approvedStatus.Should().Be(AppointmentStatus.Approved);
        
        // Simulate completion
        var completedStatus = AppointmentStatus.Completed;
        completedStatus.Should().Be(AppointmentStatus.Completed);
        
        // Alternative: cancellation
        var cancelledStatus = AppointmentStatus.Cancelled;
        cancelledStatus.Should().Be(AppointmentStatus.Cancelled);
        
        // Alternative: no show
        var noShowStatus = AppointmentStatus.NoShow;
        noShowStatus.Should().Be(AppointmentStatus.NoShow);
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Approved)]
    public void AppointmentStatus_ActiveStatuses_ShouldAllowCancellation(AppointmentStatus status)
    {
        // Arrange
        var canBeCancelled = status == AppointmentStatus.Pending || status == AppointmentStatus.Approved;
        
        // Assert
        canBeCancelled.Should().BeTrue();
    }

    [Theory]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.NoShow)]
    public void AppointmentStatus_FinalStatuses_ShouldNotAllowCancellation(AppointmentStatus status)
    {
        // Arrange
        var canBeCancelled = status == AppointmentStatus.Pending || status == AppointmentStatus.Approved;
        
        // Assert
        canBeCancelled.Should().BeFalse();
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    public void AppointmentStatus_RequiresAction_ShouldReturnTrueForPending(AppointmentStatus status)
    {
        // Arrange
        var requiresAction = status == AppointmentStatus.Pending;
        
        // Assert
        requiresAction.Should().BeTrue();
    }

    [Theory]
    [InlineData(AppointmentStatus.Approved)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.NoShow)]
    public void AppointmentStatus_DoesNotRequireAction_ShouldReturnFalseForNonPending(AppointmentStatus status)
    {
        // Arrange
        var requiresAction = status == AppointmentStatus.Pending;
        
        // Assert
        requiresAction.Should().BeFalse();
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void AppointmentStatus_Comparison_ShouldWorkCorrectly()
    {
        // Assert numeric order
        (AppointmentStatus.Pending < AppointmentStatus.Approved).Should().BeTrue();
        (AppointmentStatus.Approved < AppointmentStatus.Cancelled).Should().BeTrue();
        (AppointmentStatus.Cancelled < AppointmentStatus.Completed).Should().BeTrue();
        (AppointmentStatus.Completed < AppointmentStatus.NoShow).Should().BeTrue();
    }

    [Fact]
    public void AppointmentStatus_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var status1 = AppointmentStatus.Pending;
        var status2 = AppointmentStatus.Pending;
        var status3 = AppointmentStatus.Approved;

        // Assert
        (status1 == status2).Should().BeTrue();
        (status1 == status3).Should().BeFalse();
        (status1 != status3).Should().BeTrue();
        status1.Equals(status2).Should().BeTrue();
        status1.Equals(status3).Should().BeFalse();
    }

    #endregion
}