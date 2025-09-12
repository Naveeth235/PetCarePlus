using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Moq;
using PetCare.Application.Users.Profile;
using PetCare.Infrastructure.Auth;
using Xunit;

namespace PetCare.Application.Users.Profile.Tests
{
    public class UpdateProfileCommandTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public UpdateProfileCommandTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var command = new UpdateProfileCommand(_userManagerMock.Object);
            var req = new UpdateProfileRequest { FullName = "Test", PhoneNumber = "1234567890" };

            var result = await command.ExecuteAsync("userId", req);

            Assert.False(result.ok);
            Assert.Equal("not_found", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsIdentityError_WhenUpdateFails()
        {
            var user = new ApplicationUser { Id = "userId", Email = "user@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Update failed" });
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(identityResult);

            var command = new UpdateProfileCommand(_userManagerMock.Object);
            var req = new UpdateProfileRequest { FullName = "Test", PhoneNumber = "1234567890" };

            var result = await command.ExecuteAsync("userId", req);

            Assert.False(result.ok);
            Assert.StartsWith("identity_error:", result.error);
            Assert.Contains("Update failed", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsSuccess_WhenUpdateSucceeds()
        {
            var user = new ApplicationUser
            {
                Id = "userId",
                Email = "user@example.com",
                FullName = "Old Name",
                PhoneNumber = "0000000000"
            };
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new[] { "Owner" });

            var command = new UpdateProfileCommand(_userManagerMock.Object);
            var req = new UpdateProfileRequest { FullName = "New Name", PhoneNumber = "1234567890" };

            var result = await command.ExecuteAsync("userId", req);

            Assert.True(result.ok);
            Assert.Null(result.error);
            Assert.NotNull(result.data);
            Assert.Equal("userId", result.data.Id);
            Assert.Equal("New Name", result.data.FullName);
            Assert.Equal("user@example.com", result.data.Email);
            Assert.Equal("1234567890", result.data.PhoneNumber);
            Assert.Equal("Owner", result.data.Role);
        }

        [Fact]
        public async Task ExecuteAsync_SetsPhoneNumberToNull_WhenPhoneNumberIsWhitespace()
        {
            var user = new ApplicationUser
            {
                Id = "userId",
                Email = "user@example.com",
                FullName = "Old Name",
                PhoneNumber = "0000000000"
            };
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new[] { "Owner" });

            var command = new UpdateProfileCommand(_userManagerMock.Object);
            var req = new UpdateProfileRequest { FullName = "New Name", PhoneNumber = "   " };

            var result = await command.ExecuteAsync("userId", req);

            Assert.True(result.ok);
            Assert.Null(result.error);
            Assert.NotNull(result.data);
            Assert.Null(result.data.PhoneNumber);
        }
    }
}