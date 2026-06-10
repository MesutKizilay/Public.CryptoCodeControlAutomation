namespace CryptoCodeControlAutomation.Domain.Enums
{
    public enum UploadJobStatus : byte
    {
        New = 0,
        Importing = 1,
        Done = 2,
        Failed = 3,
        Deleted = 4
    }
}
