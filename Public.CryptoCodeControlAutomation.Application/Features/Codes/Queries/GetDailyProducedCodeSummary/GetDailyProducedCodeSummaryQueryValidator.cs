using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetDailyProducedCodeSummary
{
    public class GetDailyProducedCodeSummaryQueryValidator : AbstractValidator<GetDailyProducedCodeSummaryQuery>
    {
        public GetDailyProducedCodeSummaryQueryValidator()
        {
            RuleFor(x => x)
                .Must(x => HasValidId(x.SalesOrderItemId) || HasValidId(x.PlannedOrderId))
                .WithMessage("Satis siparisi veya planli siparisten en az biri secilmelidir.");
        }

        private static bool HasValidId(long? id)
        {
            return id.HasValue && id.Value > 0;
        }
    }
}
