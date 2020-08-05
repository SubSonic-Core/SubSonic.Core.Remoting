using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal static class Converter
    {
        internal static readonly Type s_typeofISerializable = typeof(ISerializable);
        internal static readonly Type s_typeofString = typeof(string);
        internal static readonly Type s_typeofConverter = typeof(Converter);
        internal static readonly Type s_typeofBoolean = typeof(bool);
        internal static readonly Type s_typeofByte = typeof(byte);
        internal static readonly Type s_typeofChar = typeof(char);
        internal static readonly Type s_typeofDecimal = typeof(decimal);
        internal static readonly Type s_typeofDouble = typeof(double);
        internal static readonly Type s_typeofInt16 = typeof(short);
        internal static readonly Type s_typeofInt32 = typeof(int);
        internal static readonly Type s_typeofInt64 = typeof(long);
        internal static readonly Type s_typeofSByte = typeof(sbyte);
        internal static readonly Type s_typeofSingle = typeof(float);
        internal static readonly Type s_typeofTimeSpan = typeof(TimeSpan);
        internal static readonly Type s_typeofDateTime = typeof(DateTime);
        internal static readonly Type s_typeofUInt16 = typeof(ushort);
        internal static readonly Type s_typeofUInt32 = typeof(uint);
        internal static readonly Type s_typeofUInt64 = typeof(ulong);
        internal static readonly Type s_typeofObject = typeof(object);
        internal static readonly Type s_typeofSystemVoid = typeof(void);
        internal static readonly Assembly s_urtAssembly = Assembly.Load("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        internal static readonly string s_urtAssemblyString = s_urtAssembly.FullName;
        internal static readonly Assembly s_urtAlternativeAssembly = s_typeofString.Assembly;
        internal static readonly string s_urtAlternativeAssemblyString = s_urtAlternativeAssembly.FullName;
        internal static readonly Type s_typeofTypeArray = typeof(Type[]);
        internal static readonly Type s_typeofObjectArray = typeof(object[]);
        internal static readonly Type s_typeofStringArray = typeof(string[]);
        internal static readonly Type s_typeofBooleanArray = typeof(bool[]);
        internal static readonly Type s_typeofByteArray = typeof(byte[]);
        internal static readonly Type s_typeofCharArray = typeof(char[]);
        internal static readonly Type s_typeofDecimalArray = typeof(decimal[]);
        internal static readonly Type s_typeofDoubleArray = typeof(double[]);
        internal static readonly Type s_typeofInt16Array = typeof(short[]);
        internal static readonly Type s_typeofInt32Array = typeof(int[]);
        internal static readonly Type s_typeofInt64Array = typeof(long[]);
        internal static readonly Type s_typeofSByteArray = typeof(sbyte[]);
        internal static readonly Type s_typeofSingleArray = typeof(float[]);
        internal static readonly Type s_typeofTimeSpanArray = typeof(TimeSpan[]);
        internal static readonly Type s_typeofDateTimeArray = typeof(DateTime[]);
        internal static readonly Type s_typeofUInt16Array = typeof(ushort[]);
        internal static readonly Type s_typeofUInt32Array = typeof(uint[]);
        internal static readonly Type s_typeofUInt64Array = typeof(ulong[]);
        internal static readonly Type s_typeofMarshalByRefObject = typeof(MarshalByRefObject);
        private static volatile Type[] s_typeA;
        private static volatile Type[] s_arrayTypeA;
        private static volatile string[] s_valueA;
        private static volatile TypeCode[] s_typeCodeA;
        private static volatile PrimitiveTypeEnum[] s_codeA;

        internal static Array CreatePrimitiveArray(PrimitiveTypeEnum code, int length)
        {
            switch (code)
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

        private static void InitArrayTypeA()
        {
            Type[] typeArray = new Type[] { null, s_typeofBooleanArray, s_typeofByteArray, s_typeofCharArray };
            typeArray[5] = s_typeofDecimalArray;
            typeArray[6] = s_typeofDoubleArray;
            typeArray[7] = s_typeofInt16Array;
            typeArray[8] = s_typeofInt32Array;
            typeArray[9] = s_typeofInt64Array;
            typeArray[10] = s_typeofSByteArray;
            typeArray[11] = s_typeofSingleArray;
            typeArray[12] = s_typeofTimeSpanArray;
            typeArray[13] = s_typeofDateTimeArray;
            typeArray[14] = s_typeofUInt16Array;
            typeArray[15] = s_typeofUInt32Array;
            typeArray[0x10] = s_typeofUInt64Array;
            s_arrayTypeA = typeArray;
        }

        private static void InitCodeA()
        {
            PrimitiveTypeEnum[] eeArray = new PrimitiveTypeEnum[] { PrimitiveTypeEnum.Invalid, PrimitiveTypeEnum.Invalid, PrimitiveTypeEnum.Invalid, PrimitiveTypeEnum.Boolean, PrimitiveTypeEnum.Char, PrimitiveTypeEnum.SByte, PrimitiveTypeEnum.Byte, PrimitiveTypeEnum.Int16, PrimitiveTypeEnum.UInt16 };
            eeArray[9] = PrimitiveTypeEnum.Int32;
            eeArray[10] = PrimitiveTypeEnum.UInt32;
            eeArray[11] = PrimitiveTypeEnum.Int64;
            eeArray[12] = PrimitiveTypeEnum.UInt64;
            eeArray[13] = PrimitiveTypeEnum.Single;
            eeArray[14] = PrimitiveTypeEnum.Double;
            eeArray[15] = PrimitiveTypeEnum.Decimal;
            eeArray[0x10] = PrimitiveTypeEnum.DateTime;
            eeArray[0x11] = PrimitiveTypeEnum.Invalid;
            eeArray[0x12] = PrimitiveTypeEnum.Invalid;
            s_codeA = eeArray;
        }

        private static void InitTypeA()
        {
            Type[] typeArray = new Type[] { null, s_typeofBoolean, s_typeofByte, s_typeofChar };
            typeArray[5] = s_typeofDecimal;
            typeArray[6] = s_typeofDouble;
            typeArray[7] = s_typeofInt16;
            typeArray[8] = s_typeofInt32;
            typeArray[9] = s_typeofInt64;
            typeArray[10] = s_typeofSByte;
            typeArray[11] = s_typeofSingle;
            typeArray[12] = s_typeofTimeSpan;
            typeArray[13] = s_typeofDateTime;
            typeArray[14] = s_typeofUInt16;
            typeArray[15] = s_typeofUInt32;
            typeArray[0x10] = s_typeofUInt64;
            s_typeA = typeArray;
        }

        private static void InitTypeCodeA()
        {
            TypeCode[] codeArray = new TypeCode[] { TypeCode.Object, TypeCode.Boolean, TypeCode.Byte, TypeCode.Char };
            codeArray[5] = TypeCode.Decimal;
            codeArray[6] = TypeCode.Double;
            codeArray[7] = TypeCode.Int16;
            codeArray[8] = TypeCode.Int32;
            codeArray[9] = TypeCode.Int64;
            codeArray[10] = TypeCode.SByte;
            codeArray[11] = TypeCode.Single;
            codeArray[12] = TypeCode.Object;
            codeArray[13] = TypeCode.DateTime;
            codeArray[14] = TypeCode.UInt16;
            codeArray[15] = TypeCode.UInt32;
            codeArray[0x10] = TypeCode.UInt64;
            s_typeCodeA = codeArray;
        }

        private static void InitValueA()
        {
            string[] strArray = new string[] { null, "Boolean", "Byte", "Char" };
            strArray[5] = "Decimal";
            strArray[6] = "Double";
            strArray[7] = "Int16";
            strArray[8] = "Int32";
            strArray[9] = "Int64";
            strArray[10] = "SByte";
            strArray[11] = "Single";
            strArray[12] = "TimeSpan";
            strArray[13] = "DateTime";
            strArray[14] = "UInt16";
            strArray[15] = "UInt32";
            strArray[0x10] = "UInt64";
            s_valueA = strArray;
        }

        internal static bool IsPrimitiveArray(Type type, out object typeInformation)
        {
            bool flag = true;
            if (object.ReferenceEquals(type, s_typeofBooleanArray))
            {
                typeInformation = PrimitiveTypeEnum.Boolean;
            }
            else if (object.ReferenceEquals(type, s_typeofByteArray))
            {
                typeInformation = PrimitiveTypeEnum.Byte;
            }
            else if (object.ReferenceEquals(type, s_typeofCharArray))
            {
                typeInformation = PrimitiveTypeEnum.Char;
            }
            else if (object.ReferenceEquals(type, s_typeofDoubleArray))
            {
                typeInformation = PrimitiveTypeEnum.Double;
            }
            else if (object.ReferenceEquals(type, s_typeofInt16Array))
            {
                typeInformation = PrimitiveTypeEnum.Int16;
            }
            else if (object.ReferenceEquals(type, s_typeofInt32Array))
            {
                typeInformation = PrimitiveTypeEnum.Int32;
            }
            else if (object.ReferenceEquals(type, s_typeofInt64Array))
            {
                typeInformation = PrimitiveTypeEnum.Int64;
            }
            else if (object.ReferenceEquals(type, s_typeofSByteArray))
            {
                typeInformation = PrimitiveTypeEnum.SByte;
            }
            else if (object.ReferenceEquals(type, s_typeofSingleArray))
            {
                typeInformation = PrimitiveTypeEnum.Single;
            }
            else if (object.ReferenceEquals(type, s_typeofUInt16Array))
            {
                typeInformation = PrimitiveTypeEnum.UInt16;
            }
            else if (object.ReferenceEquals(type, s_typeofUInt32Array))
            {
                typeInformation = PrimitiveTypeEnum.UInt32;
            }
            else if (object.ReferenceEquals(type, s_typeofUInt64Array))
            {
                typeInformation = PrimitiveTypeEnum.UInt64;
            }
            else
            {
                typeInformation = null;
                flag = false;
            }
            return flag;
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

        internal static Type ToArrayType(PrimitiveTypeEnum code)
        {
            if (s_arrayTypeA == null)
            {
                InitArrayTypeA();
            }
            return s_arrayTypeA[(int)code];
        }

        internal static PrimitiveTypeEnum ToCode(Type type)
        {
            return ((type == null) ? ToPrimitiveTypeEnum(TypeCode.Empty) : (type.IsPrimitive ? ToPrimitiveTypeEnum(Type.GetTypeCode(type)) : (object.ReferenceEquals(type, s_typeofDateTime) ? PrimitiveTypeEnum.DateTime : (object.ReferenceEquals(type, s_typeofTimeSpan) ? PrimitiveTypeEnum.TimeSpan : (object.ReferenceEquals(type, s_typeofDecimal) ? PrimitiveTypeEnum.Decimal : PrimitiveTypeEnum.Invalid)))));
        }

        internal static string ToComType(PrimitiveTypeEnum code)
        {
            if (s_valueA == null)
            {
                InitValueA();
            }
            return s_valueA[(int)code];
        }

        internal static PrimitiveTypeEnum ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            if (s_codeA == null)
            {
                InitCodeA();
            }
            return s_codeA[(int)typeCode];
        }

        internal static Type ToType(PrimitiveTypeEnum code)
        {
            if (s_typeA == null)
            {
                InitTypeA();
            }
            return s_typeA[(int)code];
        }

        internal static TypeCode ToTypeCode(PrimitiveTypeEnum code)
        {
            if (s_typeCodeA == null)
            {
                InitTypeCodeA();
            }
            return s_typeCodeA[(int)code];
        }

        internal static int TypeLength(PrimitiveTypeEnum code)
        {
            switch (code)
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
