using PetCare.Application.Auth.Login;
using Xunit;
using FluentValidation.TestHelper;

namespace PetCare.Application.Auth.Login.Tests
{
    public class LoginValidatorTests
    {
        private readonly LoginValidator _validator = new LoginValidator();

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            var model = new LoginRequest { Email = "", Password = "password123" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var model = new LoginRequest { Email = "invalid-email", Password = "password123" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            var model = new LoginRequest { Email = "user@example.com", Password = "" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new LoginRequest { Email = "user@example.com", Password = "password123" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }
    }
}