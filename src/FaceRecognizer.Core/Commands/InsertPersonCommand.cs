using FaceRecognizer.Core.Entities;
using MediatR;

namespace FaceRecognizer.Core.Commands
{
    public class InsertPersonCommand : INotification
    {
        public Person Person { get; set; }
    }
}
