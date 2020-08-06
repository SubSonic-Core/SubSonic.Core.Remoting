using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class WriteObjectInfo
    {
        internal int _objectInfoId;
        internal object _obj;
        internal Type _objectType;
        internal bool _isSi;
        internal bool _isNamed;
        internal bool _isArray;
        internal SerializationInfo _si;
        internal SerializationObjectInfoCache _cache;
        internal object[] _memberData;
        internal ISerializationSurrogate _serializationSurrogate;
        internal StreamingContext _context;
        internal SerializationObjectInfo _serObjectInfoInit;
        internal long _objectId;
        internal long _assemId;
        private string _binderTypeName;
        private string _binderAssemblyString;

        internal WriteObjectInfo()
        {
        }

        private static void CheckTypeForwardedFrom(SerializationObjectInfoCache cache, Type objectType, string binderAssemblyString)
        {
        }

        internal string GetAssemblyString()
        {
            return _binderAssemblyString ?? _cache.AssemblyString;
        }

        internal void GetMemberInfo(out string[] outMemberNames, out Type[] outMemberTypes, out object[] outMemberData)
        {
            outMemberNames = _cache.MemberNames;
            outMemberTypes = _cache.MemberTypes;
            outMemberData = _memberData;
            if (_isSi && !_isNamed)
            {
                throw new SerializationException(RemotingResources.SerializationMemberInfo);
            }
        }

        private static WriteObjectInfo GetObjectInfo(SerializationObjectInfo serObjectInfoInit)
        {
            WriteObjectInfo info;
            if (!serObjectInfoInit._oiPool.IsEmpty())
            {
                info = (WriteObjectInfo)serObjectInfoInit._oiPool.Pop();
                info.InternalInit();
            }
            else
            {
                info = new WriteObjectInfo();
                int num = serObjectInfoInit._objectInfoIdCount;
                serObjectInfoInit._objectInfoIdCount = num + 1;
                info._objectInfoId = num;
            }
            return info;
        }

        internal string GetTypeFullName()
        {
            return _binderTypeName ?? _cache.FullTypeName;
        }

        private void InitMemberInfo()
        {
            if (!_serObjectInfoInit.SeenBeforeTable.TryGetValue(_objectType, out _cache))
            {
                _cache = new SerializationObjectInfoCache(_objectType)
                {
                    MemberInfos = FormatterServices.GetSerializableMembers(_objectType, _context)
                };
                int length = _cache.MemberInfos.Length;
                _cache.MemberNames = new string[length];
                _cache.MemberTypes = new Type[length];
                int index = 0;
                while (true)
                {
                    if (index >= length)
                    {
                        _serObjectInfoInit.SeenBeforeTable.Add(_objectType, _cache);
                        break;
                    }
                    _cache.MemberNames[index] = _cache.MemberInfos[index].Name;
                    _cache.MemberTypes[index] = ((FieldInfo)_cache.MemberInfos[index]).FieldType;
                    index++;
                }
            }
            if (_obj != null)
            {
                _memberData = FormatterServices.GetObjectData(_obj, _cache.MemberInfos);
            }
            _isNamed = true;
        }

        private void InitNoMembers()
        {
            if (!_serObjectInfoInit.SeenBeforeTable.TryGetValue(_objectType, out _cache))
            {
                _cache = new SerializationObjectInfoCache(_objectType);
                _serObjectInfoInit.SeenBeforeTable.Add(_objectType, _cache);
            }
        }

        internal void InitSerialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, SerializationBinder binder)
        {
            _objectType = objectType;
            _context = context;
            _serObjectInfoInit = serObjectInfoInit;
            if (objectType.IsArray)
            {
                InitNoMembers();
            }
            else
            {
                InvokeSerializationBinder(binder);

                if (surrogateSelector != null)
                {
                    _serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out _);
                }
                if (_serializationSurrogate != null)
                {
                    _si = new SerializationInfo(objectType, converter);
                    _cache = new SerializationObjectInfoCache(objectType);
                    _isSi = true;
                }
                else if (!ReferenceEquals(objectType, Converter.s_typeofObject) && Converter.s_typeofISerializable.IsAssignableFrom(objectType))
                {
                    _si = new SerializationInfo(objectType, converter);
                    _cache = new SerializationObjectInfoCache(objectType);
                    CheckTypeForwardedFrom(_cache, objectType, _binderAssemblyString);
                    _isSi = true;
                }
                if (!_isSi)
                {
                    InitMemberInfo();
                    CheckTypeForwardedFrom(_cache, objectType, _binderAssemblyString);
                }
            }
        }

        internal void InitSerialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter, SerializationBinder binder)
        {
            _context = context;
            _obj = obj;
            _serObjectInfoInit = serObjectInfoInit;
            _objectType = obj.GetType();
            if (_objectType.IsArray)
            {
                _isArray = true;
                InitNoMembers();
            }
            else
            {
                ISurrogateSelector selector;
                InvokeSerializationBinder(binder);
                objectWriter.ObjectManager.RegisterObject(obj);
                if (surrogateSelector != null && (_serializationSurrogate = surrogateSelector.GetSurrogate(_objectType, context, out selector)) != null)
                {
                    _si = new SerializationInfo(_objectType, converter);
                    if (!_objectType.IsPrimitive)
                    {
                        _serializationSurrogate.GetObjectData(obj, _si, context);
                    }
                    InitSiWrite();
                }
                else if (obj is ISerializable serializable)
                {
                    if (!_objectType.IsSerializable)
                    {
                        throw new SerializationException(RemotingResources.NotMarkedForSerialization.Format(_objectType.FullName, _objectType.Assembly.FullName));
                    }
                    _si = new SerializationInfo(_objectType, converter);
                    serializable.GetObjectData(_si, context);
                    InitSiWrite();
                    CheckTypeForwardedFrom(_cache, _objectType, _binderAssemblyString);
                }
                else
                {
                    InitMemberInfo();
                    CheckTypeForwardedFrom(_cache, _objectType, _binderAssemblyString);

                    
                }
            }
        }

        private void InitSiWrite()
        {
            SerializationInfoEnumerator enumerator = null;
            _isSi = true;
            enumerator = _si.GetEnumerator();
            int memberCount = _si.MemberCount;
            TypeInformation typeInformation = null;
            string fullTypeName = _si.FullTypeName;
            string assemblyName = _si.AssemblyName;
            bool hasTypeForwardedFrom = false;
            if (!_si.IsFullTypeNameSetExplicit)
            {
                typeInformation = BinaryFormatter.GetTypeInformation(_si.ObjectType);
                fullTypeName = typeInformation.FullTypeName;
                hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
            }
            if (!_si.IsAssemblyNameSetExplicit)
            {
                if (typeInformation == null)
                {
                    typeInformation = BinaryFormatter.GetTypeInformation(_si.ObjectType);
                }
                assemblyName = typeInformation.AssemblyString;
                hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
            }
            _cache = new SerializationObjectInfoCache(fullTypeName, assemblyName, hasTypeForwardedFrom)
            {
                MemberNames = new string[memberCount],
                MemberTypes = new Type[memberCount]
            };
            _memberData = new object[memberCount];
            enumerator = _si.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                _cache.MemberNames[i] = enumerator.Name;
                _cache.MemberTypes[i] = enumerator.ObjectType;
                _memberData[i] = enumerator.Value;
            }
            _isNamed = true;
        }

        private void InternalInit()
        {
            _obj = null;
            _objectType = null;
            _isSi = false;
            _isNamed = false;
            _isArray = false;
            _si = null;
            _cache = null;
            _memberData = null;
            _objectId = 0L;
            _assemId = 0L;
            _binderTypeName = null;
            _binderAssemblyString = null;
        }

        private void InvokeSerializationBinder(SerializationBinder binder)
        {
            if (binder != null)
            {
                binder.BindToName(_objectType, out _binderAssemblyString, out _binderTypeName);
            }
        }

        internal void ObjectEnd()
        {
            PutObjectInfo(_serObjectInfoInit, this);
        }

        private static void PutObjectInfo(SerializationObjectInfo serObjectInfoInit, WriteObjectInfo objectInfo)
        {
            serObjectInfoInit._oiPool.Push(objectInfo);
        }

        internal static WriteObjectInfo Serialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, SerializationBinder binder)
        {
            WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.InitSerialize(objectType, surrogateSelector, context, serObjectInfoInit, converter, binder);
            return objectInfo;
        }

        internal static WriteObjectInfo Serialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerializationObjectInfo serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter, SerializationBinder binder)
        {
            WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
            objectInfo.InitSerialize(obj, surrogateSelector, context, serObjectInfoInit, converter, objectWriter, binder);
            return objectInfo;
        }
    }
}