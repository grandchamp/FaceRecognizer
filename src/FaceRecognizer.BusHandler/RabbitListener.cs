using FaceRecognizer.CrossCutting;
using FaceRecognizer.Bus.RabbitMQ;
using FaceRecognizer.Core.Entities;
using FaceRecognizer.Core.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer.BusHandler
{
    public class RabbitListener : IDisposable
    {
        private readonly ILogger<RabbitListener> _log;
        private readonly RabbitConnection _rabbitConnection;
        private readonly IOptions<RabbitConfiguration> _rabbitConfiguration;
        private readonly IFaceService _faceService;
        private IModel _channel;
        public RabbitListener(ILogger<RabbitListener> log, RabbitConnection rabbitConnection, IOptions<RabbitConfiguration> rabbitConfiguration, IFaceService faceService)
        {
            _log = log;
            _rabbitConnection = rabbitConnection;
            _rabbitConfiguration = rabbitConfiguration;
            _faceService = faceService;
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
            }
        }

        public void StartExtractFaces()
        {
            try
            {
                if (_channel == null)
                    CreateChannel(_rabbitConfiguration.Value.ExtractFacesQueueName);

                var consumer = new EventingBasicConsumer(_channel);
                _channel.BasicConsume(queue: _rabbitConfiguration.Value.ExtractFacesQueueName, autoAck: false, consumer: consumer);

                consumer.Received += async (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var image = JsonConvert.DeserializeObject<byte[]>(Encoding.UTF8.GetString(body));

                        var result = await _faceService.ExtractFaces(image);

                        var envelope = new Envelope<IEnumerable<Face>>();
                        result.Map(x => envelope.Value = x)
                              .OnSuccess(x => envelope.IsSuccess = true)
                              .OnFailure(error =>
                              {
                                  envelope.IsSuccess = false;
                                  envelope.ErrorMessage = error;
                              });

                        response = JsonConvert.SerializeObject(envelope);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, ex.Message);
                        response = JsonConvert.SerializeObject(Result.Fail<IEnumerable<Face>>($"There was an error on ExtractFaces job. {ex.Message}"));
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        _channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                              basicProperties: replyProps, body: responseBytes);

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                          multiple: false);
                    }
                };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
        }

        private void CreateChannel(string queueName)
        {
            _channel = _rabbitConnection.GetConnection().CreateModel();
            _channel.QueueDeclare(queueName, true, false, false, null);
        }
    }
}
