using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Mono.TextTemplating;
using Mono.TextTemplating.Tests;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NUnit.Framework;
using SubSonic.Core.Remoting.Channels;
using SubSonic.Core.Remoting.Channels.Ipc.NamedPipes;
using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
using SubSonic.Core.Remoting.Testing.Components;
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
        [TestCase(typeof(RemoteTransformationRunner))]
        public void CanWrapRuntimeTypesWithSerializableWrapper(Type type)
        {
            RemoteType remoteType = new RemoteType(type);

            remoteType.GetRuntimeType().Should().BeAssignableTo(type);
        }

        [Test]
        [TestCase(typeof(RemoteTransformationRunner))]
        public async Task CanWrapRuntimeTypesWithSerializableWrapperAsync(Type type)
        {
            RemoteType remoteType = new RemoteType(type);

            (await remoteType.GetRuntimeTypeAsync()).Should().BeAssignableTo(type);
        }

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
        [Order(1)]
        public async Task RemotingShouldReturnRemoteProcedureObjectProxy()
        {
            IProcessTransformationRunFactory factory = await RemotingServices.ConnectAsync<IProcessTransformationRunFactory>(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}/{TransformationRunFactory.TransformationRunFactoryMethod}"));

            factory.Should().NotBeNull();

            if (factory is IProcessTransformationRunFactory runFactory)
            {
                runFactory.IsRunFactoryAlive().Should().BeTrue();
                
                if (runFactory.CreateTransformationRunner() is IProcessTransformationRunner runner)
                {
                    runner.Should().NotBeNull();
                    runner.RunnerId.Should().NotBeEmpty();

                    FluentActions.Invoking(() => runFactory.PrepareTransformation(runner.RunnerId, new Mono.TextTemplating.ParsedTemplate("test"), "content", null, new Mono.TextTemplating.TemplateSettings())).Should().Throw<TargetInvocationException>();

                    runFactory.StartTransformation(runner.RunnerId).Should().Be("// Error Generating Output");
                }
            }
        }

        [Test]
        [Order(2)]
        public async Task RemoteProcedureTestWithDummyHost()
        {
            IProcessTransformationRunFactory factory = await RemotingServices.ConnectAsync<IProcessTransformationRunFactory>(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}/{TransformationRunFactory.TransformationRunFactoryMethod}"));

            IProcessTextTemplatingEngine engine = new TemplatingEngine();

            if (factory?.IsRunFactoryAlive() ?? default)
            {
                IProcessTransformationRunner runner = engine.PrepareTransformationRunner(Samples.template, new DummyHost(), factory);

                runner.Should().NotBeNull();

                string result = factory.StartTransformation(runner.RunnerId);

                var errors = factory.GetErrors(runner.RunnerId);

                if (errors.HasErrors)
                {
                    foreach(TemplateError error in errors)
                    {
                        Console.Out.WriteLine(error.Message);
                    }
                }

                result.Should().Be(Samples.outcome);
            }
        }

        [Test]
        [Order(100)]
        [TestCase("ipc://BeforeTransformationRunFactoryService", false)]
        [TestCase("ipc://TransformationRunFactoryService", true)]
        [TestCase("ipc://AfterTransformationRunFactoryService", false)]
        public void CanDisconnectServiceUsingUri(string uri, bool expected)
        {
            RemotingServices.Disconnect(new Uri(uri)).Should().Be(expected);
        }

        [Test]
        [Order(1000)]
        [TestCase("ipc://BeforeTransformationRunFactoryService", false)]
        [TestCase("ipc://TransformationRunFactoryService", true)]
        [TestCase("ipc://AfterTransformationRunFactoryService", false)]
        public void CanUnRegisterServiceUsingUri(string uri, bool expected)
        {
            ChannelServices.UnRegisterChannel(new Uri(uri)).Should().Be(expected);
        }
    }
}

