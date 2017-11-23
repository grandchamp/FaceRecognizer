using FaceRecognizer.API.Models;
using FaceRecognizer.Core.Commands;
using FaceRecognizer.CrossCutting;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace FaceRecognizer.API.Controllers
{
    [Route("api/v1/recognition")]
    public class RecognitionController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RecognitionController> _log;
        public RecognitionController(IMediator mediator, ILogger<RecognitionController> log)
        {
            _mediator = mediator;
            _log = log;
        }

        [HttpPost("faces/extract")]
        public async Task<IActionResult> ExtractFaces([FromBody]ImageRequest model)
        {
            try
            {
                var extractedFaces = await _mediator.Send(new ExtractFacesCommand { Image = model.Image });

                IActionResult response = NoContent();

                extractedFaces.OnSuccess(x =>
                {
                    if (x.Any())
                        response = Json(x);
                });

                return response;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody]ImageRequest model)
        {
            try
            {
                var extractedFaces = await _mediator.Send(new ExtractFacesCommand { Image = model.Image });

                IActionResult response = NoContent();

                extractedFaces.OnSuccess(x =>
                {
                    if (x.Any())
                        response = Json(x);
                });

                return response;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
