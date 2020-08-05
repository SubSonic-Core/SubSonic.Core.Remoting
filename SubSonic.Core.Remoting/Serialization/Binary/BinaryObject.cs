namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObject
        : IStreamable
    {
        protected int objectId;
        protected int mapId;

        public BinaryObject()
        {
        }

        public virtual void Read(BinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.mapId = input.ReadInt32();
        }

        internal void Set(int objectId, int mapId)
        {
            this.objectId = objectId;
            this.mapId = mapId;
        }

        public virtual void Write(BinaryFormatterWriter output)
        {
            output.WriteByte(1);
            output.WriteInt32(this.objectId);
            output.WriteInt32(this.mapId);
        }
    }
}
