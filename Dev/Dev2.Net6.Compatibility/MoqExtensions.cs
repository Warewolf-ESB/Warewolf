using Moq;
using System.Runtime.Serialization;
namespace Dev2.Tests
{
    public static class MoqExtensions
    {
        public static Mock<T> Create<T>(this Mock<T> mock) where T : class
        {
            mock.As<ISerializable>();
            return mock;
        }

    }
}


//namespace Dev2.Studio.Core.Tests
//{
//    public static class MoqExtensions
//    {
//        public static Mock<T> Create<T>(this Mock<T> mock) where T : class
//        {
//            mock.As<ISerializable>();
//            return mock;
//        }

//    }
//}


//namespace Dev2.Core.Tests
//{
//    public static class MoqExtensions
//    {
//        public static Mock<T> Create<T>(this Mock<T> mock) where T : class
//        {
//            mock.As<ISerializable>();
//            return mock;
//        }

//    }
//}
