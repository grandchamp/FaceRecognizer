using FaceRecognizer.CrossCutting;
using FaceRecognizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer.Core.Services.Contracts
{
    public interface IFaceService
    {
        Task<Result<IEnumerable<Face>>> ExtractFaces(byte[] image);
    }
}
