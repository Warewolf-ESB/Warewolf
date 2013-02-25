//using System;
//using Dev2.Common;
//using ServiceStack.Redis;

//namespace Dev2.Data.Binary_Objects
//{
//    public class Dev2RedisClient
//    {
//        // ReSharper disable InconsistentNaming - This is due to Singleton Pattern
//        static RedisClient _redisClient;
//        // ReSharper restore InconsistentNaming

//        public static void StartRedis()
//        {
//            _redisClient = new RedisClient();
//            _redisClient.FlushAll();

//        }

//        public static RedisClient RedisClient
//        {
//            get
//            {
//                if (_redisClient == null) StartRedis();
//                return _redisClient;
//            }
//        }

//        public static void StopRedis()
//        {
//            if (_redisClient != null)
//            {
//                _redisClient.FlushAll();
//                _redisClient.Dispose();
//            }
//        }
//    }
//}