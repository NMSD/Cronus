﻿using System;
using System.Threading;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Transports.RabbitMQ;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Protoreg;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static CommandPublisher commandPublisher;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<RegisterNewUser>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            var session = rabbitMqSessionFactory.OpenSession();
            commandPublisher = new CommandPublisher(new CommandPipelineConvention(), new RabbitMqPipelineFactory(session), serializer);

            HostUI(1111111100);
            session.Close();
        }

        private static void HostUI(int messageDelayInMilliseconds = 0, int batchSize = 1)
        {

            for (int i = 0; i < 1000; i++)
            {
                if (messageDelayInMilliseconds == 0)
                {
                    PublishCommands();
                }
                else
                {
                    for (int j = 0; j < batchSize; j++)
                    {
                        PublishCommands();
                    }

                    Thread.Sleep(messageDelayInMilliseconds);
                }
            }
        }

        private static void PublishCommands()
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = "mynkow@gmail.com";
            commandPublisher.Publish(new RegisterNewUser(userId, email));
            Thread.Sleep(10000);
            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail@gmail.com"));
        }
    }
}
