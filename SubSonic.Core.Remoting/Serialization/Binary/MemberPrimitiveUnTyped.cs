using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class MemberPrimitiveUnTyped : IStreamable
    {
        internal PrimitiveTypeEnum _typeInformation;
        internal object _value;

        internal MemberPrimitiveUnTyped()
        {
        }

        public void Read(BinaryParser input)
        {
            this._value = input.ReadValue(this._typeInformation);
        }

        internal void Set(PrimitiveTypeEnum typeInformation)
        {
            this._typeInformation = typeInformation;
        }

        internal void Set(PrimitiveTypeEnum typeInformation, object value)
        {
            this._typeInformation = typeInformation;
            this._value = value;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteValue(this._typeInformation, this._value);
        }
    }
}
