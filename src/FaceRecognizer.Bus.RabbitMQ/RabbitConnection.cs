using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FaceRecognizer.Bus.RabbitMQ
{
    public class RabbitConnection
    {
        private readonly IOptions<RabbitConfiguration> _rabbitConfiguration;
        private IConnection _connection;
        public RabbitConnection(IOptions<RabbitConfiguration> rabbitConfiguration)
        {
            _rabbitConfiguration = rabbitConfiguration;
        }

        public IConnection GetConnection()
        {
            if (_connection?.IsOpen == null || _connection?.IsOpen == false)
            {
                var rabbitFactory = new ConnectionFactory
                {
                    HostName = _rabbitConfiguration.Value.Host,
                    UserName = _rabbitConfiguration.Value.User,
                    Password = _rabbitConfiguration.Value.Password
                };

                _connection = rabbitFactory.CreateConnection();
            }

            return _connection;
        }
    }
}
