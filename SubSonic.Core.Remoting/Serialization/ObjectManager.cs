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

        }

        public object TopObject
        {
            get
            {
                PropertyInfo property = base.GetType().GetProperty(nameof(TopObject), BindingFlags.NonPublic | BindingFlags.Instance);

                if (property != null)
                {
                    return property.GetValue(this);
                }

                return null;
            }

            set
            {
                PropertyInfo property = base.GetType().GetProperty(nameof(TopObject), BindingFlags.NonPublic | BindingFlags.Instance);

                if (property != null)
                {
                    property.SetValue(this, value);
                }
            }
        }
    }
}
