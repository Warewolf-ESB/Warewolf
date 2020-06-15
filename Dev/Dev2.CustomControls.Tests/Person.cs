using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.CustomControls.Tests
{
    class Person
    {
        public static TimeSpan Time => new TimeSpan(2, 2, 2, 2);
        public static string StringTime => "10";
        public static string EmptyVal => "";
    }
}