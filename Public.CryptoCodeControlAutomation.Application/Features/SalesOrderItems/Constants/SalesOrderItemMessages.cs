namespace CryptoCodeControlAutomation.Application.Features.Users.Constants
{
    public static class SalesOrderItemMessages
    {
        public const string SalesOrderItemMessagesAlreadyExist = "Girdiğiniz satış siparişi sistemde kayıtlıdır.";
        public const string CanNotDeleteSalesOrderItemsWhenImporting = "Kod kayıt işlemi devam ederken bu işlemi yapamazsınız.";
        public const string CanNotDeleteSalesOrderItemsWhenItHasProducedCodes = "Üretimine başlanmış satış siparişini silemezsiniz.";
        public const string ActiveSalesOrderItemAlreadyExists = "Aktif bir sipariş varken yeni siparişi başlatamazsınız.";
        public const string SapPlannedUnitQtyExceedsImportedCodeCount = "Girilen planlanan miktar, yüklenen kod adedindeki %5 fire payını aşıyor.";
    }
}
