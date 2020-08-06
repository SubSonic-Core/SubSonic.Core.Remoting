﻿namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObject
        : IStreamable
    {
        internal int ObjectId;
        internal int MapId;
        
        public BinaryObject()
        {
        }

        public virtual void Read(BinaryParser input)
        {
            this.ObjectId = input.ReadInt32();
            this.MapId = input.ReadInt32();
        }

        internal void Set(int objectId, int mapId)
        {
            this.ObjectId = objectId;
            this.MapId = mapId;
        }

        public virtual void Write(BinaryFormatterWriter output)
        {
            output.Write((byte)1);
            output.Write(this.ObjectId);
            output.Write(this.MapId);
        }
    }
}
