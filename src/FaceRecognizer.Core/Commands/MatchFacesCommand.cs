using FaceRecognizer.Core.Entities;
using MediatR;
using System.Collections.Generic;

namespace FaceRecognizer.Core.Commands
{
    public class MatchFacesCommand : IRequest<IEnumerable<Person>>
    {
        public IEnumerable<Face> ExtractedFaces { get; set; }
    }
}
