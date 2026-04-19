using AuthService.DTOs;
using FluentValidation;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalı");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Geçerli bir email giriniz");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalı");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{10,13}$").WithMessage("Geçerli bir telefon numarası giriniz");
    }
}