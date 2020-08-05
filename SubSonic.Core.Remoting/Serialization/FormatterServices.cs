using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization
{
    public static class FormatterServices
    {
        private static readonly ConcurrentDictionary<MemberHolder, MemberInfo[]> s_memberInfoTable = new ConcurrentDictionary<MemberHolder, MemberInfo[]>();

        public static string GetClrAssemblyName(Type type, out bool hasTypeForwardedFrom)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Type elementType = type;
            while (elementType.HasElementType)
            {
                elementType = elementType.GetElementType();
            }
            object[] customAttributes = elementType.GetCustomAttributes(typeof(TypeForwardedFromAttribute), false);
            int index = 0;
            if (index >= customAttributes.Length)
            {
                hasTypeForwardedFrom = false;
                return type.Assembly.FullName;
            }
            Attribute attribute = (Attribute)customAttributes[index];
            hasTypeForwardedFrom = true;
            return ((TypeForwardedFromAttribute)attribute).AssemblyFullName;
        }

        public static string GetClrTypeFullName(Type type)
        {
            return (type.IsArray ? GetClrTypeFullNameForArray(type) : GetClrTypeFullNameForNonArrayTypes(type));
        }

        private static string GetClrTypeFullNameForArray(Type type)
        {
            int arrayRank = type.GetArrayRank();
            string clrTypeFullName = GetClrTypeFullName(type.GetElementType());
            return ((arrayRank == 1) ? (clrTypeFullName + "[]") : (clrTypeFullName + "[" + ((string)new string(',', arrayRank - 1)) + "]"));
        }

        private static string GetClrTypeFullNameForNonArrayTypes(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.FullName;
            }
            StringBuilder builder = new StringBuilder(type.GetGenericTypeDefinition().FullName).Append("[");
            foreach (Type type2 in type.GetGenericArguments())
            {
                bool flag;
                builder.Append("[").Append(GetClrTypeFullName(type2)).Append(", ");
                builder.Append(GetClrAssemblyName(type2, out flag)).Append("],");
            }
            return builder.Remove(builder.Length - 1, 1).Append("]").ToString();
        }

        public static Type GetTypeFromAssembly(Assembly assembly, string typeName)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return assembly.GetType(typeName, false, false);
        }

        public static MemberInfo[] GetSerializableMembers(Type type)
        {
            return GetSerializableMembers(type, new StreamingContext(StreamingContextStates.All));
        }

        public static MemberInfo[] GetSerializableMembers(Type type, StreamingContext context)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return s_memberInfoTable.GetOrAdd(new MemberHolder(type, context), delegate (MemberHolder mh) {
                return InternalGetSerializableMembers(mh.MemberType);
            });
        }

        private static FieldInfo[] InternalGetSerializableMembers(Type type)
        {
            if (type.IsInterface)
            {
                return Array.Empty<FieldInfo>();
            }
            if (!type.IsSerializable)
            {
                throw new SerializationException(System.SR.Format(System.SR.Serialization_NonSerType, type.FullName, type.Assembly.FullName));
            }
            FieldInfo[] serializableFields = GetSerializableFields(type);
            Type baseType = type.BaseType;
            if ((baseType != null) && (baseType != typeof(object)))
            {
                Type[] typeArray;
                int num;
                bool flag = GetParentTypes(baseType, out typeArray, out num);
                if (num > 0)
                {
                    List<FieldInfo> list = new List<FieldInfo>();
                    int index = 0;
                    while (true)
                    {
                        if (index >= num)
                        {
                            if ((list != null) && (list.Count > 0))
                            {
                                FieldInfo[] destinationArray = new FieldInfo[list.Count + serializableFields.Length];
                                Array.Copy(serializableFields, 0, destinationArray, 0, serializableFields.Length);
                                list.CopyTo(destinationArray, serializableFields.Length);
                                serializableFields = destinationArray;
                            }
                            break;
                        }
                        baseType = typeArray[index];
                        if (!baseType.IsSerializable)
                        {
                            throw new SerializationException(System.SR.Format(System.SR.Serialization_NonSerType, baseType.FullName, baseType.Module.Assembly.FullName));
                        }
                        FieldInfo[] fields = baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                        string namePrefix = flag ? baseType.Name : baseType.FullName;
                        FieldInfo[] infoArray3 = fields;
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 >= infoArray3.Length)
                            {
                                index++;
                                break;
                            }
                            FieldInfo field = infoArray3[num3];
                            if (!field.IsNotSerialized)
                            {
                                list.Add(new SerializationFieldInfo(field, namePrefix));
                            }
                            num3++;
                        }
                    }
                }
            }
            return serializableFields;
        }
    }
}
