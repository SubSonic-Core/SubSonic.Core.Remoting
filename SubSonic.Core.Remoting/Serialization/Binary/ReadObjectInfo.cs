using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class ReadObjectInfo
    {
        private int objectInfoId;
        private static int readObjectInfoCounter;
        private Type objectType;
        private ObjectManager objectManager;
        private int count;
        private bool isSi;
        private bool isTyped;
        private bool isSimpleAssembly;
        private SerializationObjectInfoCache cache;
        private string[] wireMemberNames;
        private Type[] wireMemberTypes;
        private int lastPosition;
        private ISerializationSurrogate serializationSurrogate;
        private StreamingContext context;
        private List<Type> memberTypesList;
        private SerializationObjectInfo serObjectInfoInit;
        private IFormatterConverter converter;

        public ReadObjectInfo()
        {
        }

        internal void AddValue(string name, object value, ref SerializationInfo si, ref object[] memberData)
        {
            if (this.isSi)
            {
                si.AddValue(name, value);
            }
            else
            {
                int index = this.Position(name);
                if (index != -1)
                {
                    memberData[index] = value;
                }
            }
        }

        internal static ReadObjectInfo Create(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            ReadObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.Init(objectType, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
            return objectInfo;
        }

        internal static ReadObjectInfo Create(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            ReadObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.Init(objectType, memberNames, memberTypes, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
            return objectInfo;
        }

        internal MemberInfo GetMemberInfo(string name)
        {
            string text2;
            if (this.cache == null)
            {
                return null;
            }
            if (this.isSi)
            {
                string text1;
                if (this.objectType != null)
                {
                    text1 = this.objectType.ToString();
                }
                else
                {
                    Type local1 = this.objectType;
                    text1 = null;
                }
                throw new SerializationException(System.SR.Format(System.SR.Serialization_MemberInfo, text1 + " " + name));
            }
            if (this.cache._memberInfos != null)
            {
                int index = this.Position(name);
                return ((index != -1) ? this.cache._memberInfos[index] : null);
            }
            if (this.objectType != null)
            {
                text2 = this.objectType.ToString();
            }
            else
            {
                Type local2 = this.objectType;
                text2 = null;
            }
            throw new SerializationException(System.SR.Format(System.SR.Serialization_NoMemberInfo, text2 + " " + name));
        }

        internal Type GetMemberType(MemberInfo objMember)
        {
            if (!(objMember is FieldInfo))
            {
                throw new SerializationException(System.SR.Format(System.SR.Serialization_SerMemberInfo, objMember.GetType()));
            }
            return ((FieldInfo)objMember).FieldType;
        }

        internal Type[] GetMemberTypes(string[] inMemberNames, Type objectType)
        {
            if (this.isSi)
            {
                throw new SerializationException(System.SR.Format(System.SR.Serialization_ISerializableTypes, objectType));
            }
            if (this.cache == null)
            {
                return null;
            }
            if (this.cache._memberTypes == null)
            {
                this.cache._memberTypes = new Type[this.count];
                for (int j = 0; j < this.count; j++)
                {
                    this.cache._memberTypes[j] = this.GetMemberType(this.cache._memberInfos[j]);
                }
            }
            bool flag = false;
            if (inMemberNames.Length < this.cache._memberInfos.Length)
            {
                flag = true;
            }
            Type[] typeArray = new Type[this.cache._memberInfos.Length];
            bool flag2 = false;
            for (int i = 0; i < this.cache._memberInfos.Length; i++)
            {
                if (!flag && inMemberNames[i].Equals(this.cache._memberInfos[i].Name))
                {
                    typeArray[i] = this.cache._memberTypes[i];
                }
                else
                {
                    flag2 = false;
                    int index = 0;
                    while (true)
                    {
                        if (index < inMemberNames.Length)
                        {
                            if (!this.cache._memberInfos[i].Name.Equals(inMemberNames[index]))
                            {
                                index++;
                                continue;
                            }
                            typeArray[i] = this.cache._memberTypes[i];
                            flag2 = true;
                        }
                        if (flag2 || (this.isSimpleAssembly || (this.cache._memberInfos[i].GetCustomAttribute(typeof(OptionalFieldAttribute), false) != null)))
                        {
                            break;
                        }
                        throw new SerializationException(System.SR.Format(System.SR.Serialization_MissingMember, this.cache._memberNames[i], objectType, typeof(OptionalFieldAttribute).FullName));
                    }
                }
            }
            return typeArray;
        }

        private static ReadObjectInfo GetObjectInfo(SerObjectInfoInit serObjectInfoInit)
        {
            return new ReadObjectInfo { objectInfoId = Interlocked.Increment(ref readObjectInfoCounter) };
        }

        internal Type GetType(string name)
        {
            string text1;
            int index = this.Position(name);
            if (index == -1)
            {
                return null;
            }
            Type type = this.isTyped ? this.cache._memberTypes[index] : this.memberTypesList[index];
            if (type != null)
            {
                return type;
            }
            if (this.objectType != null)
            {
                text1 = this.objectType.ToString();
            }
            else
            {
                Type local1 = this.objectType;
                text1 = null;
            }
            throw new SerializationException(System.SR.Format(System.SR.Serialization_ISerializableTypes, text1 + " " + name));
        }

        internal void Init(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            this.objectType = objectType;
            this.objectManager = objectManager;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.converter = converter;
            this.isSimpleAssembly = bSimpleAssembly;
            this.InitReadConstructor(objectType, surrogateSelector, context);
        }

        internal void Init(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            this.objectType = objectType;
            this.objectManager = objectManager;
            this.wireMemberNames = memberNames;
            this.wireMemberTypes = memberTypes;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.converter = converter;
            this.isSimpleAssembly = bSimpleAssembly;
            if (memberTypes != null)
            {
                this.isTyped = true;
            }
            if (objectType != null)
            {
                this.InitReadConstructor(objectType, surrogateSelector, context);
            }
        }

        internal void InitDataStore(ref SerializationInfo si, ref object[] memberData)
        {
            if (!this.isSi)
            {
                if ((memberData == null) && (this.cache != null))
                {
                    memberData = new object[this.cache._memberNames.Length];
                }
            }
            else if (si == null)
            {
                si = new SerializationInfo(this.objectType, this.converter);
            }
        }

        private void InitMemberInfo()
        {
            this.cache = new SerObjectInfoCache(this.objectType);
            this.cache._memberInfos = FormatterServices.GetSerializableMembers(this.objectType, this.context);
            this.count = this.cache._memberInfos.Length;
            this.cache._memberNames = new string[this.count];
            this.cache._memberTypes = new Type[this.count];
            for (int i = 0; i < this.count; i++)
            {
                this.cache._memberNames[i] = this.cache._memberInfos[i].Name;
                this.cache._memberTypes[i] = this.GetMemberType(this.cache._memberInfos[i]);
            }
            this.isTyped = true;
        }

        private void InitNoMembers()
        {
            this.cache = new SerObjectInfoCache(this.objectType);
        }

        private void InitReadConstructor(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context)
        {
            if (objectType.IsArray)
            {
                this.InitNoMembers();
            }
            else
            {
                ISurrogateSelector selector = null;
                if (surrogateSelector != null)
                {
                    this.serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out selector);
                }
                if (this.serializationSurrogate != null)
                {
                    this.isSi = true;
                }
                else if (!object.ReferenceEquals(objectType, Converter.s_typeofObject) && Converter.s_typeofISerializable.IsAssignableFrom(objectType))
                {
                    this.isSi = true;
                }
                if (this.isSi)
                {
                    this.InitSiRead();
                }
                else
                {
                    this.InitMemberInfo();
                }
            }
        }

        private void InitSiRead()
        {
            if (this.memberTypesList != null)
            {
                this.memberTypesList = new List<Type>(20);
            }
        }

        internal void ObjectEnd()
        {
        }

        internal void PopulateObjectMembers(object obj, object[] memberData)
        {
            if (!this.isSi && (memberData != null))
            {
                FormatterServices.PopulateObjectMembers(obj, this.cache._memberInfos, memberData);
            }
        }

        private int Position(string name)
        {
            if (this.cache != null)
            {
                if ((this.cache._memberNames.Length != 0) && this.cache._memberNames[this.lastPosition].Equals(name))
                {
                    return this.lastPosition;
                }
                int num = this.lastPosition + 1;
                this.lastPosition = num;
                if ((num < this.cache._memberNames.Length) && this.cache._memberNames[this.lastPosition].Equals(name))
                {
                    return this.lastPosition;
                }
                for (int i = 0; i < this.cache._memberNames.Length; i++)
                {
                    if (this.cache._memberNames[i].Equals(name))
                    {
                        this.lastPosition = i;
                        return this.lastPosition;
                    }
                }
                this.lastPosition = 0;
            }
            return -1;
        }

        internal void PrepareForReuse()
        {
            this.lastPosition = 0;
        }

        internal void RecordFixup(long objectId, string name, long idRef)
        {
            if (this.isSi)
            {
                if (this.objectManager == null)
                {
                    throw new SerializationException(System.SR.Serialization_CorruptedStream);
                }
                this.objectManager.RecordDelayedFixup(objectId, name, idRef);
            }
            else
            {
                int index = this.Position(name);
                if (index != -1)
                {
                    if (this.objectManager == null)
                    {
                        throw new SerializationException(System.SR.Serialization_CorruptedStream);
                    }
                    this.objectManager.RecordFixup(objectId, this.cache._memberInfos[index], idRef);
                }
            }
        }
    }
}
