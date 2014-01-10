﻿using System.Reflection;
using NMSD.Cronus.Core;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.UnitOfWork;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Protoreg;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Transports.RabbitMQ;
using NMSD.Cronus.Core.EventSourcing;

namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            protoRegistration.RegisterAssembly<NewUserRegistered>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            var session = rabbitMqSessionFactory.OpenSession();
            var commandPublisher = new CommandPublisher(new CommandPipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);

            var eventConsumer = new EventConsumer(new EventHandlerPerEndpoint(new EventHandlersPipelinePerApplication()), new RabbitMqEndpointFactory(session), serializer);
            eventConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            eventConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)),
                type =>
                {
                    var handler = FastActivator.CreateInstance(type);
                    var port = handler as IPort;
                    if (port != null)
                        port.CommandPublisher = commandPublisher;

                    return (port ?? handler) as IMessageHandler;
                });
            eventConsumer.Start(2);
            System.Console.ReadLine();
            session.Close();
        }
    }
}
