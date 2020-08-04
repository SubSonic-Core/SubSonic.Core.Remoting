namespace SubSonic.Core
{
    public static partial class Extensions
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNotNullOrEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        public static string Format(this string source, params object[] arguments)
        {
            return string.Format(source, arguments);
        }
    }
}
