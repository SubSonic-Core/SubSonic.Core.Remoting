using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class ObjectReader
    {
        private class TypeNAssembly
        {
            public Type Type;
            public string AssemblyName;
        }

        private sealed class TopLevelAssemblyTypeResolver
        {
            private readonly Assembly topLevelAssembly;

            public TopLevelAssemblyTypeResolver(Assembly topLevelAssembly)
            {
                this.topLevelAssembly = topLevelAssembly;
            }

            public Type ResolveType(Assembly assembly, string simpleTypeName, bool ignoreCase)
            {
                if (assembly == null)
                {
                    assembly = this.topLevelAssembly;
                }
                return assembly.GetType(simpleTypeName, false, ignoreCase);
            }
        }

        private Stream stream;
        private ISurrogateSelector surrogates;
        private StreamingContext context;
        private ObjectManager objectManager;
        private FormatterHelper fh;
        private SerializationBinder binder;
        private long topId;
        private bool isSimpleAssembly;
        private object topObject;
        private SerializationObjectInfo objectInfo;
        private IFormatterConverter converter;
        private SerializationStack stack;
        private SerializationStack valueFixupStack;
        public object[] CrossAppDomainArray { get; set; }
        private bool fullDeserialization;
        private bool oldFormatDetected;
        private IntSizedArray valTypeObjectIdTable;
        private readonly NameCache typeCache;
        private string previousAssemblyString;
        private string previousName;
        private Type previousType;

        public ObjectReader(Stream serializationStream, ISurrogateSelector surrogateSelector, StreamingContext context, FormatterHelper fh, SerializationBinder binder)
            : this()
        {
            this.stream = serializationStream ?? throw new ArgumentNullException(nameof(serializationStream));
            this.surrogates = surrogateSelector;
            this.context = context;
            this.fh = fh;
            this.binder = binder;
        }

        private ObjectReader()
        {
            this.typeCache = new NameCache();
        }

        private SerializationStack ValueFixupStack
        {
            get
            {
                return valueFixupStack ?? (valueFixupStack = new SerializationStack("ValueType Fixup Stack"));
            }
        }

        private static void CheckTypeForwardedTo(Assembly sourceAssembly, Assembly destAssembly, Type resolvedType)
        {
        }

        public Type Bind(string assemblyString, string typeString)
        {
            Type type = null;
            if (this.binder != null)
            {
                type = this.binder.BindToType(assemblyString, typeString);
            }
            if (type == null)
            {
                type = this.FastBindToType(assemblyString, typeString);
            }
            return type;
        }

        private void CheckSerializable(Type type)
        {
            if (!type.IsSerializable && !HasSurrogate(type))
            {
                throw new SerializationException(RemotingResources.NotMarkedForSerialization.Format(type.FullName, type.Assembly.FullName));
            }
        }

        public ReadObjectInfo CreateReadObjectInfo(Type objectType)
        {
            return ReadObjectInfo.Create(objectType, this.surrogates, this.context, this.objectManager, this.objectInfo, this.converter, this.isSimpleAssembly);
        }

        public ReadObjectInfo CreateReadObjectInfo(Type objectType, string[] memberNames, Type[] memberTypes)
        {
            return ReadObjectInfo.Create(objectType, memberNames, memberTypes, this.surrogates, this.context, this.objectManager, this.objectInfo, this.converter, this.isSimpleAssembly);
        }

        public object CrossAppDomainArrayAt(int index)
        {
            return this.CrossAppDomainArray?[index];
        }

        internal object Deserialize(BinaryParser parser, bool fCheck)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("serParser");
            }
            this.fullDeserialization = false;
            this.TopObject = null;
            this.topId = 0L;
            this.isSimpleAssembly = this.fh.AssemblyFormat == FormatterAssemblyStyle.Simple;
            //using (SerializationInfo.StartDeserialization())
            //{
                if (this.fullDeserialization)
                {
                    this.objectManager = new ObjectManager(this.surrogates, this.context);
                    this.objectInfo = new SerializationObjectInfo();
                }
                parser.Run();
                if (this.fullDeserialization)
                {
                    this.objectManager.DoFixups();
                }
                if (this.TopObject == null)
                {
                    throw new SerializationException(RemotingResources.SerializationTopObjectMissing);
                }
                if (this.HasSurrogate(this.TopObject.GetType()) && (this.topId != 0))
                {
                    this.TopObject = this.objectManager.GetObject(this.topId);
                }
                if (this.TopObject is IObjectReference)
                {
                    this.TopObject = ((IObjectReference)this.TopObject).GetRealObject(this.context);
                }
                if (this.fullDeserialization)
                {
                    this.objectManager.RaiseDeserializationEvent();
                }
                return this.TopObject;
            //}
        }

        public object TopObject
        {
            get
            {
                return this.topObject;
            }
            set
            {
                this.topObject = value;
                if (this.objectManager != null)
                {
                    this.objectManager.TopObject = value;
                }
            }
        }

        private bool HasSurrogate(Type type)
        {
            return ((this.surrogates != null) && (this.surrogates.GetSurrogate(type, context, out _) != null));
        }

        private Type FastBindToType(string assemblyName, string typeName)
        {
            Type typeFromAssembly = null;
            TypeNAssembly cachedValue = (TypeNAssembly)this.typeCache.GetCachedValue(typeName);
            if ((cachedValue == null) || (cachedValue.AssemblyName != assemblyName))
            {
                if (assemblyName == null)
                {
                    return null;
                }
                Assembly assm = null;
                AssemblyName name = null;
                try
                {
                    name = new AssemblyName(assemblyName);
                }
                catch
                {
                    return null;
                }
                if (this.isSimpleAssembly)
                {
                    assm = ResolveSimpleAssemblyName(name);
                }
                else
                {
                    try
                    {
                        assm = Assembly.Load(name);
                    }
                    catch
                    {
                    }
                }
                if (assm == null)
                {
                    return null;
                }
                if (this.isSimpleAssembly)
                {
                    GetSimplyNamedTypeFromAssembly(assm, typeName, ref typeFromAssembly);
                }
                else
                {
                    typeFromAssembly = FormatterServices.GetTypeFromAssembly(assm, typeName);
                }

                if (typeFromAssembly == null)
                {
                    return null;
                }
                
                cachedValue = new TypeNAssembly
                {
                    Type = typeFromAssembly,
                    AssemblyName = assemblyName
                };
                this.typeCache.SetCachedValue(typeName, cachedValue);
            }
            return cachedValue.Type;
        }

        public long GetId(long objectId)
        {
            if (!this.fullDeserialization)
            {
                this.InitFullDeserialization();
            }
            if (objectId > 0L)
            {
                return objectId;
            }
            if (!this.oldFormatDetected && (objectId != -1L))
            {
                return (-1L * objectId);
            }
            this.oldFormatDetected = true;
            if (this.valTypeObjectIdTable == null)
            {
                this.valTypeObjectIdTable = new IntSizedArray();
            }
            long num = 0L;
            num = this.valTypeObjectIdTable[(int)objectId];
            if (num == 0)
            {
                num = 0x7fffffffL + objectId;
                this.valTypeObjectIdTable[(int)objectId] = (int)num;
            }
            return num;
        }

        private void InitFullDeserialization()
        {
            this.fullDeserialization = true;
            this.stack = new SerializationStack("ObjectReader Object Stack");
            this.objectManager = new ObjectManager(this.surrogates, this.context);
            if (this.converter == null)
            {
                this.converter = new FormatterConverter();
            }
        }

        private static void GetSimplyNamedTypeFromAssembly(Assembly assm, string typeName, ref Type type)
        {
            try
            {
                type = FormatterServices.GetTypeFromAssembly(assm, typeName);
            }
            catch (TypeLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            if (type == null)
            {
                type = Type.GetType(typeName, new Func<AssemblyName, Assembly>(ResolveSimpleAssemblyName), new Func<Assembly, string, bool, Type>(new TopLevelAssemblyTypeResolver(assm).ResolveType), false);
            }
        }

        public Type GetType(BinaryAssemblyInfo assemblyInfo, string name)
        {
            Type typeFromAssembly = null;
            if ((this.previousName != null) && ((this.previousName.Length == name.Length) && (this.previousName.Equals(name) && ((this.previousAssemblyString != null) && ((this.previousAssemblyString.Length == assemblyInfo.AssemblyString.Length) && this.previousAssemblyString.Equals(assemblyInfo.AssemblyString))))))
            {
                typeFromAssembly = this.previousType;
            }
            else
            {
                typeFromAssembly = this.Bind(assemblyInfo.AssemblyString, name);
                if (typeFromAssembly == null)
                {
                    Assembly assm = assemblyInfo.GetAssembly();
                    if (this.isSimpleAssembly)
                    {
                        GetSimplyNamedTypeFromAssembly(assm, name, ref typeFromAssembly);
                    }
                    else
                    {
                        typeFromAssembly = FormatterServices.GetTypeFromAssembly(assm, name);
                    }
                    if (typeFromAssembly != null)
                    {
                        CheckTypeForwardedTo(assm, typeFromAssembly.Assembly, typeFromAssembly);
                    }
                }
                this.previousAssemblyString = assemblyInfo.AssemblyString;
                this.previousName = name;
                this.previousType = typeFromAssembly;
            }
            return typeFromAssembly;
        }

        private static Assembly ResolveSimpleAssemblyName(AssemblyName assemblyName)
        {
            if (assemblyName != null)
            {
                try
                {
                    if (Assembly.Load(assemblyName.Name) is Assembly assembly)
                    {
                        return assembly;
                    }
                }
                catch { }
            }
            return null;
        }

        private unsafe void NextRectangleMap(ParseRecord pr)
        {
            for (int i = pr.rank - 1; i > -1; i--)
            {
                if (pr.rectangularMap[i] < (pr.lengthA[i] - 1))
                {
                    fixed(int* numPtr1 = pr.rectangularMap)
                    {
                        numPtr1[i]++;
                    }

                    if (i < (pr.rank - 1))
                    {
                        for (int j = i + 1; j < pr.rank; j++)
                        {
                            pr.rectangularMap[j] = 0;
                        }
                    }
                    Array.Copy(pr.rectangularMap, 0, pr.indexMap, 0, pr.rank);
                    return;
                }
            }
        }

        public void Parse(ParseRecord pr)
        {
            switch (pr.parseTypeEnum)
            {
                case ParseTypeEnum.SerializedStreamHeader:
                    this.ParseSerializedStreamHeader(pr);
                    return;

                case ParseTypeEnum.Object:
                    this.ParseObject(pr);
                    return;

                case ParseTypeEnum.Member:
                    this.ParseMember(pr);
                    return;

                case ParseTypeEnum.ObjectEnd:
                    this.ParseObjectEnd(pr);
                    return;

                case ParseTypeEnum.MemberEnd:
                    this.ParseMemberEnd(pr);
                    return;

                case ParseTypeEnum.SerializedStreamHeaderEnd:
                    this.ParseSerializedStreamHeaderEnd(pr);
                    return;

                case ParseTypeEnum.Envelope:
                case ParseTypeEnum.EnvelopeEnd:
                case ParseTypeEnum.Body:
                case ParseTypeEnum.BodyEnd:
                    return;
            }
            throw new SerializationException(RemotingResources.SerializationXmlElement.Format(pr.name));
        }

        private void ParseArray(ParseRecord pr)
        {
            long num = pr.objectId;
            if (pr.arrayTypeEnum == ArrayTypeEnum.Base64)
            {
                pr.newObj = (pr.value.Length > 0) ? Convert.FromBase64String(pr.value) : Array.Empty<byte>();
                if (this.stack.Peek() == pr)
                {
                    this.stack.Pop();
                }
                if (pr.objectPositionEnum == ObjectPositionEnum.Top)
                {
                    this.TopObject = pr.newObj;
                }
                ParseRecord objectPr = (ParseRecord)this.stack.Peek();
                this.RegisterObject(pr.newObj, pr, objectPr);
            }
            else if ((pr.newObj != null) && Converter.IsWriteAsByteArray(pr.arrayElementTypeCode))
            {
                if (pr.objectPositionEnum == ObjectPositionEnum.Top)
                {
                    this.TopObject = pr.newObj;
                }
                ParseRecord objectPr = (ParseRecord)this.stack.Peek();
                this.RegisterObject(pr.newObj, pr, objectPr);
            }
            else if ((pr.arrayTypeEnum != ArrayTypeEnum.Jagged) && (pr.arrayTypeEnum != ArrayTypeEnum.Single))
            {
                if (pr.arrayTypeEnum != ArrayTypeEnum.Rectangular)
                {
                    throw new SerializationException(RemotingResources.SerializationArrayType.Format(pr.arrayTypeEnum));
                }
                pr.isLowerBound = false;
                if (pr.lowerBoundA != null)
                {
                    for (int j = 0; j < pr.rank; j++)
                    {
                        if (pr.lowerBoundA[j] != 0)
                        {
                            pr.isLowerBound = true;
                        }
                    }
                }
                if (pr.arrayElementType != null)
                {
                    pr.newObj = !pr.isLowerBound ? Array.CreateInstance(pr.arrayElementType, pr.lengthA) : Array.CreateInstance(pr.arrayElementType, pr.lengthA, pr.lowerBoundA);
                }
                int num2 = 1;
                for (int i = 0; i < pr.rank; i++)
                {
                    num2 *= pr.lengthA[i];
                }
                pr.indexMap = new int[pr.rank];
                pr.rectangularMap = new int[pr.rank];
                pr.linearlength = num2;
            }
            else
            {
                bool flag = true;
                if ((pr.lowerBoundA != null) && (pr.lowerBoundA[0] != 0))
                {
                    if (pr.arrayElementType != null)
                    {
                        pr.newObj = Array.CreateInstance(pr.arrayElementType, pr.lengthA, pr.lowerBoundA);
                    }
                    pr.isLowerBound = true;
                }
                else
                {
                    if (object.ReferenceEquals(pr.arrayElementType, Converter.s_typeofString))
                    {
                        pr.objectA = new string[pr.lengthA[0]];
                        pr.newObj = pr.objectA;
                        flag = false;
                    }
                    else if (object.ReferenceEquals(pr.arrayElementType, Converter.s_typeofObject))
                    {
                        pr.objectA = new object[pr.lengthA[0]];
                        pr.newObj = pr.objectA;
                        flag = false;
                    }
                    else if (pr.arrayElementType != null)
                    {
                        pr.newObj = Array.CreateInstance(pr.arrayElementType, pr.lengthA[0]);
                    }
                    pr.isLowerBound = false;
                }
                if (pr.arrayTypeEnum == ArrayTypeEnum.Single)
                {
                    if (!pr.isLowerBound && Converter.IsWriteAsByteArray(pr.arrayElementTypeCode))
                    {
                        pr.primitiveArray = new PrimitiveArray(pr.arrayElementTypeCode, (Array)pr.newObj);
                    }
                    else if (flag && ((pr.arrayElementType != null) && (!pr.arrayElementType.IsValueType && !pr.isLowerBound)))
                    {
                        pr.objectA = (object[])pr.newObj;
                    }
                }
                pr.indexMap = new int[1];
            }
        }

        private void ParseArrayMember(ParseRecord pr)
        {
            ParseRecord record = (ParseRecord)this.stack.Peek();
            if (record.arrayTypeEnum != ArrayTypeEnum.Rectangular)
            {
                record.indexMap[0] = !record.isLowerBound ? record.memberIndex : (record.lowerBoundA[0] + record.memberIndex);
            }
            else
            {
                if (record.memberIndex > 0)
                {
                    this.NextRectangleMap(record);
                }
                if (record.isLowerBound)
                {
                    for (int i = 0; i < record.rank; i++)
                    {
                        record.indexMap[i] = record.rectangularMap[i] + record.lowerBoundA[i];
                    }
                }
            }
            if (pr.memberValueEnum == MemberValueEnum.Reference)
            {
                object obj2 = this.objectManager.GetObject(pr.idRef);
                if (obj2 == null)
                {
                    int[] destinationArray = new int[record.rank];
                    Array.Copy(record.indexMap, 0, destinationArray, 0, record.rank);
                    this.objectManager.RecordArrayElementFixup(record.objectId, destinationArray, pr.idRef);
                }
                else if (record.objectA != null)
                {
                    record.objectA[record.indexMap[0]] = obj2;
                }
                else
                {
                    ((Array)record.newObj).SetValue(obj2, record.indexMap);
                }
            }
            else if (pr.memberValueEnum == MemberValueEnum.Nested)
            {
                if (pr.dtType == null)
                {
                    pr.dtType = record.arrayElementType;
                }
                this.ParseObject(pr);
                this.stack.Push(pr);
                if (record.arrayElementType != null)
                {
                    if (record.arrayElementType.IsValueType && (pr.arrayElementTypeCode == PrimitiveTypeEnum.Invalid))
                    {
                        pr.isValueTypeFixup = true;
                        this.ValueFixupStack.Push(new ValueFixup((Array)record.newObj, record.indexMap));
                    }
                    else if (record.objectA != null)
                    {
                        record.objectA[record.indexMap[0]] = pr.newObj;
                    }
                    else
                    {
                        ((Array)record.newObj).SetValue(pr.newObj, record.indexMap);
                    }
                }
            }
            else if (pr.memberValueEnum != MemberValueEnum.InlineValue)
            {
                if (pr.memberValueEnum == MemberValueEnum.Null)
                {
                    record.memberIndex += pr.consecutiveNullArrayEntryCount - 1;
                }
                else
                {
                    this.ParseError(pr, record);
                }
            }
            else if (object.ReferenceEquals(record.arrayElementType, Converter.s_typeofString) || object.ReferenceEquals(pr.dtType, Converter.s_typeofString))
            {
                this.ParseString(pr, record);
                if (record.objectA != null)
                {
                    record.objectA[record.indexMap[0]] = pr.value;
                }
                else
                {
                    ((Array)record.newObj).SetValue(pr.value, record.indexMap);
                }
            }
            else if (!record.isArrayVariant)
            {
                if (record.primitiveArray != null)
                {
                    record.primitiveArray.SetValue(pr.value, record.indexMap[0]);
                }
                else
                {
                    object obj4 = (pr.varValue != null) ? pr.varValue : Converter.FromString(pr.value, record.arrayElementTypeCode);
                    if (record.objectA != null)
                    {
                        record.objectA[record.indexMap[0]] = obj4;
                    }
                    else
                    {
                        ((Array)record.newObj).SetValue(obj4, record.indexMap);
                    }
                }
            }
            else
            {
                if (pr.keyDt == null)
                {
                    throw new SerializationException(RemotingResources.SerializationArrayTypeObjectNotInitialized);
                }
                object uninitializedObject = null;
                if (object.ReferenceEquals(pr.dtType, Converter.s_typeofString))
                {
                    this.ParseString(pr, record);
                    uninitializedObject = pr.value;
                }
                else if (pr.dtTypeCode != PrimitiveTypeEnum.Invalid)
                {
                    uninitializedObject = (pr.varValue != null) ? pr.varValue : Converter.FromString(pr.value, pr.dtTypeCode);
                }
                else
                {
                    this.CheckSerializable(pr.dtType);
                    uninitializedObject = FormatterServices.GetUninitializedObject(pr.dtType);
                }
                if (record.objectA != null)
                {
                    record.objectA[record.indexMap[0]] = uninitializedObject;
                }
                else
                {
                    ((Array)record.newObj).SetValue(uninitializedObject, record.indexMap);
                }
            }
            record.memberIndex++;
        }

        private void ParseArrayMemberEnd(ParseRecord pr)
        {
            if (pr.memberValueEnum == MemberValueEnum.Nested)
            {
                this.ParseObjectEnd(pr);
            }
        }

        private void ParseError(ParseRecord processing, ParseRecord onStack)
        {
            string[] textArray1 = new string[] { onStack.name, " ", onStack.parseTypeEnum.ToString(), " ", processing.name, " ", processing.parseTypeEnum.ToString() };
            throw new SerializationException(RemotingResources.SerializationParseError.Format(string.Concat(textArray1)));
        }

        private void ParseMember(ParseRecord pr)
        {
            ParseRecord onStack = (ParseRecord)this.stack.Peek();
            string str = (onStack != null) ? onStack.name : null;
            MemberTypeEnum ee = pr.memberTypeEnum;
            if ((ee != MemberTypeEnum.Field) && (ee == MemberTypeEnum.Item))
            {
                this.ParseArrayMember(pr);
            }
            else
            {
                if ((pr.dtType == null) && onStack.objectInfo.IsTyped)
                {
                    pr.dtType = onStack.objectInfo.GetType(pr.name);
                    if (pr.dtType != null)
                    {
                        pr.dtTypeCode = Converter.ToCode(pr.dtType);
                    }
                }
                if (pr.memberValueEnum == MemberValueEnum.Null)
                {
                    onStack.objectInfo.AddValue(pr.name, null, ref onStack.si, ref onStack.memberData);
                }
                else if (pr.memberValueEnum == MemberValueEnum.Nested)
                {
                    this.ParseObject(pr);
                    this.stack.Push(pr);
                    if ((pr.objectInfo == null) || ((pr.objectInfo.ObjectType == null) || !pr.objectInfo.ObjectType.IsValueType))
                    {
                        onStack.objectInfo.AddValue(pr.name, pr.newObj, ref onStack.si, ref onStack.memberData);
                    }
                    else
                    {
                        pr.isValueTypeFixup = true;
                        this.ValueFixupStack.Push(new ValueFixup(onStack.newObj, pr.name, onStack.objectInfo));
                    }
                }
                else if (pr.memberValueEnum == MemberValueEnum.Reference)
                {
                    object obj2 = this.objectManager.GetObject(pr.idRef);
                    if (obj2 != null)
                    {
                        onStack.objectInfo.AddValue(pr.name, obj2, ref onStack.si, ref onStack.memberData);
                    }
                    else
                    {
                        onStack.objectInfo.AddValue(pr.name, null, ref onStack.si, ref onStack.memberData);
                        onStack.objectInfo.RecordFixup(onStack.objectId, pr.name, pr.idRef);
                    }
                }
                else if (pr.memberValueEnum != MemberValueEnum.InlineValue)
                {
                    this.ParseError(pr, onStack);
                }
                else if (object.ReferenceEquals(pr.dtType, Converter.s_typeofString))
                {
                    this.ParseString(pr, onStack);
                    onStack.objectInfo.AddValue(pr.name, pr.value, ref onStack.si, ref onStack.memberData);
                }
                else if (pr.dtTypeCode != PrimitiveTypeEnum.Invalid)
                {
                    object obj3 = (pr.varValue != null) ? pr.varValue : Converter.FromString(pr.value, pr.dtTypeCode);
                    onStack.objectInfo.AddValue(pr.name, obj3, ref onStack.si, ref onStack.memberData);
                }
                else if (pr.arrayTypeEnum == ArrayTypeEnum.Base64)
                {
                    onStack.objectInfo.AddValue(pr.name, Convert.FromBase64String(pr.value), ref onStack.si, ref onStack.memberData);
                }
                else
                {
                    if (object.ReferenceEquals(pr.dtType, Converter.s_typeofObject))
                    {
                        throw new SerializationException(RemotingResources.SerializationTypeMissing.Format(pr.name));
                    }
                    this.ParseString(pr, onStack);
                    if (object.ReferenceEquals(pr.dtType, Converter.s_typeofSystemVoid))
                    {
                        onStack.objectInfo.AddValue(pr.name, pr.dtType, ref onStack.si, ref onStack.memberData);
                    }
                    else if (onStack.objectInfo.IsSi)
                    {
                        onStack.objectInfo.AddValue(pr.name, pr.value, ref onStack.si, ref onStack.memberData);
                    }
                }
            }
        }

        private void ParseMemberEnd(ParseRecord pr)
        {
            MemberTypeEnum ee = pr.memberTypeEnum;
            if (ee == MemberTypeEnum.Field)
            {
                if (pr.memberValueEnum == MemberValueEnum.Nested)
                {
                    this.ParseObjectEnd(pr);
                }
            }
            else if (ee == MemberTypeEnum.Item)
            {
                this.ParseArrayMemberEnd(pr);
            }
            else
            {
                this.ParseError(pr, (ParseRecord)this.stack.Peek());
            }
        }

        private void ParseObject(ParseRecord pr)
        {
            if (!this.fullDeserialization)
            {
                this.InitFullDeserialization();
            }
            if (pr.objectPositionEnum == ObjectPositionEnum.Top)
            {
                this.topId = pr.objectId;
            }
            if (pr.parseTypeEnum == ParseTypeEnum.Object)
            {
                this.stack.Push(pr);
            }
            if (pr.objectTypeEnum == ObjectTypeEnum.Array)
            {
                this.ParseArray(pr);
            }
            else if (pr.dtType == null)
            {
                pr.newObj = new TypeLoadExceptionHolder(pr.keyDt);
            }
            else if (object.ReferenceEquals(pr.dtType, Converter.s_typeofString))
            {
                if (pr.value != null)
                {
                    pr.newObj = pr.value;
                    if (pr.objectPositionEnum == ObjectPositionEnum.Top)
                    {
                        this.TopObject = pr.newObj;
                    }
                    else
                    {
                        this.stack.Pop();
                        this.RegisterObject(pr.newObj, pr, (ParseRecord)this.stack.Peek());
                    }
                }
            }
            else
            {
                this.CheckSerializable(pr.dtType);
                pr.newObj = FormatterServices.GetUninitializedObject(pr.dtType);
                this.objectManager.RaiseOnDeserializingEvent(pr.newObj);
                if (pr.newObj == null)
                {
                    throw new SerializationException(RemotingResources.SerializationTopObjectNotInstanciated.Format(pr.dtType));
                }
                if (pr.objectPositionEnum == ObjectPositionEnum.Top)
                {
                    this.TopObject = pr.newObj;
                }
                if (pr.objectInfo == null)
                {
                    pr.objectInfo = ReadObjectInfo.Create(pr.dtType, this.surrogates, this.context, this.objectManager, this.objectInfo, this.converter, this.isSimpleAssembly);
                }
            }
        }

        private void ParseObjectEnd(ParseRecord pr)
        {
            ParseRecord record = ((ParseRecord)this.stack.Peek()) ?? pr;
            if ((record.objectPositionEnum == ObjectPositionEnum.Top) && object.ReferenceEquals(record.dtType, Converter.s_typeofString))
            {
                record.newObj = record.value;
                this.TopObject = record.newObj;
            }
            else
            {
                this.stack.Pop();
                ParseRecord objectPr = (ParseRecord)this.stack.Peek();
                if (record.newObj != null)
                {
                    if (record.objectTypeEnum == ObjectTypeEnum.Array)
                    {
                        if (record.objectPositionEnum == ObjectPositionEnum.Top)
                        {
                            this.TopObject = record.newObj;
                        }
                        this.RegisterObject(record.newObj, record, objectPr);
                    }
                    else
                    {
                        record.objectInfo.PopulateObjectMembers(record.newObj, record.memberData);
                        if (!record.isRegistered && (record.objectId > 0L))
                        {
                            this.RegisterObject(record.newObj, record, objectPr);
                        }
                        if (record.isValueTypeFixup)
                        {
                            ((ValueFixup)this.ValueFixupStack.Pop()).Fixup(record, objectPr);
                        }
                        if (record.objectPositionEnum == ObjectPositionEnum.Top)
                        {
                            this.TopObject = record.newObj;
                        }
                        record.objectInfo.ObjectEnd();
                    }
                }
            }
        }

        private void ParseSerializedStreamHeader(ParseRecord pr)
        {
            this.stack.Push(pr);
        }

        private void ParseSerializedStreamHeaderEnd(ParseRecord pr)
        {
            this.stack.Pop();
        }

        private void ParseString(ParseRecord pr, ParseRecord parentPr)
        {
            if (!pr.isRegistered && (pr.objectId > 0L))
            {
                this.RegisterObject(pr.value, pr, parentPr, true);
            }
        }

        private void RegisterObject(object obj, ParseRecord pr, ParseRecord objectPr)
        {
            this.RegisterObject(obj, pr, objectPr, false);
        }

        private void RegisterObject(object obj, ParseRecord pr, ParseRecord objectPr, bool bIsString)
        {
            if (!pr.isRegistered)
            {
                pr.isRegistered = true;
                SerializationInfo info = null;
                long idOfContainingObj = 0L;
                MemberInfo member = null;
                int[] arrayIndex = null;
                if (objectPr != null)
                {
                    arrayIndex = objectPr.indexMap;
                    idOfContainingObj = objectPr.objectId;
                    if ((objectPr.objectInfo != null) && !objectPr.objectInfo.IsSi)
                    {
                        member = objectPr.objectInfo.GetMemberInfo(pr.name);
                    }
                }
                info = pr.si;
                if (bIsString)
                {
                    this.objectManager.RegisterString((string)((string)obj), pr.objectId, info, idOfContainingObj, member);
                }
                else
                {
                    this.objectManager.RegisterObject(obj, pr.objectId, info, idOfContainingObj, member, arrayIndex);
                }
            }
        }
    }
}
