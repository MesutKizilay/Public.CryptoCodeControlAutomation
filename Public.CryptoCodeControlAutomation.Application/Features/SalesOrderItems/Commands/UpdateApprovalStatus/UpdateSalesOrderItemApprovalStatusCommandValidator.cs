using FluentValidation;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.UpdateApprovalStatus
{
    public class UpdateSalesOrderItemApprovalStatusCommandValidator : AbstractValidator<UpdateSalesOrderItemApprovalStatusCommand>
    {
        public UpdateSalesOrderItemApprovalStatusCommandValidator()
        {
            RuleFor(x => x.SalesOrderItemId)
                .GreaterThan(0).WithMessage("Geçerli bir SalesOrderItemId giriniz.");

            RuleFor(x => x.ApprovalStatus)
                .IsInEnum().WithMessage("Geçerli bir onay durumu seçiniz.");
        }
    }
}
