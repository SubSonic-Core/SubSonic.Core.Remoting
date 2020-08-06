namespace SubSonic.Core.Remoting.Serialization.Binary
{
    using System;

    internal sealed class NameInfo
    {
        internal string _fullName;
        internal long _objectId;
        internal long _assemId;
        internal PrimitiveTypeEnum _primitiveTypeEnum;
        internal Type _type;
        internal bool _isSealed;
        internal bool _isArray;
        internal bool _isArrayItem;
        internal bool _transmitTypeOnObject;
        internal bool _transmitTypeOnMember;
        internal bool _isParentTypeOnObject;
        internal ArrayTypeEnum _arrayEnum;
        private bool _sealedStatusChecked;

        internal NameInfo()
        {
        }

        internal void Init()
        {
            this._fullName = null;
            this._objectId = 0L;
            this._assemId = 0L;
            this._primitiveTypeEnum = PrimitiveTypeEnum.Invalid;
            this._type = null;
            this._isSealed = false;
            this._transmitTypeOnObject = false;
            this._transmitTypeOnMember = false;
            this._isParentTypeOnObject = false;
            this._isArray = false;
            this._isArrayItem = false;
            this._arrayEnum = ArrayTypeEnum.Empty;
            this._sealedStatusChecked = false;
        }

        public bool IsSealed
        {
            get
            {
                if (!this._sealedStatusChecked)
                {
                    this._isSealed = this._type.IsSealed;
                    this._sealedStatusChecked = true;
                }
                return this._isSealed;
            }
        }

        public string NIname
        {
            get
            {
                string text2 = this._fullName;
                if (this._fullName == null)
                {
                    text2 = this._fullName = this._type.FullName;
                }
                return text2;
            }
            set
            {
                this._fullName = value;
            }
        }
    }
}
