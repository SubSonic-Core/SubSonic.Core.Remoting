using System.Reflection;
using System.Runtime.Serialization;

namespace SubSonic.Core.Remoting.Serialization.Binary
{

    public sealed class BinaryAssemblyInfo
    {
        public string AssemblyString { get; }
        private Assembly _assembly;

        public BinaryAssemblyInfo(string assemblyString)
        {
            AssemblyString = assemblyString;
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
                _assembly = FormatterServices.LoadAssemblyFromStringNoThrow(AssemblyString);
                if (_assembly == null)
                {
                    throw new SerializationException(RemotingResources.SerializationAssemblyNotFound.Format(AssemblyString));
                }
            }
            return _assembly;
        }
    }
}
