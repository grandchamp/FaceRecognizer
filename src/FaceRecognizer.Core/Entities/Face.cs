using System;
using System.Drawing;

namespace FaceRecognizer.Core.Entities
{
    public class Face
    {
        public byte[] FullImage { get; set; }
        public byte[] FaceImage { get; set; }

        public Rectangle FaceCoordinates { get; set; }

        public Person Person { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
