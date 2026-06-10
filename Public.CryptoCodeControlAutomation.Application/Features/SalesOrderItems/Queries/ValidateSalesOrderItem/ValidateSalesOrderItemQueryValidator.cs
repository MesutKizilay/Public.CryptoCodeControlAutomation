using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.ValidateSalesOrderItem
{
    public class ValidateSalesOrderItemQueryValidator : AbstractValidator<ValidateSalesOrderItemQuery>
    {
        public ValidateSalesOrderItemQueryValidator()
        {
            RuleFor(x => x.SalesOrderNo)
                .NotEmpty().WithMessage("Lütfen Sales Order No alanýný doldurunuz.");

            RuleFor(x => x.SalesItemNo)
                .NotEmpty().WithMessage("Lütfen Sales Item No alanýný doldurunuz.");
        }
    }
}