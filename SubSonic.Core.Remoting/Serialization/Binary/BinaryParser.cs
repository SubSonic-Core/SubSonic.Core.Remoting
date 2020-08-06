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
            unsafe
            {
                DateTime time1 = new DateTime(dateData & 0x3fffffffffffffffL);

                return MemoryMarshal.Cast<long, DateTime>(new ReadOnlySpan<long>(&dateData, 1))[0];
            }
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
                opPool = new SerializationStack("opPool");
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
                    throw new SerializationException(RemotingResources.SerializationAssemblyId.Format(array._typeInformation));
                }
                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[array._assemId];
            }
            ObjectProgress op = GetOp();
            ParseRecord pr = op._pr;
            op._objectTypeEnum = ObjectTypeEnum.Array;
            op._binaryTypeEnum = array._binaryTypeEnum;
            op._typeInformation = array._typeInformation;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || array.ObjectId > 0)
            {
                op._name = "System.Array";
                pr.parseTypeEnum = ParseTypeEnum.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr.parseTypeEnum = ParseTypeEnum.Member;
                pr.memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                ObjectTypeEnum ee = progress2._objectTypeEnum;
                if (ee == ObjectTypeEnum.Object)
                {
                    pr.name = progress2._name;
                    pr.memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                    pr.keyDt = progress2._name;
                    pr.dtType = progress2._dtType;
                }
                else
                {
                    if (ee != ObjectTypeEnum.Array)
                    {
                        throw new SerializationException(RemotingResources.SerializationObjectTypeEnum.Format(progress2._objectTypeEnum.ToString()));
                    }
                    pr.memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            pr.objectId = objectReader.GetId((long)array.ObjectId);
            pr.objectPositionEnum = pr.objectId != topId ? headerId <= 0L || pr.objectId != headerId ? ObjectPositionEnum.Child : ObjectPositionEnum.Headers : ObjectPositionEnum.Top;
            pr.objectTypeEnum = ObjectTypeEnum.Array;
            BinaryTypeConverter.TypeFromInfo(array._binaryTypeEnum, array._typeInformation, objectReader, assemblyInfo, out pr.arrayElementTypeCode, out pr.arrayElementTypeString, out pr.arrayElementType, out pr.isArrayVariant);
            pr.dtTypeCode = PrimitiveTypeEnum.Invalid;
            pr.rank = array._rank;
            pr.lengthA = array._lengthA;
            pr.lowerBoundA = array._lowerBoundA;
            bool flag = false;
            switch (array._binaryArrayTypeEnum)
            {
                case BinaryArrayTypeEnum.Single:
                case BinaryArrayTypeEnum.SingleOffset:
                    op._numItems = array._lengthA[0];
                    pr.arrayTypeEnum = ArrayTypeEnum.Single;
                    if (Converter.IsWriteAsByteArray(pr.arrayElementTypeCode) && array._lowerBoundA[0] == 0)
                    {
                        flag = true;
                        ReadArrayAsBytes(pr);
                    }
                    break;

                case BinaryArrayTypeEnum.Jagged:
                case BinaryArrayTypeEnum.JaggedOffset:
                    op._numItems = array._lengthA[0];
                    pr.arrayTypeEnum = ArrayTypeEnum.Jagged;
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
                                pr.arrayTypeEnum = ArrayTypeEnum.Rectangular;
                                break;
                            }
                            num *= array._lengthA[index];
                            index++;
                        }
                        break;
                    }
                default:
                    throw new SerializationException(RemotingResources.SerializationArrayType.Format(array._binaryArrayTypeEnum.ToString()));
            }
            if (!flag)
            {
                stack.Push(op);
            }
            else
            {
                PutOp(op);
            }
            objectReader.Parse(pr);
            if (flag)
            {
                pr.parseTypeEnum = ParseTypeEnum.ObjectEnd;
                objectReader.Parse(pr);
            }
        }

        private void ReadArrayAsBytes(ParseRecord pr)
        {
            if (pr.arrayElementTypeCode == PrimitiveTypeEnum.Byte)
            {
                pr.newObj = this.ReadBytes(pr.lengthA[0]);
            }
            else if (pr.arrayElementTypeCode == PrimitiveTypeEnum.Char)
            {
                pr.newObj = this.ReadChars(pr.lengthA[0]);
            }
            else
            {
                int num = Converter.TypeLength(pr.arrayElementTypeCode);
                pr.newObj = Converter.CreatePrimitiveArray(pr.arrayElementTypeCode, pr.lengthA[0]);
                Array dst = (Array)pr.newObj;
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
                assembly._assemblyString = objectReader.CrossAppDomainArrayAt(assembly2._assemblyIndex) as string;
                if (assembly._assemblyString == null)
                {
                    throw new SerializationException(RemotingResources.SerializationCrossDomainError.Format(nameof(String), (int)assembly2._assemblyIndex));
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
                    throw new EndOfStreamException(RemotingResources.SerializationReadBeyondEOF);
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
            object obj2 = objectReader.CrossAppDomainArrayAt(map._crossAppDomainArrayIndex);
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
                    throw new SerializationException(RemotingResources.SerializationCrossDomainError.Format("BinaryObjectMap", obj2));
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
            PRs.objectTypeEnum = ObjectTypeEnum.Object;
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            PRs.Init();
            PRs.varValue = _memberPrimitiveTyped.Value;
            PRs.keyDt = Converter.ToComType(_memberPrimitiveTyped.PrimitiveTypeEnum);
            PRs.dtType = Converter.ToType(_memberPrimitiveTyped.PrimitiveTypeEnum);
            PRs.dtTypeCode = _memberPrimitiveTyped.PrimitiveTypeEnum;
            if (progress == null)
            {
                PRs.parseTypeEnum = ParseTypeEnum.Object;
                PRs.name = "System.Variant";
            }
            else
            {
                PRs.parseTypeEnum = ParseTypeEnum.Member;
                PRs.memberValueEnum = MemberValueEnum.InlineValue;
                ObjectTypeEnum ee = progress._objectTypeEnum;
                if (ee == ObjectTypeEnum.Object)
                {
                    PRs.name = progress._name;
                    PRs.memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != ObjectTypeEnum.Array)
                    {
                        throw new SerializationException(RemotingResources.SerializationObjectTypeEnum.Format(progress._objectTypeEnum.ToString()));
                    }
                    PRs.memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            objectReader.Parse(PRs);
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
            PRs.varValue = memberPrimitiveUnTyped._value;
            PRs.dtTypeCode = (PrimitiveTypeEnum)expectedTypeInformation;
            PRs.dtType = Converter.ToType(PRs.dtTypeCode);
            PRs.parseTypeEnum = ParseTypeEnum.Member;
            PRs.memberValueEnum = MemberValueEnum.InlineValue;
            if (progress._objectTypeEnum != ObjectTypeEnum.Object)
            {
                PRs.memberTypeEnum = MemberTypeEnum.Item;
            }
            else
            {
                PRs.memberTypeEnum = MemberTypeEnum.Field;
                PRs.name = progress._name;
            }
            objectReader.Parse(PRs);
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
            PRs.idRef = objectReader.GetId((long)_memberReference._idRef);
            PRs.parseTypeEnum = ParseTypeEnum.Member;
            PRs.memberValueEnum = MemberValueEnum.Reference;
            if (progress._objectTypeEnum != ObjectTypeEnum.Object)
            {
                PRs.memberTypeEnum = MemberTypeEnum.Item;
            }
            else
            {
                PRs.memberTypeEnum = MemberTypeEnum.Field;
                PRs.name = progress._name;
                PRs.dtType = progress._dtType;
            }
            objectReader.Parse(PRs);
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
                throw new SerializationException(RemotingResources.SerializationStreamEnd);
            }
        }

        private void ReadObject()
        {
            if (_binaryObject == null)
            {
                _binaryObject = new BinaryObject();
            }
            _binaryObject.Read(this);
            ObjectMap map = (ObjectMap)ObjectMapIdTable[_binaryObject.MapId];
            if (map == null)
            {
                throw new SerializationException(RemotingResources.SerializationMap.Format((int)_binaryObject.MapId));
            }
            ObjectProgress op = GetOp();
            ParseRecord pr = op._pr;
            stack.Push(op);
            op._objectTypeEnum = ObjectTypeEnum.Object;
            op._binaryTypeEnumA = map._binaryTypeEnumA;
            op._memberNames = map._memberNames;
            op._memberTypes = map._memberTypes;
            op._typeInformationA = map._typeInformationA;
            op._memberLength = op._binaryTypeEnumA.Length;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || progress2._isInitial)
            {
                op._name = map._objectName;
                pr.parseTypeEnum = ParseTypeEnum.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr.parseTypeEnum = ParseTypeEnum.Member;
                pr.memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                ObjectTypeEnum ee = progress2._objectTypeEnum;
                if (ee == ObjectTypeEnum.Object)
                {
                    pr.name = progress2._name;
                    pr.memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != ObjectTypeEnum.Array)
                    {
                        throw new SerializationException(RemotingResources.SerializationMap.Format(progress2._objectTypeEnum.ToString()));
                    }
                    pr.memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            pr.objectId = objectReader.GetId((long)_binaryObject.ObjectId);
            pr.objectInfo = map.CreateObjectInfo(ref pr.si, ref pr.memberData);
            if (pr.objectId == topId)
            {
                pr.objectPositionEnum = ObjectPositionEnum.Top;
            }
            pr.objectTypeEnum = ObjectTypeEnum.Object;
            pr.keyDt = map._objectName;
            pr.dtType = map._objectType;
            pr.dtTypeCode = PrimitiveTypeEnum.Invalid;
            objectReader.Parse(pr);
        }

        private void ReadObjectNull(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (_objectNull == null)
            {
                _objectNull = new ObjectNull();
            }
            _objectNull.Set(_objectNull.NullCount, binaryHeaderEnum);
            _objectNull.Read(this);
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            PRs.Init();
            PRs.parseTypeEnum = ParseTypeEnum.Member;
            PRs.memberValueEnum = MemberValueEnum.Null;
            if (progress._objectTypeEnum == ObjectTypeEnum.Object)
            {
                PRs.memberTypeEnum = MemberTypeEnum.Field;
                PRs.name = progress._name;
                PRs.dtType = progress._dtType;
            }
            else
            {
                PRs.memberTypeEnum = MemberTypeEnum.Item;
                PRs.consecutiveNullArrayEntryCount = _objectNull.NullCount;
                progress.ArrayCountIncrement(_objectNull.NullCount - 1);
            }
            objectReader.Parse(PRs);
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
                _objectString.Value = objectReader.CrossAppDomainArrayAt(_crossAppDomainString._value) as string;
                if (_objectString.Value == null)
                {
                    throw new SerializationException(RemotingResources.SerializationCrossDomainError.Format(nameof(String), (int)_crossAppDomainString._value));
                }
                _objectString.ObjectId = _crossAppDomainString._objectId;
            }
            PRs.Init();
            PRs.parseTypeEnum = ParseTypeEnum.Object;
            PRs.objectId = objectReader.GetId((long)_objectString.ObjectId);
            if (PRs.objectId == topId)
            {
                PRs.objectPositionEnum = ObjectPositionEnum.Top;
            }
            PRs.objectTypeEnum = ObjectTypeEnum.Object;
            ObjectProgress progress = (ObjectProgress)stack.Peek();
            PRs.value = _objectString.Value;
            PRs.keyDt = "System.String";
            PRs.dtType = Converter.s_typeofString;
            PRs.dtTypeCode = PrimitiveTypeEnum.Invalid;
            PRs.varValue = _objectString.Value;
            if (progress == null)
            {
                PRs.parseTypeEnum = ParseTypeEnum.Object;
                PRs.name = "System.String";
            }
            else
            {
                PRs.parseTypeEnum = ParseTypeEnum.Member;
                PRs.memberValueEnum = MemberValueEnum.InlineValue;
                ObjectTypeEnum ee = progress._objectTypeEnum;
                if (ee == ObjectTypeEnum.Object)
                {
                    PRs.name = progress._name;
                    PRs.memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != ObjectTypeEnum.Array)
                    {
                        throw new SerializationException(RemotingResources.SerializationObjectTypeEnum.Format(progress._objectTypeEnum.ToString()));
                    }
                    PRs.memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            objectReader.Parse(PRs);
        }

        internal void ReadObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (_bowm == null)
            {
                _bowm = new BinaryObjectWithMap(binaryHeaderEnum);
            }
            else
            {
                _bowm.BinaryHeaderEnum = binaryHeaderEnum;
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
            if (record.BinaryHeaderEnum != BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                if (record.BinaryHeaderEnum == BinaryHeaderEnum.ObjectWithMap)
                {
                    assemblyInfo = SystemAssemblyInfo;
                }
            }
            else
            {
                if (record.AssemId < 1)
                {
                    throw new SerializationException(RemotingResources.SerializationAssemblyNotFound.Format(record.Name));
                }
                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record.AssemId];
                if (assemblyInfo == null)
                {
                    throw new SerializationException(RemotingResources.SerializationAssemblyNotFound.Format(record.AssemId.ToString() + " " + record.Name));
                }
            }
            Type objectType = objectReader.GetType(assemblyInfo, record.Name);
            ObjectMap map = ObjectMap.Create(record.Name, objectType, record.MemberNames, objectReader, record.ObjectId, assemblyInfo);
            ObjectMapIdTable[record.ObjectId] = map;
            op._objectTypeEnum = ObjectTypeEnum.Object;
            op._binaryTypeEnumA = map._binaryTypeEnumA;
            op._typeInformationA = map._typeInformationA;
            op._memberLength = op._binaryTypeEnumA.Length;
            op._memberNames = map._memberNames;
            op._memberTypes = map._memberTypes;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || progress2._isInitial)
            {
                op._name = record.Name;
                pr.parseTypeEnum = ParseTypeEnum.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr.parseTypeEnum = ParseTypeEnum.Member;
                pr.memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                ObjectTypeEnum ee = progress2._objectTypeEnum;
                if (ee == ObjectTypeEnum.Object)
                {
                    pr.name = progress2._name;
                    pr.memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != ObjectTypeEnum.Array)
                    {
                        throw new SerializationException(RemotingResources.SerializationObjectTypeEnum.Format(progress2._objectTypeEnum.ToString()));
                    }
                    pr.memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
            }
            pr.objectTypeEnum = ObjectTypeEnum.Object;
            pr.objectId = objectReader.GetId((long)record.ObjectId);
            pr.objectInfo = map.CreateObjectInfo(ref pr.si, ref pr.memberData);
            if (pr.objectId == topId)
            {
                pr.objectPositionEnum = ObjectPositionEnum.Top;
            }
            pr.keyDt = record.Name;
            pr.dtType = map._objectType;
            pr.dtTypeCode = PrimitiveTypeEnum.Invalid;
            objectReader.Parse(pr);
        }

        internal void ReadObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (_bowmt == null)
            {
                _bowmt = new BinaryObjectWithMapTyped(binaryHeaderEnum);
            }
            else
            {
                _bowmt.BinaryHeaderEnum = binaryHeaderEnum;
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
            if (record.BinaryHeaderEnum != BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                if (record.BinaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTyped)
                {
                    assemblyInfo = SystemAssemblyInfo;
                }
            }
            else
            {
                if (record.AssemId < 1)
                {
                    throw new SerializationException(RemotingResources.SerializationAssemblyId.Format(record.Name));
                }
                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record.AssemId];
                if (assemblyInfo == null)
                {
                    throw new SerializationException(RemotingResources.SerializationAssemblyId.Format(((int)record.AssemId).ToString() + " " + record.Name));
                }
            }
            ObjectMap map = ObjectMap.Create(record.Name, record.MemberNames, record.BinaryTypeEnumArray, record.TypeInformationArray, record.MemberAssemIds, objectReader, record.ObjectId, assemblyInfo, AssemIdToAssemblyTable);
            ObjectMapIdTable[record.ObjectId] = map;
            op._objectTypeEnum = ObjectTypeEnum.Object;
            op._binaryTypeEnumA = map._binaryTypeEnumA;
            op._typeInformationA = map._typeInformationA;
            op._memberLength = op._binaryTypeEnumA.Length;
            op._memberNames = map._memberNames;
            op._memberTypes = map._memberTypes;
            ObjectProgress progress2 = (ObjectProgress)stack.PeekPeek();
            if (progress2 == null || progress2._isInitial)
            {
                op._name = record.Name;
                pr.parseTypeEnum = ParseTypeEnum.Object;
                op._memberValueEnum = MemberValueEnum.Empty;
            }
            else
            {
                pr.parseTypeEnum = ParseTypeEnum.Member;
                pr.memberValueEnum = MemberValueEnum.Nested;
                op._memberValueEnum = MemberValueEnum.Nested;
                ObjectTypeEnum ee = progress2._objectTypeEnum;
                if (ee == ObjectTypeEnum.Object)
                {
                    pr.name = progress2._name;
                    pr.memberTypeEnum = MemberTypeEnum.Field;
                    op._memberTypeEnum = MemberTypeEnum.Field;
                }
                else
                {
                    if (ee != ObjectTypeEnum.Array)
                    {
                        throw new SerializationException(RemotingResources.SerializationObjectTypeEnum.Format(progress2._objectTypeEnum.ToString()));
                    }
                    pr.memberTypeEnum = MemberTypeEnum.Item;
                    op._memberTypeEnum = MemberTypeEnum.Item;
                }
            }
            pr.objectTypeEnum = ObjectTypeEnum.Object;
            pr.objectInfo = map.CreateObjectInfo(ref pr.si, ref pr.memberData);
            pr.objectId = objectReader.GetId((long)record.ObjectId);
            if (pr.objectId == topId)
            {
                pr.objectPositionEnum = ObjectPositionEnum.Top;
            }
            pr.keyDt = record.Name;
            pr.dtType = map._objectType;
            pr.dtTypeCode = PrimitiveTypeEnum.Invalid;
            objectReader.Parse(pr);
        }

        internal sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        internal void ReadSerializationHeaderRecord()
        {
            SerializationHeaderRecord record = new SerializationHeaderRecord();
            record.Read(this);
            topId = record._topId > 0 ? objectReader.GetId((long)record._topId) : (long)record._topId;
            headerId = record._headerId > 0 ? objectReader.GetId((long)record._headerId) : (long)record._headerId;
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
                    throw new SerializationException(RemotingResources.SerializationTypeCode.Format(code.ToString()));
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
                        if (enum3 - (BinaryTypeEnum)1 > (int)BinaryTypeEnum.StringArray)
                        {
                            throw new SerializationException(RemotingResources.SerializationTypeExpected);
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
                                throw new SerializationException(RemotingResources.SerializationBinaryHeader.Format(num));
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
                                    PRs.parseTypeEnum = ParseTypeEnum.MemberEnd;
                                    PRs.memberTypeEnum = op._memberTypeEnum;
                                    PRs.memberValueEnum = op._memberValueEnum;
                                    objectReader.Parse(PRs);
                                }
                                else
                                {
                                    PRs.parseTypeEnum = ParseTypeEnum.ObjectEnd;
                                    PRs.memberTypeEnum = op._memberTypeEnum;
                                    PRs.memberValueEnum = op._memberValueEnum;
                                    objectReader.Parse(PRs);
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
                throw new SerializationException(RemotingResources.SerializationStreamEnd);
            }
        }

        internal BinaryAssemblyInfo SystemAssemblyInfo
        {
            get
            {
                return _systemAssemblyInfo ?? (_systemAssemblyInfo = new BinaryAssemblyInfo(Converter.s_urtAssemblyString, Converter.s_urtAssembly));
            }
        }

        internal SizedArray ObjectMapIdTable
        {
            get
            {
                return objectMapIdTable ?? (objectMapIdTable = new SizedArray());
            }
        }

        internal SizedArray AssemIdToAssemblyTable
        {
            get
            {
                return assemIdToAssemblyTable ?? (assemIdToAssemblyTable = new SizedArray(2));
            }
        }

        internal ParseRecord PRs
        {
            get
            {
                return _prs ?? (_prs = new ParseRecord());
            }
        }
    }
}

