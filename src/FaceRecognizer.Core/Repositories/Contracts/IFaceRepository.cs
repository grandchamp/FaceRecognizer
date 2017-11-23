using FaceRecognizer.CrossCutting;
using FaceRecognizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognizer.Core.Repositories.Contracts
{
    public interface IFaceRepository
    {
        Task<IEnumerable<Face>> GetAllAsync();

        Task<Result<bool>> InsertAsync(Face face);
    }
}
