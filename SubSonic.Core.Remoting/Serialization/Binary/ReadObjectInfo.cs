﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class ReadObjectInfo
    {
        private int objectInfoId;
        private static int readObjectInfoCounter;
        private int count;
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

        public bool IsSi;

        public bool IsTyped;

        public ObjectManager ObjectManager;

        public Type ObjectType;

        public void AddValue(string name, object value, ref SerializationInfo si, ref object[] memberData)
        {
            if (this.IsSi)
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

        public static ReadObjectInfo Create(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            ReadObjectInfo objectInfo = GetObjectInfo();
            objectInfo.Init(objectType, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
            return objectInfo;
        }

        public static ReadObjectInfo Create(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            ReadObjectInfo objectInfo = GetObjectInfo();
            objectInfo.Init(objectType, memberNames, memberTypes, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
            return objectInfo;
        }

        public MemberInfo GetMemberInfo(string name)
        {
            string text2;
            if (this.cache == null)
            {
                return null;
            }
            if (this.IsSi)
            {
                string text1;
                if (this.ObjectType != null)
                {
                    text1 = this.ObjectType.ToString();
                }
                else
                {
                    text1 = null;
                }
                throw new SerializationException(RemotingResources.SerializationNoMemberInfo.Format(text1, name));
            }
            if (this.cache.MemberInfos != null)
            {
                int index = this.Position(name);
                return ((index != -1) ? this.cache.MemberInfos[index] : null);
            }
            if (this.ObjectType != null)
            {
                text2 = this.ObjectType.ToString();
            }
            else
            {
                text2 = null;
            }
            throw new SerializationException(RemotingResources.SerializationNoMemberInfo.Format(text2, name));
        }

        public Type GetMemberType(MemberInfo objMember)
        {
            if (objMember is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType;
            }
            throw new SerializationException(RemotingResources.SerializationMemberInfo.Format(objMember.GetType()));
        }
        
        public Type[] GetMemberTypes(string[] inMemberNames, Type objectType)
        {
            if (this.IsSi)
            {
                throw new SerializationException(RemotingResources.Serialization_ISerializableTypes.Format(objectType));
            }
            if (this.cache == null)
            {
                return null;
            }
            if (this.cache.MemberTypes == null)
            {
                this.cache.MemberTypes = new Type[this.count];
                for (int j = 0; j < this.count; j++)
                {
                    this.cache.MemberTypes[j] = this.GetMemberType(this.cache.MemberTypes[j]);
                }
            }
            bool flag = false;
            if (inMemberNames.Length < this.cache.MemberInfos.Length)
            {
                flag = true;
            }
            Type[] typeArray = new Type[this.cache.MemberInfos.Length];
            for (int i = 0; i < this.cache.MemberInfos.Length; i++)
            {
                if (!flag && inMemberNames[i].Equals(this.cache.MemberInfos[i].Name))
                {
                    typeArray[i] = this.cache.MemberTypes[i];
                }
                else
                {
                    bool flag2 = false;
                    int index = 0;
                    while (true)
                    {
                        if (index < inMemberNames.Length)
                        {
                            if (!this.cache.MemberInfos[i].Name.Equals(inMemberNames[index]))
                            {
                                index++;
                                continue;
                            }
                            typeArray[i] = this.cache.MemberTypes[i];
                            flag2 = true;
                        }
                        if (flag2 || (this.isSimpleAssembly || (this.cache.MemberInfos[i].GetCustomAttribute(typeof(OptionalFieldAttribute), false) != null)))
                        {
                            break;
                        }

                        throw new SerializationException(RemotingResources.SerializationMissingMember.Format(cache.MemberNames[i], objectType, typeof(OptionalFieldAttribute).FullName));
                    }
                }
            }
            return typeArray;
        }

        private static ReadObjectInfo GetObjectInfo(/*SerializationObjectInfo serObjectInfoInit*/)
        {
            return new ReadObjectInfo() { objectInfoId = Interlocked.Increment(ref readObjectInfoCounter) };
        }

        public Type GetType(string name)
        {
            string text1;
            int index = this.Position(name);
            if (index == -1)
            {
                return null;
            }
            Type type = this.IsTyped ? this.cache.MemberTypes[index] : this.memberTypesList[index];
            if (type != null)
            {
                return type;
            }
            if (this.ObjectType != null)
            {
                text1 = this.ObjectType.ToString();
            }
            else
            {
                text1 = null;
            }
            throw new SerializationException(RemotingResources.Serialization_ISerializableTypes.Format($"{text1} {name}"));
        }

        public void Init(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            this.ObjectType = objectType;
            this.ObjectManager = objectManager;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.converter = converter;
            this.isSimpleAssembly = bSimpleAssembly;
            this.InitReadConstructor(objectType, surrogateSelector, context);
        }

        public void Init(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            this.ObjectType = objectType;
            this.ObjectManager = objectManager;
            this.wireMemberNames = memberNames;
            this.wireMemberTypes = memberTypes;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.converter = converter;
            this.isSimpleAssembly = bSimpleAssembly;
            if (memberTypes != null)
            {
                this.IsTyped = true;
            }
            if (objectType != null)
            {
                this.InitReadConstructor(objectType, surrogateSelector, context);
            }
        }

        public void InitDataStore(ref SerializationInfo si, ref object[] memberData)
        {
            if (!this.IsSi)
            {
                if ((memberData == null) && (this.cache != null))
                {
                    memberData = new object[this.cache.MemberNames.Length];
                }
            }
            else if (si == null)
            {
                si = new SerializationInfo(this.ObjectType, this.converter);
            }
        }

        private void InitMemberInfo()
        {
            this.cache = new SerializationObjectInfoCache(this.ObjectType)
            {
                MemberInfos = FormatterServices.GetSerializableMembers(this.ObjectType, this.context)
            };
            this.count = this.cache.MemberInfos.Length;
            this.cache.MemberNames = new string[this.count];
            this.cache.MemberTypes = new Type[this.count];
            for (int i = 0; i < this.count; i++)
            {
                this.cache.MemberNames[i] = this.cache.MemberInfos[i].Name;
                this.cache.MemberTypes[i] = this.GetMemberType(this.cache.MemberInfos[i]);
            }
            this.IsTyped = true;
        }

        private void InitNoMembers()
        {
            this.cache = new SerializationObjectInfoCache(this.ObjectType);
        }

        private void InitReadConstructor(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context)
        {
            if (objectType.IsArray)
            {
                this.InitNoMembers();
            }
            else
            {
                if (surrogateSelector != null)
                {
                    this.serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out _);
                }
                if (this.serializationSurrogate != null)
                {
                    this.IsSi = true;
                }
                else if (!object.ReferenceEquals(objectType, Converter.s_typeofObject) && Converter.s_typeofISerializable.IsAssignableFrom(objectType))
                {
                    this.IsSi = true;
                }
                if (this.IsSi)
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

        public void ObjectEnd()
        {
        }

        public void PopulateObjectMembers(object obj, object[] memberData)
        {
            if (!this.IsSi && (memberData != null))
            {
                FormatterServices.PopulateObjectMembers(obj, this.cache.MemberInfos, memberData);
            }
        }

        private int Position(string name)
        {
            if (this.cache != null)
            {
                if ((this.cache.MemberNames.Length != 0) && this.cache.MemberNames[this.lastPosition].Equals(name))
                {
                    return this.lastPosition;
                }
                int num = this.lastPosition + 1;
                this.lastPosition = num;
                if ((num < this.cache.MemberNames.Length) && this.cache.MemberNames[this.lastPosition].Equals(name))
                {
                    return this.lastPosition;
                }
                for (int i = 0; i < this.cache.MemberNames.Length; i++)
                {
                    if (this.cache.MemberNames[i].Equals(name))
                    {
                        this.lastPosition = i;
                        return this.lastPosition;
                    }
                }
                this.lastPosition = 0;
            }
            return -1;
        }

        public void PrepareForReuse()
        {
            this.lastPosition = 0;
        }

        public void RecordFixup(long objectId, string name, long idRef)
        {
            if (this.IsSi)
            {
                if (this.ObjectManager == null)
                {
                    throw new SerializationException(RemotingResources.ThBinaryStreamHasBeenCorrupted);
                }
                this.ObjectManager.RecordDelayedFixup(objectId, name, idRef);
            }
            else
            {
                int index = this.Position(name);
                if (index != -1)
                {
                    if (this.ObjectManager == null)
                    {
                        throw new SerializationException(RemotingResources.ThBinaryStreamHasBeenCorrupted);
                    }
                    this.ObjectManager.RecordFixup(objectId, this.cache.MemberInfos[index], idRef);
                }
            }
        }
    }
}
