using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObjectWithMap 
        : BinaryObject
    {
        public BinaryHeaderEnum BinaryHeaderEnum { get; set; }
        public string Name { get; set; }
        public int NumMembers { get; set; }
        public string[] MemberNames { get; set; }
        public int AssemId { get; set; }

        public BinaryObjectWithMap()
        {
        }

        public BinaryObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.BinaryHeaderEnum = binaryHeaderEnum;
        }

        public override void Read(BinaryParser input)
        {
            this.ObjectId = input.ReadInt32();
            this.Name = input.ReadString();
            this.NumMembers = input.ReadInt32();
            this.MemberNames = new string[this.NumMembers];
            for (int i = 0; i < this.NumMembers; i++)
            {
                this.MemberNames[i] = input.ReadString();
            }
            if (this.BinaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                this.AssemId = input.ReadInt32();
            }
        }

        internal void Set(int objectId, string name, int numMembers, string[] memberNames, int assemId)
        {
            this.ObjectId = objectId;
            this.Name = name;
            this.NumMembers = numMembers;
            this.MemberNames = memberNames;
            this.AssemId = assemId;
            this.BinaryHeaderEnum = (assemId > 0) ? BinaryHeaderEnum.ObjectWithMapAssemId : BinaryHeaderEnum.ObjectWithMap;
        }

        public override void Write(BinaryFormatterWriter output)
        {
            output.Write((byte)this.BinaryHeaderEnum);
            output.Write(this.ObjectId);
            output.Write(this.Name);
            output.Write(this.NumMembers);
            for (int i = 0; i < this.NumMembers; i++)
            {
                output.Write(this.MemberNames[i]);
            }
            if (this.AssemId > 0)
            {
                output.Write(this.AssemId);
            }
        }
    }
}
