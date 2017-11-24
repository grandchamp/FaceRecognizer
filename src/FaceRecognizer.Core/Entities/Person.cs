using System.Collections.Generic;

namespace FaceRecognizer.Core.Entities
{
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Face> Faces { get; set; }
    }
}
