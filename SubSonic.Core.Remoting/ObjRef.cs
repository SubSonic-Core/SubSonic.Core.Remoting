using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace SubSonic.Core.Remoting
{
    [Serializable, SecurityCritical, ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, Flags= SecurityPermissionFlag.Infrastructure)]
    public class ObjRef
        : IObjectReference
        , ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public object GetRealObject(StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
