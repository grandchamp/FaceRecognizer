using FaceRecognizer.CrossCutting;
using FaceRecognizer.Core.Entities;
using MediatR;
using System.Collections;
using System.Collections.Generic;

namespace FaceRecognizer.Core.Commands
{
    public class ExtractFacesCommand : IRequest<Result<IEnumerable<Face>>>
    {
        public byte[] Image { get; set; }
    }
}
