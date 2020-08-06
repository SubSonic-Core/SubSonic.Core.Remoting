using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class SerializationHeaderRecord : IStreamable
    {
        internal BinaryHeaderEnum _binaryHeaderEnum;
        internal int _topId;
        internal int _headerId;
        internal int _majorVersion;
        internal int _minorVersion;

        internal SerializationHeaderRecord()
        {
        }

        internal SerializationHeaderRecord(BinaryHeaderEnum binaryHeaderEnum, int topId, int headerId, int majorVersion, int minorVersion)
        {
            this._binaryHeaderEnum = binaryHeaderEnum;
            this._topId = topId;
            this._headerId = headerId;
            this._majorVersion = majorVersion;
            this._minorVersion = minorVersion;
        }

        private static int GetInt32(byte[] buffer, int index)
        {
            return (((buffer[index] | (buffer[index + 1] << 8)) | (buffer[index + 2] << 0x10)) | (buffer[index + 3] << 0x18));
        }

        public void Read(BinaryParser input)
        {
            byte[] buffer = input.ReadBytes(0x11);
            if (buffer.Length < 0x11)
            {
                throw new EndOfStreamException(RemotingResources.SerializationReadBeyondEOF);
            }
            this._majorVersion = GetInt32(buffer, 9);
            if (this._majorVersion > 1)
            {
                throw new SerializationException(RemotingResources.SerializationInvalidFormat.Format(BitConverter.ToString(buffer)));
            }
            this._binaryHeaderEnum = (BinaryHeaderEnum)buffer[0];
            this._topId = GetInt32(buffer, 1);
            this._headerId = GetInt32(buffer, 5);
            this._minorVersion = GetInt32(buffer, 13);
        }

        public void Write(BinaryFormatterWriter output)
        {
            this._majorVersion = 1;
            this._minorVersion = 0;
            output.WriteByte((byte)this._binaryHeaderEnum);
            output.WriteInt32(this._topId);
            output.WriteInt32(this._headerId);
            output.WriteInt32(1);
            output.WriteInt32(0);
        }
    }
}
