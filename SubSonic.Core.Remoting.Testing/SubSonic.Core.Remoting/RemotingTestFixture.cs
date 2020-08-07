using Mono.VisualStudio.TextTemplating.VSHost;
using NUnit.Framework;
using ServiceWire.NamedPipes;
using SubSonic.Core.Remoting.Serialization;
using SubSonic.Core.Testing;
using SubSonic.Core.VisualStudio.Testing.Components;
using System;

namespace SubSonic.Core.Remoting
{
    [SetUpFixture]
    public class RemotingTestFixture
    {
        NpHost HostServer { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            HostServer = new NpHost(TransformationRunFactory.TransformationRunFactoryService, serializer: new BinarySerializationProvider());
            HostServer.AddService<ITransformationRunFactoryService>(new TransformationRunFactoryService(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}")));
            HostServer.Open();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            HostServer?.Close();
            HostServer?.Dispose();
            HostServer = null;
        }
    }
}