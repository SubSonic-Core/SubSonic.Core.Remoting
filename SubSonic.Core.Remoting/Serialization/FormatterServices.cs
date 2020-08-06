using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using RuntimeFormatterServices = System.Runtime.Serialization.FormatterServices;

namespace SubSonic.Core.Remoting.Serialization
{
    public static class FormatterServices
    {
        public static string GetClrAssemblyName(Type type, out bool hasTypeForwardedFrom)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
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
                builder.Append("[").Append(GetClrTypeFullName(type2)).Append(", ");
                builder.Append(GetClrAssemblyName(type2, out _)).Append("],");
            }
            return builder.Remove(builder.Length - 1, 1).Append("]").ToString();
        }

        public static Type GetTypeFromAssembly(Assembly assembly, string typeName)
        {
            return RuntimeFormatterServices.GetTypeFromAssembly(assembly, typeName);
        }

        public static MemberInfo[] GetSerializableMembers(Type type)
        {
            return RuntimeFormatterServices.GetSerializableMembers(type);
        }

        public static MemberInfo[] GetSerializableMembers(Type type, StreamingContext context)
        {
            return RuntimeFormatterServices.GetSerializableMembers(type, context);
        }

        public static object[] GetObjectData(object obj, MemberInfo[] memberInfos)
        {
            return RuntimeFormatterServices.GetObjectData(obj, memberInfos);
        }

        public static Assembly LoadAssemblyFromStringNoThrow(string assemblyName)
        {
            Assembly assembly;
            try
            {
                assembly = LoadAssemblyFromString(assemblyName);
            }
            catch (Exception)
            {
                return null;
            }
            return assembly;
        }

        public static Assembly LoadAssemblyFromString(string assemblyName)
        {
            return Assembly.Load(new AssemblyName(assemblyName));
        }

        public static object PopulateObjectMembers(object obj, MemberInfo[] members, object[] data)
        {
            return RuntimeFormatterServices.PopulateObjectMembers(obj, members, data);
        }

        public static object GetUninitializedObject(Type type)
        {
            return RuntimeFormatterServices.GetUninitializedObject(type);
        }
    }
}
