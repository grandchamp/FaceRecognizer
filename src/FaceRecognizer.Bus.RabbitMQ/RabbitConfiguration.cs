namespace FaceRecognizer.Bus.RabbitMQ
{
    public class RabbitConfiguration
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }

        public string ExtractFacesQueueName { get; set; }
        public string VerifyPersonByFaceQueueName { get; set; }
    }
}
