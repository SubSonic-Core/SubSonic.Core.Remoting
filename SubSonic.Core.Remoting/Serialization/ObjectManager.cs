using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using RuntimeObjectManager = System.Runtime.Serialization.ObjectManager;

namespace SubSonic.Core.Remoting.Serialization
{
    public class ObjectManager
        : RuntimeObjectManager
    {
        public ObjectManager(ISurrogateSelector selector, StreamingContext context)
            : base(selector, context)
        {
            topObject = typeof(RuntimeObjectManager).GetProperty(nameof(TopObject), BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();
            registerString = typeof(RuntimeObjectManager).GetMethod(nameof(RegisterString), BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();
        }

        private static PropertyInfo topObject;

        public object TopObject
        {
            get
            {
                return topObject.GetValue(this);
            }

            set
            {
                topObject.SetValue(this, value);
            }
        }

        private static MethodInfo registerString;

        public void RegisterString(string obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member)
        {
            registerString.Invoke(this, new object[] { obj, objectID, info, idOfContainingObj, member });
        }
    }
}
