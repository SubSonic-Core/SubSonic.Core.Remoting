using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization
{
    public static class FormatterServices
    {
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
    }
}
