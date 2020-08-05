using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class BinaryReader
    {
        private class TypeNAssembly
        {
            public Type Type;
            public string AssemblyName;
        }

        private sealed class TopLevelAssemblyTypeResolver
        {
            private readonly Assembly topLevelAssembly;

            public TopLevelAssemblyTypeResolver(Assembly topLevelAssembly)
            {
                this.topLevelAssembly = topLevelAssembly;
            }

            public Type ResolveType(Assembly assembly, string simpleTypeName, bool ignoreCase)
            {
                if (assembly == null)
                {
                    assembly = this.topLevelAssembly;
                }
                return assembly.GetType(simpleTypeName, false, ignoreCase);
            }
        }

        private Stream stream;
        private ISurrogateSelector surrogates;
        private StreamingContext context;
        private ObjectManager objectManager;
        private FormatterHelper fh;
        private SerializationBinder binder;
        private long topId;
        private bool isSimpleAssembly;
        private object topObject;
        private SerializationObjectInfo objectInfo;
        private IFormatterConverter converter;
        private SerializationStack stack;
        private object[] crossAppDomainArray;
        private bool fullDeserialization;
        private bool oldFormatDetected;
        private IntSizedArray valTypeObjectIdTable;
        private readonly NameCache typeCache;
        private string previousAssemblyString;
        private string previousName;
        private Type previousType;

        public BinaryReader(Stream serializationStream, ISurrogateSelector surrogateSelector, StreamingContext context, FormatterHelper fh, SerializationBinder binder)
            : this()
        {
            this.stream = serializationStream ?? throw new ArgumentNullException(nameof(serializationStream));
            this.surrogates = surrogateSelector;
            this.context = context;
            this.fh = fh;
            this.binder = binder;
        }

        private BinaryReader()
        {
            this.typeCache = new NameCache();
        }

        public Type Bind(string assemblyString, string typeString)
        {
            Type type = null;
            if (this.binder != null)
            {
                type = this.binder.BindToType(assemblyString, typeString);
            }
            if (type == null)
            {
                type = this.FastBindToType(assemblyString, typeString);
            }
            return type;
        }

        private void CheckSerializable(Type type)
        {
            if (!type.IsSerializable && !HasSurrogate(type))
            {
                throw new SerializationException(RemotingResources.NotMarkedForSerialization.Format(type.FullName, type.Assembly.FullName));
            }
        }

        private bool HasSurrogate(Type type)
        {
            return ((this.surrogates != null) && (this.surrogates.GetSurrogate(type, context, out _) != null));
        }

        private Type FastBindToType(string assemblyName, string typeName)
        {
            Type typeFromAssembly = null;
            TypeNAssembly cachedValue = (TypeNAssembly)this.typeCache.GetCachedValue(typeName);
            if ((cachedValue == null) || (cachedValue.AssemblyName != assemblyName))
            {
                if (assemblyName == null)
                {
                    return null;
                }
                Assembly assm = null;
                AssemblyName name = null;
                try
                {
                    name = new AssemblyName(assemblyName);
                }
                catch
                {
                    return null;
                }
                if (this.isSimpleAssembly)
                {
                    assm = ResolveSimpleAssemblyName(name);
                }
                else
                {
                    try
                    {
                        assm = Assembly.Load(name);
                    }
                    catch
                    {
                    }
                }
                if (assm == null)
                {
                    return null;
                }
                if (this.isSimpleAssembly)
                {
                    GetSimplyNamedTypeFromAssembly(assm, typeName, ref typeFromAssembly);
                }
                else
                {
                    typeFromAssembly = FormatterServices.GetTypeFromAssembly(assm, typeName);
                }

                if (typeFromAssembly == null)
                {
                    return null;
                }
                
                cachedValue = new TypeNAssembly
                {
                    Type = typeFromAssembly,
                    AssemblyName = assemblyName
                };
                this.typeCache.SetCachedValue(typeName, cachedValue);
            }
            return cachedValue.Type;
        }

        private static void GetSimplyNamedTypeFromAssembly(Assembly assm, string typeName, ref Type type)
        {
            try
            {
                type = FormatterServices.GetTypeFromAssembly(assm, typeName);
            }
            catch (TypeLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            if (type == null)
            {
                type = Type.GetType(typeName, new Func<AssemblyName, Assembly>(ResolveSimpleAssemblyName), new Func<Assembly, string, bool, Type>(new TopLevelAssemblyTypeResolver(assm).ResolveType), false);
            }
        }

        private static Assembly ResolveSimpleAssemblyName(AssemblyName assemblyName)
        {
            if (assemblyName != null)
            {
                try
                {
                    if (Assembly.Load(assemblyName.Name) is Assembly assembly)
                    {
                        return assembly;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
