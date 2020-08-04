using FluentAssertions;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NUnit.Framework;
using SubSonic.Core.Remoting.Channels;
using SubSonic.Core.Remoting.Channels.Ipc;
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
            Hashtable properties = new Hashtable();

            properties[nameof(NamedPipeChannel<TransformationRunFactory>.ChannelName)] = TransformationRunFactory.TransformationRunFactoryPrefix;

            ChannelServices.RegisterChannel(new NamedPipeChannel<TransformationRunFactory>(properties)).Should().BeTrue();
        }

        [Test]
        [Order(-1)]
        public void ShouldNotBeAbleToRegisterNamedPipeChannel2ndTime()
        {
            FluentActions.Invoking(() =>
            {
                Hashtable properties = new Hashtable();

                properties[nameof(NamedPipeChannel<TransformationRunFactory>.ChannelName)] = TransformationRunFactory.TransformationRunFactoryPrefix;

                ChannelServices.RegisterChannel(new NamedPipeChannel<TransformationRunFactory>(properties)).Should().BeTrue();
            }).Should().Throw<SubSonicRemotingException>().WithMessage("Remoting channel has already been registered: TransformationRunFactoryService");
        }

        [Test]
        [TestCase(typeof(ProcessUtilities), "Type is not remotable by reference: SubSonic.Core.Utilities.ProcessUtilities")]
        public void RemotingShouldThrowWhenTypeIsNotMarshalByReference(Type typeToProxy, string message)
        {
            FluentActions.Invoking(async () =>
            {
                await RemotingServices.ConnectAsync(typeToProxy, new Uri("ipc://unknown"));
            }).Should().Throw<SubSonicRemotingException>().WithMessage(message);
        }

        [Test]
        [TestCase(null, "classToProxy")]
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
        public async Task RemotingShouldReturnMarshaledTypeObject(Type typeToProxy, string uri)
        {
            object factory = await RemotingServices.ConnectAsync(typeToProxy, new Uri(uri.Format(TransformationRunFactory.TransformationRunFactoryPrefix, TransformationRunFactory.TransformationRunFactorySuffix)));

            factory.Should().NotBeNull();

            if (factory is IProcessTransformationRunFactory runFactory)
            {
                runFactory.IsAlive.Should().BeTrue();
            }
        }
    }
}

