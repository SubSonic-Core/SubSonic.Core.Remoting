using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal static class Converter
    {
        internal static readonly Type s_typeofISerializable = typeof(ISerializable);
        internal static readonly Type s_typeofString = typeof(string);
        //internal static readonly Type s_typeofConverter = typeof(Converter);
        //internal static readonly Type s_typeofBoolean = typeof(bool);
        internal static readonly Type s_typeofByte = typeof(byte);
        //internal static readonly Type s_typeofChar = typeof(char);
        internal static readonly Type s_typeofDecimal = typeof(decimal);
        //internal static readonly Type s_typeofDouble = typeof(double);
        //internal static readonly Type s_typeofInt16 = typeof(short);
        //internal static readonly Type s_typeofInt32 = typeof(int);
        //internal static readonly Type s_typeofInt64 = typeof(long);
        //internal static readonly Type s_typeofSByte = typeof(sbyte);
        //internal static readonly Type s_typeofSingle = typeof(float);
        internal static readonly Type s_typeofTimeSpan = typeof(TimeSpan);
        internal static readonly Type s_typeofDateTime = typeof(DateTime);
        //internal static readonly Type s_typeofUInt16 = typeof(ushort);
        //internal static readonly Type s_typeofUInt32 = typeof(uint);
        //internal static readonly Type s_typeofUInt64 = typeof(ulong);
        internal static readonly Type s_typeofObject = typeof(object);
        internal static readonly Type s_typeofSystemVoid = typeof(void);
        internal static readonly Assembly s_urtAssembly = Assembly.Load("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        internal static readonly string s_urtAssemblyString = s_urtAssembly.FullName;
        internal static readonly Assembly s_urtAlternativeAssembly = s_typeofString.Assembly;
        internal static readonly string s_urtAlternativeAssemblyString = s_urtAlternativeAssembly.FullName;
        //internal static readonly Type s_typeofTypeArray = typeof(Type[]);
        internal static readonly Type s_typeofObjectArray = typeof(object[]);
        internal static readonly Type s_typeofStringArray = typeof(string[]);
        //internal static readonly Type s_typeofBooleanArray = typeof(bool[]);
        //internal static readonly Type s_typeofByteArray = typeof(byte[]);
        //internal static readonly Type s_typeofCharArray = typeof(char[]);
        //internal static readonly Type s_typeofDecimalArray = typeof(decimal[]);
        //internal static readonly Type s_typeofDoubleArray = typeof(double[]);
        //internal static readonly Type s_typeofInt16Array = typeof(short[]);
        //internal static readonly Type s_typeofInt32Array = typeof(int[]);
        //internal static readonly Type s_typeofInt64Array = typeof(long[]);
        //internal static readonly Type s_typeofSByteArray = typeof(sbyte[]);
        //internal static readonly Type s_typeofSingleArray = typeof(float[]);
        //internal static readonly Type s_typeofTimeSpanArray = typeof(TimeSpan[]);
        //internal static readonly Type s_typeofDateTimeArray = typeof(DateTime[]);
        //internal static readonly Type s_typeofUInt16Array = typeof(ushort[]);
        //internal static readonly Type s_typeofUInt32Array = typeof(uint[]);
        //internal static readonly Type s_typeofUInt64Array = typeof(ulong[]);
        //internal static readonly Type s_typeofMarshalByRefObject = typeof(MarshalByRefObject);
        private static readonly Hashtable s_PrimitiveTypeToTypeLookup;
        private static readonly Hashtable s_PrimitiveTypeToArrayTypeLookup;
        private static readonly Hashtable s_PrimitiveTypeToStringLookup;
        private static readonly Hashtable s_PrimitiveTypeToTypeCodeLookup;
        private static readonly Hashtable s_TypeCodeToPrimitiveTypeLookup;

        /// <summary>
        /// Initialize the look up tables before anyone comes a calling
        /// </summary>
        static Converter()
        {
            Hashtable
                PrimitiveTypeToTypeLookup = new Hashtable(),
                PrimitiveTypeToArrayTypeLookup = new Hashtable(),
                PrimitiveTypeToStringLookup = new Hashtable(),
                PrimitiveTypeToTypeCodeLookup = new Hashtable(),
                TypeCodeToPrimitiveTypeLookup = new Hashtable();

            foreach (string type in Enum.GetNames(typeof(PrimitiveTypeEnum)))
            {
                PrimitiveTypeEnum primitiveType = type.Parse<PrimitiveTypeEnum>();

                PrimitiveTypeToStringLookup[primitiveType] = type;

                if (type.Equals(nameof(PrimitiveTypeEnum.Invalid), StringComparison.Ordinal))
                {
                    continue;
                }

                Type realType = Type.GetType($"System.{type}");
                TypeCode typeCode = Type.GetTypeCode(realType);

                PrimitiveTypeToTypeLookup[primitiveType] = realType;
                PrimitiveTypeToTypeCodeLookup[primitiveType] = typeCode;
                TypeCodeToPrimitiveTypeLookup[typeCode] = primitiveType;

                if (type.Equals(nameof(PrimitiveTypeEnum.Null), StringComparison.Ordinal))
                {
                    continue;
                }

                PrimitiveTypeToArrayTypeLookup[primitiveType] = realType.MakeArrayType();
            }

            s_PrimitiveTypeToTypeLookup = PrimitiveTypeToTypeLookup;
            s_PrimitiveTypeToArrayTypeLookup = PrimitiveTypeToArrayTypeLookup;
            s_PrimitiveTypeToStringLookup = PrimitiveTypeToStringLookup;
            s_PrimitiveTypeToTypeCodeLookup = PrimitiveTypeToTypeCodeLookup;
            s_TypeCodeToPrimitiveTypeLookup = TypeCodeToPrimitiveTypeLookup;
        }

        public static Array CreatePrimitiveArray(PrimitiveTypeEnum code, int length)
        {
            if( Activator.CreateInstance((Type)s_PrimitiveTypeToArrayTypeLookup[code], length) is Array success)
            {
                return success;
            }
            return default;
        }

        public static object FromString(string value, PrimitiveTypeEnum code)
        {
            return ((code != PrimitiveTypeEnum.Invalid) ? Convert.ChangeType(value, ToTypeCode(code), CultureInfo.InvariantCulture) : value);
        }

        public static bool IsPrimitiveArray(Type type, out PrimitiveTypeEnum typeInformation)
        {
            bool success = true;

            foreach(DictionaryEntry entry in s_PrimitiveTypeToArrayTypeLookup)
            {
                if (object.ReferenceEquals(type, entry.Value))
                {
                    typeInformation = (PrimitiveTypeEnum)entry.Key;
                    return success;
                }
            }

            typeInformation = PrimitiveTypeEnum.Invalid;
            success = false;

            return success;
        }

        internal static bool IsWriteAsByteArray(PrimitiveTypeEnum code)
        {
            switch (code)
            {
                case PrimitiveTypeEnum.Boolean:
                case PrimitiveTypeEnum.Byte:
                case PrimitiveTypeEnum.Char:
                case PrimitiveTypeEnum.Double:
                case PrimitiveTypeEnum.Int16:
                case PrimitiveTypeEnum.Int32:
                case PrimitiveTypeEnum.Int64:
                case PrimitiveTypeEnum.SByte:
                case PrimitiveTypeEnum.Single:
                case PrimitiveTypeEnum.UInt16:
                case PrimitiveTypeEnum.UInt32:
                case PrimitiveTypeEnum.UInt64:
                    return true;
            }
            return false;
        }

        public static Type ToArrayType(PrimitiveTypeEnum code)
        {
            return (Type)s_PrimitiveTypeToArrayTypeLookup[code];
        }

        public static PrimitiveTypeEnum ToCode(Type type)
        {
            return ((type == null) ? ToPrimitiveTypeEnum(TypeCode.Empty) : (type.IsPrimitive ? ToPrimitiveTypeEnum(Type.GetTypeCode(type)) : (object.ReferenceEquals(type, s_typeofDateTime) ? PrimitiveTypeEnum.DateTime : (object.ReferenceEquals(type, s_typeofTimeSpan) ? PrimitiveTypeEnum.TimeSpan : (object.ReferenceEquals(type, s_typeofDecimal) ? PrimitiveTypeEnum.Decimal : PrimitiveTypeEnum.Invalid)))));
        }

        public static string ToComType(PrimitiveTypeEnum code)
        {
            return (string)s_PrimitiveTypeToStringLookup[code];
        }

        public static PrimitiveTypeEnum ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            return (PrimitiveTypeEnum)s_TypeCodeToPrimitiveTypeLookup[typeCode];
        }

        public static Type ToType(PrimitiveTypeEnum code)
        {
            return (Type)s_PrimitiveTypeToTypeLookup[code];
        }

        public static TypeCode ToTypeCode(PrimitiveTypeEnum code)
        {
            return (TypeCode)s_PrimitiveTypeToTypeCodeLookup[code];
        }

        public static int TypeLength(PrimitiveTypeEnum code)
        {
#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (code)
#pragma warning restore IDE0066 // Convert switch statement to expression
            {
                case PrimitiveTypeEnum.Boolean:
                    return 1;

                case PrimitiveTypeEnum.Byte:
                    return 1;

                case PrimitiveTypeEnum.Char:
                    return 2;

                case PrimitiveTypeEnum.Double:
                    return 8;

                case PrimitiveTypeEnum.Int16:
                    return 2;

                case PrimitiveTypeEnum.Int32:
                    return 4;

                case PrimitiveTypeEnum.Int64:
                    return 8;

                case PrimitiveTypeEnum.SByte:
                    return 1;

                case PrimitiveTypeEnum.Single:
                    return 4;

                case PrimitiveTypeEnum.UInt16:
                    return 2;

                case PrimitiveTypeEnum.UInt32:
                    return 4;

                case PrimitiveTypeEnum.UInt64:
                    return 8;
            }
            return 0;
        }
    }
}
