using System.Reflection;
using System.Runtime.Serialization;

namespace SubSonic.Core.Remoting.Serialization.Binary
{

    public sealed class BinaryAssemblyInfo
    {
        private string _assemblyString;
        private Assembly _assembly;

        public BinaryAssemblyInfo(string assemblyString)
        {
            _assemblyString = assemblyString;
        }

        public BinaryAssemblyInfo(string assemblyString, Assembly assembly) 
            : this(assemblyString)
        {
            _assembly = assembly;
        }

        public Assembly GetAssembly()
        {
            if (_assembly == null)
            {
                _assembly = FormatterServices.LoadAssemblyFromStringNoThrow(_assemblyString);
                if (_assembly == null)
                {
                    throw new SerializationException(RemotingResources.SerializationAssemblyNotFound.Format(_assemblyString));
                }
            }
            return _assembly;
        }
    }
}
