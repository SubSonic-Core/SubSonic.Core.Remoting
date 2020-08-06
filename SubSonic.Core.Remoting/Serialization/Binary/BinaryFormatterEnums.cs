using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public enum ParseTypeEnum
    {
        Empty,
        SerializedStreamHeader,
        Object,
        Member,
        ObjectEnd,
        MemberEnd,
        Headers,
        HeadersEnd,
        SerializedStreamHeaderEnd,
        Envelope,
        EnvelopeEnd,
        Body,
        BodyEnd
    }

    public enum ObjectTypeEnum
    {
        Empty,
        Object,
        Array
    }

    public enum ArrayTypeEnum
    {
        Empty,
        Single,
        Jagged,
        Rectangular,
        Base64
    }

    internal enum BinaryArrayTypeEnum
    {
        Single = 0,
        Jagged = 1,
        Rectangular = 2,
        SingleOffset = 3,
        JaggedOffset = 4,
        RectangularOffset = 5
    }

    public enum MemberTypeEnum
    {
        Empty,
        Header,
        Field,
        Item
    }

    public enum MemberValueEnum
    {
        Empty,
        InlineValue,
        Nested,
        Reference,
        Null
    }

    public enum ObjectPositionEnum
    {
        Empty,
        Top,
        Child,
        Headers
    }
}
