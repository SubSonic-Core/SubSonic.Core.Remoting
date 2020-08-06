using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class ObjectMap
    {
        internal string _objectName;
        internal Type _objectType;
        internal BinaryTypeEnum[] _binaryTypeEnumA;
        internal object[] _typeInformationA;
        internal Type[] _memberTypes;
        internal string[] _memberNames;
        internal ReadObjectInfo _objectInfo;
        internal bool _isInitObjectInfo;
        internal ObjectReader _objectReader;
        internal int _objectId;
        internal BinaryAssemblyInfo _assemblyInfo;

        public ObjectMap(string objectName, Type objectType, string[] memberNames, ObjectReader objectReader, int objectId, BinaryAssemblyInfo assemblyInfo)
        {
            this._isInitObjectInfo = true;
            this._objectName = objectName;
            this._objectType = objectType;
            this._memberNames = memberNames;
            this._objectReader = objectReader;
            this._objectId = objectId;
            this._assemblyInfo = assemblyInfo;
            this._objectInfo = objectReader.CreateReadObjectInfo(objectType);
            this._memberTypes = this._objectInfo.GetMemberTypes(memberNames, objectType);
            this._binaryTypeEnumA = new BinaryTypeEnum[this._memberTypes.Length];
            this._typeInformationA = new object[this._memberTypes.Length];
            for (int i = 0; i < this._memberTypes.Length; i++)
            {
                this._binaryTypeEnumA[i] = BinaryTypeConverter.GetParserBinaryTypeInfo(this._memberTypes[i], out object typeInformation);
                this._typeInformationA[i] = typeInformation;
            }
        }

        public ObjectMap(string objectName, string[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, object[] typeInformationA, int[] memberAssemIds, ObjectReader objectReader, int objectId, BinaryAssemblyInfo assemblyInfo, SizedArray assemIdToAssemblyTable)
        {
            this._isInitObjectInfo = true;
            this._objectName = objectName;
            this._memberNames = memberNames;
            this._binaryTypeEnumA = binaryTypeEnumA;
            this._typeInformationA = typeInformationA;
            this._objectReader = objectReader;
            this._objectId = objectId;
            this._assemblyInfo = assemblyInfo;
            if (assemblyInfo == null)
            {
                throw new SerializationException(RemotingResources.SerializationAssemblyNotFound.Format(objectName));
            }
            this._objectType = objectReader.GetType(assemblyInfo, objectName);
            this._memberTypes = new Type[memberNames.Length];
            for (int i = 0; i < memberNames.Length; i++)
            {
                BinaryTypeConverter.TypeFromInfo(binaryTypeEnumA[i], typeInformationA[i], objectReader, (BinaryAssemblyInfo)assemIdToAssemblyTable[memberAssemIds[i]], out _, out _, out Type type, out _);
                this._memberTypes[i] = type;
            }
            this._objectInfo = objectReader.CreateReadObjectInfo(this._objectType, memberNames, null);
            if (!this._objectInfo.IsSi)
            {
                this._objectInfo.GetMemberTypes(memberNames, this._objectInfo.ObjectType);
            }
        }

        public static ObjectMap Create(string name, Type objectType, string[] memberNames, ObjectReader objectReader, int objectId, BinaryAssemblyInfo assemblyInfo)
        {
            return new ObjectMap(name, objectType, memberNames, objectReader, objectId, assemblyInfo);
        }

        public static ObjectMap Create(string name, string[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, object[] typeInformationA, int[] memberAssemIds, ObjectReader objectReader, int objectId, BinaryAssemblyInfo assemblyInfo, SizedArray assemIdToAssemblyTable)
        {
            return new ObjectMap(name, memberNames, binaryTypeEnumA, typeInformationA, memberAssemIds, objectReader, objectId, assemblyInfo, assemIdToAssemblyTable);
        }

        public ReadObjectInfo CreateObjectInfo(ref SerializationInfo si, ref object[] memberData)
        {
            if (this._isInitObjectInfo)
            {
                this._isInitObjectInfo = false;
                this._objectInfo.InitDataStore(ref si, ref memberData);
                return this._objectInfo;
            }
            this._objectInfo.PrepareForReuse();
            this._objectInfo.InitDataStore(ref si, ref memberData);
            return this._objectInfo;
        }
    }
}
