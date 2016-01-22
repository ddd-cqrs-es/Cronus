﻿using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.CircuitBreaker;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;
using Elders.Cronus.EventStore;
using Elders.Cronus.AtomicAction;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelineConsumerSettings<TContract> : SettingsBuilder, IConsumerSettings<TContract>
        where TContract : IMessage
    {
        public PipelineConsumerSettings(ISettingsBuilder settingsBuilder, string name)
            : base(settingsBuilder, name)
        {
            this.WithDefaultCircuitBreaker();
            this.SetNumberOfConsumerThreads(2);
        }

        int IConsumerSettings.NumberOfWorkers { get; set; }

        MessageThreshold IConsumerSettings.MessageTreshold { get; set; }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            Func<IPipelineTransport> transport = () => builder.Container.Resolve<IPipelineTransport>(builder.Name);
            Func<ISerializer> serializer = () => builder.Container.Resolve<ISerializer>();
            Func<IMessageProcessor> messageHandlerProcessor = () => builder.Container.Resolve<IMessageProcessor>(builder.Name);
            Func<IEndpontCircuitBreakerFactrory> endpointCircuitBreaker = () => builder.Container.Resolve<IEndpontCircuitBreakerFactrory>(builder.Name);
            Func<IEndpointConsumer> consumer = () => new EndpointConsumer(transport(), messageHandlerProcessor(), serializer(), (this as IConsumerSettings<TContract>).MessageTreshold, endpointCircuitBreaker());
            builder.Container.RegisterSingleton<IEndpointConsumer>(() => consumer(), builder.Name);
        }
    }

    public class CommandConsumerSettings : PipelineConsumerSettings<ICommand>
    {
        public CommandConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }

        public override void Build()
        {
            base.Build();

            var builder = this as ISettingsBuilder;
            Func<IAggregateRootAtomicAction> atomicAction = () => builder.Container.Resolve<IAggregateRootAtomicAction>();
            Func<IAggregateRepository> aggregateRepository = () => new AggregateRepository(builder.Container.Resolve<IEventStore>(builder.Name), atomicAction());
            builder.Container.RegisterSingleton<IAggregateRepository>(() => aggregateRepository(), builder.Name);
        }
    }

    public class ProjectionConsumerSettings : PipelineConsumerSettings<IEvent>
    {
        public ProjectionConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public class PortConsumerSettings : PipelineConsumerSettings<IEvent>
    {
        public PortConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public static class ConsumerSettingsExtensions
    {
        public static T SetNumberOfConsumerThreads<T>(this T self, int numberOfConsumers) where T : IConsumerSettings
        {
            self.NumberOfWorkers = numberOfConsumers;
            return self;
        }

        public static T SetMessageThreshold<T>(this T self, uint size, uint delay) where T : IConsumerSettings
        {
            self.MessageTreshold = new MessageThreshold(size, delay);
            return self;
        }

        public static T UseCommandConsumer<T>(this T self, Action<CommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseCommandConsumer(self, null, configure);
        }

        public static T UseCommandConsumer<T>(this T self, string name, Action<CommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            CommandConsumerSettings settings = new CommandConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseProjectionConsumer<T>(this T self, Action<ProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseProjectionConsumer(self, null, configure);
        }

        public static T UseProjectionConsumer<T>(this T self, string name, Action<ProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            ProjectionConsumerSettings settings = new ProjectionConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UsePortConsumer<T>(this T self, Action<PortConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UsePortConsumer(self, null, configure);
        }

        public static T UsePortConsumer<T>(this T self, string name, Action<PortConsumerSettings> configure = null) where T : ICronusSettings
        {
            PortConsumerSettings settings = new PortConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}
