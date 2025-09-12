using PetCare.Application.Admin.Users.CreateVet;
using Xunit;

namespace PetCare.Application.Admin.Users.CreateVet.Tests
{
    public class CreateVetResponseTests
    {
        [Fact]
        public void Constructor_Sets_Properties_Correctly()
        {
            var response = new CreateVetResponse(
                VetUserId: "vet123",
                Email: "vet@example.com"
            );

            Assert.Equal("vet123", response.VetUserId);
            Assert.Equal("vet@example.com", response.Email);
        }

        [Fact]
        public void WithExpression_Creates_New_Instance_With_Updated_Values()
        {
            var original = new CreateVetResponse(
                VetUserId: "vet123",
                Email: "vet@example.com"
            );

            var updated = original with { Email = "newvet@example.com" };

            Assert.Equal("vet123", updated.VetUserId);
            Assert.Equal("newvet@example.com", updated.Email);
        }
    }
}