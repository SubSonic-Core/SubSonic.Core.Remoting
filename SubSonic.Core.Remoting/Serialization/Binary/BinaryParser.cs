using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    public sealed class BinaryParser
    {
        private static readonly Encoding s_encoding = new UTF8Encoding(false, true);
        private ObjectReader objectReader;
        private Stream input;
        private long topId;
        private long headerId;
        private SizedArray objectMapIdTable;
        private SizedArray assemIdToAssemblyTable;
        private SerializationStack stack = new SerializationStack("ObjectProgressStack");
        private BinaryTypeEnum expectedType = BinaryTypeEnum.ObjectUrt;
        private object expectedTypeInformation;
        private ParseRecord _prs;
        private BinaryAssemblyInfo _systemAssemblyInfo;
        private BinaryReader dataReader;
        private SerializationStack opPool;
        private BinaryObject _binaryObject;
        private BinaryObjectWithMap _bowm;
        private BinaryObjectWithMapTyped _bowmt;
        private BinaryObjectString _objectString;
        private BinaryCrossAppDomainString _crossAppDomainString;
        private MemberPrimitiveTyped _memberPrimitiveTyped;
        private byte[] _byteBuffer;
        private MemberPrimitiveUnTyped memberPrimitiveUnTyped;
        private MemberReference _memberReference;
        private ObjectNull _objectNull;
        private static volatile MessageEnd _messageEnd;

        public BinaryParser(Stream stream, ObjectReader objectReader)
        {
            this.input = stream ?? throw new ArgumentNullException(nameof(stream));
            this.objectReader = objectReader;
            this.dataReader = new BinaryReader(input, s_encoding);
        }

        private static DateTime FromBinaryRaw(long dateData)
        {
            DateTime time1 = new DateTime(dateData & 0x3fffffffffffffffL);
            return MemoryMarshal.Cast<long, DateTime>(MemoryMarshal.CreateReadOnlySpan(ref dateData, 1))[0];
        }

        private ObjectProgress GetOp()
        {
            ObjectProgress progress = null;
            if (opPool == null || opPool.IsEmpty())
            {
                progress = new ObjectProgress();
            }
            else
            {
                progress = (ObjectProgress)opPool.Pop();
                progress.Init();
            }
            return progress;
        }

        private void PutOp(ObjectProgress op)
        {
            if (opPool == null)
            {
                opPool = new SerStack("opPool");
            }
            opPool.Push(op);
        }

        private void ReadArray(BinaryHeaderEnum binaryHeaderEnum)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            BinaryArray array = new BinaryArray(binaryHeaderEnum);
            array.Read(this);
            if (array._binaryTypeEnum != BinaryTypeEnum.ObjectUser)
            {
                assemblyInfo = SystemAssemblyInfo;
            }
            else
            {
                if (array._assemId < 1)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_AssemblyId, array._typeInformation));
                }
                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[array._assemId];
            }
            ObjectProgress op = GetOp();
            ParseRecord pr = op._pr;
            op._objectTypeEnum = InternalObjectTypeE.Array;
            op._binaryTypeEnum = array._binaryTypeEnum;
            op._typeInformation = array._typeInformation;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || array._objectId > 0)
            {
                op._name = "System.Array";
                pr._parseTypeEnum = InternalParseTypeE.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr._parseTypeEnum = InternalParseTypeE.Member;
                pr._memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                InternalObjectTypeE ee = progress2._objectTypeEnum;
                if (ee == InternalObjectTypeE.Object)
                {
                    pr._name = progress2._name;
                    pr._memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                    pr._keyDt = progress2._name;
                    pr._dtType = progress2._dtType;
                }
                else
                {
                    if (ee != InternalObjectTypeE.Array)
                    {
                        throw new SerializationException(System.SR.Format(System.SR.Serialization_ObjectTypeEnum, progress2._objectTypeEnum.ToString()));
                    }
                    pr._memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            pr._objectId = _objectReader.GetId((long)array._objectId);
            pr._objectPositionEnum = pr._objectId != topId ? headerId <= 0L || pr._objectId != headerId ? ObjectPositionEnum.Child : ObjectPositionEnum.Headers : ObjectPositionEnum.Top;
            pr._objectTypeEnum = InternalObjectTypeE.Array;
            BinaryTypeConverter.TypeFromInfo(array._binaryTypeEnum, array._typeInformation, _objectReader, assemblyInfo, out pr._arrayElementTypeCode, out pr._arrayElementTypeString, out pr._arrayElementType, out pr._isArrayVariant);
            pr._dtTypeCode = PrimitiveTypeEnum.Invalid;
            pr._rank = array._rank;
            pr._lengthA = array._lengthA;
            pr._lowerBoundA = array._lowerBoundA;
            bool flag = false;
            switch (array._binaryArrayTypeEnum)
            {
                case BinaryArrayTypeEnum.Single:
                case BinaryArrayTypeEnum.SingleOffset:
                    op._numItems = array._lengthA[0];
                    pr._arrayTypeEnum = InternalArrayTypeE.Single;
                    if (Converter.IsWriteAsByteArray(pr._arrayElementTypeCode) && array._lowerBoundA[0] == 0)
                    {
                        flag = true;
                        ReadArrayAsBytes(pr);
                    }
                    break;

                case BinaryArrayTypeEnum.Jagged:
                case BinaryArrayTypeEnum.JaggedOffset:
                    op._numItems = array._lengthA[0];
                    pr._arrayTypeEnum = InternalArrayTypeE.Jagged;
                    break;

                case BinaryArrayTypeEnum.Rectangular:
                case BinaryArrayTypeEnum.RectangularOffset:
                    {
                        int num = 1;
                        int index = 0;
                        while (true)
                        {
                            if (index >= array._rank)
                            {
                                op._numItems = num;
                                pr._arrayTypeEnum = InternalArrayTypeE.Rectangular;
                                break;
                            }
                            num *= array._lengthA[index];
                            index++;
                        }
                        break;
                    }
                default:
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_ArrayType, array._binaryArrayTypeEnum.ToString()));
            }
            if (!flag)
            {
                stack.Push(op);
            }
            else
            {
                PutOp(op);
            }
            _objectReader.Parse(pr);
            if (flag)
            {
                pr._parseTypeEnum = InternalParseTypeE.ObjectEnd;
                _objectReader.Parse(pr);
            }
        }

        private void ReadArrayAsBytes(ParseRecord pr)
        {
            if (pr._arrayElementTypeCode == PrimitiveTypeEnum.Byte)
            {
                pr._newObj = this.ReadBytes(pr._lengthA[0]);
            }
            else if (pr._arrayElementTypeCode == PrimitiveTypeEnum.Char)
            {
                pr._newObj = this.ReadChars(pr._lengthA[0]);
            }
            else
            {
                int num = Converter.TypeLength(pr._arrayElementTypeCode);
                pr._newObj = Converter.CreatePrimitiveArray(pr._arrayElementTypeCode, pr._lengthA[0]);
                Array dst = (Array)pr._newObj;
                int num2 = 0;
                if (_byteBuffer == null)
                {
                    _byteBuffer = new byte[0x1000];
                }
                while (num2 < dst.Length)
                {
                    int num3 = Math.Min(0x1000 / num, dst.Length - num2);
                    int size = num3 * num;
                    ReadBytes(_byteBuffer, 0, size);
                    if (!BitConverter.IsLittleEndian)
                    {
                        int num5 = 0;
                        while (num5 < size)
                        {
                            int num6 = 0;
                            while (true)
                            {
                                if (num6 >= num / 2)
                                {
                                    num5 += num;
                                    break;
                                }
                                byte num7 = _byteBuffer[num5 + num6];
                                _byteBuffer[num5 + num6] = _byteBuffer[num5 + num - 1 - num6];
                                _byteBuffer[num5 + num - 1 - num6] = num7;
                                num6++;
                            }
                        }
                    }
                    Buffer.BlockCopy(_byteBuffer, 0, dst, num2 * num, size);
                    num2 += num3;
                }
            }
        }

        internal void ReadAssembly(BinaryHeaderEnum binaryHeaderEnum)
        {
            BinaryAssembly assembly = new BinaryAssembly();
            if (binaryHeaderEnum != BinaryHeaderEnum.CrossAppDomainAssembly)
            {
                assembly.Read(this);
            }
            else
            {
                BinaryCrossAppDomainAssembly assembly2 = new BinaryCrossAppDomainAssembly();
                assembly2.Read(this);
                assembly._assemId = assembly2._assemId;
                assembly._assemblyString = _objectReader.CrossAppDomainArray(assembly2._assemblyIndex) as string;
                if (assembly._assemblyString == null)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_CrossAppDomainError, "String", (int)assembly2._assemblyIndex));
                }
            }
            AssemIdToAssemblyTable[assembly._assemId] = new BinaryAssemblyInfo(assembly._assemblyString);
        }

        internal void ReadBegin()
        {
        }

        internal bool ReadBoolean()
        {
            return dataReader.ReadBoolean();
        }

        internal byte ReadByte()
        {
            return dataReader.ReadByte();
        }

        internal byte[] ReadBytes(int length)
        {
            return dataReader.ReadBytes(length);
        }

        internal void ReadBytes(byte[] byteA, int offset, int size)
        {
            while (size > 0)
            {
                int num = dataReader.Read(byteA, offset, size);
                if (num == 0)
                {
                    throw new EndOfStreamException(System.SR.IO_EOF_ReadBeyondEOF);
                }
                offset += num;
                size -= num;
            }
        }

        internal char ReadChar()
        {
            return dataReader.ReadChar();
        }

        internal char[] ReadChars(int length)
        {
            return dataReader.ReadChars(length);
        }

        internal void ReadCrossAppDomainMap()
        {
            BinaryCrossAppDomainMap map = new BinaryCrossAppDomainMap();
            map.Read(this);
            object obj2 = _objectReader.CrossAppDomainArray(map._crossAppDomainArrayIndex);
            BinaryObjectWithMap record = obj2 as BinaryObjectWithMap;
            if (record != null)
            {
                ReadObjectWithMap(record);
            }
            else
            {
                BinaryObjectWithMapTyped typed = obj2 as BinaryObjectWithMapTyped;
                if (typed == null)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_CrossAppDomainError, "BinaryObjectMap", obj2));
                }
                ReadObjectWithMapTyped(typed);
            }
        }

        internal DateTime ReadDateTime()
        {
            return FromBinaryRaw(ReadInt64());
        }

        internal decimal ReadDecimal()
        {
            return decimal.Parse(dataReader.ReadString(), CultureInfo.InvariantCulture);
        }

        internal double ReadDouble()
        {
            return dataReader.ReadDouble();
        }

        internal void ReadEnd()
        {
        }

        internal short ReadInt16()
        {
            return dataReader.ReadInt16();
        }

        internal int ReadInt32()
        {
            return dataReader.ReadInt32();
        }

        internal long ReadInt64()
        {
            return dataReader.ReadInt64();
        }

        private void ReadMemberPrimitiveTyped()
        {
            if (_memberPrimitiveTyped == null)
            {
                _memberPrimitiveTyped = new MemberPrimitiveTyped();
            }
            _memberPrimitiveTyped.Read(this);
            PRs._objectTypeEnum = InternalObjectTypeE.Object;
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            PRs.Init();
            PRs._varValue = _memberPrimitiveTyped._value;
            PRs._keyDt = Converter.ToComType(_memberPrimitiveTyped._primitiveTypeEnum);
            PRs._dtType = Converter.ToType(_memberPrimitiveTyped._primitiveTypeEnum);
            PRs._dtTypeCode = _memberPrimitiveTyped._primitiveTypeEnum;
            if (progress == null)
            {
                PRs._parseTypeEnum = InternalParseTypeE.Object;
                PRs._name = "System.Variant";
            }
            else
            {
                PRs._parseTypeEnum = InternalParseTypeE.Member;
                PRs._memberValueEnum = MemberValueEnum.InlineValue;
                InternalObjectTypeE ee = progress._objectTypeEnum;
                if (ee == InternalObjectTypeE.Object)
                {
                    PRs._name = progress._name;
                    PRs._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != InternalObjectTypeE.Array)
                    {
                        throw new SerializationException(System.SR.Format(System.SR.Serialization_ObjectTypeEnum, progress._objectTypeEnum.ToString()));
                    }
                    PRs._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            _objectReader.Parse(PRs);
        }

        private void ReadMemberPrimitiveUnTyped()
        {
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            if (memberPrimitiveUnTyped == null)
            {
                memberPrimitiveUnTyped = new MemberPrimitiveUnTyped();
            }
            memberPrimitiveUnTyped.Set((PrimitiveTypeEnum)expectedTypeInformation);
            memberPrimitiveUnTyped.Read(this);
            PRs.Init();
            PRs._varValue = memberPrimitiveUnTyped._value;
            PRs._dtTypeCode = (PrimitiveTypeEnum)expectedTypeInformation;
            PRs._dtType = Converter.ToType(PRs._dtTypeCode);
            PRs._parseTypeEnum = InternalParseTypeE.Member;
            PRs._memberValueEnum = MemberValueEnum.InlineValue;
            if (progress._objectTypeEnum != InternalObjectTypeE.Object)
            {
                PRs._memberTypeEnum = MemberTypeEnum.Item;
            }
            else
            {
                PRs._memberTypeEnum = MemberTypeEnum.Field;
                PRs._name = progress._name;
            }
            _objectReader.Parse(PRs);
        }

        private void ReadMemberReference()
        {
            if (_memberReference == null)
            {
                _memberReference = new MemberReference();
            }
            _memberReference.Read(this);
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            PRs.Init();
            PRs._idRef = _objectReader.GetId((long)_memberReference._idRef);
            PRs._parseTypeEnum = InternalParseTypeE.Member;
            PRs._memberValueEnum = MemberValueEnum.Reference;
            if (progress._objectTypeEnum != InternalObjectTypeE.Object)
            {
                PRs._memberTypeEnum = MemberTypeEnum.Item;
            }
            else
            {
                PRs._memberTypeEnum = MemberTypeEnum.Field;
                PRs._name = progress._name;
                PRs._dtType = progress._dtType;
            }
            _objectReader.Parse(PRs);
        }

        private void ReadMessageEnd()
        {
            if (_messageEnd == null)
            {
                _messageEnd = new MessageEnd();
            }
            _messageEnd.Read(this);
            if (!stack.IsEmpty())
            {
                throw new SerializationException(System.SR.Serialization_StreamEnd);
            }
        }

        private void ReadObject()
        {
            if (_binaryObject == null)
            {
                _binaryObject = new BinaryObject();
            }
            _binaryObject.Read(this);
            ObjectMap map = (ObjectMap)ObjectMapIdTable[_binaryObject.mapId];
            if (map == null)
            {
                throw new SerializationException(System.SR.Format(System.SR.Serialization_Map, (int)_binaryObject.mapId));
            }
            ObjectProgress op = GetOp();
            ParseRecord pr = op._pr;
            stack.Push(op);
            op._objectTypeEnum = InternalObjectTypeE.Object;
            op._binaryTypeEnumA = map._binaryTypeEnumA;
            op._memberNames = map._memberNames;
            op._memberTypes = map._memberTypes;
            op._typeInformationA = map._typeInformationA;
            op._memberLength = op._binaryTypeEnumA.Length;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || progress2._isInitial)
            {
                op._name = map._objectName;
                pr._parseTypeEnum = InternalParseTypeE.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr._parseTypeEnum = InternalParseTypeE.Member;
                pr._memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                InternalObjectTypeE ee = progress2._objectTypeEnum;
                if (ee == InternalObjectTypeE.Object)
                {
                    pr._name = progress2._name;
                    pr._memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != InternalObjectTypeE.Array)
                    {
                        throw new SerializationException(System.SR.Format(System.SR.Serialization_Map, progress2._objectTypeEnum.ToString()));
                    }
                    pr._memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            pr._objectId = _objectReader.GetId((long)_binaryObject.objectId);
            pr._objectInfo = map.CreateObjectInfo(ref pr._si, ref pr._memberData);
            if (pr._objectId == topId)
            {
                pr._objectPositionEnum = ObjectPositionEnum.Top;
            }
            pr._objectTypeEnum = InternalObjectTypeE.Object;
            pr._keyDt = map._objectName;
            pr._dtType = map._objectType;
            pr._dtTypeCode = PrimitiveTypeEnum.Invalid;
            _objectReader.Parse(pr);
        }

        private void ReadObjectNull(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (_objectNull == null)
            {
                _objectNull = new ObjectNull();
            }
            _objectNull.Read(this, binaryHeaderEnum);
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            PRs.Init();
            PRs._parseTypeEnum = InternalParseTypeE.Member;
            PRs._memberValueEnum = MemberValueEnum.Null;
            if (progress._objectTypeEnum == InternalObjectTypeE.Object)
            {
                PRs._memberTypeEnum = MemberTypeEnum.Field;
                PRs._name = progress._name;
                PRs._dtType = progress._dtType;
            }
            else
            {
                PRs._memberTypeEnum = MemberTypeEnum.Item;
                PRs._consecutiveNullArrayEntryCount = _objectNull._nullCount;
                progress.ArrayCountIncrement(_objectNull._nullCount - 1);
            }
            _objectReader.Parse(PRs);
        }

        private void ReadObjectString(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (_objectString == null)
            {
                _objectString = new BinaryObjectString();
            }
            if (binaryHeaderEnum == BinaryHeaderEnum.ObjectString)
            {
                _objectString.Read(this);
            }
            else
            {
                if (_crossAppDomainString == null)
                {
                    _crossAppDomainString = new BinaryCrossAppDomainString();
                }
                _crossAppDomainString.Read(this);
                _objectString._value = _objectReader.CrossAppDomainArray(_crossAppDomainString._value) as string;
                if (_objectString._value == null)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_CrossAppDomainError, "String", (int)_crossAppDomainString._value));
                }
                _objectString._objectId = _crossAppDomainString._objectId;
            }
            PRs.Init();
            PRs._parseTypeEnum = InternalParseTypeE.Object;
            PRs._objectId = _objectReader.GetId((long)_objectString._objectId);
            if (PRs._objectId == topId)
            {
                PRs._objectPositionEnum = ObjectPositionEnum.Top;
            }
            PRs._objectTypeEnum = InternalObjectTypeE.Object;
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            PRs._value = _objectString._value;
            PRs._keyDt = "System.String";
            PRs._dtType = Converter.s_typeofString;
            PRs._dtTypeCode = PrimitiveTypeEnum.Invalid;
            PRs._varValue = _objectString._value;
            if (progress == null)
            {
                PRs._parseTypeEnum = InternalParseTypeE.Object;
                PRs._name = "System.String";
            }
            else
            {
                PRs._parseTypeEnum = InternalParseTypeE.Member;
                PRs._memberValueEnum = MemberValueEnum.InlineValue;
                InternalObjectTypeE ee = progress._objectTypeEnum;
                if (ee == InternalObjectTypeE.Object)
                {
                    PRs._name = progress._name;
                    PRs._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != InternalObjectTypeE.Array)
                    {
                        throw new SerializationException(System.SR.Format(System.SR.Serialization_ObjectTypeEnum, progress._objectTypeEnum.ToString()));
                    }
                    PRs._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            _objectReader.Parse(PRs);
        }

        internal void ReadObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (_bowm == null)
            {
                _bowm = new BinaryObjectWithMap(binaryHeaderEnum);
            }
            else
            {
                _bowm.binaryHeaderEnum = binaryHeaderEnum;
            }
            _bowm.Read(this);
            ReadObjectWithMap(_bowm);
        }

        private void ReadObjectWithMap(BinaryObjectWithMap record)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            ObjectProgress op = GetOp();
            ParseRecord pr = op._pr;
            stack.Push(op);
            if (record.binaryHeaderEnum != BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMap)
                {
                    assemblyInfo = SystemAssemblyInfo;
                }
            }
            else
            {
                if (record.assemId < 1)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_Assembly, record.name));
                }
                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record.assemId];
                if (assemblyInfo == null)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_Assembly, ((int)record.assemId).ToString() + " " + record.name));
                }
            }
            Type objectType = _objectReader.GetType(assemblyInfo, record.name);
            ObjectMap map = ObjectMap.Create(record.name, objectType, record.memberNames, _objectReader, record._objectId, assemblyInfo);
            ObjectMapIdTable[record._objectId] = map;
            op._objectTypeEnum = InternalObjectTypeE.Object;
            op._binaryTypeEnumA = map._binaryTypeEnumA;
            op._typeInformationA = map._typeInformationA;
            op._memberLength = op._binaryTypeEnumA.Length;
            op._memberNames = map._memberNames;
            op._memberTypes = map._memberTypes;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || progress2._isInitial)
            {
                op._name = record.name;
                pr._parseTypeEnum = InternalParseTypeE.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr._parseTypeEnum = InternalParseTypeE.Member;
                pr._memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                InternalObjectTypeE ee = progress2._objectTypeEnum;
                if (ee == InternalObjectTypeE.Object)
                {
                    pr._name = progress2._name;
                    pr._memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != InternalObjectTypeE.Array)
                    {
                        throw new SerializationException(System.SR.Format(System.SR.Serialization_ObjectTypeEnum, progress2._objectTypeEnum.ToString()));
                    }
                    pr._memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
            }
            pr._objectTypeEnum = InternalObjectTypeE.Object;
            pr._objectId = _objectReader.GetId((long)record._objectId);
            pr._objectInfo = map.CreateObjectInfo(ref pr._si, ref pr._memberData);
            if (pr._objectId == topId)
            {
                pr._objectPositionEnum = ObjectPositionEnum.Top;
            }
            pr._keyDt = record.name;
            pr._dtType = map._objectType;
            pr._dtTypeCode = PrimitiveTypeEnum.Invalid;
            _objectReader.Parse(pr);
        }

        internal void ReadObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (_bowmt == null)
            {
                _bowmt = new BinaryObjectWithMapTyped(binaryHeaderEnum);
            }
            else
            {
                _bowmt._binaryHeaderEnum = binaryHeaderEnum;
            }
            _bowmt.Read(this);
            ReadObjectWithMapTyped(_bowmt);
        }

        private void ReadObjectWithMapTyped(BinaryObjectWithMapTyped record)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            ObjectProgress op = GetOp();
            ParseRecord pr = op._pr;
            stack.Push(op);
            if (record._binaryHeaderEnum != BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                if (record._binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTyped)
                {
                    assemblyInfo = SystemAssemblyInfo;
                }
            }
            else
            {
                if (record._assemId < 1)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_AssemblyId, record._name));
                }
                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record._assemId];
                if (assemblyInfo == null)
                {
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_AssemblyId, ((int)record._assemId).ToString() + " " + record._name));
                }
            }
            ObjectMap map = ObjectMap.Create(record._name, record._memberNames, record._binaryTypeEnumA, record._typeInformationA, record._memberAssemIds, _objectReader, record._objectId, assemblyInfo, AssemIdToAssemblyTable);
            ObjectMapIdTable[record._objectId] = map;
            op._objectTypeEnum = InternalObjectTypeE.Object;
            op._binaryTypeEnumA = map._binaryTypeEnumA;
            op._typeInformationA = map._typeInformationA;
            op._memberLength = op._binaryTypeEnumA.Length;
            op._memberNames = map._memberNames;
            op._memberTypes = map._memberTypes;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || progress2._isInitial)
            {
                op._name = record._name;
                pr._parseTypeEnum = InternalParseTypeE.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr._parseTypeEnum = InternalParseTypeE.Member;
                pr._memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                InternalObjectTypeE ee = progress2._objectTypeEnum;
                if (ee == InternalObjectTypeE.Object)
                {
                    pr._name = progress2._name;
                    pr._memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != InternalObjectTypeE.Array)
                    {
                        throw new SerializationException(System.SR.Format(System.SR.Serialization_ObjectTypeEnum, progress2._objectTypeEnum.ToString()));
                    }
                    pr._memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            pr._objectTypeEnum = InternalObjectTypeE.Object;
            pr._objectInfo = map.CreateObjectInfo(ref pr._si, ref pr._memberData);
            pr._objectId = _objectReader.GetId((long)record._objectId);
            if (pr._objectId == topId)
            {
                pr._objectPositionEnum = ObjectPositionEnum.Top;
            }
            pr._keyDt = record._name;
            pr._dtType = map._objectType;
            pr._dtTypeCode = PrimitiveTypeEnum.Invalid;
            _objectReader.Parse(pr);
        }

        internal sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        internal void ReadSerializationHeaderRecord()
        {
            SerializationHeaderRecord record = new SerializationHeaderRecord();
            record.Read(this);
            topId = record._topId > 0 ? _objectReader.GetId((long)record._topId) : (long)record._topId;
            headerId = record._headerId > 0 ? _objectReader.GetId((long)record._headerId) : (long)record._headerId;
        }

        internal float ReadSingle()
        {
            return dataReader.ReadSingle();
        }

        internal string ReadString()
        {
            return dataReader.ReadString();
        }

        internal TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(ReadInt64());
        }

        internal ushort ReadUInt16()
        {
            return dataReader.ReadUInt16();
        }

        internal uint ReadUInt32()
        {
            return dataReader.ReadUInt32();
        }

        internal ulong ReadUInt64()
        {
            return dataReader.ReadUInt64();
        }

        internal object ReadValue(PrimitiveTypeEnum code)
        {
            object obj2 = null;
            switch (code)
            {
                case PrimitiveTypeEnum.Boolean:
                    obj2 = ReadBoolean();
                    break;

                case PrimitiveTypeEnum.Byte:
                    obj2 = ReadByte();
                    break;

                case PrimitiveTypeEnum.Char:
                    obj2 = ReadChar();
                    break;

                case PrimitiveTypeEnum.Decimal:
                    obj2 = ReadDecimal();
                    break;

                case PrimitiveTypeEnum.Double:
                    obj2 = (double)ReadDouble();
                    break;

                case PrimitiveTypeEnum.Int16:
                    obj2 = ReadInt16();
                    break;

                case PrimitiveTypeEnum.Int32:
                    obj2 = ReadInt32();
                    break;

                case PrimitiveTypeEnum.Int64:
                    obj2 = ReadInt64();
                    break;

                case PrimitiveTypeEnum.SByte:
                    obj2 = ReadSByte();
                    break;

                case PrimitiveTypeEnum.Single:
                    obj2 = (float)ReadSingle();
                    break;

                case PrimitiveTypeEnum.TimeSpan:
                    obj2 = ReadTimeSpan();
                    break;

                case PrimitiveTypeEnum.DateTime:
                    obj2 = ReadDateTime();
                    break;

                case PrimitiveTypeEnum.UInt16:
                    obj2 = ReadUInt16();
                    break;

                case PrimitiveTypeEnum.UInt32:
                    obj2 = ReadUInt32();
                    break;

                case PrimitiveTypeEnum.UInt64:
                    obj2 = ReadUInt64();
                    break;

                default:
                    throw new SerializationException(System.SR.Format(System.SR.Serialization_TypeCode, code.ToString()));
            }
            return obj2;
        }

        internal void Run()
        {
            try
            {
                bool flag = true;
                ReadBegin();
                ReadSerializationHeaderRecord();
                while (flag)
                {
                    BinaryHeaderEnum binaryHeaderEnum = BinaryHeaderEnum.Object;
                    BinaryTypeEnum enum3 = expectedType;
                    if (enum3 == BinaryTypeEnum.Primitive)
                    {
                        ReadMemberPrimitiveUnTyped();
                    }
                    else
                    {
                        if (enum3 - (BinaryTypeEnum)1 > BinaryTypeEnum.StringArray)
                        {
                            throw new SerializationException(System.SR.Serialization_TypeExpected);
                        }
                        byte num = dataReader.ReadByte();
                        binaryHeaderEnum = (BinaryHeaderEnum)num;
                        switch (binaryHeaderEnum)
                        {
                            case BinaryHeaderEnum.Object:
                                ReadObject();
                                break;

                            case BinaryHeaderEnum.ObjectWithMap:
                            case BinaryHeaderEnum.ObjectWithMapAssemId:
                                ReadObjectWithMap(binaryHeaderEnum);
                                break;

                            case BinaryHeaderEnum.ObjectWithMapTyped:
                            case BinaryHeaderEnum.ObjectWithMapTypedAssemId:
                                ReadObjectWithMapTyped(binaryHeaderEnum);
                                break;

                            case BinaryHeaderEnum.ObjectString:
                            case BinaryHeaderEnum.CrossAppDomainString:
                                ReadObjectString(binaryHeaderEnum);
                                break;

                            case BinaryHeaderEnum.Array:
                            case BinaryHeaderEnum.ArraySinglePrimitive:
                            case BinaryHeaderEnum.ArraySingleObject:
                            case BinaryHeaderEnum.ArraySingleString:
                                ReadArray(binaryHeaderEnum);
                                break;

                            case BinaryHeaderEnum.MemberPrimitiveTyped:
                                ReadMemberPrimitiveTyped();
                                break;

                            case BinaryHeaderEnum.MemberReference:
                                ReadMemberReference();
                                break;

                            case BinaryHeaderEnum.ObjectNull:
                            case BinaryHeaderEnum.ObjectNullMultiple256:
                            case BinaryHeaderEnum.ObjectNullMultiple:
                                ReadObjectNull(binaryHeaderEnum);
                                break;

                            case BinaryHeaderEnum.MessageEnd:
                                flag = false;
                                ReadMessageEnd();
                                ReadEnd();
                                break;

                            case BinaryHeaderEnum.Assembly:
                            case BinaryHeaderEnum.CrossAppDomainAssembly:
                                ReadAssembly(binaryHeaderEnum);
                                break;

                            case BinaryHeaderEnum.CrossAppDomainMap:
                                ReadCrossAppDomainMap();
                                break;

                            default:
                                throw new SerializationException(System.SR.Format(System.SR.Serialization_BinaryHeader, num));
                        }
                    }
                    if (binaryHeaderEnum != BinaryHeaderEnum.Assembly)
                    {
                        bool next = false;
                        while (!next)
                        {
                            ObjectProgress op = (ObjectProgress)stack.Peek();
                            if (op == null)
                            {
                                expectedType = BinaryTypeEnum.ObjectUrt;
                                expectedTypeInformation = null;
                                next = true;
                                continue;
                            }
                            next = op.GetNext(out op._expectedType, out op._expectedTypeInformation);
                            expectedType = op._expectedType;
                            expectedTypeInformation = op._expectedTypeInformation;
                            if (!next)
                            {
                                PRs.Init();
                                if (op._memberValueEnum == MemberValueEnum.Nested)
                                {
                                    PRs._parseTypeEnum = InternalParseTypeE.MemberEnd;
                                    PRs._memberTypeEnum = op._memberTypeEnum;
                                    PRs._memberValueEnum = op._memberValueEnum;
                                    _objectReader.Parse(PRs);
                                }
                                else
                                {
                                    PRs._parseTypeEnum = InternalParseTypeE.ObjectEnd;
                                    PRs._memberTypeEnum = op._memberTypeEnum;
                                    PRs._memberValueEnum = op._memberValueEnum;
                                    _objectReader.Parse(PRs);
                                }
                                stack.Pop();
                                PutOp(op);
                            }
                        }
                    }
                }
            }
            catch (EndOfStreamException)
            {
                throw new SerializationException(System.SR.Serialization_StreamEnd);
            }
        }

        internal BinaryAssemblyInfo SystemAssemblyInfo
        {
            get
            {
                BinaryAssemblyInfo info2 = _systemAssemblyInfo;
                if (_systemAssemblyInfo == null)
                {
                    BinaryAssemblyInfo local1 = _systemAssemblyInfo;
                    info2 = _systemAssemblyInfo = new BinaryAssemblyInfo(Converter.s_urtAssemblyString, Converter.s_urtAssembly);
                }
                return info2;
            }
        }

        internal SizedArray ObjectMapIdTable
        {
            get
            {
                SizedArray array2 = objectMapIdTable;
                if (objectMapIdTable == null)
                {
                    SizedArray local1 = objectMapIdTable;
                    array2 = objectMapIdTable = new SizedArray();
                }
                return array2;
            }
        }

        internal SizedArray AssemIdToAssemblyTable
        {
            get
            {
                SizedArray array2 = assemIdToAssemblyTable;
                if (assemIdToAssemblyTable == null)
                {
                    SizedArray local1 = assemIdToAssemblyTable;
                    array2 = assemIdToAssemblyTable = new SizedArray(2);
                }
                return array2;
            }
        }

        internal ParseRecord PRs
        {
            get
            {
                ParseRecord record2 = _prs;
                if (_prs == null)
                {
                    ParseRecord local1 = _prs;
                    record2 = _prs = new ParseRecord();
                }
                return record2;
            }
        }
    }
}

