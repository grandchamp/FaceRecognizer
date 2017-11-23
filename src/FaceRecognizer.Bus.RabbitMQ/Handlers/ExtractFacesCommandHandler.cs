using FaceRecognizer.CrossCutting;
using FaceRecognizer.Core.Commands;
using FaceRecognizer.Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer.Bus.RabbitMQ.Handlers
{
    public class ExtractFacesCommandHandler : IAsyncRequestHandler<ExtractFacesCommand, Result<IEnumerable<Face>>>
    {
        private readonly IRabbitService _rabbitService;
        private readonly ILogger<ExtractFacesCommandHandler> _log;
        private readonly IOptionsSnapshot<RabbitConfiguration> _rabbitConfiguration;
        public ExtractFacesCommandHandler(IRabbitService rabbitService, ILogger<ExtractFacesCommandHandler> log, IOptionsSnapshot<RabbitConfiguration> rabbitConfiguration)
        {
            _rabbitService = rabbitService;
            _log = log;
            _rabbitConfiguration = rabbitConfiguration;
        }

        public async Task<Result<IEnumerable<Face>>> Handle(ExtractFacesCommand message)
        {
            try
            {
                var faceExtractionResult = await _rabbitService.SendCommandRPC<byte[], Envelope<IEnumerable<Face>>>(_rabbitConfiguration.Value.ExtractFacesQueueName, message.Image);

                if (faceExtractionResult.IsSuccess)
                    return Result.Ok(faceExtractionResult.Value);
                else
                    return Result.Fail<IEnumerable<Face>>(faceExtractionResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Result.Fail<IEnumerable<Face>>($"There was an error trying to enroll the image. {ex.Message}");
            }
        }
    }
}
