namespace CryptoCodeControlAutomation.Domain.Enums
{
    public enum CodeStatus : byte
    {
        Available = 0,
        Allocated = 1,
        ProducedOk = 2,
        Reject = 3,
        Scrap = 4,
        Void = 5
    }
}
