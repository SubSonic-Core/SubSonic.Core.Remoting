using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class MemberPrimitiveTyped : IStreamable
    {
        public PrimitiveTypeEnum PrimitiveTypeEnum { get; set; }
        public object Value { get; set; }

        internal MemberPrimitiveTyped()
        {
        }

        public void Read(BinaryParser input)
        {
            this.PrimitiveTypeEnum = (PrimitiveTypeEnum)input.ReadByte();
            this.Value = input.ReadValue(this.PrimitiveTypeEnum);
        }

        internal void Set(PrimitiveTypeEnum primitiveTypeEnum, object value)
        {
            this.PrimitiveTypeEnum = primitiveTypeEnum;
            this.Value = value;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte(8);
            output.WriteByte((byte)this.PrimitiveTypeEnum);
            output.WriteValue(this.PrimitiveTypeEnum, this.Value);
        }
    }
}
