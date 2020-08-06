using FluentAssertions;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NUnit.Framework;
using SubSonic.Core.Remoting.Channels;
using SubSonic.Core.Remoting.Channels.Ipc.NamedPipes;
using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
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
        public async Task ShouldBeAbleToRegisterNamedPipeChannel()
        {
            Hashtable properties = new Hashtable();

            properties[nameof(IChannel.ChannelName)] = TransformationRunFactory.TransformationRunFactoryService;

            (await ChannelServices.RegisterChannelAsync(new NamedPipeChannel<ITransformationRunFactoryService>(properties, new BinarySerializationProvider()))).Should().BeTrue();
        }

        [Test]
        [Order(-1)]
        public void ShouldNotBeAbleToRegisterNamedPipeChannel2ndTime()
        {
            FluentActions.Invoking(async () =>
            {
                Hashtable properties = new Hashtable();

                properties[nameof(IChannel.ChannelName)] = TransformationRunFactory.TransformationRunFactoryService;

                (await ChannelServices.RegisterChannelAsync(new NamedPipeChannel<ITransformationRunFactoryService>(properties, new BinarySerializationProvider()))).Should().BeTrue();
            }).Should().Throw<SubSonicRemotingException>().WithMessage(RemotingResources.ChannelNameAlreadyRegistered.Format(TransformationRunFactory.TransformationRunFactoryService));
        }

        //[Test]
        //[TestCase(typeof(ProcessUtilities))]
        //public void RemotingShouldThrowWhenTypeIsNotMarshalByReference(Type typeToProxy)
        //{
        //    FluentActions.Invoking(async () =>
        //    {
        //        await RemotingServices.ConnectAsync(typeToProxy, new Uri("ipc://unknown"));
        //    }).Should().Throw<SubSonicRemotingException>().WithMessage(RemotingResources.NotRemotableByReference.Format(typeToProxy.FullName));
        //}

        //[Test]
        //[TestCase(null, "classToProxy")]
        //[TestCase(typeof(ProcessUtilities), "uri")]
        //public void RemotingShouldThrowWhenTypeToProxyOrUriIsNull(Type typeToProxy, string name)
        //{
        //    FluentActions.Invoking(async () =>
        //    {
        //        await RemotingServices.ConnectAsync(typeToProxy, null);
        //    }).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(name);
        //}

        //[Test]
        //[TestCase(typeof(IProcessTransformationRunFactory), "ipc://{0}/{1}")]
        //public async Task RemotingShouldReturnMarshaledTypeObject(Type typeToProxy, string uri)
        //{
        //    object factory = await RemotingServices.ConnectAsync(typeToProxy, new Uri(uri.Format(TransformationRunFactory.TransformationRunFactoryPrefix, TransformationRunFactory.TransformationRunFactorySuffix)));

        //    factory.Should().NotBeNull();

        //    if (factory is IProcessTransformationRunFactory runFactory)
        //    {
        //        runFactory.IsAlive.Should().BeTrue();
        //    }
        //}
    }
}

