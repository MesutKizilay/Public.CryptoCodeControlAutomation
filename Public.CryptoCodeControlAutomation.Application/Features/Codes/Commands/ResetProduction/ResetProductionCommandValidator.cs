using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.ResetProduction
{
    public class ResetProductionCommandValidator : AbstractValidator<ResetProductionCommand>
    {
        public ResetProductionCommandValidator()
        {
            RuleFor(x => x)
                .Must(HasSelection)
                .WithMessage("Satis siparisi veya planli siparis seciniz.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Sifre zorunludur.");
        }

        private static bool HasSelection(ResetProductionCommand command)
        {
            return command.SalesOrderItemId.HasValue && command.SalesOrderItemId.Value > 0
                || command.PlannedOrderId.HasValue && command.PlannedOrderId.Value > 0;
        }
    }
}
