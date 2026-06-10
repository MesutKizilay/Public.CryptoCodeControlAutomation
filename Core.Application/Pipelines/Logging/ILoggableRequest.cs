namespace Core.Application.Pipelines.Logging
{
    public interface ILoggableRequest
    {
        public string LogMessage { get; }
    }
}