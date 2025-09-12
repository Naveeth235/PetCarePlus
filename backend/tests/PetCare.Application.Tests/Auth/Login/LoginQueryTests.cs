using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using PetCare.Application.Auth.Login;
using PetCare.Domain.Users;
using PetCare.Infrastructure.Auth;
using PetCare.Infrastructure.Jwt;
using Xunit;

namespace PetCare.Application.Auth.Login.Tests
{
    public class LoginQueryTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;

        public LoginQueryTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsInvalidCredentials_WhenUserNotFound()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var query = new LoginQuery(_userManagerMock.Object, _jwtTokenGeneratorMock.Object);
            var request = new LoginRequest { Email = "test@test.com", Password = "pass" };

            var result = await query.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Equal("invalid_credentials", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsInactive_WhenAccountStatusIsNotActive()
        {
            var user = new ApplicationUser { AccountStatus = AccountStatus.Inactive };
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var query = new LoginQuery(_userManagerMock.Object, _jwtTokenGeneratorMock.Object);
            var request = new LoginRequest { Email = "test@test.com", Password = "pass" };

            var result = await query.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Equal("inactive", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsInvalidCredentials_WhenPasswordIsIncorrect()
        {
            var user = new ApplicationUser { AccountStatus = AccountStatus.Active };
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(false);

            var query = new LoginQuery(_userManagerMock.Object, _jwtTokenGeneratorMock.Object);
            var request = new LoginRequest { Email = "test@test.com", Password = "wrongpass" };

            var result = await query.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Equal("invalid_credentials", result.error);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsSuccess_WhenCredentialsAreValid()
        {
            var user = new ApplicationUser
            {
                Id = "user123",
                AccountStatus = AccountStatus.Active,
                FullName = "Test User",
                Email = "test@test.com"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new[] { "Admin" });

            var token = "jwt-token";
            var expiresAt = DateTime.UtcNow.AddHours(1);
            _jwtTokenGeneratorMock.Setup(x => x.Create(user, "Admin"))
                .Returns((token, expiresAt));

            var query = new LoginQuery(_userManagerMock.Object, _jwtTokenGeneratorMock.Object);
            var request = new LoginRequest { Email = "test@test.com", Password = "pass" };

            var result = await query.ExecuteAsync(request);

            Assert.True(result.ok);
            Assert.Null(result.error);
            Assert.NotNull(result.data);
            Assert.Equal(token, result.data.AccessToken);
            Assert.Equal(expiresAt, result.data.ExpiresAt);
            Assert.Equal("user123", result.data.User.Id);
            Assert.Equal("Admin", result.data.User.Role);
            Assert.Equal("Test User", result.data.User.FullName);
            Assert.Equal("test@test.com", result.data.User.Email);
        }
    }
}