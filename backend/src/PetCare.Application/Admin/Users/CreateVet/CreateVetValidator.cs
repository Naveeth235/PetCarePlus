using FluentValidation;

namespace PetCare.Application.Admin.Users.CreateVet;

public sealed class CreateVetValidator : AbstractValidator<CreateVetRequest>
{
    public CreateVetValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        // Password ≥ 8 chars, must contain at least one letter & one number
        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$")
            .WithMessage("Password must be at least 8 characters and contain letters and numbers.");
    }
}
