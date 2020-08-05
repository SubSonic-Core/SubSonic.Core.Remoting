namespace SubSonic.Core.Remoting.Serialization.Binary
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    public sealed class SizedArray : ICloneable
    {
        internal object[] objects;
        internal object[] negObjects;

        internal SizedArray()
        {
            objects = new object[0x10];
            negObjects = new object[4];
        }

        internal SizedArray(int length)
        {
            objects = new object[length];
            negObjects = new object[length];
        }

        private SizedArray(SizedArray sizedArray)
        {
            objects = new object[sizedArray.objects.Length];
            sizedArray.objects.CopyTo(objects, 0);
            negObjects = new object[sizedArray.negObjects.Length];
            sizedArray.negObjects.CopyTo(negObjects, 0);
        }

        public object Clone()
        {
            return new SizedArray(this);
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    object[] destinationArray = new object[Math.Max(negObjects.Length * 2, -index + 1)];
                    Array.Copy(negObjects, 0, destinationArray, 0, negObjects.Length);
                    negObjects = destinationArray;
                }
                else
                {
                    object[] destinationArray = new object[Math.Max(objects.Length * 2, index + 1)];
                    Array.Copy(objects, 0, destinationArray, 0, objects.Length);
                    objects = destinationArray;
                }
            }
            catch (Exception)
            {
                throw new SerializationException(RemotingResources.ThBinaryStreamHasBeenCorrupted);
            }
        }

        public object this[int index]
        {
            get
            {
                return index >= 0 ? index > objects.Length - 1 ? null : objects[index] : -index > negObjects.Length - 1 ? null : negObjects[-index];
            }
            set
            {
                if (index < 0)
                {
                    if (-index > negObjects.Length - 1)
                    {
                        IncreaseCapacity(index);
                    }
                    negObjects[-index] = value;
                }
                else
                {
                    if (index > objects.Length - 1)
                    {
                        IncreaseCapacity(index);
                    }
                    objects[index] = value;
                }
            }
        }
    }
}
