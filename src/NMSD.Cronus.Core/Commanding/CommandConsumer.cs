using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NMSD.Cronus.RabbitMQ;
using NMSD.Protoreg;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using NMSD.Cronus.Core.UnitOfWork;
using RabbitMQ.Client.Events;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Transports;
using Cronus.Core.EventStore;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Core.Commanding
{
    public class CommandConsumer : BaseInMemoryConsumer<ICommand, IMessageHandler> //: RabbitConsumer<ICommand, IMessageHandler>
    {
        private readonly ICommandHandlerEndpointConvention convention;
        private readonly IEventStore eventStore;
        private readonly IEndpointFactory factory;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommandConsumer));
        private List<WorkPool> pools;
        private readonly ProtoregSerializer serialiser;

        public CommandConsumer(ICommandHandlerEndpointConvention convention, IEndpointFactory factory, ProtoregSerializer serialiser, IEventStore eventStore)
        {
            this.eventStore = eventStore;
            this.factory = factory;
            this.convention = convention;
            this.serialiser = serialiser;
        }

        public void RegisterAllHandlersInAssembly(Assembly assemblyContainingMessageHandlers)
        {
            RegisterAllHandlersInAssembly(assemblyContainingMessageHandlers, x => (IMessageHandler)FastActivator.CreateInstance(x));
        }
        public void RegisterAllHandlersInAssembly(Assembly assemblyContainingMessageHandlers, Func<Type, IMessageHandler> messageHandlerFactory)
        {
            MessageHandlerRegistrations.RegisterAllHandlersInAssembly<IMessageHandler>(this, assemblyContainingMessageHandlers, x =>
            {
                var handler = messageHandlerFactory(x);
                (handler as IAggregateRootApplicationService).EventStore = eventStore;
                return handler;
            });
        }

        public override void Start(int numberOfWorkers)
        {
            pools = new List<WorkPool>();
            var endpoints = convention.GetEndpointDefinitions(base.RegisteredHandlers.Keys.ToArray());
            
            foreach (var endpoint in endpoints)
            {
                var pool = new WorkPool(String.Format("Workpoll {0}", endpoint.EndpointName), numberOfWorkers);
                for (int i = 0; i < numberOfWorkers; i++)
                {
                    pool.AddWork(new ConsumerWork(this, factory.CreateEndpoint(endpoint)));
                }
                pools.Add(pool);
                pool.StartCrawlers();
            }
        }

        public override void Stop()
        {
            foreach (WorkPool pool in pools)
            {
                pool.Stop();
            }
        }

        private class ConsumerWork : IWork
        {
            private CommandConsumer consumer;
            private readonly IEndpoint endpoint;
            public ConsumerWork(CommandConsumer consumer, IEndpoint endpoint)
            {
                this.endpoint = endpoint;
                this.consumer = consumer;
            }

            public DateTime ScheduledStart { get; set; }

            public void Start()
            {
                try
                {
                    endpoint.Open();
                    while (true)
                    {
                        try
                        {
                            using (var batchedUoW = consumer.UnitOfWorkFactory.NewBatch())
                            {
                                var rawMessage = endpoint.BlockDequeue();

                                ICommand command;
                                using (var stream = new MemoryStream(rawMessage.Body))
                                {
                                    command = consumer.serialiser.Deserialize(stream) as ICommand;
                                }

                                if (consumer.Handle(command, batchedUoW))
                                    endpoint.Acknowledge(rawMessage);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                            break;
                        }

                    }
                    endpoint.Close();
                }
                catch (OperationInterruptedException ex)
                {
                    ScheduledStart = DateTime.UtcNow.AddMilliseconds(1000);
                }
            }
        }


    }
}