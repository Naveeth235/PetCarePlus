// C#
using System;
using Xunit;
using PetCare.Application.Auth.Login;

namespace PetCare.Application.Auth.Login
{
    public class LoginResponseTests
    {
        [Fact]
        public void CanSetAndGetLoginResponseProperties()
        {
            var userDto = new LoginResponse.UserDto
            {
                Id = "user123",
                Role = "Admin",
                FullName = "Jane Doe",
                Email = "jane@example.com"
            };

            var response = new LoginResponse
            {
                AccessToken = "token_abc",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = userDto
            };

            Assert.Equal("token_abc", response.AccessToken);
            Assert.True(response.ExpiresAt > DateTime.UtcNow);
            Assert.Equal(userDto, response.User);
        }

        [Fact]
        public void CanSetAndGetUserDtoProperties()
        {
            var userDto = new LoginResponse.UserDto
            {
                Id = "user456",
                Role = "User",
                FullName = "John Smith",
                Email = "john@example.com"
            };

            Assert.Equal("user456", userDto.Id);
            Assert.Equal("User", userDto.Role);
            Assert.Equal("John Smith", userDto.FullName);
            Assert.Equal("john@example.com", userDto.Email);
        }

        [Fact]
        public void DefaultValuesAreNotNull()
        {
            var response = new LoginResponse();
            Assert.NotNull(response.AccessToken);
            Assert.NotNull(response.User);

            var userDto = new LoginResponse.UserDto();
            Assert.NotNull(userDto.Id);
            Assert.NotNull(userDto.Role);
            Assert.NotNull(userDto.FullName);
            Assert.NotNull(userDto.Email);
        }
    }
}