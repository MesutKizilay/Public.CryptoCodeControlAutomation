using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(l => l.UserForLoginDto.UserName).NotEmpty().WithMessage("Lütfen kullanıcı adı alanını doldurunuz.");
            RuleFor(l => l.UserForLoginDto.PasswordHash).NotEmpty().WithMessage("Lütfen şifre alanını doldurunuz.");
        }
    }
}