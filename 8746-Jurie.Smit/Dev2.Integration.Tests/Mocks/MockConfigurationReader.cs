// FIX ME
//using System;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Unlimited.Framework;
//using System.Collections;

//namespace Dev2.Integration.Tests.Mocks {
//    public class MockConfigurationReader : IDev2ConfigurationProvider {
//        private static IDictionary d = new Dictionary<string, string>();

//        public void Init(string[] key, string[] val) {
//            int pos = 0;
//            d.Clear();

//            foreach(string k in key) {
//                d.Add(k, val[pos]);
//                pos++;
//            }
//        }

//        public string ReadKey(string key) {
//            return (string)d[key];
//        }

//        public void OnReadFailure() {

//        }
//    }
//}
