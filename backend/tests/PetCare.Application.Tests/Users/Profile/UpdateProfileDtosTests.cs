using PetCare.Application.Users.Profile;
using Xunit;

namespace PetCare.Application.Users.Profile.Tests
{
    public class UpdateProfileDtosTests
    {
        [Fact]
        public void UpdateProfileRequest_Properties_SetAndGet()
        {
            var req = new UpdateProfileRequest
            {
                FullName = "Jane Doe",
                PhoneNumber = "1234567890"
            };

            Assert.Equal("Jane Doe", req.FullName);
            Assert.Equal("1234567890", req.PhoneNumber);
        }

        [Fact]
        public void UpdateProfileRequest_PhoneNumber_CanBeNull()
        {
            var req = new UpdateProfileRequest
            {
                FullName = "Jane Doe",
                PhoneNumber = null
            };

            Assert.Equal("Jane Doe", req.FullName);
            Assert.Null(req.PhoneNumber);
        }

        [Fact]
        public void UpdateProfileResponse_Properties_SetAndGet()
        {
            var res = new UpdateProfileResponse
            {
                Id = "user123",
                FullName = "Jane Doe",
                Email = "jane@example.com",
                PhoneNumber = "1234567890",
                Role = "Owner"
            };

            Assert.Equal("user123", res.Id);
            Assert.Equal("Jane Doe", res.FullName);
            Assert.Equal("jane@example.com", res.Email);
            Assert.Equal("1234567890", res.PhoneNumber);
            Assert.Equal("Owner", res.Role);
        }

        [Fact]
        public void UpdateProfileResponse_PhoneNumber_CanBeNull()
        {
            var res = new UpdateProfileResponse
            {
                Id = "user123",
                FullName = "Jane Doe",
                Email = "jane@example.com",
                PhoneNumber = null,
                Role = "Owner"
            };

            Assert.Equal("user123", res.Id);
            Assert.Equal("Jane Doe", res.FullName);
            Assert.Equal("jane@example.com", res.Email);
            Assert.Null(res.PhoneNumber);
            Assert.Equal("Owner", res.Role);
        }
    }
}