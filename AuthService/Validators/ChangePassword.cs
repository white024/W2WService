using FluentValidation;

namespace AuthService.Validators;

public class ChangePassword : AbstractValidator<UserChangePasswordDto>
{
    public ChangePassword()
    {


        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalı");

        RuleFor(x => x.NewPassword)
         .NotEmpty()
         .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalı")
         .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre eski şifre ile aynı olamaz");

    }
}
