using System;

namespace Dev2.CustomControls.Tests.Converters
{
    internal class Person
    {
        public static TimeSpan Time => new TimeSpan(2, 2, 2, 2);
        public static string StringTime => "10";
        public static string EmptyVal => "";
    }
}