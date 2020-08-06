using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class BinaryFormatterWriter
    {
        private readonly Stream _outputStream;
        private readonly FormatterTypeStyle _formatterTypeStyle;
        private readonly ObjectWriter _objectWriter;
        private readonly BinaryWriter _dataWriter;
        private int _consecutiveNullArrayEntryCount;
        private Dictionary<string, ObjectMapInfo> _objectMapTable;
        private BinaryObject _binaryObject;
        private BinaryObjectWithMap _binaryObjectWithMap;
        private BinaryObjectWithMapTyped _binaryObjectWithMapTyped;
        private BinaryObjectString _binaryObjectString;
        private BinaryArray _binaryArray;
        private byte[] _byteBuffer;
        private MemberPrimitiveUnTyped _memberPrimitiveUnTyped;
        private MemberPrimitiveTyped _memberPrimitiveTyped;
        private ObjectNull _objectNull;
        private MemberReference _memberReference;
        private BinaryAssembly _binaryAssembly;

        internal BinaryFormatterWriter(Stream outputStream, ObjectWriter objectWriter, FormatterTypeStyle formatterTypeStyle)
        {
            this._outputStream = outputStream;
            this._formatterTypeStyle = formatterTypeStyle;
            this._objectWriter = objectWriter;
            this._dataWriter = new BinaryWriter(outputStream, Encoding.UTF8);
        }

        private void InternalWriteItemNull()
        {
            if (this._consecutiveNullArrayEntryCount > 0)
            {
                if (this._objectNull == null)
                {
                    this._objectNull = new ObjectNull();
                }
                this._objectNull.Set(this._consecutiveNullArrayEntryCount);
                this._objectNull.Write(this);
                this._consecutiveNullArrayEntryCount = 0;
            }
        }

        private void WriteArrayAsBytes(Array array, int typeLength)
        {
            this.InternalWriteItemNull();
            //int num = array.Length * typeLength;
            int num2 = 0;
            if (this._byteBuffer == null)
            {
                this._byteBuffer = new byte[0x1000];
            }
            while (num2 < array.Length)
            {
                int num3 = Math.Min(0x1000 / typeLength, array.Length - num2);
                int count = num3 * typeLength;
                Buffer.BlockCopy(array, num2 * typeLength, this._byteBuffer, 0, count);
                if (!BitConverter.IsLittleEndian)
                {
                    int num5 = 0;
                    while (num5 < count)
                    {
                        int num6 = 0;
                        while (true)
                        {
                            if (num6 >= (typeLength / 2))
                            {
                                num5 += typeLength;
                                break;
                            }
                            byte num7 = this._byteBuffer[num5 + num6];
                            this._byteBuffer[num5 + num6] = this._byteBuffer[((num5 + typeLength) - 1) - num6];
                            this._byteBuffer[((num5 + typeLength) - 1) - num6] = num7;
                            num6++;
                        }
                    }
                }
                this.WriteBytes(this._byteBuffer, 0, count);
                num2 += num3;
            }
        }

        internal void WriteAssembly(string assemblyString, int assemId, bool isNew)
        {
            this.InternalWriteItemNull();
            if (assemblyString == null)
            {
                assemblyString = string.Empty;
            }
            if (isNew)
            {
                if (this._binaryAssembly == null)
                {
                    this._binaryAssembly = new BinaryAssembly();
                }
                this._binaryAssembly.Set(assemId, assemblyString);
                this._binaryAssembly.Write(this);
            }
        }

        internal void WriteBegin()
        {
        }

        internal void WriteBoolean(bool value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteByte(byte value)
        {
            this._dataWriter.Write(value);
        }

        private void WriteBytes(byte[] value)
        {
            this._dataWriter.Write(value);
        }

        private void WriteBytes(byte[] byteA, int offset, int size)
        {
            this._dataWriter.Write(byteA, offset, size);
        }

        internal void WriteChar(char value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteChars(char[] value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteDateTime(DateTime value)
        {
            long num = Unsafe.As<DateTime, long>(ref value);
            this.WriteInt64(num);
        }

        internal void WriteDecimal(decimal value)
        {
            this.WriteString(value.ToString(CultureInfo.InvariantCulture));
        }

        internal void WriteDelayedNullItem()
        {
            this._consecutiveNullArrayEntryCount++;
        }

        internal void WriteDouble(double value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteEnd()
        {
            this._dataWriter.Flush();
        }

        internal void WriteInt16(short value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteInt32(int value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteInt64(long value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteItem(NameInfo itemNameInfo, NameInfo typeNameInfo, object value)
        {
            this.InternalWriteItemNull();
            this.WriteMember(itemNameInfo, typeNameInfo, value);
        }

        internal void WriteItemEnd()
        {
            this.InternalWriteItemNull();
        }

        internal void WriteItemObjectRef(/*NameInfo nameInfo,*/ int idRef)
        {
            this.InternalWriteItemNull();
            this.WriteMemberObjectRef(/*nameInfo,*/ idRef);
        }

        internal void WriteJaggedArray(NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound)
        {
            BinaryArrayTypeEnum jagged;
            this.InternalWriteItemNull();
            int[] lengthA = new int[] { length };
            int[] lowerBoundA = null;
            if (lowerBound == 0)
            {
                jagged = BinaryArrayTypeEnum.Jagged;
            }
            else
            {
                jagged = BinaryArrayTypeEnum.JaggedOffset;
                lowerBoundA = new int[] { lowerBound };
            }
            BinaryTypeEnum binaryTypeEnum = BinaryTypeConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo._type, objectInfo, _objectWriter, out object typeInformation, out int assemId);
            if (this._binaryArray == null)
            {
                this._binaryArray = new BinaryArray();
            }
            this._binaryArray.Set((int)arrayNameInfo._objectId, 1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, jagged, assemId);
            this._binaryArray.Write(this);
        }

        internal void WriteMember(NameInfo memberNameInfo, NameInfo typeNameInfo, object value)
        {
            this.InternalWriteItemNull();
            PrimitiveTypeEnum primitiveTypeEnum = typeNameInfo._primitiveTypeEnum;
            if (memberNameInfo._transmitTypeOnMember)
            {
                if (this._memberPrimitiveTyped == null)
                {
                    this._memberPrimitiveTyped = new MemberPrimitiveTyped();
                }
                this._memberPrimitiveTyped.Set(primitiveTypeEnum, value);
                this._memberPrimitiveTyped.Write(this);
            }
            else
            {
                if (this._memberPrimitiveUnTyped == null)
                {
                    this._memberPrimitiveUnTyped = new MemberPrimitiveUnTyped();
                }
                this._memberPrimitiveUnTyped.Set(primitiveTypeEnum, value);
                this._memberPrimitiveUnTyped.Write(this);
            }
        }

        internal void WriteMemberNested(/*NameInfo memberNameInfo*/)
        {
            this.InternalWriteItemNull();
        }

        internal void WriteMemberObjectRef(/*NameInfo memberNameInfo,*/ int idRef)
        {
            this.InternalWriteItemNull();
            if (this._memberReference == null)
            {
                this._memberReference = new MemberReference();
            }
            this._memberReference.Set(idRef);
            this._memberReference.Write(this);
        }

        internal void WriteMemberString(/*NameInfo memberNameInfo,*/ NameInfo typeNameInfo, string value)
        {
            this.InternalWriteItemNull();
            this.WriteObjectString((int)typeNameInfo._objectId, value);
        }

        internal void WriteNullItem(/*NameInfo itemNameInfo, NameInfo typeNameInfo*/)
        {
            this._consecutiveNullArrayEntryCount++;
            this.InternalWriteItemNull();
        }

        internal void WriteNullMember(NameInfo memberNameInfo/*, NameInfo typeNameInfo*/)
        {
            this.InternalWriteItemNull();
            if (this._objectNull == null)
            {
                this._objectNull = new ObjectNull();
            }
            if (!memberNameInfo._isArrayItem)
            {
                this._objectNull.Set(1);
                this._objectNull.Write(this);
                this._consecutiveNullArrayEntryCount = 0;
            }
        }

        internal void WriteObject(NameInfo nameInfo, NameInfo typeNameInfo, int numMembers, string[] memberNames, Type[] memberTypes, WriteObjectInfo[] memberObjectInfos)
        {
            string str;
            this.InternalWriteItemNull();
            int objectId = (int)nameInfo._objectId;
            str = (objectId < 0) ? typeNameInfo.NIname : nameInfo.NIname;
            if (this._objectMapTable == null)
            {
                this._objectMapTable = new Dictionary<string, ObjectMapInfo>();
            }
            if (this._objectMapTable.TryGetValue(str, out ObjectMapInfo info) && info.IsCompatible(numMembers, memberNames, memberTypes))
            {
                if (this._binaryObject == null)
                {
                    this._binaryObject = new BinaryObject();
                }
                this._binaryObject.Set(objectId, info._objectId);
                this._binaryObject.Write(this);
            }
            else
            {
                int num;
                if (!typeNameInfo._transmitTypeOnObject)
                {
                    if (this._binaryObjectWithMap == null)
                    {
                        this._binaryObjectWithMap = new BinaryObjectWithMap();
                    }
                    num = (int)typeNameInfo._assemId;
                    this._binaryObjectWithMap.Set(objectId, str, numMembers, memberNames, num);
                    this._binaryObjectWithMap.Write(this);
                    if (info == null)
                    {
                        this._objectMapTable.Add(str, new ObjectMapInfo(objectId, numMembers, memberNames, memberTypes));
                    }
                }
                else
                {
                    BinaryTypeEnum[] binaryTypeEnumA = new BinaryTypeEnum[numMembers];
                    object[] typeInformationA = new object[numMembers];
                    int[] memberAssemIds = new int[numMembers];
                    int index = 0;
                    while (true)
                    {
                        if (index >= numMembers)
                        {
                            if (this._binaryObjectWithMapTyped == null)
                            {
                                this._binaryObjectWithMapTyped = new BinaryObjectWithMapTyped();
                            }
                            this._binaryObjectWithMapTyped.Set(objectId, str, numMembers, memberNames, binaryTypeEnumA, typeInformationA, memberAssemIds, (int)typeNameInfo._assemId);
                            this._binaryObjectWithMapTyped.Write(this);
                            if (info == null)
                            {
                                this._objectMapTable.Add(str, new ObjectMapInfo(objectId, numMembers, memberNames, memberTypes));
                            }
                            break;
                        }
                        binaryTypeEnumA[index] = BinaryTypeConverter.GetBinaryTypeInfo(memberTypes[index], memberObjectInfos[index], this._objectWriter, out object typeInformation, out num);
                        typeInformationA[index] = typeInformation;
                        memberAssemIds[index] = num;
                        index++;
                    }
                }
            }
        }

        internal void WriteObjectByteArray(/*NameInfo memberNameInfo,*/ NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, byte[] byteA)
        {
            this.InternalWriteItemNull();
            this.WriteSingleArray(/*memberNameInfo,*/ arrayNameInfo, objectInfo, arrayElemTypeNameInfo, length, lowerBound, byteA);
        }

        //internal void WriteObjectEnd(NameInfo memberNameInfo, NameInfo typeNameInfo)
        //{
        //}

        internal void WriteObjectString(int objectId, string value)
        {
            this.InternalWriteItemNull();
            if (this._binaryObjectString == null)
            {
                this._binaryObjectString = new BinaryObjectString();
            }
            this._binaryObjectString.Set(objectId, value);
            this._binaryObjectString.Write(this);
        }

        internal void WriteRectangleArray(/*NameInfo memberNameInfo,*/ NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int rank, int[] lengthA, int[] lowerBoundA)
        {
            this.InternalWriteItemNull();
            BinaryArrayTypeEnum rectangular = BinaryArrayTypeEnum.Rectangular;
            BinaryTypeEnum binaryTypeEnum = BinaryTypeConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo._type, objectInfo, this._objectWriter, out object typeInformation, out int assemId);
            if (this._binaryArray == null)
            {
                this._binaryArray = new BinaryArray();
            }
            int index = 0;
            while (true)
            {
                if (index < rank)
                {
                    if (lowerBoundA[index] == 0)
                    {
                        index++;
                        continue;
                    }
                    rectangular = BinaryArrayTypeEnum.RectangularOffset;
                }
                this._binaryArray.Set((int)arrayNameInfo._objectId, rank, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, rectangular, assemId);
                this._binaryArray.Write(this);
                return;
            }
        }

        internal void WriteSByte(sbyte value)
        {
            this.WriteByte((byte)value);
        }

        internal void WriteSerializationHeader(int topId, int headerId, int minorVersion, int majorVersion)
        {
            new SerializationHeaderRecord(BinaryHeaderEnum.SerializedStreamHeader, topId, headerId, minorVersion, majorVersion).Write(this);
        }

        internal void WriteSerializationHeaderEnd()
        {
            new MessageEnd().Write(this);
        }

        internal void WriteSingle(float value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteSingleArray(/*NameInfo memberNameInfo,*/ NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, Array array)
        {
            BinaryArrayTypeEnum single;
            this.InternalWriteItemNull();
            int[] lengthA = new int[] { length };
            int[] lowerBoundA = null;
            if (lowerBound == 0)
            {
                single = BinaryArrayTypeEnum.Single;
            }
            else
            {
                single = BinaryArrayTypeEnum.SingleOffset;
                lowerBoundA = new int[] { lowerBound };
            }
            BinaryTypeEnum binaryTypeEnum = BinaryTypeConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo._type, objectInfo, this._objectWriter, out object typeInformation, out int num);
            if (this._binaryArray == null)
            {
                this._binaryArray = new BinaryArray();
            }
            this._binaryArray.Set((int)arrayNameInfo._objectId, 1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, single, num);
            this._binaryArray.Write(this);
            if (Converter.IsWriteAsByteArray(arrayElemTypeNameInfo._primitiveTypeEnum) && (lowerBound == 0))
            {
                if (arrayElemTypeNameInfo._primitiveTypeEnum == PrimitiveTypeEnum.Byte)
                {
                    this.WriteBytes((byte[])array);
                }
                else if (arrayElemTypeNameInfo._primitiveTypeEnum == PrimitiveTypeEnum.Char)
                {
                    this.WriteChars((char[])array);
                }
                else
                {
                    this.WriteArrayAsBytes(array, Converter.TypeLength(arrayElemTypeNameInfo._primitiveTypeEnum));
                }
            }
        }

        internal void WriteString(string value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteTimeSpan(TimeSpan value)
        {
            this.WriteInt64(value.Ticks);
        }

        internal void WriteUInt16(ushort value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteUInt32(uint value)
        {
            this._dataWriter.Write(value);
        }

        internal void WriteUInt64(ulong value)
        {
            this._dataWriter.Write(value);
        }

        public void WriteValue(PrimitiveTypeEnum code, object value)
        {
            switch (code)
            {
                case PrimitiveTypeEnum.Boolean:
                    this.WriteBoolean(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Byte:
                    this.WriteByte(Convert.ToByte(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Char:
                    this.WriteChar(Convert.ToChar(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Decimal:
                    this.WriteDecimal(Convert.ToDecimal(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Double:
                    this.WriteDouble(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Int16:
                    this.WriteInt16(Convert.ToInt16(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Int32:
                    this.WriteInt32(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Int64:
                    this.WriteInt64(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.SByte:
                    this.WriteSByte(Convert.ToSByte(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.Single:
                    this.WriteSingle(Convert.ToSingle(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.TimeSpan:
                    this.WriteTimeSpan((TimeSpan)value);
                    return;

                case PrimitiveTypeEnum.DateTime:
                    this.WriteDateTime((DateTime)value);
                    return;

                case PrimitiveTypeEnum.UInt16:
                    this.WriteUInt16(Convert.ToUInt16(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.UInt32:
                    this.WriteUInt32(Convert.ToUInt32(value, CultureInfo.InvariantCulture));
                    return;

                case PrimitiveTypeEnum.UInt64:
                    this.WriteUInt64(Convert.ToUInt64(value, CultureInfo.InvariantCulture));
                    return;
            }
            throw new SerializationException(RemotingResources.SerializationTypeCode.Format(code.ToString()));
        }

        private sealed class ObjectMapInfo
        {
            internal readonly int _objectId;
            private readonly int _numMembers;
            private readonly string[] _memberNames;
            private readonly Type[] _memberTypes;

            internal ObjectMapInfo(int objectId, int numMembers, string[] memberNames, Type[] memberTypes)
            {
                this._objectId = objectId;
                this._numMembers = numMembers;
                this._memberNames = memberNames;
                this._memberTypes = memberTypes;
            }

            internal bool IsCompatible(int numMembers, string[] memberNames, Type[] memberTypes)
            {
                if (this._numMembers != numMembers)
                {
                    return false;
                }
                for (int i = 0; i < numMembers; i++)
                {
                    if (!this._memberNames[i].Equals(memberNames[i]))
                    {
                        return false;
                    }
                    if ((memberTypes != null) && (this._memberTypes[i] != memberTypes[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
