using FluentAssertions;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NUnit.Framework;
using SubSonic.Core.Remoting.Channels;
using SubSonic.Core.Remoting.Channels.Ipc.NamedPipes;
using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
using SubSonic.Core.Testing;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    [TestFixture]
    public class RemotingTests
    {
        [Test]
        [Order(-2)]
        public void ShouldBeAbleToRegisterNamedPipeChannel()
        {
            Hashtable properties = new Hashtable
            {
                [nameof(IChannel.ChannelName)] = TransformationRunFactory.TransformationRunFactoryService
            };

            ChannelServices.RegisterChannel(new NamedPipeChannel<ITransformationRunFactoryService>(properties, new BinarySerializationProvider())).Should().BeTrue();
        }

        [Test]
        [Order(-1)]
        public void ShouldNotBeAbleToRegisterNamedPipeChannel2ndTime()
        {
            FluentActions.Invoking(() =>
            {
                Hashtable properties = new Hashtable
                {
                    [nameof(IChannel.ChannelName)] = TransformationRunFactory.TransformationRunFactoryService
                };

                ChannelServices.RegisterChannel(new NamedPipeChannel<ITransformationRunFactoryService>(properties, new BinarySerializationProvider())).Should().BeTrue();
            }).Should().Throw<SubSonicRemotingException>().WithMessage(RemotingResources.ChannelNameAlreadyRegistered.Format(TransformationRunFactory.TransformationRunFactoryService));
        }

        [Test]
        [TestCase(null, "typeToProxy")]
        [TestCase(typeof(ProcessUtilities), "uri")]
        public void RemotingShouldThrowWhenTypeToProxyOrUriIsNull(Type typeToProxy, string name)
        {
            FluentActions.Invoking(async () =>
            {
                await RemotingServices.ConnectAsync(typeToProxy, null);
            }).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(name);
        }

        [Test]
        [TestCase(typeof(IProcessTransformationRunFactory), "ipc://{0}/{1}")]
        public async Task RemotingShouldReturnRemoteProcedureObjectProxy(Type typeToProxy, string uri)
        {
            object factory = await RemotingServices.ConnectAsync(typeToProxy, new Uri(uri.Format(TransformationRunFactory.TransformationRunFactoryService, TransformationRunFactory.TransformationRunFactoryMethod)));

            factory.Should().NotBeNull();

            if (factory is IProcessTransformationRunFactory runFactory)
            {
                runFactory.IsAlive.Should().BeTrue();
            }
        }
    }
}

