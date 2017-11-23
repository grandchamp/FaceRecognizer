using FaceRecognizer.CrossCutting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer.Bus.RabbitMQ
{
    public class RabbitService : IRabbitService, IDisposable
    {
        private readonly RabbitConnection _rabbitConnection;
        private readonly ILogger<RabbitService> _log;
        private IModel _channel;
        public RabbitService(RabbitConnection rabbitConnection, ILogger<RabbitService> log)
        {
            _rabbitConnection = rabbitConnection;
            _log = log;
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
            }
        }

        public Task<Result> SendCommand<T>(string queueName, T command)
        {
            try
            {
                if (_channel == null)
                    CreateChannel(queueName);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                var json = JsonConvert.SerializeObject(command);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish("", queueName, properties, body);

                return Task.FromResult(Result.Ok());
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Task.FromResult(Result.Fail($"There was an error trying to send the command to RabbitMQ bus. {ex.Message}"));
            }
        }

        public async Task<TResponse> SendCommandRPC<TCommand, TResponse>(string queueName, TCommand command)
        {
            try
            {
                if (_channel == null)
                    CreateChannel(queueName);

                var replyQueueName = _channel.QueueDeclare().QueueName;
                var consumer = new EventingBasicConsumer(_channel);
                var props = _channel.CreateBasicProperties();

                var correlationId = Guid.NewGuid().ToString();
                props.CorrelationId = correlationId;
                props.ReplyTo = replyQueueName;

                var tcs = new TaskCompletionSource<TResponse>();

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var response = Encoding.UTF8.GetString(body);
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var result = JsonConvert.DeserializeObject<TResponse>(response);

                        tcs.SetResult(result);
                    }
                };

                var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: messageBytes);

                _channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                throw;
            }
        }

        private void CreateChannel(string queueName)
        {
            _channel = _rabbitConnection.GetConnection().CreateModel();
            _channel.QueueDeclare(queueName, true, false, false, null);
        }
    }
}
