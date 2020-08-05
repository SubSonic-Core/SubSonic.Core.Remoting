using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObjectWithMap 
        : BinaryObject
    {
        protected BinaryHeaderEnum binaryHeaderEnum;
        protected string name;
        protected int numMembers;
        protected string[] memberNames;
        protected int assemId;

        public BinaryObjectWithMap()
        {
        }

        public BinaryObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

        public void Read(BinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.name = input.ReadString();
            this.numMembers = input.ReadInt32();
            this.memberNames = new string[this.numMembers];
            for (int i = 0; i < this.numMembers; i++)
            {
                this.memberNames[i] = input.ReadString();
            }
            if (this.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                this.assemId = input.ReadInt32();
            }
        }

        internal void Set(int objectId, string name, int numMembers, string[] memberNames, int assemId)
        {
            this._objectId = objectId;
            this.name = name;
            this.numMembers = numMembers;
            this.memberNames = memberNames;
            this.assemId = assemId;
            this.binaryHeaderEnum = (assemId > 0) ? BinaryHeaderEnum.ObjectWithMapAssemId : BinaryHeaderEnum.ObjectWithMap;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)this.binaryHeaderEnum);
            output.WriteInt32(this._objectId);
            output.WriteString(this.name);
            output.WriteInt32(this.numMembers);
            for (int i = 0; i < this.numMembers; i++)
            {
                output.WriteString(this.memberNames[i]);
            }
            if (this.assemId > 0)
            {
                output.WriteInt32(this.assemId);
            }
        }
    }
}
