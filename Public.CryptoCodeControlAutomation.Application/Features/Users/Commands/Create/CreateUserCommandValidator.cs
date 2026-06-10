using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.Users.Commands.Create
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(u => u.Username).NotEmpty().WithMessage("Lütfen kullanıcı ad alanını doldurunuz.")
                                     .MinimumLength(2).WithMessage("Ad alanı en az 2 karakter olabilir.");


            //RuleFor(u => u.PasswordHash).NotEmpty().WithMessage("Lütfen şifre alanını doldurunuz.")
            //                        .MinimumLength(3).WithMessage("Şifre alanı en az 3 karakter olabilir.");

            RuleFor(u => u.PasswordHash)
                .NotEmpty().When(u => !u.RequiresLdapAuthentication)
                .WithMessage("LDAP kullanılmayan kullanıcılar için şifre giriniz.");

            RuleFor(u => u.FullName).NotEmpty().WithMessage("Lütfen tam ad alanını doldurunuz.");
                                 //.EmailAddress().WithMessage("Lütfen uygun formatta mail giriniz.");

            RuleFor(u => u.UserRoles).NotEmpty().WithMessage("Lütfen yeni kullanıcı için rol seçiniz.");
        }
    }
}
