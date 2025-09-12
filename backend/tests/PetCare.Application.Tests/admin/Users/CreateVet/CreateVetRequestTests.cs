using PetCare.Application.Admin.Users.CreateVet;
using Xunit;

namespace PetCare.Application.Admin.Users.CreateVet.Tests
{
    public class CreateVetRequestTests
    {
        [Fact]
        public void Constructor_Sets_Properties_Correctly()
        {
            var request = new CreateVetRequest(
                FullName: "Dr. Jane Doe",
                Email: "jane.doe@vetclinic.com",
                Password: "StrongPassword123"
            );

            Assert.Equal("Dr. Jane Doe", request.FullName);
            Assert.Equal("jane.doe@vetclinic.com", request.Email);
            Assert.Equal("StrongPassword123", request.Password);
        }

        [Fact]
        public void WithExpression_Creates_New_Instance_With_Updated_Values()
        {
            var original = new CreateVetRequest(
                FullName: "Dr. John Smith",
                Email: "john.smith@vetclinic.com",
                Password: "Password1"
            );

            var updated = original with { FullName = "Dr. John S. Smith" };

            Assert.Equal("Dr. John S. Smith", updated.FullName);
            Assert.Equal(original.Email, updated.Email);
            Assert.Equal(original.Password, updated.Password);
        }
    }
}