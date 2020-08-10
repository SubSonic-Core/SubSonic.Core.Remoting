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
            _currentCount = typeof(RuntimeObjcectIDGenerator).GetField(nameof(_currentCount), BindingFlags.NonPublic | BindingFlags.Instance) ??
                            typeof(RuntimeObjcectIDGenerator).GetField("m_currentCount", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static FieldInfo _currentCount;

        public int CurrentCount
        {
            get
            {
                if ((_currentCount.GetValue(this) ?? 1) is int value)
                {
                    return value;
                }
                return default;
            }
            set
            {
                _currentCount.SetValue(this, value);
            }
        }
    }
}
