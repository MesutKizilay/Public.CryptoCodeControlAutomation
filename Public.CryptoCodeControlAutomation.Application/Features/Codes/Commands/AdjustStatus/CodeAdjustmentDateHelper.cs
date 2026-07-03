using Core.CrossCuttingConcerns.Exceptions.Types;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.AdjustStatus
{
    internal static class CodeAdjustmentDateHelper
    {
        public static DateTime CalculateExpirationDate(DateTime shiftDate, int shelfLifeValue, byte shelfLifeUnit)
        {
            return shelfLifeUnit switch
            {
                0 => shiftDate.Date.AddDays(shelfLifeValue),
                1 => shiftDate.Date.AddDays(shelfLifeValue * 7),
                2 => shiftDate.Date.AddMonths(shelfLifeValue),
                3 => shiftDate.Date.AddYears(shelfLifeValue),
                _ => throw new BusinessException("Gecersiz raf omru birimi.")
            };
        }
    }
}
