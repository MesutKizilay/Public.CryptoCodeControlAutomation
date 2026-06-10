using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.Users.Commands.Update
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(u => u.Username).NotEmpty().WithMessage("Lütfen kullanıcı ad alanını doldurunuz.")
                                     .MinimumLength(2).WithMessage("Ad alanı en az 2 karakter olabilir.");


            RuleFor(u => u.PasswordHash)
                .NotEmpty().When(u => !u.RequiresLdapAuthentication)
                .WithMessage("LDAP kullanılmayan kullanıcılar için şifre giriniz.");

            RuleFor(u => u.FullName).NotEmpty().WithMessage("Lütfen tam ad alanını doldurunuz.");
                                 //.EmailAddress().WithMessage("Lütfen uygun formatta mail giriniz.");

            RuleFor(u => u.UserRoles).NotEmpty().WithMessage("Lütfen kullanıcı için rol seçiniz.");
        }
    }
}