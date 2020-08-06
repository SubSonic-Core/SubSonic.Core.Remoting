using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RuntimeObjcectIDGenerator = System.Runtime.Serialization.ObjectIDGenerator;

namespace SubSonic.Core.Remoting.Serialization
{
    public class ObjectIDGenerator
        : RuntimeObjcectIDGenerator
    {
        public ObjectIDGenerator()
            : base()
        {
            
        }

        public int CurrentCount
        {
            get
            {
                FieldInfo field = base.GetType().GetField("_currentCount", BindingFlags.NonPublic | BindingFlags.Instance);

                if ((field?.GetValue(this) ?? 1) is int value)
                {
                    return value;
                }
                return default;
            }
            set
            {
                FieldInfo field = base.GetType().GetField("_currentCount", BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(this, value);
                }
            }
        }
    }
}
