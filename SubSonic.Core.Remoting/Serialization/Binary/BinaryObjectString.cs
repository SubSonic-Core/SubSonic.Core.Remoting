using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObjectString
        : BinaryObject
    {
        public override void Read(BinaryParser input)
        {
            ObjectId = input.ReadInt32();
            Value = input.ReadString();
        }

        public string Value { get; set; }

        public void Set(int objectId, string value)
        {
            this.ObjectId = objectId;
            this.Value = value;
        }

        public override void Write(BinaryFormatterWriter output)
        {
            output.Write((byte)6);
            output.Write(ObjectId);
            output.Write(Value);
        }
    }
}
