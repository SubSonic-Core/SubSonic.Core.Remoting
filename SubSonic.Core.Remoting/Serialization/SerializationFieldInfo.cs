using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization
{
    public sealed class SerializationFieldInfo 
        : System.Reflection.FieldInfo
    {
        private readonly System.Reflection.FieldInfo m_field;
        private readonly string m_serializationName;

        public SerializationFieldInfo(System.Reflection.FieldInfo field, string namePrefix)
        {
            this.m_field = field;
            this.m_serializationName = namePrefix + "+" + this.m_field.Name;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.m_field.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.m_field.GetCustomAttributes(attributeType, inherit);
        }

        public override object GetValue(object obj)
        {
            return this.m_field.GetValue(obj);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.m_field.IsDefined(attributeType, inherit);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            this.m_field.SetValue(obj, value, invokeAttr, binder, culture);
        }

        public System.Reflection.FieldInfo FieldInfo
        {
            get
            {
                return this.m_field;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_serializationName;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_field.Module;
            }
        }

        public override int MetadataToken
        {
            get
            {
                return this.m_field.MetadataToken;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_field.DeclaringType;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_field.ReflectedType;
            }
        }

        public override Type FieldType
        {
            get
            {
                return this.m_field.FieldType;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                return this.m_field.FieldHandle;
            }
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return this.m_field.Attributes;
            }
        }
    }
}
