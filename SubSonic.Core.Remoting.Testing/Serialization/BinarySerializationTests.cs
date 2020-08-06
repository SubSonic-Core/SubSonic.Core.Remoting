using FluentAssertions;
using NUnit.Framework;
using SubSonic.Core.Remoting.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.VisualStudio.Testing.Serialization
{
    [TestFixture]
    public class BinarySerializationTests
    {
        [Serializable]
        public class SerializeMe
        {
            public Int64 LargeInteger { get; set; }
            public Int32 AnInteger { get; set; }
            public Int16 SmallInteger { get; set; }
            public decimal SomeDecimal { get; set; }
            public double DoubleTrouble { get; set; }
            public bool IsThisWrite { get; set; }
            public DateTime WhenIWasInstanciated { get; set; }
            public int[] ArrayOfInts { get; set; }

            public void ThisShouldDoSomething()
            {

            }

            public int[] WillReturnAnArrayOfInts() => ArrayOfInts;
        }

        [Test]
        public void ShouldBeAbleToSerializeAndDeserializeAnObject()
        {
            DateTime testRanAt = DateTime.Now;

            Random random = new Random();

            Decimal pi = 3.141592653589793238M;

            int[] arrayOfInts = new[] { 1, 2, 3, 4, 6, 7, 9 };

            SerializeMe serializeMe = new SerializeMe()
            {
                LargeInteger = random.Next(100000),
                AnInteger = random.Next(100),
                SmallInteger = 15,
                SomeDecimal = pi,
                DoubleTrouble = Convert.ToDouble(pi),
                IsThisWrite = true,
                WhenIWasInstanciated = testRanAt,
                ArrayOfInts = arrayOfInts
            };

            BinarySerializationProvider provider = new BinarySerializationProvider();

            byte[] serializedMe = provider.Serialize<SerializeMe>(serializeMe);

            SerializeMe deserializedMe = provider.Deserialize<SerializeMe>(serializedMe);

            deserializedMe.LargeInteger.Should().Be(serializeMe.LargeInteger);
            deserializedMe.AnInteger.Should().Be(serializeMe.AnInteger);
            deserializedMe.SmallInteger.Should().Be(serializeMe.SmallInteger);
            deserializedMe.SomeDecimal.Should().Be(pi);
            deserializedMe.DoubleTrouble.Should().Be(serializeMe.DoubleTrouble);
            deserializedMe.IsThisWrite.Should().Be(serializeMe.IsThisWrite);
            deserializedMe.WhenIWasInstanciated.Should().Be(testRanAt);
            deserializedMe.ArrayOfInts.Should().BeEquivalentTo(arrayOfInts);

            FluentActions.Invoking(() => deserializedMe.ThisShouldDoSomething()).Should().NotThrow();

            deserializedMe.WillReturnAnArrayOfInts().Should().BeEquivalentTo(arrayOfInts);
        }
    }
}
