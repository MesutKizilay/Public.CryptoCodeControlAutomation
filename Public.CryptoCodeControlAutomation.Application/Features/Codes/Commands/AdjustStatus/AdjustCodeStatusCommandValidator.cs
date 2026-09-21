using CryptoCodeControlAutomation.Domain.Enums;
using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus
{
    public class AdjustCodeStatusCommandValidator : AbstractValidator<AdjustCodeStatusCommand>
    {
        public AdjustCodeStatusCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => HasValidId(x.SalesOrderItemId) || HasValidId(x.PlannedOrderId))
                .WithMessage("Satis siparisi veya planli siparisten en az biri secilmelidir.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Adet sifirdan buyuk olmalidir.");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Aciklama zorunludur.")
                .MaximumLength(500)
                .WithMessage("Aciklama en fazla 500 karakter olabilir.");

            RuleFor(x => x.FromStatus)
                .IsInEnum()
                .WithMessage("Gecerli bir kaynak status secilmelidir.");

            RuleFor(x => x.ToStatus)
                .IsInEnum()
                .WithMessage("Gecerli bir hedef status secilmelidir.");

            RuleFor(x => x)
                .Must(IsAllowedTransition)
                .WithMessage("Gecerli bir durum gecisi secilmelidir.");

            RuleFor(x => x.ShiftDate)
                .NotNull()
                .When(RequiresShiftDate)
                .WithMessage("Kod durumu duzeltilirken uretim tarihi zorunludur.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .When(IsAvailableTransition)
                .WithMessage("Sifre zorunludur.");
        }

        private static bool HasValidId(long? id)
        {
            return id.HasValue && id.Value > 0;
        }

        private static bool IsAllowedTransition(AdjustCodeStatusCommand command)
        {
            return IsAvailableTransition(command)
                || command.FromStatus == CodeStatus.Allocated && command.ToStatus == CodeStatus.ProducedOk
                || command.FromStatus == CodeStatus.ProducedOk && command.ToStatus == CodeStatus.Allocated;
        }

        private static bool IsAvailableTransition(AdjustCodeStatusCommand command)
        {
            return command.FromStatus == CodeStatus.Available && command.ToStatus == CodeStatus.Allocated
                || command.FromStatus == CodeStatus.Allocated && command.ToStatus == CodeStatus.Available;
        }

        private static bool RequiresShiftDate(AdjustCodeStatusCommand command)
        {
            return command.FromStatus == CodeStatus.Allocated && command.ToStatus == CodeStatus.ProducedOk
                || command.FromStatus == CodeStatus.ProducedOk && command.ToStatus == CodeStatus.Allocated;
        }
    }
}
