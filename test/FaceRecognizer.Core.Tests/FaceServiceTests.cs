using Castle.Core.Logging;
using FaceRecognizer.Core.Configuration;
using FaceRecognizer.Core.Entities;
using FaceRecognizer.Core.Repositories.Contracts;
using FaceRecognizer.Core.Services;
using FaceRecognizer.Core.Services.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace FaceRecognizer.Core.Tests
{
    public class FaceServiceTests
    {
        private readonly IFaceService _faceService;
        private readonly string _mainPath;
        public FaceServiceTests()
        {
            var hostingEnvironment = Substitute.For<IHostingEnvironment>();
            hostingEnvironment.ContentRootPath
                              .Returns(@"C:\Users\nicolas.grandchamp\Source\Repos\FaceRecognizer2\src\FaceRecognizer.BusHandler");

            var options = Substitute.For<IOptionsSnapshot<EmguCVConfiguration>>();
            options.Value
                   .Returns(new EmguCVConfiguration
                   {
                       FaceRecognitionBaseDataFile = "haarcascade_frontalface_default.xml",
                       TrainedRecognizerFile = "trained.yml"
                   });

            var logger = Substitute.For<ILogger<FaceService>>();

            _mainPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

            var personRepository = Substitute.For<IPersonRepository>();
            personRepository.GetAllAsync()
                            .Returns(JsonConvert.DeserializeObject<List<Person>>(File.ReadAllText(Path.Combine(_mainPath, "people.json"))));

            _faceService = new FaceService(hostingEnvironment, options, logger, personRepository);

            _faceService.TrainFaceRecognizer();
        }

        [Fact]
        public async Task CanMatchFace()
        {
            var face = JsonConvert.DeserializeObject<Face>(File.ReadAllText(Path.Combine(_mainPath, "face.json")));
            var matchResult = await _faceService.MatchFace(face);
        }
    }
}
