using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class ObjectNull : IStreamable
    {
        private BinaryHeaderEnum binaryHeaderEnum;

        public ObjectNull()
        {
        }

        public int NullCount { get; private set; }

        public void Read(BinaryParser input)
        {
            switch (binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ObjectNull:
                    this.NullCount = 1;
                    return;

                case BinaryHeaderEnum.MessageEnd:
                case BinaryHeaderEnum.Assembly:
                    break;

                case BinaryHeaderEnum.ObjectNullMultiple256:
                    this.NullCount = input.ReadByte();
                    return;

                case BinaryHeaderEnum.ObjectNullMultiple:
                    this.NullCount = input.ReadInt32();
                    break;

                default:
                    return;
            }
        }

        internal void Set(int nullCount, BinaryHeaderEnum binaryHeaderEnum = BinaryHeaderEnum.ObjectNull)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
            this.NullCount = nullCount;
        }

        public void Write(BinaryFormatterWriter output)
        {
            if (this.NullCount == 1)
            {
                output.Write((byte)10);
            }
            else if (this.NullCount < 0x100)
            {
                output.Write((byte)13);
                output.Write((byte)this.NullCount);
            }
            else
            {
                output.Write((byte)14);
                output.Write(this.NullCount);
            }
        }
    }
}
