using FaceRecognizer.Core.Commands;
using FaceRecognizer.Core.Entities;
using FaceRecognizer.CrossCutting;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceRecognizer.Bus.RabbitMQ.Handlers
{
    public class MatchFacesCommandHandler : IAsyncRequestHandler<MatchFacesCommand, IEnumerable<Person>>
    {
        private readonly IRabbitService _rabbitService;
        private readonly ILogger<MatchFacesCommandHandler> _log;
        private readonly IOptionsSnapshot<RabbitConfiguration> _rabbitConfiguration;
        public MatchFacesCommandHandler(IRabbitService rabbitService, ILogger<MatchFacesCommandHandler> log, IOptionsSnapshot<RabbitConfiguration> rabbitConfiguration)
        {
            _rabbitService = rabbitService;
            _log = log;
            _rabbitConfiguration = rabbitConfiguration;
        }

        public async Task<IEnumerable<Person>> Handle(MatchFacesCommand message)
        {
            var personList = new ConcurrentBag<Person>();

            try
            {
                var taskList = new List<Task>();

                Parallel.ForEach(message.ExtractedFaces, face =>
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        var faceMatchResult = await _rabbitService.SendCommandRPC<Face, Envelope<Person>>(_rabbitConfiguration.Value.VerifyPersonByFaceQueueName, face);

                        if (faceMatchResult.IsSuccess)
                        {
                            faceMatchResult.Value.Faces.Add(face);

                            personList.Add(faceMatchResult.Value);
                        }
                        else
                        {
                            personList.Add(new Person
                            {
                                Id = 0,
                                Name = "Unknown",
                                Faces = new List<Face> { face }
                            });
                        }
                    }));
                });

                await Task.WhenAll(taskList.ToArray());
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }

            return personList;
        }
    }
}
