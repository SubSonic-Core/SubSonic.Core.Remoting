using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObjectWithMapTyped
        : BinaryObjectWithMap
    {
        BinaryTypeEnum[] binaryTypeEnumArray;
        protected object[] typeInformationArray;
        protected int[] memberAssemIds;

        public BinaryObjectWithMapTyped()
            : base() { }

        public BinaryObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
            : base(binaryHeaderEnum) { }

        public override void Read(BinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.name = input.ReadString();
            this.numMembers = input.ReadInt32();
            this.memberNames = new string[this.numMembers];
            this.binaryTypeEnumArray = new BinaryTypeEnum[this.numMembers];
            this.typeInformationArray = new object[this.numMembers];
            this.memberAssemIds = new int[this.numMembers];
            for (int i = 0; i < this.numMembers; i++)
            {
                this.memberNames[i] = input.ReadString();
            }
            for (int j = 0; j < this.numMembers; j++)
            {
                this.binaryTypeEnumArray[j] = (BinaryTypeEnum)input.ReadByte();
            }
            for (int k = 0; k < this.numMembers; k++)
            {
                if ((this.binaryTypeEnumArray[k] != BinaryTypeEnum.ObjectUrt) && (this.binaryTypeEnumArray[k] != BinaryTypeEnum.ObjectUser))
                {
                    this._typeInformationA[k] = BinaryTypeConverter.ReadTypeInfo(this.binaryTypeEnumArray[k], input, out this.memberAssemIds[k]);
                }
                else
                {
                    BinaryTypeConverter.ReadTypeInfo(this._binaryTypeEnumA[k], input, out this._memberAssemIds[k]);
                }
            }
            if (this._binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                this._assemId = input.ReadInt32();
            }
        }

        internal void Set(int objectId, string name, int numMembers, string[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, object[] typeInformationA, int[] memberAssemIds, int assemId)
        {
            this._objectId = objectId;
            this._assemId = assemId;
            this._name = name;
            this._numMembers = numMembers;
            this._memberNames = memberNames;
            this._binaryTypeEnumA = binaryTypeEnumA;
            this._typeInformationA = typeInformationA;
            this._memberAssemIds = memberAssemIds;
            this._assemId = assemId;
            this._binaryHeaderEnum = (assemId > 0) ? BinaryHeaderEnum.ObjectWithMapTypedAssemId : BinaryHeaderEnum.ObjectWithMapTyped;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)this._binaryHeaderEnum);
            output.WriteInt32(this._objectId);
            output.WriteString(this._name);
            output.WriteInt32(this._numMembers);
            for (int i = 0; i < this._numMembers; i++)
            {
                output.WriteString(this._memberNames[i]);
            }
            for (int j = 0; j < this._numMembers; j++)
            {
                output.WriteByte((byte)this._binaryTypeEnumA[j]);
            }
            for (int k = 0; k < this._numMembers; k++)
            {
                BinaryTypeConverter.WriteTypeInfo(this._binaryTypeEnumA[k], this._typeInformationA[k], this._memberAssemIds[k], output);
            }
            if (this._assemId > 0)
            {
                output.WriteInt32(this._assemId);
            }
        }
    }
}
