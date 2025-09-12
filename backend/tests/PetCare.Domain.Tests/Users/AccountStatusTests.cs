using Xunit;
using PetCare.Domain.Users;

namespace PetCare.Domain.Tests.Users
{
    public class AccountStatusTests
    {
        [Fact]
        public void AccountStatus_HasExpectedValues()
        {
            Assert.Equal(1, (byte)AccountStatus.Active);
            Assert.Equal(2, (byte)AccountStatus.Inactive);
            Assert.Equal(3, (byte)AccountStatus.Suspended);
        }

        [Fact]
        public void AccountStatus_ContainsAllExpectedNames()
        {
            var names = Enum.GetNames(typeof(AccountStatus));
            Assert.Contains("Active", names);
            Assert.Contains("Inactive", names);
            Assert.Contains("Suspended", names);
        }
    }
}
