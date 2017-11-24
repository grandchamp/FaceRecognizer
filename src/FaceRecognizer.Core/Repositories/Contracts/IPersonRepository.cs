using FaceRecognizer.Core.Entities;
using FaceRecognizer.CrossCutting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceRecognizer.Core.Repositories.Contracts
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllAsync();

        Task<Result<bool>> InsertAsync(Person person);

        Task<Result<Person>> FindPersonByIdAsync(int id);
    }
}
