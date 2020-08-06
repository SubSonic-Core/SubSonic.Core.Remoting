using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.VisualStudio.Testing.Serialization
{
    public class SerializationCommonBaseTest
    {
        [Serializable]
        public class SerializeMe
        {
            public Guid Guid { get; set; }
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

        protected SerializeMe InitializeMe()
        {
            DateTime testRanAt = DateTime.Now;

            Random random = new Random();

            Decimal pi = 3.141592653589793238M;

            int[] arrayOfInts = new[] { 1, 2, 3, 4, 6, 7, 9 };

            return new SerializeMe()
            {
                Guid = Guid.NewGuid(),
                LargeInteger = random.Next(100000),
                AnInteger = random.Next(100),
                SmallInteger = 15,
                SomeDecimal = pi,
                DoubleTrouble = Convert.ToDouble(pi),
                IsThisWrite = true,
                WhenIWasInstanciated = testRanAt,
                ArrayOfInts = arrayOfInts
            };
        }
    }
}
