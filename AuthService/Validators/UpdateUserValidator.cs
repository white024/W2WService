using FluentValidation;

namespace AuthService.Validators;
public class UpdateUserValidator : AbstractValidator<UserUpdateDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{10,13}$").WithMessage("Geçerli bir telefon numarası giriniz");

    }
}
