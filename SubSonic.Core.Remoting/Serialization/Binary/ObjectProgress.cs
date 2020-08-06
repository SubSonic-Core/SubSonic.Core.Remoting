using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class ObjectProgress
    {
        internal bool _isInitial;
        internal int _count;
        internal BinaryTypeEnum _expectedType = BinaryTypeEnum.ObjectUrt;
        internal object _expectedTypeInformation;
        internal string _name;
        internal ObjectTypeEnum _objectTypeEnum;
        internal MemberTypeEnum _memberTypeEnum;
        internal MemberValueEnum _memberValueEnum;
        internal Type _dtType;
        internal int _numItems;
        internal BinaryTypeEnum _binaryTypeEnum;
        internal object _typeInformation;
        internal int _memberLength;
        internal BinaryTypeEnum[] _binaryTypeEnumA;
        internal object[] _typeInformationA;
        internal string[] _memberNames;
        internal Type[] _memberTypes;
        internal ParseRecord _pr = new ParseRecord();

        internal ObjectProgress()
        {
        }

        internal void ArrayCountIncrement(int value)
        {
            this._count += value;
        }

        internal bool GetNext(out BinaryTypeEnum outBinaryTypeEnum, out object outTypeInformation)
        {
            outBinaryTypeEnum = BinaryTypeEnum.Primitive;
            outTypeInformation = null;
            if (this._objectTypeEnum == ObjectTypeEnum.Array)
            {
                if (this._count == this._numItems)
                {
                    return false;
                }
                outBinaryTypeEnum = this._binaryTypeEnum;
                outTypeInformation = this._typeInformation;
                if (this._count == 0)
                {
                    this._isInitial = false;
                }
                this._count++;
                return true;
            }
            if ((this._count == this._memberLength) && !this._isInitial)
            {
                return false;
            }
            outBinaryTypeEnum = this._binaryTypeEnumA[this._count];
            outTypeInformation = this._typeInformationA[this._count];
            if (this._count == 0)
            {
                this._isInitial = false;
            }
            this._name = this._memberNames[this._count];
            this._dtType = this._memberTypes[this._count];
            this._count++;
            return true;
        }

        internal void Init()
        {
            this._isInitial = false;
            this._count = 0;
            this._expectedType = BinaryTypeEnum.ObjectUrt;
            this._expectedTypeInformation = null;
            this._name = null;
            this._objectTypeEnum = ObjectTypeEnum.Empty;
            this._memberTypeEnum = MemberTypeEnum.Empty;
            this._memberValueEnum = MemberValueEnum.Empty;
            this._dtType = null;
            this._numItems = 0;
            this._typeInformation = null;
            this._memberLength = 0;
            this._binaryTypeEnumA = null;
            this._typeInformationA = null;
            this._memberNames = null;
            this._memberTypes = null;
            this._pr.Init();
        }
    }
}
