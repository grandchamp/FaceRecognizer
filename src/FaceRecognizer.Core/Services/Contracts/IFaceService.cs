using FaceRecognizer.Core.Entities;
using FaceRecognizer.CrossCutting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceRecognizer.Core.Services.Contracts
{
    public interface IFaceService
    {
        Task<Result<IEnumerable<Face>>> ExtractFaces(byte[] image);
        Task<Result<Person>> MatchFace(Face candidateFace);
        Task TrainFaceRecognizer();
    }
}
