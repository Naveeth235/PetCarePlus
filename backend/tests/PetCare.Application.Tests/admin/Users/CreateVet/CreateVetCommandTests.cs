using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using PetCare.Application.Admin.Users.CreateVet;
using PetCare.Infrastructure.Auth;
using Xunit;

namespace PetCare.Application.Admin.Users.CreateVet.Tests
{
    public class CreateVetCommandTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public CreateVetCommandTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsValidationFailed_WhenFieldsAreMissing()
        {
            var command = new CreateVetCommand(_userManagerMock.Object);

            var request = new CreateVetRequest { Email = "", FullName = "", Password = "" };
            var result = await command.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Equal("validation_failed", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsEmailInUse_WhenUserExists()
        {
            var existingUser = new ApplicationUser();
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingUser);

            var command = new CreateVetCommand(_userManagerMock.Object);
            var request = new CreateVetRequest { Email = "vet@example.com", FullName = "Vet Name", Password = "Password1" };
            var result = await command.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Equal("email_in_use", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsIdentityError_WhenCreateFails()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var identityResult = IdentityResult.Failed(new IdentityError { Code = "Password", Description = "Too weak" });
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult);

            var command = new CreateVetCommand(_userManagerMock.Object);
            var request = new CreateVetRequest { Email = "vet@example.com", FullName = "Vet Name", Password = "Password1" };
            var result = await command.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.StartsWith("identity_error:", result.error);
            Assert.Contains("Password:Too weak", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsRoleError_WhenRoleAssignmentFails()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var roleResult = IdentityResult.Failed(new IdentityError { Code = "Role", Description = "Role add failed" });
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "VET"))
                .ReturnsAsync(roleResult);

            var command = new CreateVetCommand(_userManagerMock.Object);
            var request = new CreateVetRequest { Email = "vet@example.com", FullName = "Vet Name", Password = "Password1" };
            var result = await command.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.StartsWith("role_error:", result.error);
            Assert.Contains("Role:Role add failed", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsSuccess_WhenAllStepsSucceed()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "VET"))
                .ReturnsAsync(IdentityResult.Success);

            var command = new CreateVetCommand(_userManagerMock.Object);
            var request = new CreateVetRequest { Email = "vet@example.com", FullName = "Vet Name", Password = "Password1" };
            var result = await command.ExecuteAsync(request);

            Assert.True(result.ok);
            Assert.Null(result.error);
            Assert.NotNull(result.data);
            Assert.Equal("vet@example.com", result.data.Email);
        }
    }
}