﻿using FluentAssertions;
using NUnit.Framework;
using SubSonic.Core.Remoting.Serialization;
using RuntimeBinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace SubSonic.Core.VisualStudio.Testing.Serialization
{
    [TestFixture]
    public class RuntimeBinarySerializationTests
        : SerializationCommonBaseTest
    {
        [Test]
        public void RuntimeBinarySerializationForComparison()
        {
            SerializeMe serializeMe = InitializeMe();

            ISerializationProvider provider = new BinarySerializationProvider(new RuntimeBinaryFormatter());

            byte[] serializedMe = provider.Serialize(serializeMe);

            SerializeMe deserializedMe = provider.Deserialize<SerializeMe>(serializedMe);

            deserializedMe.LargeInteger.Should().Be(serializeMe.LargeInteger);
            deserializedMe.AnInteger.Should().Be(serializeMe.AnInteger);
            deserializedMe.SmallInteger.Should().Be(serializeMe.SmallInteger);
            deserializedMe.SomeDecimal.Should().Be(serializeMe.SomeDecimal);
            deserializedMe.DoubleTrouble.Should().Be(serializeMe.DoubleTrouble);
            deserializedMe.IsThisWrite.Should().Be(serializeMe.IsThisWrite);
            deserializedMe.WhenIWasInstanciated.Should().Be(serializeMe.WhenIWasInstanciated);
            deserializedMe.ArrayOfInts.Should().BeEquivalentTo(serializeMe.ArrayOfInts);
            deserializedMe.Guid.Should().Be(serializeMe.Guid);

            FluentActions.Invoking(() => deserializedMe.ThisShouldDoSomething()).Should().NotThrow();

            deserializedMe.WillReturnAnArrayOfInts().Should().BeEquivalentTo(serializeMe.ArrayOfInts);
        }
    }
}
