namespace FaceRecognizer.CrossCutting
{
    public class Envelope<T>
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public T Value { get; set; }
    }
}
