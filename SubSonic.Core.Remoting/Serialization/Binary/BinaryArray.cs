using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryArray
        : BinaryObject
    {
        internal int _rank;
        internal int[] _lengthA;
        internal int[] _lowerBoundA;
        internal BinaryTypeEnum _binaryTypeEnum;
        internal object _typeInformation;
        internal int _assemId;
        private BinaryHeaderEnum _binaryHeaderEnum;
        internal BinaryArrayTypeEnum _binaryArrayTypeEnum;

        public BinaryArray() { }
        public BinaryArray(BinaryHeaderEnum binaryHeaderEnum)
        {
            _binaryHeaderEnum = binaryHeaderEnum;
        }

        public override void Read(BinaryParser input)
        {
            switch (this._binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ArraySinglePrimitive:
                    this.ObjectId = input.ReadInt32();
                    this._lengthA = new int[] { input.ReadInt32() };
                    this._binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this._rank = 1;
                    this._lowerBoundA = new int[this._rank];
                    this._binaryTypeEnum = BinaryTypeEnum.Primitive;
                    this._typeInformation = (PrimitiveTypeEnum)input.ReadByte();
                    return;

                case BinaryHeaderEnum.ArraySingleObject:
                    this.ObjectId = input.ReadInt32();
                    this._lengthA = new int[] { input.ReadInt32() };
                    this._binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this._rank = 1;
                    this._lowerBoundA = new int[this._rank];
                    this._binaryTypeEnum = BinaryTypeEnum.Object;
                    this._typeInformation = null;
                    return;

                case BinaryHeaderEnum.ArraySingleString:
                    this.ObjectId = input.ReadInt32();
                    this._lengthA = new int[] { input.ReadInt32() };
                    this._binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this._rank = 1;
                    this._lowerBoundA = new int[this._rank];
                    this._binaryTypeEnum = BinaryTypeEnum.String;
                    this._typeInformation = null;
                    return;
            }
            this.ObjectId = input.ReadInt32();
            this._binaryArrayTypeEnum = (BinaryArrayTypeEnum)input.ReadByte();
            this._rank = input.ReadInt32();
            this._lengthA = new int[this._rank];
            this._lowerBoundA = new int[this._rank];
            for (int i = 0; i < this._rank; i++)
            {
                this._lengthA[i] = input.ReadInt32();
            }
            if ((this._binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) || ((this._binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset) || (this._binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset)))
            {
                for (int j = 0; j < this._rank; j++)
                {
                    this._lowerBoundA[j] = input.ReadInt32();
                }
            }
            this._binaryTypeEnum = (BinaryTypeEnum)input.ReadByte();
            this._typeInformation = BinaryTypeConverter.ReadTypeInfo(this._binaryTypeEnum, input, out this._assemId);
        }

        internal void Set(int objectId, int rank, int[] lengthA, int[] lowerBoundA, BinaryTypeEnum binaryTypeEnum, object typeInformation, BinaryArrayTypeEnum binaryArrayTypeEnum, int assemId)
        {
            this.ObjectId = objectId;
            this._binaryArrayTypeEnum = binaryArrayTypeEnum;
            this._rank = rank;
            this._lengthA = lengthA;
            this._lowerBoundA = lowerBoundA;
            this._binaryTypeEnum = binaryTypeEnum;
            this._typeInformation = typeInformation;
            this._assemId = assemId;
            this._binaryHeaderEnum = BinaryHeaderEnum.Array;
            if (binaryArrayTypeEnum == BinaryArrayTypeEnum.Single)
            {
                if (binaryTypeEnum == BinaryTypeEnum.Primitive)
                {
                    this._binaryHeaderEnum = BinaryHeaderEnum.ArraySinglePrimitive;
                }
                else if (binaryTypeEnum == BinaryTypeEnum.String)
                {
                    this._binaryHeaderEnum = BinaryHeaderEnum.ArraySingleString;
                }
                else if (binaryTypeEnum == BinaryTypeEnum.Object)
                {
                    this._binaryHeaderEnum = BinaryHeaderEnum.ArraySingleObject;
                }
            }
        }

        public override void Write(BinaryFormatterWriter output)
        {
            switch (this._binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ArraySinglePrimitive:
                    output.Write((byte)this._binaryHeaderEnum);
                    output.Write(this.ObjectId);
                    output.Write(this._lengthA[0]);
                    output.Write((byte)((PrimitiveTypeEnum)this._typeInformation));
                    return;

                case BinaryHeaderEnum.ArraySingleObject:
                    output.Write((byte)this._binaryHeaderEnum);
                    output.Write(this.ObjectId);
                    output.Write(this._lengthA[0]);
                    return;

                case BinaryHeaderEnum.ArraySingleString:
                    output.Write((byte)this._binaryHeaderEnum);
                    output.Write(this.ObjectId);
                    output.Write(this._lengthA[0]);
                    return;
            }
            output.Write((byte)this._binaryHeaderEnum);
            output.Write(this.ObjectId);
            output.Write((byte)this._binaryArrayTypeEnum);
            output.Write(this._rank);
            for (int i = 0; i < this._rank; i++)
            {
                output.Write(this._lengthA[i]);
            }
            if ((this._binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) || ((this._binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset) || (this._binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset)))
            {
                for (int j = 0; j < this._rank; j++)
                {
                    output.Write(this._lowerBoundA[j]);
                }
            }
            output.Write((byte)this._binaryTypeEnum);
            BinaryTypeConverter.WriteTypeInfo(this._binaryTypeEnum, this._typeInformation, this._assemId, output);
        }
    }
}
