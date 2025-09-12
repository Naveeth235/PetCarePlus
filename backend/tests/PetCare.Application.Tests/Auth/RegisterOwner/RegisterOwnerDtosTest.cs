using System;
using PetCare.Application.Auth.RegisterOwner;
using Xunit;

namespace PetCare.Application.Auth.RegisterOwner.Tests
{
    public class RegisterOwnerDtosTests
    {
        [Fact]
        public void RegisterOwnerRequest_Properties_SetAndGet()
        {
            var pet = new RegisterOwnerRequest.PetRequest
            {
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Dob = new DateTime(2022, 1, 1)
            };

            var request = new RegisterOwnerRequest
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "securepassword",
                Pet = pet
            };

            Assert.Equal("John Doe", request.FullName);
            Assert.Equal("john@example.com", request.Email);
            Assert.Equal("securepassword", request.Password);
            Assert.NotNull(request.Pet);
            Assert.Equal("Buddy", request.Pet!.Name);
            Assert.Equal("Dog", request.Pet!.Species);
            Assert.Equal("Golden Retriever", request.Pet!.Breed);
            Assert.Equal(new DateTime(2022, 1, 1), request.Pet!.Dob);
        }

        [Fact]
        public void RegisterOwnerRequest_PetRequest_Defaults()
        {
            var pet = new RegisterOwnerRequest.PetRequest();
            Assert.NotNull(pet.Name);
            Assert.NotNull(pet.Species);
            Assert.Null(pet.Breed);
            Assert.Null(pet.Dob);
        }

        [Fact]
        public void RegisterOwnerResponse_DefaultMessage()
        {
            var response = new RegisterOwnerResponse();
            Assert.Equal("Owner registered successfully.", response.Message);
        }

        [Fact]
        public void RegisterOwnerRequest_Pet_CanBeNull()
        {
            var request = new RegisterOwnerRequest
            {
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Password = "password123",
                Pet = null
            };

            Assert.Null(request.Pet);
        }
    }
}