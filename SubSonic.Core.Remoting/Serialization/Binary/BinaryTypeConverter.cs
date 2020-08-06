using System;
using System.Runtime.Serialization;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public static class BinaryTypeConverter
    {
        public static BinaryTypeEnum GetBinaryTypeInfo(Type type, WriteObjectInfo objectInfo, ObjectWriter objectWriter, out object typeInformation, out int assemId)
        {
            BinaryTypeEnum stringArray;
            assemId = 0;
            typeInformation = null;
            if (ReferenceEquals(type, Converter.s_typeofString))
            {
                stringArray = BinaryTypeEnum.String;
            }
            else if ((objectInfo == null || objectInfo != null && !objectInfo._isSi) && ReferenceEquals(type, Converter.s_typeofObject))
            {
                stringArray = BinaryTypeEnum.Object;
            }
            else if (ReferenceEquals(type, Converter.s_typeofStringArray))
            {
                stringArray = BinaryTypeEnum.StringArray;
            }
            else if (ReferenceEquals(type, Converter.s_typeofObjectArray))
            {
                stringArray = BinaryTypeEnum.ObjectArray;
            }
            else if (Converter.IsPrimitiveArray(type, out PrimitiveTypeEnum primitive))
            {
                typeInformation = primitive;
                stringArray = BinaryTypeEnum.PrimitiveArray;
            }
            else
            {
                PrimitiveTypeEnum ee = objectWriter.ToCode(type);
                if (ee != PrimitiveTypeEnum.Invalid)
                {
                    stringArray = BinaryTypeEnum.Primitive;
                    typeInformation = ee;
                }
                else
                {
                    string fullName;
                    if (objectInfo == null)
                    {
                        fullName = type.Assembly.FullName;
                        typeInformation = type.FullName;
                    }
                    else
                    {
                        fullName = objectInfo.GetAssemblyString();
                        typeInformation = objectInfo.GetTypeFullName();
                    }
                    if (fullName.Equals(Converter.s_urtAssemblyString) || fullName.Equals(Converter.s_urtAlternativeAssemblyString))
                    {
                        stringArray = BinaryTypeEnum.ObjectUrt;
                        assemId = 0;
                    }
                    else
                    {
                        stringArray = BinaryTypeEnum.ObjectUser;
                        assemId = (int)objectInfo._assemId;
                        if (assemId == 0)
                        {
                            throw new SerializationException(RemotingResources.SerializationAssemblyId.Format(typeInformation));
                        }
                    }
                }
            }
            return stringArray;
        }

        public static BinaryTypeEnum GetParserBinaryTypeInfo(Type type, out object typeInformation)
        {
            BinaryTypeEnum objectArray;
            typeInformation = null;
            if (ReferenceEquals(type, Converter.s_typeofString))
            {
                objectArray = BinaryTypeEnum.String;
            }
            else if (ReferenceEquals(type, Converter.s_typeofObject))
            {
                objectArray = BinaryTypeEnum.Object;
            }
            else if (ReferenceEquals(type, Converter.s_typeofObjectArray))
            {
                objectArray = BinaryTypeEnum.ObjectArray;
            }
            else if (ReferenceEquals(type, Converter.s_typeofStringArray))
            {
                objectArray = BinaryTypeEnum.StringArray;
            }
            else if (Converter.IsPrimitiveArray(type, out PrimitiveTypeEnum primitive))
            {
                typeInformation = primitive;
                objectArray = BinaryTypeEnum.PrimitiveArray;
            }
            else
            {
                PrimitiveTypeEnum ee = Converter.ToCode(type);
                if (ee == PrimitiveTypeEnum.Invalid)
                {
                    objectArray = type.Assembly == Converter.s_urtAssembly ? BinaryTypeEnum.ObjectUrt : BinaryTypeEnum.ObjectUser;
                    typeInformation = type.FullName;
                }
                else
                {
                    objectArray = BinaryTypeEnum.Primitive;
                    typeInformation = ee;
                }
            }
            return objectArray;
        }

        public static object ReadTypeInfo(BinaryTypeEnum binaryTypeEnum, BinaryParser input, out int assemId)
        {
            object obj2 = null;
            int num = 0;
            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                case BinaryTypeEnum.PrimitiveArray:
                    obj2 = (PrimitiveTypeEnum)input.ReadByte();
                    break;

                case BinaryTypeEnum.String:
                case BinaryTypeEnum.Object:
                case BinaryTypeEnum.ObjectArray:
                case BinaryTypeEnum.StringArray:
                    break;

                case BinaryTypeEnum.ObjectUrt:
                    obj2 = input.ReadString();
                    break;

                case BinaryTypeEnum.ObjectUser:
                    obj2 = input.ReadString();
                    num = input.ReadInt32();
                    break;

                default:
                    throw new SerializationException(RemotingResources.SerializationTypeRead.Format(binaryTypeEnum));
            }
            assemId = num;
            return obj2;
        }

        public static void TypeFromInfo(BinaryTypeEnum binaryTypeEnum, object typeInformation, ObjectReader objectReader, BinaryAssemblyInfo assemblyInfo, out PrimitiveTypeEnum primitiveTypeEnum, out string typeString, out Type type, out bool isVariant)
        {
            isVariant = false;
            primitiveTypeEnum = PrimitiveTypeEnum.Invalid;
            typeString = null;
            type = null;
            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                    primitiveTypeEnum = (PrimitiveTypeEnum)typeInformation;
                    typeString = Converter.ToComType(primitiveTypeEnum);
                    type = Converter.ToType(primitiveTypeEnum);
                    return;

                case BinaryTypeEnum.String:
                    type = Converter.s_typeofString;
                    return;

                case BinaryTypeEnum.Object:
                    type = Converter.s_typeofObject;
                    isVariant = true;
                    return;

                case BinaryTypeEnum.ObjectUrt:
                case BinaryTypeEnum.ObjectUser:
                    if (typeInformation != null)
                    {
                        typeString = typeInformation.ToString();
                        type = objectReader.GetType(assemblyInfo, typeString);
                        if (type == Converter.s_typeofObject)
                        {
                            isVariant = true;
                            return;
                        }
                    }
                    return;

                case BinaryTypeEnum.ObjectArray:
                    type = Converter.s_typeofObjectArray;
                    return;

                case BinaryTypeEnum.StringArray:
                    type = Converter.s_typeofStringArray;
                    return;

                case BinaryTypeEnum.PrimitiveArray:
                    primitiveTypeEnum = (PrimitiveTypeEnum)typeInformation;
                    type = Converter.ToArrayType(primitiveTypeEnum);
                    return;
            }
            throw new SerializationException(RemotingResources.SerializationTypeRead.Format(binaryTypeEnum));
        }

        internal static void WriteTypeInfo(BinaryTypeEnum binaryTypeEnum, object typeInformation, int assemId, BinaryFormatterWriter output)
        {
            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                case BinaryTypeEnum.PrimitiveArray:
                    output.Write((byte)(PrimitiveTypeEnum)typeInformation);
                    return;

                case BinaryTypeEnum.String:
                case BinaryTypeEnum.Object:
                case BinaryTypeEnum.ObjectArray:
                case BinaryTypeEnum.StringArray:
                    return;

                case BinaryTypeEnum.ObjectUrt:
                    output.Write(typeInformation.ToString());
                    return;

                case BinaryTypeEnum.ObjectUser:
                    output.Write(typeInformation.ToString());
                    output.Write(assemId);
                    return;
            }
            throw new SerializationException(RemotingResources.SerializationTypeWrite.Format(binaryTypeEnum));
        }
    }
}
