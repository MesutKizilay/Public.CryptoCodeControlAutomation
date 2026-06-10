using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Create
{
    public class CreateSalesOrderItemCommandValidator : AbstractValidator<CreateSalesOrderItemCommand>
    {
        public CreateSalesOrderItemCommandValidator()
        {
            RuleFor(x => x.SalesOrderNo)
                .NotEmpty().WithMessage("Lütfen Sales Order No alanını doldurunuz.");

            RuleFor(x => x.SalesItemNo)
                .NotEmpty().WithMessage("Lütfen Sales Item No alanını doldurunuz.");

            RuleFor(x => x.MaterialNo)
                .NotEmpty().WithMessage("Lütfen Material No alanını doldurunuz.");

            RuleFor(x => x.SapCaseQty)
                .NotNull().WithMessage("Case Qty alanı boş olamaz.")
                .GreaterThan(0).WithMessage("Case Qty 0'dan büyük olmalıdır.");

            //RuleFor(x => x.SapValidatedAt)
            //    .NotNull().WithMessage("Sap Validated At alanı boş olamaz.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("Lütfen dosya yükleyiniz.");
                //.Must(file => file != null && file.Length > 0).WithMessage("Dosya boÅŸ olamaz.");
        }
    }
}
