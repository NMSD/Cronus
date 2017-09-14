namespace Elders.Cronus.Pipeline
{
    public interface IConsumer
    {

    }

    interface IInMemoryConsumer : IConsumer
    {

    }

    public interface IEndpointConsumer : IConsumer
    {
        int NumberOfWorkers { get; }

        string Name { get; }

        void Start(int? numberOfWorkers = null);

        void Stop();
    }
}
