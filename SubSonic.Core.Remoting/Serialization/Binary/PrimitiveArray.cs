namespace SubSonic.Core.Remoting.Serialization.Binary
{
    using System;
    using System.Globalization;

    public sealed class PrimitiveArray
    {
        private readonly PrimitiveTypeEnum _code;
        private readonly bool[] _booleanA;
        private readonly char[] _charA;
        private readonly double[] _doubleA;
        private readonly short[] _int16A;
        private readonly int[] _int32A;
        private readonly long[] _int64A;
        private readonly sbyte[] _sbyteA;
        private readonly float[] _singleA;
        private readonly ushort[] _uint16A;
        private readonly uint[] _uint32A;
        private readonly ulong[] _uint64A;

        public PrimitiveArray(PrimitiveTypeEnum code, Array array)
        {
            _code = code;
            switch (code)
            {
                case PrimitiveTypeEnum.Boolean:
                    _booleanA = (bool[])array;
                    return;

                case PrimitiveTypeEnum.Byte:
                case PrimitiveTypeEnum.Currency:
                case PrimitiveTypeEnum.Decimal:
                case PrimitiveTypeEnum.TimeSpan:
                case PrimitiveTypeEnum.DateTime:
                    break;

                case PrimitiveTypeEnum.Char:
                    _charA = (char[])array;
                    return;

                case PrimitiveTypeEnum.Double:
                    _doubleA = (double[])array;
                    return;

                case PrimitiveTypeEnum.Int16:
                    _int16A = (short[])array;
                    return;

                case PrimitiveTypeEnum.Int32:
                    _int32A = (int[])array;
                    return;

                case PrimitiveTypeEnum.Int64:
                    _int64A = (long[])array;
                    return;

                case PrimitiveTypeEnum.SByte:
                    _sbyteA = (sbyte[])array;
                    return;

                case PrimitiveTypeEnum.Single:
                    _singleA = (float[])array;
                    return;

                case PrimitiveTypeEnum.UInt16:
                    _uint16A = (ushort[])array;
                    return;

                case PrimitiveTypeEnum.UInt32:
                    _uint32A = (uint[])array;
                    return;

                case PrimitiveTypeEnum.UInt64:
                    _uint64A = (ulong[])array;
                    break;

                default:
                    return;
            }
        }

        public void SetValue(string value, int index)
        {
            switch (_code)
            {
                case PrimitiveTypeEnum.Boolean:
                    _booleanA[index] = bool.Parse(value);
                    return;

                case PrimitiveTypeEnum.Byte:
                case PrimitiveTypeEnum.Currency:
                case PrimitiveTypeEnum.Decimal:
                case PrimitiveTypeEnum.TimeSpan:
                case PrimitiveTypeEnum.DateTime:
                    break;

                case PrimitiveTypeEnum.Char:
                    if (value[0] == '_' && value.Equals("_0x00_"))
                    {
                        _charA[index] = '\0';
                        return;
                    }
                    _charA[index] = char.Parse(value);
                    return;

                case PrimitiveTypeEnum.Double:
                    _doubleA[index] = double.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.Int16:
                    _int16A[index] = short.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.Int32:
                    _int32A[index] = int.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.Int64:
                    _int64A[index] = long.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.SByte:
                    _sbyteA[index] = sbyte.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.Single:
                    _singleA[index] = float.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.UInt16:
                    _uint16A[index] = ushort.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.UInt32:
                    _uint32A[index] = uint.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case PrimitiveTypeEnum.UInt64:
                    _uint64A[index] = ulong.Parse(value, CultureInfo.InvariantCulture);
                    break;

                default:
                    return;
            }
        }
    }
}
