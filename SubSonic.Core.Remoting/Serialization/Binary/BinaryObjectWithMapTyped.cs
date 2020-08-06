using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObjectWithMapTyped
        : BinaryObjectWithMap
    {
        public BinaryTypeEnum[] BinaryTypeEnumArray { get; set; }
        public object[] TypeInformationArray { get; set; }
        public int[] MemberAssemIds { get; set; }

        public BinaryObjectWithMapTyped()
            : base() { }

        public BinaryObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
            : base(binaryHeaderEnum) { }

        public override void Read(BinaryParser input)
        {
            this.ObjectId = input.ReadInt32();
            this.Name = input.ReadString();
            this.NumMembers = input.ReadInt32();
            this.MemberNames = new string[this.NumMembers];
            this.BinaryTypeEnumArray = new BinaryTypeEnum[this.NumMembers];
            this.TypeInformationArray = new object[this.NumMembers];
            this.MemberAssemIds = new int[this.NumMembers];
            for (int i = 0; i < this.NumMembers; i++)
            {
                this.MemberNames[i] = input.ReadString();
            }
            for (int j = 0; j < this.NumMembers; j++)
            {
                this.BinaryTypeEnumArray[j] = (BinaryTypeEnum)input.ReadByte();
            }
            for (int k = 0; k < this.NumMembers; k++)
            {
                if ((this.BinaryTypeEnumArray[k] != BinaryTypeEnum.ObjectUrt) && (this.BinaryTypeEnumArray[k] != BinaryTypeEnum.ObjectUser))
                {
                    this.TypeInformationArray[k] = BinaryTypeConverter.ReadTypeInfo(this.BinaryTypeEnumArray[k], input, out this.MemberAssemIds[k]);
                }
                else
                {
                    BinaryTypeConverter.ReadTypeInfo(this.BinaryTypeEnumArray[k], input, out this.MemberAssemIds[k]);
                }
            }
            if (this.BinaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                this.AssemId = input.ReadInt32();
            }
        }

        internal void Set(int objectId, string name, int numMembers, string[] memberNames, BinaryTypeEnum[] binaryTypeEnumArray, object[] typeInformationArray, int[] memberAssemIds, int assemId)
        {
            this.ObjectId = objectId;
            this.AssemId = assemId;
            this.Name = name;
            this.NumMembers = numMembers;
            this.MemberNames = memberNames;
            this.BinaryTypeEnumArray = binaryTypeEnumArray;
            this.TypeInformationArray = typeInformationArray;
            this.MemberAssemIds = memberAssemIds;
            this.AssemId = assemId;
            this.BinaryHeaderEnum = (assemId > 0) ? BinaryHeaderEnum.ObjectWithMapTypedAssemId : BinaryHeaderEnum.ObjectWithMapTyped;
        }

        public override void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)this.BinaryHeaderEnum);
            output.WriteInt32(this.ObjectId);
            output.WriteString(this.Name);
            output.WriteInt32(this.NumMembers);
            for (int i = 0; i < this.NumMembers; i++)
            {
                output.WriteString(this.MemberNames[i]);
            }
            for (int j = 0; j < this.NumMembers; j++)
            {
                output.WriteByte((byte)this.BinaryTypeEnumArray[j]);
            }
            for (int k = 0; k < this.NumMembers; k++)
            {
                BinaryTypeConverter.WriteTypeInfo(this.BinaryTypeEnumArray[k], this.TypeInformationArray[k], this.MemberAssemIds[k], output);
            }
            if (this.AssemId > 0)
            {
                output.WriteInt32(this.AssemId);
            }
        }
    }
}
