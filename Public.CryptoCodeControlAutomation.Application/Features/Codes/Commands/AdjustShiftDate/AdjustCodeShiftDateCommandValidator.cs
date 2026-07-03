using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustShiftDate
{
    public class AdjustCodeShiftDateCommandValidator : AbstractValidator<AdjustCodeShiftDateCommand>
    {
        public AdjustCodeShiftDateCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => HasValidId(x.SalesOrderItemId) || HasValidId(x.PlannedOrderId))
                .WithMessage("Satis siparisi veya planli siparisten en az biri secilmelidir.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Adet sifirdan buyuk olmalidir.");

            RuleFor(x => x.FromShiftDate)
                .NotNull()
                .WithMessage("Mevcut vardiya tarihi zorunludur.");

            RuleFor(x => x.ToShiftDate)
                .NotNull()
                .WithMessage("Yeni vardiya tarihi zorunludur.");

            RuleFor(x => x)
                .Must(HaveDifferentShiftDates)
                .WithMessage("Eski ve yeni vardiya tarihleri farkli olmalidir.");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Aciklama zorunludur.")
                .MaximumLength(500)
                .WithMessage("Aciklama en fazla 500 karakter olabilir.");
        }

        private static bool HasValidId(long? id)
        {
            return id.HasValue && id.Value > 0;
        }

        private static bool HaveDifferentShiftDates(AdjustCodeShiftDateCommand command)
        {
            return !command.FromShiftDate.HasValue
                || !command.ToShiftDate.HasValue
                || command.FromShiftDate.Value.Date != command.ToShiftDate.Value.Date;
        }
    }
}
