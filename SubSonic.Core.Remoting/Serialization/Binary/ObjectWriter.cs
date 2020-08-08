using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class ObjectWriter
    {
        private readonly Queue<object> _objectQueue;
        private readonly ObjectIDGenerator _idGenerator;
        private int _currentId = 1;
        private readonly ISurrogateSelector _surrogates;
        private readonly StreamingContext _context;
        private BinaryFormatterWriter _serWriter;
        private long _topId;
        private string _topName;
        private readonly FormatterHelper _formatterEnums;
        private readonly SerializationBinder _binder;
        private readonly SerializationObjectInfo _serObjectInfoInit;
        private readonly IFormatterConverter _formatterConverter;
        internal object[] _crossAppDomainArray;
        private object _previousObj;
        private long _previousId;
        private Type _previousType;
        private PrimitiveTypeEnum _previousCode;
        private Dictionary<string, long> _assemblyToIdTable;
        private readonly SerializationStack _niPool = new SerializationStack("NameInfo Pool");

        internal ObjectWriter(ISurrogateSelector selector, StreamingContext context, FormatterHelper formatterEnums, SerializationBinder binder)
        {
            this._surrogates = selector;
            this._context = context;
            this._binder = binder;
            this._formatterEnums = formatterEnums;
            this.ObjectManager = new SerializationObjectManager(context);
            this._idGenerator = new ObjectIDGenerator();
            this._objectQueue = new Queue<object>();
            this._formatterConverter = new FormatterConverter();
            this._serObjectInfoInit = new SerializationObjectInfo();
        }

        private bool CheckForNull(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, object data)
        {
            bool flag = data == null;
            if (flag && ((this._formatterEnums.SerializerType == SerializerType.Binary) || (memberNameInfo._isArrayItem || (memberNameInfo._transmitTypeOnObject || (memberNameInfo._transmitTypeOnMember || (objectInfo._isSi || this.CheckTypeFormat(this._formatterEnums.TypeFormat, FormatterTypeStyle.TypesAlways)))))))
            {
                if (!typeNameInfo._isArrayItem)
                {
                    this._serWriter.WriteNullMember(memberNameInfo/*, typeNameInfo*/);
                }
                else if (typeNameInfo._arrayEnum == ArrayTypeEnum.Single)
                {
                    this._serWriter.WriteDelayedNullItem();
                }
                else
                {
                    this._serWriter.WriteNullItem(/*memberNameInfo, typeNameInfo*/);
                }
            }
            return flag;
        }

        private bool CheckTypeFormat(FormatterTypeStyle test, FormatterTypeStyle want)
        {
            return ((test & want) == want);
        }

        private long GetAssemblyId(WriteObjectInfo objectInfo)
        {
            if (this._assemblyToIdTable == null)
            {
                this._assemblyToIdTable = new Dictionary<string, long>();
            }

            string assemblyString = objectInfo.GetAssemblyString();
            string str2 = assemblyString;
            long num;
            if (assemblyString.Length == 0)
            {
                num = 0L;
            }
            else if (assemblyString.Equals(Converter.s_urtAssemblyString) || assemblyString.Equals(Converter.s_urtAlternativeAssemblyString))
            {
                num = 0L;
            }
            else
            {
                bool isNew;
                if (this._assemblyToIdTable.TryGetValue(assemblyString, out num))
                {
                    isNew = false;
                }
                else
                {
                    num = this.InternalGetId("___AssemblyString___" + assemblyString, false, null, out isNew);
                    this._assemblyToIdTable[assemblyString] = num;
                }
                this._serWriter.WriteAssembly(str2, (int)num, isNew);
            }
            return num;
        }

        private NameInfo GetNameInfo()
        {
            NameInfo info;
            if (this._niPool.IsEmpty())
            {
                info = new NameInfo();
            }
            else
            {
                info = (NameInfo)this._niPool.Pop();
                info.Init();
            }
            return info;
        }

        private object GetNext(out long objID)
        {
            if (_objectQueue.Count == 0)
            {
                objID = 0L;
                return null;
            }
            object obj2 = this._objectQueue.Dequeue();
            object obj3 = (obj2 is WriteObjectInfo info) ? info._obj : obj2;
            objID = this._idGenerator.HasId(obj3, out bool flag);
            if (flag)
            {
                throw new SerializationException(RemotingResources.SerializationNoObjectID.Format(obj3));
            }
            return obj2;
        }

        private Type GetType(object obj)
        {
            return obj.GetType();
        }

        private long InternalGetId(object obj, bool assignUniqueIdToValueType, Type type, out bool isNew)
        {
            if (obj == this._previousObj)
            {
                isNew = false;
                return this._previousId;
            }
            this._idGenerator.CurrentCount = this._currentId;
            if ((type != null) && (type.IsValueType && !assignUniqueIdToValueType))
            {
                isNew = false;
                int num2 = this._currentId;
                this._currentId = num2 + 1;
                return (long)(-1 * num2);
            }
            this._currentId++;
            long id = this._idGenerator.GetId(obj, out isNew);
            this._previousObj = obj;
            this._previousId = id;
            return id;
        }

        private NameInfo MemberToNameInfo(string name)
        {
            NameInfo nameInfo = this.GetNameInfo();
            nameInfo.NIname = name;
            return nameInfo;
        }

        private void PutNameInfo(NameInfo nameInfo)
        {
            this._niPool.Push(nameInfo);
        }

        private long Schedule(object obj, bool assignUniqueIdToValueType, Type type)
        {
            return this.Schedule(obj, assignUniqueIdToValueType, type, null);
        }

        private long Schedule(object obj, bool assignUniqueIdToValueType, Type type, WriteObjectInfo objectInfo)
        {
            long num = 0L;
            if (obj != null)
            {
                num = this.InternalGetId(obj, assignUniqueIdToValueType, type, out bool flag);
                if (flag && (num > 0L))
                {
                    this._objectQueue.Enqueue(objectInfo ?? obj);
                }
            }
            return num;
        }

        internal void Serialize(object graph, BinaryFormatterWriter serWriter)
        {
            object obj2;
            if (graph == null)
            {
                throw new ArgumentNullException("graph");
            }

            this._serWriter = serWriter ?? throw new ArgumentNullException("serWriter");
            serWriter.WriteBegin();
            
            this._topId = this.InternalGetId(graph, false, null, out _);
            long headerId = -1L;
            this.WriteSerializedStreamHeader(this._topId, headerId);
            this._objectQueue.Enqueue(graph);
            while ((obj2 = this.GetNext(out long num2)) != null)
            {
                WriteObjectInfo objectInfo;
                if (obj2 is WriteObjectInfo info)
                {
                    objectInfo = info;
                }
                else
                {
                    objectInfo = WriteObjectInfo.Serialize(obj2, this._surrogates, this._context, this._serObjectInfoInit, this._formatterConverter, this, this._binder);
                    objectInfo._assemId = this.GetAssemblyId(objectInfo);
                }
                objectInfo._objectId = num2;
                NameInfo memberNameInfo = this.TypeToNameInfo(objectInfo);
                this.Write(objectInfo, memberNameInfo, memberNameInfo);
                this.PutNameInfo(memberNameInfo);
                objectInfo.ObjectEnd();
            }
            serWriter.WriteSerializationHeaderEnd();
            serWriter.WriteEnd();
            this.ObjectManager.RaiseOnSerializedEvent();
        }

        internal PrimitiveTypeEnum ToCode(Type type)
        {
            if (object.ReferenceEquals(this._previousType, type))
            {
                return this._previousCode;
            }
            PrimitiveTypeEnum ee = Converter.ToCode(type);
            if (ee != PrimitiveTypeEnum.Invalid)
            {
                this._previousType = type;
                this._previousCode = ee;
            }
            return ee;
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo)
        {
            return this.TypeToNameInfo(objectInfo._objectType, objectInfo, ToCode(objectInfo._objectType), null);
        }

        private NameInfo TypeToNameInfo(Type type)
        {
            return this.TypeToNameInfo(type, null, this.ToCode(type), null);
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo, NameInfo nameInfo)
        {
            return this.TypeToNameInfo(objectInfo._objectType, objectInfo, this.ToCode(objectInfo._objectType), nameInfo);
        }

        private void TypeToNameInfo(Type type, NameInfo nameInfo)
        {
            this.TypeToNameInfo(type, null, this.ToCode(type), nameInfo);
        }

        private NameInfo TypeToNameInfo(Type type, WriteObjectInfo objectInfo, PrimitiveTypeEnum code, NameInfo nameInfo)
        {
            if (nameInfo == null)
            {
                nameInfo = this.GetNameInfo();
            }
            else
            {
                nameInfo.Init();
            }
            if ((code == PrimitiveTypeEnum.Invalid) && (objectInfo != null))
            {
                nameInfo.NIname = objectInfo.GetTypeFullName();
                nameInfo._assemId = objectInfo._assemId;
            }
            nameInfo._primitiveTypeEnum = code;
            nameInfo._type = type;
            return nameInfo;
        }

        private void Write(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            object obj2 = objectInfo._obj;
            if (obj2 == null)
            {
                throw new ArgumentNullException("objectInfo._obj");
            }
            long num = objectInfo._objectId;
            if (object.ReferenceEquals(objectInfo._objectType, Converter.s_typeofString))
            {
                memberNameInfo._objectId = num;
                this._serWriter.WriteObjectString((int)num, obj2.ToString());
            }
            else if (objectInfo._isArray)
            {
                this.WriteArray(objectInfo, memberNameInfo);
            }
            else
            {
                objectInfo.GetMemberInfo(out string[] strArray, out Type[] typeArray, out object[] objArray);
                if (objectInfo._isSi || this.CheckTypeFormat(this._formatterEnums.TypeFormat, FormatterTypeStyle.TypesAlways))
                {
                    memberNameInfo._transmitTypeOnObject = true;
                    memberNameInfo._isParentTypeOnObject = true;
                    typeNameInfo._transmitTypeOnObject = true;
                    typeNameInfo._isParentTypeOnObject = true;
                }
                WriteObjectInfo[] memberObjectInfos = new WriteObjectInfo[strArray.Length];
                for (int i = 0; i < typeArray.Length; i++)
                {
                    Type type = typeArray[i] ?? ((objArray[i] != null) ? this.GetType(objArray[i]) : Converter.s_typeofObject);
                    if ((this.ToCode(type) == PrimitiveTypeEnum.Invalid) && !object.ReferenceEquals(type, Converter.s_typeofString))
                    {
                        if (objArray[i] != null)
                        {
                            memberObjectInfos[i] = WriteObjectInfo.Serialize(objArray[i], this._surrogates, this._context, this._serObjectInfoInit, this._formatterConverter, this, this._binder);
                            memberObjectInfos[i]._assemId = this.GetAssemblyId(memberObjectInfos[i]);
                        }
                        else
                        {
                            memberObjectInfos[i] = WriteObjectInfo.Serialize(typeArray[i], this._surrogates, this._context, this._serObjectInfoInit, this._formatterConverter, this._binder);
                            memberObjectInfos[i]._assemId = this.GetAssemblyId(memberObjectInfos[i]);
                        }
                    }
                }
                this.Write(objectInfo, memberNameInfo, typeNameInfo, strArray, typeArray, objArray, memberObjectInfos);
            }
        }

        private void Write(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, string[] memberNames, Type[] memberTypes, object[] memberData, WriteObjectInfo[] memberObjectInfos)
        {
            int length = memberNames.Length;
            NameInfo nameInfo = null;
            if (memberNameInfo != null)
            {
                memberNameInfo._objectId = objectInfo._objectId;
                this._serWriter.WriteObject(memberNameInfo, typeNameInfo, length, memberNames, memberTypes, memberObjectInfos);
            }
            else if ((objectInfo._objectId == this._topId) && (this._topName != null))
            {
                nameInfo = this.MemberToNameInfo(this._topName);
                nameInfo._objectId = objectInfo._objectId;
                this._serWriter.WriteObject(nameInfo, typeNameInfo, length, memberNames, memberTypes, memberObjectInfos);
            }
            else if (!object.ReferenceEquals(objectInfo._objectType, Converter.s_typeofString))
            {
                typeNameInfo._objectId = objectInfo._objectId;
                this._serWriter.WriteObject(typeNameInfo, null, length, memberNames, memberTypes, memberObjectInfos);
            }
            if (!memberNameInfo._isParentTypeOnObject)
            {
                memberNameInfo._transmitTypeOnObject = false;
            }
            else
            {
                memberNameInfo._transmitTypeOnObject = true;
                memberNameInfo._isParentTypeOnObject = false;
            }
            for (int i = 0; i < length; i++)
            {
                this.WriteMemberSetup(objectInfo, memberNameInfo, memberNames[i], memberTypes[i], memberData[i], memberObjectInfos[i]);
            }
            if (memberNameInfo != null)
            {
                memberNameInfo._objectId = objectInfo._objectId;
                //this._serWriter.WriteObjectEnd(memberNameInfo, typeNameInfo);
            }
            else if ((objectInfo._objectId == this._topId) && (this._topName != null))
            {
                //this._serWriter.WriteObjectEnd(nameInfo, typeNameInfo);
                this.PutNameInfo(nameInfo);
            }
            else if (!object.ReferenceEquals(objectInfo._objectType, Converter.s_typeofString))
            {
                //this._serWriter.WriteObjectEnd(typeNameInfo, typeNameInfo);
            }
        }

        private void WriteArray(WriteObjectInfo objectInfo, NameInfo memberNameInfo)
        {
            bool flag = false;
            if (memberNameInfo == null)
            {
                memberNameInfo = this.TypeToNameInfo(objectInfo);
                flag = true;
            }
            memberNameInfo._isArray = true;
            long num = objectInfo._objectId;
            memberNameInfo._objectId = objectInfo._objectId;
            Array array = (Array)objectInfo._obj;
            Type elementType = objectInfo._objectType.GetElementType();
            WriteObjectInfo info = null;
            if (!elementType.IsPrimitive)
            {
                info = WriteObjectInfo.Serialize(elementType, this._surrogates, this._context, this._serObjectInfoInit, this._formatterConverter, this._binder);
                info._assemId = this.GetAssemblyId(info);
            }
            NameInfo arrayElemTypeNameInfo = (info == null) ? this.TypeToNameInfo(elementType) : this.TypeToNameInfo(info);
            arrayElemTypeNameInfo._isArray = arrayElemTypeNameInfo._type.IsArray;
            NameInfo arrayNameInfo = memberNameInfo;
            arrayNameInfo._objectId = num;
            arrayNameInfo._isArray = true;
            arrayElemTypeNameInfo._objectId = num;
            arrayElemTypeNameInfo._transmitTypeOnMember = memberNameInfo._transmitTypeOnMember;
            arrayElemTypeNameInfo._transmitTypeOnObject = memberNameInfo._transmitTypeOnObject;
            arrayElemTypeNameInfo._isParentTypeOnObject = memberNameInfo._isParentTypeOnObject;
            int rank = array.Rank;
            int[] lengthA = new int[rank];
            int[] lowerBoundA = new int[rank];
            int[] numArray3 = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                lengthA[i] = array.GetLength(i);
                lowerBoundA[i] = array.GetLowerBound(i);
                numArray3[i] = array.GetUpperBound(i);
            }
            ArrayTypeEnum ee = !arrayElemTypeNameInfo._isArray ? ((rank != 1) ? ArrayTypeEnum.Rectangular : ArrayTypeEnum.Single) : ((rank == 1) ? ArrayTypeEnum.Jagged : ArrayTypeEnum.Rectangular);
            arrayElemTypeNameInfo._arrayEnum = ee;
            if (object.ReferenceEquals(elementType, Converter.s_typeofByte) && ((rank == 1) && (lowerBoundA[0] == 0)))
            {
                this._serWriter.WriteObjectByteArray(/*memberNameInfo,*/ arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], (byte[])array);
            }
            else
            {
                if (object.ReferenceEquals(elementType, Converter.s_typeofObject) || (Nullable.GetUnderlyingType(elementType) != null))
                {
                    memberNameInfo._transmitTypeOnMember = true;
                    arrayElemTypeNameInfo._transmitTypeOnMember = true;
                }
                if (this.CheckTypeFormat(this._formatterEnums.TypeFormat, FormatterTypeStyle.TypesAlways))
                {
                    memberNameInfo._transmitTypeOnObject = true;
                    arrayElemTypeNameInfo._transmitTypeOnObject = true;
                }
                if (ee == ArrayTypeEnum.Single)
                {
                    this._serWriter.WriteSingleArray(/*memberNameInfo,*/ arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], array);
                    if (!Converter.IsWriteAsByteArray(arrayElemTypeNameInfo._primitiveTypeEnum) || (lowerBoundA[0] != 0))
                    {
                        object[] objArray = null;
                        if (!elementType.IsValueType)
                        {
                            objArray = (object[])array;
                        }
                        int num4 = numArray3[0] + 1;
                        int index = lowerBoundA[0];
                        while (true)
                        {
                            if (index >= num4)
                            {
                                this._serWriter.WriteItemEnd();
                                break;
                            }
                            if (objArray == null)
                            {
                                this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, array.GetValue(index));
                            }
                            else
                            {
                                this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, objArray[index]);
                            }
                            index++;
                        }
                    }
                }
                else if (ee == ArrayTypeEnum.Jagged)
                {
                    arrayNameInfo._objectId = num;
                    this._serWriter.WriteJaggedArray(arrayNameInfo, info, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0]);
                    Array array2 = array;
                    int index = lowerBoundA[0];
                    while (true)
                    {
                        if (index >= (numArray3[0] + 1))
                        {
                            this._serWriter.WriteItemEnd();
                            break;
                        }
                        this.WriteArrayMember(objectInfo, arrayElemTypeNameInfo, array2.GetValue(index));
                        index++;
                    }
                }
                else
                {
                    arrayNameInfo._objectId = num;
                    this._serWriter.WriteRectangleArray(/*memberNameInfo,*/ arrayNameInfo, info, arrayElemTypeNameInfo, rank, lengthA, lowerBoundA);
                    bool flag2 = false;
                    int index = 0;
                    while (true)
                    {
                        if (index < rank)
                        {
                            if (lengthA[index] != 0)
                            {
                                index++;
                                continue;
                            }
                            flag2 = true;
                        }
                        if (!flag2)
                        {
                            this.WriteRectangle(objectInfo, rank, lengthA, array, arrayElemTypeNameInfo, lowerBoundA);
                        }
                        this._serWriter.WriteItemEnd();
                        break;
                    }
                }
                //this._serWriter.WriteObjectEnd(memberNameInfo, arrayNameInfo);
                this.PutNameInfo(arrayElemTypeNameInfo);
                if (flag)
                {
                    this.PutNameInfo(memberNameInfo);
                }
            }
        }

        private void WriteArrayMember(WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, object data)
        {
            arrayElemTypeNameInfo._isArrayItem = true;
            if (!this.CheckForNull(objectInfo, arrayElemTypeNameInfo, arrayElemTypeNameInfo, data))
            {
                Type type = null;
                bool flag = false;
                if (arrayElemTypeNameInfo._transmitTypeOnMember)
                {
                    flag = true;
                }
                if (!flag && !arrayElemTypeNameInfo.IsSealed)
                {
                    type = this.GetType(data);
                    if (!object.ReferenceEquals(arrayElemTypeNameInfo._type, type))
                    {
                        flag = true;
                    }
                }
                NameInfo typeNameInfo;
                if (!flag)
                {
                    typeNameInfo = arrayElemTypeNameInfo;
                    typeNameInfo._isArrayItem = true;
                }
                else
                {
                    if (type == null)
                    {
                        type = this.GetType(data);
                    }
                    typeNameInfo = this.TypeToNameInfo(type);
                    typeNameInfo._transmitTypeOnMember = true;
                    typeNameInfo._objectId = arrayElemTypeNameInfo._objectId;
                    typeNameInfo._assemId = arrayElemTypeNameInfo._assemId;
                    typeNameInfo._isArrayItem = true;
                }
                if (!this.WriteKnownValueClass(arrayElemTypeNameInfo, typeNameInfo, data))
                {
                    object obj2 = data;
                    bool assignUniqueIdToValueType = false;
                    if (object.ReferenceEquals(arrayElemTypeNameInfo._type, Converter.s_typeofObject))
                    {
                        assignUniqueIdToValueType = true;
                    }
                    long num = this.Schedule(obj2, assignUniqueIdToValueType, typeNameInfo._type);
                    arrayElemTypeNameInfo._objectId = num;
                    typeNameInfo._objectId = num;
                    if (num >= 1L)
                    {
                        this._serWriter.WriteItemObjectRef(/*arrayElemTypeNameInfo,*/ (int)num);
                    }
                    else
                    {
                        WriteObjectInfo info2 = WriteObjectInfo.Serialize(obj2, this._surrogates, this._context, this._serObjectInfoInit, this._formatterConverter, this, this._binder);
                        info2._objectId = num;
                        info2._assemId = (object.ReferenceEquals(arrayElemTypeNameInfo._type, Converter.s_typeofObject) || (Nullable.GetUnderlyingType(arrayElemTypeNameInfo._type) != null)) ? this.GetAssemblyId(info2) : typeNameInfo._assemId;
                        NameInfo info3 = this.TypeToNameInfo(info2);
                        info3._objectId = num;
                        info2._objectId = num;
                        this.Write(info2, typeNameInfo, info3);
                        info2.ObjectEnd();
                    }
                }
                if (arrayElemTypeNameInfo._transmitTypeOnMember)
                {
                    this.PutNameInfo(typeNameInfo);
                }
            }
        }

        private bool WriteKnownValueClass(NameInfo memberNameInfo, NameInfo typeNameInfo, object data)
        {
            if (object.ReferenceEquals(typeNameInfo._type, Converter.s_typeofString))
            {
                this.WriteString(/*memberNameInfo,*/ typeNameInfo, data);
            }
            else
            {
                if (typeNameInfo._primitiveTypeEnum == PrimitiveTypeEnum.Invalid)
                {
                    return false;
                }
                if (typeNameInfo._isArray)
                {
                    this._serWriter.WriteItem(memberNameInfo, typeNameInfo, data);
                }
                else
                {
                    this._serWriter.WriteMember(memberNameInfo, typeNameInfo, data);
                }
            }
            return true;
        }

        private void WriteMembers(NameInfo memberNameInfo, NameInfo memberTypeNameInfo, object memberData, WriteObjectInfo objectInfo, WriteObjectInfo memberObjectInfo)
        {
            Type nullableType = memberNameInfo._type;
            bool assignUniqueIdToValueType = false;
            if (object.ReferenceEquals(nullableType, Converter.s_typeofObject) || (Nullable.GetUnderlyingType(nullableType) != null))
            {
                memberTypeNameInfo._transmitTypeOnMember = true;
                memberNameInfo._transmitTypeOnMember = true;
            }
            if (this.CheckTypeFormat(this._formatterEnums.TypeFormat, FormatterTypeStyle.TypesAlways) || objectInfo._isSi)
            {
                memberTypeNameInfo._transmitTypeOnObject = true;
                memberNameInfo._transmitTypeOnObject = true;
                memberNameInfo._isParentTypeOnObject = true;
            }
            if (!this.CheckForNull(objectInfo, memberNameInfo, memberTypeNameInfo, memberData))
            {
                object obj2 = memberData;
                Type type = null;
                if (memberTypeNameInfo._primitiveTypeEnum == PrimitiveTypeEnum.Invalid)
                {
                    type = this.GetType(obj2);
                    if (!object.ReferenceEquals(nullableType, type))
                    {
                        memberTypeNameInfo._transmitTypeOnMember = true;
                        memberNameInfo._transmitTypeOnMember = true;
                    }
                }
                if (object.ReferenceEquals(nullableType, Converter.s_typeofObject))
                {
                    assignUniqueIdToValueType = true;
                    nullableType = this.GetType(memberData);
                    if (memberObjectInfo == null)
                    {
                        this.TypeToNameInfo(nullableType, memberTypeNameInfo);
                    }
                    else
                    {
                        this.TypeToNameInfo(memberObjectInfo, memberTypeNameInfo);
                    }
                }
                if ((memberObjectInfo != null) && memberObjectInfo._isArray)
                {
                    if (type == null)
                    {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                        type = this.GetType(obj2);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                    }
                    long objectId = this.Schedule(obj2, false, null, memberObjectInfo);
                    if (objectId > 0L)
                    {
                        memberNameInfo._objectId = objectId;
                        this.WriteObjectRef(/*memberNameInfo,*/ objectId);
                    }
                    else
                    {
                        this._serWriter.WriteMemberNested();
                        memberObjectInfo._objectId = objectId;
                        memberNameInfo._objectId = objectId;
                        this.WriteArray(memberObjectInfo, memberNameInfo);
                        objectInfo.ObjectEnd();
                    }
                }
                else if (!this.WriteKnownValueClass(memberNameInfo, memberTypeNameInfo, memberData))
                {
                    if (type == null)
                    {
                        type = this.GetType(obj2);
                    }
                    long objectId = this.Schedule(obj2, assignUniqueIdToValueType, type, memberObjectInfo);
                    if (objectId < 0L)
                    {
                        memberObjectInfo._objectId = objectId;
                        NameInfo info = this.TypeToNameInfo(memberObjectInfo);
                        info._objectId = objectId;
                        this.Write(memberObjectInfo, memberNameInfo, info);
                        this.PutNameInfo(info);
                        memberObjectInfo.ObjectEnd();
                    }
                    else
                    {
                        memberNameInfo._objectId = objectId;
                        this.WriteObjectRef(/*memberNameInfo,*/ objectId);
                    }
                }
            }
        }

        private void WriteMemberSetup(WriteObjectInfo objectInfo, NameInfo memberNameInfo, string memberName, Type memberType, object memberData, WriteObjectInfo memberObjectInfo)
        {
            NameInfo info = this.MemberToNameInfo(memberName);
            if (memberObjectInfo != null)
            {
                info._assemId = memberObjectInfo._assemId;
            }
            info._type = memberType;
            NameInfo memberTypeNameInfo = memberObjectInfo != null ? TypeToNameInfo(memberObjectInfo) : TypeToNameInfo(memberType);
            info._transmitTypeOnObject = memberNameInfo._transmitTypeOnObject;
            info._isParentTypeOnObject = memberNameInfo._isParentTypeOnObject;
            this.WriteMembers(info, memberTypeNameInfo, memberData, objectInfo, memberObjectInfo);
            this.PutNameInfo(info);
            this.PutNameInfo(memberTypeNameInfo);
        }

        private void WriteObjectRef(/*NameInfo nameInfo,*/ long objectId)
        {
            this._serWriter.WriteMemberObjectRef(/*nameInfo,*/ (int)objectId);
        }

        private unsafe void WriteRectangle(WriteObjectInfo objectInfo, int rank, int[] maxA, Array array, NameInfo arrayElemNameTypeInfo, int[] lowerBoundA)
        {
            int[] indices = new int[rank];
            int[] numArray2 = null;
            bool flag = false;
            if (lowerBoundA != null)
            {
                for (int i = 0; i < rank; i++)
                {
                    if (lowerBoundA[i] != 0)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                numArray2 = new int[rank];
            }
            bool flag2 = true;
            while (flag2)
            {
                flag2 = false;
                if (!flag)
                {
                    this.WriteArrayMember(objectInfo, arrayElemNameTypeInfo, array.GetValue(indices));
                }
                else
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= rank)
                        {
                            this.WriteArrayMember(objectInfo, arrayElemNameTypeInfo, array.GetValue(numArray2));
                            break;
                        }
                        numArray2[index] = indices[index] + lowerBoundA[index];
                        index++;
                    }
                }
                for (int i = rank - 1; i > -1; i--)
                {
                    if (indices[i] < (maxA[i] - 1))
                    {
                        fixed (int* numPtr1 = indices)
                        {
                            numPtr1[i]++;
                        }
                        
                        if (i < (rank - 1))
                        {
                            for (int j = i + 1; j < rank; j++)
                            {
                                indices[j] = 0;
                            }
                        }
                        flag2 = true;
                        break;
                    }
                }
            }
        }

        private void WriteSerializedStreamHeader(long topId, long headerId)
        {
            this._serWriter.WriteSerializationHeader((int)topId, (int)headerId, 1, 0);
        }

        private void WriteString(/*NameInfo memberNameInfo,*/ NameInfo typeNameInfo, object stringObject)
        {
            bool isNew = true;
            long objectId = -1L;
            if (!this.CheckTypeFormat(this._formatterEnums.TypeFormat, FormatterTypeStyle.XsdString))
            {
                objectId = this.InternalGetId(stringObject, false, null, out isNew);
            }
            typeNameInfo._objectId = objectId;
            if (!isNew && (objectId >= 0L))
            {
                this.WriteObjectRef(/*memberNameInfo,*/ objectId);
            }
            else
            {
                this._serWriter.WriteMemberString(/*memberNameInfo,*/ typeNameInfo, (string)((string)stringObject));
            }
        }

        internal SerializationObjectManager ObjectManager { get; private set; }
    }
}
