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
using Hangfire;

namespace FaceRecognizer.BusHandler
{
    public class RabbitListener : IDisposable
    {
        private readonly ILogger<RabbitListener> _log;
        private readonly RabbitConnection _rabbitConnection;
        private readonly IOptions<RabbitConfiguration> _rabbitConfiguration;
        private readonly IFaceService _faceService;
        private IModel _extractFacesChannel;
        private IModel _matchFaceChannel;
        public RabbitListener(ILogger<RabbitListener> log, RabbitConnection rabbitConnection, IOptions<RabbitConfiguration> rabbitConfiguration, IFaceService faceService)
        {
            _log = log;
            _rabbitConnection = rabbitConnection;
            _rabbitConfiguration = rabbitConfiguration;
            _faceService = faceService;
        }

        public void Dispose()
        {
            if (_extractFacesChannel != null)
            {
                _extractFacesChannel.Close();
                _extractFacesChannel.Dispose();
            }
        }

        public void StartExtractFaces()
        {
            try
            {
                if (_extractFacesChannel == null)
                    CreateChannel(_rabbitConfiguration.Value.ExtractFacesQueueName, ref _extractFacesChannel);

                var consumer = new EventingBasicConsumer(_extractFacesChannel);
                _extractFacesChannel.BasicConsume(queue: _rabbitConfiguration.Value.ExtractFacesQueueName, autoAck: false, consumer: consumer);

                consumer.Received += async (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = _extractFacesChannel.CreateBasicProperties();
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

                        _extractFacesChannel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                              basicProperties: replyProps, body: responseBytes);

                        _extractFacesChannel.BasicAck(deliveryTag: ea.DeliveryTag,
                                          multiple: false);
                    }
                };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
        }

        public void StartMatchFaces()
        {
            try
            {
                if (_matchFaceChannel == null)
                    CreateChannel(_rabbitConfiguration.Value.VerifyPersonByFaceQueueName, ref _matchFaceChannel);

                var consumer = new EventingBasicConsumer(_matchFaceChannel);
                _matchFaceChannel.BasicConsume(queue: _rabbitConfiguration.Value.VerifyPersonByFaceQueueName, autoAck: false, consumer: consumer);

                consumer.Received += async (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = _matchFaceChannel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var image = JsonConvert.DeserializeObject<Face>(Encoding.UTF8.GetString(body));

                        var result = await _faceService.MatchFace(image);

                        var envelope = new Envelope<Person>();
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
                        response = JsonConvert.SerializeObject(Result.Fail<IEnumerable<Face>>($"There was an error on MatchFaces job. {ex.Message}"));
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        _matchFaceChannel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                              basicProperties: replyProps, body: responseBytes);

                        _matchFaceChannel.BasicAck(deliveryTag: ea.DeliveryTag,
                                          multiple: false);
                    }
                };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
        }

        public async Task StartTrainFaceRecognition()
        {
            try
            {
                await _faceService.TrainFaceRecognizer();

                BackgroundJob.Enqueue(() => StartMatchFaces());
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
        }

        private void CreateChannel(string queueName, ref IModel channel)
        {
            channel = _rabbitConnection.GetConnection().CreateModel();
            channel.QueueDeclare(queueName, true, false, false, null);
        }
    }
}
