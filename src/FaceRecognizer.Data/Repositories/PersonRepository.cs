using FaceRecognizer.CrossCutting;
using FaceRecognizer.Core.Entities;
using FaceRecognizer.Core.Repositories.Contracts;
using Marten;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecognizer.Data.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger<PersonRepository> _log;
        private readonly IDocumentSession _documentSession;
        public PersonRepository(IDocumentStore documentStore, ILogger<PersonRepository> log, IDocumentSession documentSession)
        {
            _documentStore = documentStore;
            _log = log;
            _documentSession = documentSession;
        }

        public async Task<Result<Person>> FindPersonByIdAsync(int id)
        {
            try
            {
                var result = await _documentSession.Query<Person>()
                                                   .FirstOrDefaultAsync(x => x.Id == id);

                if (result != null)
                    return Result.Ok(result);
                else
                    return Result.Fail<Person>("Person Id not found.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Result.Fail<Person>($"There was an error finding the person. {ex.Message}");
            }
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            try
            {
                var result = await _documentSession.Query<Person>()
                                                   .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Enumerable.Empty<Person>();
            }
        }

        public Task<Result<bool>> InsertAsync(Person person)
        {
            try
            {
                _documentSession.Store(person);

                return Task.FromResult(Result.Ok(true));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Task.FromResult(Result.Fail<bool>($"Can't insert person. {ex.Message}"));
            }
        }
    }
}
