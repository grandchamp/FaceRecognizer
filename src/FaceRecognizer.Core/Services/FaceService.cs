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
using FaceRecognizer.Core.Repositories.Contracts;
using Emgu.CV.Face;
using Emgu.CV.CvEnum;

namespace FaceRecognizer.Core.Services
{
    public class FaceService : IFaceService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptionsSnapshot<EmguCVConfiguration> _emguCvConfiguration;
        private readonly ILogger<FaceService> _log;
        private readonly CascadeClassifier _cascadeClassifier;
        private readonly IPersonRepository _faceRepository;
        private readonly Emgu.CV.Face.FaceRecognizer _faceRecognizer;
        public FaceService(IHostingEnvironment hostingEnvironment, IOptionsSnapshot<EmguCVConfiguration> emguCvConfiguration, ILogger<FaceService> log, IPersonRepository faceRepository)
        {
            _hostingEnvironment = hostingEnvironment;
            _emguCvConfiguration = emguCvConfiguration;
            _log = log;
            _faceRepository = faceRepository;
            _cascadeClassifier = new CascadeClassifier(Path.Combine(_hostingEnvironment.ContentRootPath ?? _hostingEnvironment.WebRootPath,
                                                       _emguCvConfiguration.Value.FaceRecognitionBaseDataFile));
            _faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
        }

        public Task<Result<IEnumerable<Face>>> ExtractFaces(byte[] image)
        {
            try
            {
                using (var img = new Mat())
                {
                    CvInvoke.Imdecode(image, ImreadModes.AnyColor, img);

                    var facesList = new List<Face>();

                    using (var imageFrame = img.ToImage<Bgr, byte>())
                    {
                        if (imageFrame != null)
                        {
                            using (var grayframe = imageFrame.Convert<Gray, byte>())
                            {
                                var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty);
                                foreach (var face in faces)
                                {
                                    facesList.Add(new Face
                                    {
                                        Date = DateTime.Now,
                                        FaceCoordinates = face,
                                        FaceImage = imageFrame.GetSubRect(face).ToJpegData(),
                                        FullImage = imageFrame.ToJpegData()
                                    });
                                }
                            }
                        }

                        if (facesList.Any())
                            return Task.FromResult(Result.Ok(facesList.AsEnumerable()));
                        else
                            return Task.FromResult(Result.Fail<IEnumerable<Face>>("No faces found"));
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Task.FromResult(Result.Fail<IEnumerable<Face>>($"There was an error trying to extract faces. {ex.Message}"));
            }
        }

        public async Task<Result<Person>> MatchFace(Face candidateFace)
        {
            try
            {
                _faceRecognizer.Read(_emguCvConfiguration.Value.TrainedRecognizerFile);

                using (var img = new Mat())
                {
                    CvInvoke.Imdecode(candidateFace.FaceImage, ImreadModes.AnyColor, img);
                    using (var imageFrame = img.ToImage<Bgr, byte>())
                    using (var grayFrame = imageFrame.Convert<Gray, byte>())
                    {
                        var result = _faceRecognizer.Predict(grayFrame.Resize(300, 300, Inter.Cubic));
                        
                        if (result.Label <= 0 || result.Distance >= 105)
                        {
                            return Result.Fail<Person>("Face not recognized.");
                        }

                        var person = await _faceRepository.FindPersonByIdAsync(result.Label);

                        return person;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Result.Fail<Person>($"There was an error trying to match face. {ex.Message}");
            }
        }

        public async Task TrainFaceRecognizer()
        {
            try
            {
                var allPerson = await _faceRepository.GetAllAsync();

                var faceImages = new List<Image<Gray, byte>>();
                var faceLabels = new List<int>();

                foreach (var person in allPerson)
                {
                    foreach (var face in person.Faces)
                    {
                        using (var img = new Mat())
                        {
                            CvInvoke.Imdecode(face.FaceImage, ImreadModes.AnyColor, img);

                            using (var imageFrame = img.ToImage<Bgr, byte>())
                            using (var grayFrame = imageFrame.Convert<Gray, byte>())
                            {
                                faceImages.Add(grayFrame.Resize(300, 300, Inter.Cubic));
                                faceLabels.Add(person.Id);
                            }
                        }
                    }
                }

                if (faceImages.Any())
                {
                    _faceRecognizer.Train(faceImages.ToArray(), faceLabels.ToArray());
                    _faceRecognizer.Write(_emguCvConfiguration.Value.TrainedRecognizerFile);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
        }
    }
}
