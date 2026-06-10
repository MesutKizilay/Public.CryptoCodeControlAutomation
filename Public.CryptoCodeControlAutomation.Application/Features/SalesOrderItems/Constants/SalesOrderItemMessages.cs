namespace CryptoCodeControlAutomation.Application.Features.Users.Constants
{
    public static class SalesOrderItemMessages
    {
        public const string SalesOrderItemMessagesAlreadyExist = "Girdiğiniz satış siparişi sistemde kayıtlıdır.";
        public const string CanNotDeleteSalesOrderItemsWhenImporting = "Kod kayıt işlemi devam ederken siparişi silemezsiniz.";
        public const string CanNotDeleteSalesOrderItemsWhenItHasProducedCodes = "Üretimine başlanmış satış siparişini silemezsiniz";
    }
}