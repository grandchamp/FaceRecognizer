using FaceRecognizer.CrossCutting;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognizer.Core.Configuration;
using FaceRecognizer.Core.Entities;
using FaceRecognizer.Core.Services.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecognizer.Core.Services
{
    public class FaceService : IFaceService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptionsSnapshot<EmguCVConfiguration> _emguCvConfiguration;
        private readonly ILogger<FaceService> _log;
        private readonly CascadeClassifier _cascadeClassifier;
        public FaceService(IHostingEnvironment hostingEnvironment, IOptionsSnapshot<EmguCVConfiguration> emguCvConfiguration, ILogger<FaceService> log)
        {
            _hostingEnvironment = hostingEnvironment;
            _emguCvConfiguration = emguCvConfiguration;
            _log = log;
            _cascadeClassifier = new CascadeClassifier(Path.Combine(_hostingEnvironment.ContentRootPath ?? _hostingEnvironment.WebRootPath,
                                                     _emguCvConfiguration.Value.FaceRecognitionBaseDataFile));
        }

        public async Task<Result<IEnumerable<Face>>> ExtractFaces(byte[] image)
        {
            try
            {
                Mat img = new Mat();
                CvInvoke.Imdecode(image, Emgu.CV.CvEnum.ImreadModes.AnyColor, img);

                var facesList = new List<Face>();

                using (var imageFrame = img.ToImage<Bgr, byte>())
                {
                    if (imageFrame != null)
                    {
                        var grayframe = imageFrame.Convert<Gray, byte>();
                        var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty);
                        foreach (var face in faces)
                        {
                            facesList.Add(new Face
                            {
                                Date = DateTime.Now,
                                FaceCoordinates = face,
                                FaceImage = imageFrame.GetSubRect(face).ToJpegData(),
                                FullImage = imageFrame.ToJpegData(),
                                Person = null
                            });
                        }
                    }

                    if (facesList.Any())
                        return Result.Ok(facesList.AsEnumerable());
                    else
                        return Result.Fail<IEnumerable<Face>>("No faces found");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Result.Fail<IEnumerable<Face>>($"There was an error trying to extract faces. {ex.Message}");
            }
        }
    }
}
