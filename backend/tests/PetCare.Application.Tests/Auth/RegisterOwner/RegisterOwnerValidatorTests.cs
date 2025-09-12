using System;
using FluentValidation.TestHelper;
using PetCare.Application.Auth.RegisterOwner;
using Xunit;

namespace PetCare.Application.Auth.RegisterOwner.Tests
{
    public class RegisterOwnerValidatorTests
    {
        private readonly RegisterOwnerValidator _validator = new RegisterOwnerValidator();

        [Fact]
        public void Should_Have_Error_When_FullName_Is_Empty()
        {
            var model = new RegisterOwnerRequest { FullName = "", Email = "user@example.com", Password = "Password1" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Should_Have_Error_When_FullName_Too_Long()
        {
            var model = new RegisterOwnerRequest { FullName = new string('a', 129), Email = "user@example.com", Password = "Password1" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty_Or_Invalid()
        {
            var model1 = new RegisterOwnerRequest { FullName = "Test", Email = "", Password = "Password1" };
            var model2 = new RegisterOwnerRequest { FullName = "Test", Email = "invalid", Password = "Password1" };
            var result1 = _validator.TestValidate(model1);
            var result2 = _validator.TestValidate(model2);
            result1.ShouldHaveValidationErrorFor(x => x.Email);
            result2.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty_Or_Weak()
        {
            var model1 = new RegisterOwnerRequest { FullName = "Test", Email = "user@example.com", Password = "" };
            var model2 = new RegisterOwnerRequest { FullName = "Test", Email = "user@example.com", Password = "short" };
            var model3 = new RegisterOwnerRequest { FullName = "Test", Email = "user@example.com", Password = "abcdefgh" };
            var model4 = new RegisterOwnerRequest { FullName = "Test", Email = "user@example.com", Password = "12345678" };
            var result1 = _validator.TestValidate(model1);
            var result2 = _validator.TestValidate(model2);
            var result3 = _validator.TestValidate(model3);
            var result4 = _validator.TestValidate(model4);
            result1.ShouldHaveValidationErrorFor(x => x.Password);
            result2.ShouldHaveValidationErrorFor(x => x.Password);
            result3.ShouldHaveValidationErrorFor(x => x.Password);
            result4.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Password_Is_Strong()
        {
            var model = new RegisterOwnerRequest { FullName = "Test", Email = "user@example.com", Password = "Password1" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Validate_Pet_Fields_When_Pet_Is_Provided()
        {
            var pet = new RegisterOwnerRequest.PetRequest
            {
                Name = "",
                Species = "",
                Breed = new string('b', 61),
                Dob = DateTime.UtcNow.AddDays(1)
            };
            var model = new RegisterOwnerRequest
            {
                FullName = "Test",
                Email = "user@example.com",
                Password = "Password1",
                Pet = pet
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Pet.Name");
            result.ShouldHaveValidationErrorFor("Pet.Species");
            result.ShouldHaveValidationErrorFor("Pet.Breed");
            result.ShouldHaveValidationErrorFor("Pet.Dob");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Pet_Fields_Are_Valid()
        {
            var pet = new RegisterOwnerRequest.PetRequest
            {
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Dob = DateTime.UtcNow.AddYears(-2)
            };
            var model = new RegisterOwnerRequest
            {
                FullName = "Test",
                Email = "user@example.com",
                Password = "Password1",
                Pet = pet
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor("Pet.Name");
            result.ShouldNotHaveValidationErrorFor("Pet.Species");
            result.ShouldNotHaveValidationErrorFor("Pet.Breed");
            result.ShouldNotHaveValidationErrorFor("Pet.Dob");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Pet_Is_Null()
        {
            var model = new RegisterOwnerRequest
            {
                FullName = "Test",
                Email = "user@example.com",
                Password = "Password1",
                Pet = null
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Pet);
        }
    }
}