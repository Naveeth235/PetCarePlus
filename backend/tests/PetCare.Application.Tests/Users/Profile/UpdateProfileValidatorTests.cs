using FluentValidation.TestHelper;
using PetCare.Application.Users.Profile;
using Xunit;

namespace PetCare.Application.Users.Profile.Tests
{
    public class UpdateProfileValidatorTests
    {
        private readonly UpdateProfileValidator _validator = new UpdateProfileValidator();

        [Fact]
        public void Should_Have_Error_When_FullName_Is_Empty()
        {
            var req = new UpdateProfileRequest { FullName = "", PhoneNumber = "1234567890" };
            var result = _validator.TestValidate(req);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Should_Have_Error_When_FullName_Too_Long()
        {
            var req = new UpdateProfileRequest { FullName = new string('a', 129), PhoneNumber = "1234567890" };
            var result = _validator.TestValidate(req);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Should_Not_Have_Error_When_FullName_Is_Valid()
        {
            var req = new UpdateProfileRequest { FullName = "Jane Doe", PhoneNumber = "1234567890" };
            var result = _validator.TestValidate(req);
            result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Should_Not_Have_Error_When_PhoneNumber_Is_Null_Or_Empty()
        {
            var req1 = new UpdateProfileRequest { FullName = "Jane Doe", PhoneNumber = null };
            var req2 = new UpdateProfileRequest { FullName = "Jane Doe", PhoneNumber = "" };
            var result1 = _validator.TestValidate(req1);
            var result2 = _validator.TestValidate(req2);
            result1.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
            result2.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        }

        [Fact]
        public void Should_Have_Error_When_PhoneNumber_Too_Long()
        {
            var req = new UpdateProfileRequest { FullName = "Jane Doe", PhoneNumber = new string('1', 26) };
            var result = _validator.TestValidate(req);
            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        }

        [Fact]
        public void Should_Have_Error_When_PhoneNumber_Has_Invalid_Characters()
        {
            var req = new UpdateProfileRequest { FullName = "Jane Doe", PhoneNumber = "123-456-7890x" };
            var result = _validator.TestValidate(req);
            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        }

        [Fact]
        public void Should_Not_Have_Error_When_PhoneNumber_Is_Valid()
        {
            var req = new UpdateProfileRequest { FullName = "Jane Doe", PhoneNumber = "+1 234-567-890" };
            var result = _validator.TestValidate(req);
            result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        }
    }
}