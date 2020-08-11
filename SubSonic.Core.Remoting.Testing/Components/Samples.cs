using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Testing.Components
{
    class Samples
    {
        public const string template =
@"<#@ template debug=""false"" hostspecific=""true"" language=""C#"" #>
<#@ import namespace=""System.Linq"" #>
<#@ import namespace=""System.Text"" #>
<#@ import namespace=""System.Collections.Generic"" #>
<#@ output extension="".cs"" #>
public class HelloWorld
{
    public const string = ""Hello World"";
}
";
        public const string outcome =
@"public class HelloWorld
{
    public const string = ""Hello World"";
}";
    }
}
