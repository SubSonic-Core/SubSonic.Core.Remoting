using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class ValueFixup
    {
        private readonly ValueFixupEnum _valueFixupEnum;
        private readonly Array _arrayObj;
        private readonly int[] _indexMap;
        private readonly object _memberObject;
        private readonly ReadObjectInfo _objectInfo;
        private readonly string _memberName;

        public ValueFixup(Array arrayObj, int[] indexMap)
        {
            _valueFixupEnum = ValueFixupEnum.Array;
            _arrayObj = arrayObj;
            _indexMap = indexMap;
        }

        public ValueFixup(object memberObject, string memberName, ReadObjectInfo objectInfo)
        {
            this._valueFixupEnum = ValueFixupEnum.Member;
            this._memberObject = memberObject;
            this._memberName = memberName;
            this._objectInfo = objectInfo;
        }

        public void Fixup(ParseRecord record, ParseRecord parent)
        {
            object obj2 = record.newObj;
            switch (this._valueFixupEnum)
            {
                case ValueFixupEnum.Array:
                    this._arrayObj.SetValue(obj2, this._indexMap);
                    return;

                case ValueFixupEnum.Header:
                    throw new PlatformNotSupportedException();

                case ValueFixupEnum.Member:
                    {
                        if (this._objectInfo.IsSi)
                        {
                            this._objectInfo.ObjectManager.RecordDelayedFixup(parent.objectId, this._memberName, record.objectId);
                            return;
                        }
                        MemberInfo memberInfo = this._objectInfo.GetMemberInfo(this._memberName);
                        if (memberInfo != null)
                        {
                            this._objectInfo.ObjectManager.RecordFixup(parent.objectId, memberInfo, record.objectId);
                        }
                        return;
                    }
            }
        }
    }
}
