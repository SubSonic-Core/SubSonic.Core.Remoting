/*
Copyright 2020 Tyler Jensen

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    public static partial class Extensions
    {
        public static string ToConfigName(this Type type)
        {
            // Do not qualify types from mscorlib/System.Private.CoreLib otherwise calling between process running with different frameworks won't work
            // i.e. "System.String, mscorlib" (.NET FW) != "System.String, System.Private.CoreLib" (.NET CORE)
            if (type.Assembly.GetName().Name == "mscorlib" ||
                type.Assembly.GetName().Name == "System.Private.CoreLib")
            {
                return type.FullName;
            }

            var name = type.AssemblyQualifiedName;
            name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
            name = Regex.Replace(name, @", Culture=\w+", string.Empty);
            name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);
            return name;
        }

        public static Type ToType(this string configName)
        {
            try
            {
                var result = Type.GetType(configName);

                return result ?? TypeMapper.GetType(configName.Substring(0, configName.IndexOf(',')));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            return null;
        }

        public static async Task<Type> ToTypeAsync(this string configName)
        {
            try
            {
                var result = Type.GetType(configName);

                return result ?? await TypeMapper.GetTypeAsync(configName.Substring(0, configName.IndexOf(',')));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            return null;
        }
    }
}
