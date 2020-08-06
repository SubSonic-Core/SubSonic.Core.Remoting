namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class BinaryObject
        : IStreamable
    {
        public int ObjectId { get; set; }
        public int MapId { get; set; }
        
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
            output.WriteByte(1);
            output.WriteInt32(this.ObjectId);
            output.WriteInt32(this.MapId);
        }
    }
}
