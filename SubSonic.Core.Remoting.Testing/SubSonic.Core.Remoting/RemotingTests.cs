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
using System.Reflection;
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
        public async Task RemotingShouldReturnRemoteProcedureObjectProxy()
        {
            IProcessTransformationRunFactory factory = await RemotingServices.ConnectAsync<IProcessTransformationRunFactory>(new Uri($"ipc://{TransformationRunFactory.TransformationRunFactoryService}/{TransformationRunFactory.TransformationRunFactoryMethod}"));

            factory.Should().NotBeNull();

            if (factory is IProcessTransformationRunFactory runFactory)
            {
                runFactory.IsAlive.Should().BeTrue();
                
                if (runFactory.CreateTransformationRunner(typeof(TransformationRunner)) is IProcessTransformationRunner runner)
                {
                    runner.Should().NotBeNull();
                }
            }
        }

        Assembly ResolveReferencedAssemblies(object sender, ResolveEventArgs args)
        {
            //AssemblyName asmName = new AssemblyName(args.Name);
            //foreach (var asmFile in settings.Assemblies)
            //{
            //    if (asmName.Name == Path.GetFileNameWithoutExtension(asmFile))
            //        return Assembly.LoadFrom(asmFile);
            //}

            //var path = host.ResolveAssemblyReference(asmName.Name);

            //if (File.Exists(path))
            //{
            //    return Assembly.LoadFrom(path);
            //}

            return null;
        }
    }
}

