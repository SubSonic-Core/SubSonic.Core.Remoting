using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class IntSizedArray
        : ICloneable
    {
        private int[] objects;
        private int[] negObjects;

        public IntSizedArray()
        {
            this.objects = new int[0x10];
            this.negObjects = new int[4];
        }

        private IntSizedArray(IntSizedArray sizedArray)
        {
            this.objects = new int[0x10];
            this.negObjects = new int[4];
            this.objects = new int[sizedArray.objects.Length];
            sizedArray.objects.CopyTo(this.objects, 0);
            this.negObjects = new int[sizedArray.negObjects.Length];
            sizedArray.negObjects.CopyTo(this.negObjects, 0);
        }

        public object Clone()
        {
            return new IntSizedArray(this);
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    int[] destinationArray = new int[Math.Max((int)(this.negObjects.Length * 2), (int)(-index + 1))];
                    Array.Copy(this.negObjects, 0, destinationArray, 0, this.negObjects.Length);
                    this.negObjects = destinationArray;
                }
                else
                {
                    int[] destinationArray = new int[Math.Max((int)(this.objects.Length * 2), (int)(index + 1))];
                    Array.Copy(this.objects, 0, destinationArray, 0, this.objects.Length);
                    this.objects = destinationArray;
                }
            }
            catch (Exception)
            {
                throw new SerializationException(RemotingResources.ThBinaryStreamHasBeenCorrupted);
            }
        }

        internal int this[int index]
        {
            get
            {
                return ((index >= 0) ? ((index > (this.objects.Length - 1)) ? 0 : this.objects[index]) : ((-index > (this.negObjects.Length - 1)) ? 0 : this.negObjects[-index]));
            }
            set
            {
                if (index < 0)
                {
                    if (-index > (this.negObjects.Length - 1))
                    {
                        this.IncreaseCapacity(index);
                    }
                    this.negObjects[-index] = value;
                }
                else
                {
                    if (index > (this.objects.Length - 1))
                    {
                        this.IncreaseCapacity(index);
                    }
                    this.objects[index] = value;
                }
            }
        }
    }
}
