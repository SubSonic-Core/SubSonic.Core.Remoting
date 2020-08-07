using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubSonic.Core.Remoting.Channels
{
    public struct MethodHelper
            : IEquatable<MethodHelper>
    {
        private static readonly Regex methodRegex = new Regex(@"^\/(?<method>[A-Za-z]*)(?:\/(?<curly>\{)?(?<parameters>[A-Za-z0-9\-]+)(?(curly)\}))*", RegexOptions.Compiled);

        public MethodHelper(string local)
        {
            Name = "";
            Parameters = Array.Empty<string>();

            Initialize(local);
        }

        private void Initialize(string local)
        {
            MatchCollection matches = methodRegex.Matches(local);

            List<string> parameters = new List<string>();

            foreach (Match match in matches)
            {
                if (match.Groups["method"].Success)
                {
                    Name = match.Groups["method"].Value;
                }

                if (match.Groups["parameters"].Success)
                {
                    foreach (Capture parameter in match.Groups["parameters"].Captures)
                    {
                        parameters.Add(parameter.Value);
                    }
                }
            }
            Parameters = parameters.ToArray();
        }

        public string Name { get; private set; }

        public IEnumerable<string> Parameters { get; private set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is MethodHelper right)
            {
                return Equals(right);
            }

            return base.Equals(obj);
        }

        public bool Equals(MethodHelper right)
        {
            return
                Name.Equals(right.Name, StringComparison.Ordinal) &&
                Parameters.Count() == right.Parameters.Count();
        }

        public static bool operator ==(MethodHelper left, MethodHelper right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MethodHelper left, MethodHelper right)
        {
            return !left.Equals(right);
        }
    }
}
