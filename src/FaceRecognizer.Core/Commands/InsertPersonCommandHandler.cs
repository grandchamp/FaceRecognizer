using FaceRecognizer.Core.Entities;
using FaceRecognizer.Core.Repositories.Contracts;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceRecognizer.Core.Commands
{
    public class InsertPersonCommandHandler : IAsyncNotificationHandler<InsertPersonCommand>
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<InsertPersonCommandHandler> _log;
        private readonly IDocumentSession _documentSession;
        public InsertPersonCommandHandler(IPersonRepository faceRepository, ILogger<InsertPersonCommandHandler> log, IDocumentSession documentSession)
        {
            _personRepository = faceRepository;
            _log = log;
            _documentSession = documentSession;
        }

        public async Task Handle(InsertPersonCommand notification)
        {
            try
            {
                var insertResult = await _personRepository.InsertAsync(notification.Person);

                await _documentSession.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
        }
    }
}
