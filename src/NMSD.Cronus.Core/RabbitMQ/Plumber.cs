using System;
using RabbitMQ.Client;

namespace NMSD.Cronus.RabbitMQ
{
    public sealed class Plumber
    {
        private IConnection connection;

        private ConnectionFactory factory;

        private readonly string hostname;

        private readonly string password;

        private readonly int port;

        private RetryPolicy retryPolicy = RetryableOperation.RetryPolicyFactory.CreateInfiniteLinearRetryPolicy(new TimeSpan(500000));

        private readonly string username;

        private readonly string virtualHost;

        public Plumber() : this("localhost") { }

        public Plumber(string hostname, string username = ConnectionFactory.DefaultUser, string password = ConnectionFactory.DefaultPass, int port = 5672, string virtualHost = ConnectionFactory.DefaultVHost)
        {
            this.hostname = hostname;
            this.username = username;
            this.password = password;
            this.port = port;
            this.virtualHost = virtualHost;
            factory = new ConnectionFactory
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost
            };
        }

        public IConnection RabbitConnection
        {
            get
            {
                if (connection == null || !connection.IsOpen)
                {
                    connection = RetryableOperation.TryExecute<IConnection>(() => factory.CreateConnection(), retryPolicy);
                }
                return connection;
            }
        }
    }
}