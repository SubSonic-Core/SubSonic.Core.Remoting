using FluentAssertions;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NUnit.Framework;
using SubSonic.Core.Remoting.Channels;
using SubSonic.Core.Remoting.Channels.Ipc.NamedPipes;
using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
using SubSonic.Core.Testing;
using SubSonic.Core.VisualStudio.Testing.Components;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    [TestFixture]
    public class RemotingTests
    {
        [Test]
        [TestCase("/TransformationRunFactory/{id}/{name}", "TransformationRunFactory", 2)]
        [TestCase("/TransformationRunFactory/4ae9d716-cdfd-4300-90e0-0f9b8a2ceba2", "TransformationRunFactory", 1)]
        [TestCase("/Dispose", "Dispose", 0)]
        [TestCase("/Shutdown/{id}", "Shutdown", 1)]
        public void MethodHelperCanParseUriLocalPath(string localPath, string method, int parameterCount)
        {
            MethodHelper helper = new MethodHelper(localPath);

            helper.Name.Should().Be(method);
            helper.Parameters.Count().Should().Be(parameterCount);
        }

        [Test]
        [TestCase("/TransformationRunFactory/{id}", "/TransformationRunFactory/4ae9d716-cdfd-4300-90e0-0f9b8a2ceba2", true)]
        [TestCase("/TransformationRunFactory/{id}/{name}", "/TransformationRunFactory/4ae9d716-cdfd-4300-90e0-0f9b8a2ceba2", false)]
        [TestCase("/Shutdown/{id}", "/TransformationRunFactory/4ae9d716-cdfd-4300-90e0-0f9b8a2ceba2", false)]
        public void MethodHelperHasEqualOperators(string leftPath, string rightPath, bool expected)
        {
            MethodHelper 
                left = new MethodHelper(leftPath),
                right = new MethodHelper(rightPath);

            (left == right).Should().Be(expected);
            (left != right).Should().Be(!expected);
        }

        [Test]
        [TestCase("/TransformationRunFactory/{id}", new[] { "id" })]
        [TestCase("/TransformationRunFactory/4ae9d716-cdfd-4300-90e0-0f9b8a2ceba2", new[] { "4ae9d716-cdfd-4300-90e0-0f9b8a2ceba2" })]
        [TestCase("/TransformationRunFactory/{id}/{name}", new[] { "id", "name" })]
        public void MethodHelperHasListOfParameters(string localPath, string[] expected)
        {
            MethodHelper method = new MethodHelper(localPath);

            method.Parameters.Should().BeEquivalentTo(expected);
        }

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
        public async Task RemotingShouldReturnRemoteProcedureObjectProxy()
        {
            IProcessTransformationRunFactory factory = await RemotingServices.ConnectAsync<IProcessTransformationRunFactory>(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}/{TransformationRunFactory.TransformationRunFactoryMethod}/{Guid.NewGuid()}"));

            factory.Should().NotBeNull();

            if (factory is IProcessTransformationRunFactory runFactory)
            {
                runFactory.IsAlive.Should().BeTrue();
                
                if (runFactory.CreateTransformationRunner(typeof(RemoteTransformationRunner)) is IProcessTransformationRunner runner)
                {
                    runner.Should().NotBeNull();
                    runner.RunnerId.Should().NotBeEmpty();

                    runFactory.PerformTransformation(runner.RunnerId).Should().Be("// Error Generating Output");
                }
            }
        }
    }
}

