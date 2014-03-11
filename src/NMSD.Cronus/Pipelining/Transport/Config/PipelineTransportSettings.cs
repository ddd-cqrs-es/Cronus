﻿using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports;
using System;
using NMSD.Cronus.DomainModelling;

namespace NMSD.Cronus.Pipelining.Transport.Config
{
    public interface IPipelineTransportSettings<out T>
        where T : ITransportIMessage
    {
        void Build();
        PipelineSettings PipelineSettings { get; }
        IEndpointFactory EndpointFactory { get; }
        IPipelineFactory<IPipeline> PipelineFactory { get; }
    }

    public class PipelineTransportSettings<T> : TransportSettings, IPipelineTransportSettings<T>
        where T : ITransportIMessage
    {
        public PipelineTransportSettings(PipelineSettings pipelineSettings = null)
        {
            PipelineSettings = pipelineSettings ?? new PipelineSettings();
        }

        public virtual void Build()
        {

        }

        public PipelineSettings PipelineSettings { get; set; }


        public IEndpointFactory EndpointFactory { get; protected set; }


        public IPipelineFactory<IPipeline> PipelineFactory { get; protected set; }
    }



    public class PipelineTransportSettings : TransportSettings
    {
        public PipelineTransportSettings(PipelineSettings pipelineSettings = null)
        {
            PipelineSettings = pipelineSettings ?? new PipelineSettings();
        }

        public virtual void Build()
        {

        }

        public PipelineSettings PipelineSettings { get; set; }


        public IEndpointFactory EndpointFactory { get; protected set; }


        public IPipelineFactory<IPipeline> PipelineFactory { get; protected set; }
    }
}