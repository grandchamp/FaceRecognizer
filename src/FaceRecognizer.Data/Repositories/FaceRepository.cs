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
    public class FaceRepository : IFaceRepository
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger<FaceRepository> _log;
        private readonly IDocumentSession _documentSession;
        public FaceRepository(IDocumentStore documentStore, ILogger<FaceRepository> log, IDocumentSession documentSession)
        {
            _documentStore = documentStore;
            _log = log;
            _documentSession = documentSession;
        }

        public async Task<IEnumerable<Face>> GetAllAsync()
        {
            try
            {
                var result = await _documentSession.Query<Face>()
                                                   .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Enumerable.Empty<Face>();
            }
        }

        public Task<Result<bool>> InsertAsync(Face face)
        {
            try
            {
                _documentSession.Store(face);

                return Task.FromResult(Result.Ok(true));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);

                return Task.FromResult(Result.Fail<bool>($"Can't insert face. {ex.Message}"));
            }
        }
    }
}
