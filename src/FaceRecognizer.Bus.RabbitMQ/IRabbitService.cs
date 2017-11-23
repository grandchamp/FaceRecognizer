using FaceRecognizer.CrossCutting;
using System.Threading.Tasks;

namespace FaceRecognizer.Bus.RabbitMQ
{
    public interface IRabbitService
    {
        Task<Result> SendCommand<T>(string queueName, T command);
        Task<TResponse> SendCommandRPC<TCommand, TResponse>(string queueName, TCommand command);
    }
}
