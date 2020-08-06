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
        private static volatile Hashtable s_PrimitiveTypeToTypeLookup;
        private static volatile Hashtable s_PrimitiveTypeToArrayTypeLookup;
        private static volatile Hashtable s_PrimitiveTypeToStringLookup;
        private static volatile Hashtable s_PrimitiveTypeToTypeCodeLookup;
        private static volatile Hashtable s_PrimitiveTypes;

        internal static Array CreatePrimitiveArray(PrimitiveTypeEnum code, int length)
        {
#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (code)
#pragma warning restore IDE0066 // Convert switch statement to expression
            {
                case PrimitiveTypeEnum.Boolean:
                    return new bool[length];

                case PrimitiveTypeEnum.Byte:
                    return new byte[length];

                case PrimitiveTypeEnum.Char:
                    return new char[length];

                case PrimitiveTypeEnum.Decimal:
                    return new decimal[length];

                case PrimitiveTypeEnum.Double:
                    return new double[length];

                case PrimitiveTypeEnum.Int16:
                    return new short[length];

                case PrimitiveTypeEnum.Int32:
                    return new int[length];

                case PrimitiveTypeEnum.Int64:
                    return new long[length];

                case PrimitiveTypeEnum.SByte:
                    return new sbyte[length];

                case PrimitiveTypeEnum.Single:
                    return new float[length];

                case PrimitiveTypeEnum.TimeSpan:
                    return new TimeSpan[length];

                case PrimitiveTypeEnum.DateTime:
                    return new DateTime[length];

                case PrimitiveTypeEnum.UInt16:
                    return new ushort[length];

                case PrimitiveTypeEnum.UInt32:
                    return new uint[length];

                case PrimitiveTypeEnum.UInt64:
                    return new ulong[length];
            }
            return null;
        }

        internal static object FromString(string value, PrimitiveTypeEnum code)
        {
            return ((code != PrimitiveTypeEnum.Invalid) ? Convert.ChangeType(value, ToTypeCode(code), CultureInfo.InvariantCulture) : value);
        }

        private static void InitializePrimitiveTypeToArrayType()
        {
            Hashtable lookup = new Hashtable();

            foreach (string type in Enum.GetNames(typeof(PrimitiveTypeEnum)))
            {
                if (type.Equals(nameof(PrimitiveTypeEnum.Invalid), StringComparison.Ordinal) ||
                    type.Equals(nameof(PrimitiveTypeEnum.Null), StringComparison.Ordinal))
                {
                    continue;
                }

                lookup[Enum.Parse(typeof(PrimitiveTypeEnum), type)] = Type.GetType($"System.{type}").MakeArrayType();
            }

            s_PrimitiveTypeToArrayTypeLookup = lookup;
        }

        private static void InitializeTypeCodeToPrimitiveTypeLookup()
        {
            Hashtable lookup = new Hashtable();

            foreach(string type in Enum.GetNames(typeof(PrimitiveTypeEnum)))
            {
                if (type.Equals(nameof(PrimitiveTypeEnum.Invalid), StringComparison.Ordinal))
                {
                    continue;
                }

                Type realType = Type.GetType($"System.{type}");

                lookup[Type.GetTypeCode(realType)] = Enum.Parse(typeof(PrimitiveTypeEnum), type);
            }            

            s_PrimitiveTypes = lookup;
        }

        private static void InitializePrimitiveTypeToTypeCodeLookup()
        {
            Hashtable lookup = new Hashtable();

            foreach (string type in Enum.GetNames(typeof(PrimitiveTypeEnum)))
            {
                if (type.Equals(nameof(PrimitiveTypeEnum.Invalid), StringComparison.Ordinal))
                {
                    continue;
                }

                Type realType = Type.GetType($"System.{type}");

                lookup[Enum.Parse(typeof(PrimitiveTypeEnum), type)] = Type.GetTypeCode(realType);
            }

            s_PrimitiveTypeToTypeCodeLookup = lookup;
        }

        private static void InitializePrimitiveTypeToTypeLookup()
        {
            Hashtable lookup = new Hashtable();

            foreach (string type in Enum.GetNames(typeof(PrimitiveTypeEnum)))
            {
                if (type.Equals(nameof(PrimitiveTypeEnum.Invalid), StringComparison.Ordinal))
                {
                    continue;
                }

                lookup[Enum.Parse(typeof(PrimitiveTypeEnum), type)] = Type.GetType($"System.{type}");
            }

            s_PrimitiveTypeToTypeLookup = lookup;
        }

        private static void InitializePrimitiveTypeToStringLookup()
        {
            Hashtable lookup = new Hashtable();

            foreach(string type in Enum.GetNames(typeof(PrimitiveTypeEnum)))
            {
                lookup[Enum.Parse(typeof(PrimitiveTypeEnum), type)] = type;
            }

            s_PrimitiveTypeToStringLookup = lookup;
        }

        internal static bool IsPrimitiveArray(Type type, out PrimitiveTypeEnum typeInformation)
        {
            bool success = true;

            if (s_PrimitiveTypeToArrayTypeLookup == null)
            {
                InitializePrimitiveTypeToArrayType();
            }

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
            if (s_PrimitiveTypeToArrayTypeLookup == null)
            {
                InitializePrimitiveTypeToArrayType();
            }
            return (Type)s_PrimitiveTypeToArrayTypeLookup[code];
        }

        public static PrimitiveTypeEnum ToCode(Type type)
        {
            return ((type == null) ? ToPrimitiveTypeEnum(TypeCode.Empty) : (type.IsPrimitive ? ToPrimitiveTypeEnum(Type.GetTypeCode(type)) : (object.ReferenceEquals(type, s_typeofDateTime) ? PrimitiveTypeEnum.DateTime : (object.ReferenceEquals(type, s_typeofTimeSpan) ? PrimitiveTypeEnum.TimeSpan : (object.ReferenceEquals(type, s_typeofDecimal) ? PrimitiveTypeEnum.Decimal : PrimitiveTypeEnum.Invalid)))));
        }

        public static string ToComType(PrimitiveTypeEnum code)
        {
            if (s_PrimitiveTypeToStringLookup == null)
            {
                InitializePrimitiveTypeToStringLookup();
            }
            return (string)s_PrimitiveTypeToStringLookup[code];
        }

        public static PrimitiveTypeEnum ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            if (s_PrimitiveTypes == null)
            {
                InitializeTypeCodeToPrimitiveTypeLookup();
            }

            return (PrimitiveTypeEnum)s_PrimitiveTypes[typeCode];
        }

        public static Type ToType(PrimitiveTypeEnum code)
        {
            if (s_PrimitiveTypeToTypeLookup == null)
            {
                InitializePrimitiveTypeToTypeLookup();
            }
            return (Type)s_PrimitiveTypeToTypeLookup[code];
        }

        public static TypeCode ToTypeCode(PrimitiveTypeEnum code)
        {
            if (s_PrimitiveTypeToTypeCodeLookup == null)
            {
                InitializePrimitiveTypeToTypeCodeLookup();
            }
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
