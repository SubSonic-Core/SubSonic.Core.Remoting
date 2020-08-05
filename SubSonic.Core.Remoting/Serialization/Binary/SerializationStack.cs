using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class SerializationStack
    {
        public object[] _objects = new object[5];
        public string _stackId;
        public int _top = -1;

        public SerializationStack(string stackId)
        {
            this._stackId = stackId;
        }

        public void IncreaseCapacity()
        {
            object[] destinationArray = new object[this._objects.Length * 2];
            Array.Copy(this._objects, 0, destinationArray, 0, this._objects.Length);
            this._objects = destinationArray;
        }

        public bool IsEmpty()
        {
            return (this._top <= 0);
        }

        public object Peek()
        {
            return ((this._top < 0) ? null : this._objects[this._top]);
        }

        public object PeekPeek()
        {
            return ((this._top < 1) ? null : this._objects[this._top - 1]);
        }

        public object Pop()
        {
            if (this._top < 0)
            {
                return null;
            }
            object obj2 = this._objects[this._top];
            int index = this._top;
            this._top = index - 1;
            this._objects[index] = null;
            return obj2;
        }

        public void Push(object obj)
        {
            if (this._top == (this._objects.Length - 1))
            {
                this.IncreaseCapacity();
            }
            int index = this._top + 1;
            this._top = index;
            this._objects[index] = obj;
        }
    }
}
