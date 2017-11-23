using System;

namespace FaceRecognizer.Core.Entities
{
    public class Person
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
    }
}
