using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Update
{
    public class UpdateSalesOrderItemCommandValidator : AbstractValidator<UpdateSalesOrderItemCommand>
    {
        public UpdateSalesOrderItemCommandValidator()
        {
            RuleFor(x => x.SalesOrderItemId)
                .GreaterThan(0).WithMessage("Geçerli bir SalesOrderItemId giriniz.");

            RuleFor(x => x.SalesOrderNo)
                .NotEmpty().WithMessage("Lütfen Sales Order No alanını doldurunuz.")
                .MinimumLength(3).WithMessage("Sales Order No en az 3 karakter olabilir.");

            RuleFor(x => x.SalesItemNo)
                .NotEmpty().WithMessage("Lütfen Sales Item No alanını doldurunuz.");

            RuleFor(x => x.MaterialNo)
                .NotEmpty().WithMessage("Lütfen Material No alanını doldurunuz.");

            RuleFor(x => x.SapPlannedUnitQty)
                .GreaterThanOrEqualTo(0).WithMessage("Planned Unit Qty negatif olamaz.");

            RuleFor(x => x.SapCaseQty)
                .GreaterThanOrEqualTo(0).When(x => x.SapCaseQty.HasValue).WithMessage("Case Qty negatif olamaz.");

            RuleFor(x => x.SapValidatedAt)
                .NotNull().WithMessage("Sap Validated At alanı boş olamaz.");
        }
    }
}