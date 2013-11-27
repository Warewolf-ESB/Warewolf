// FIX ME
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Dev2.Studio.Core.ViewModels;
//using System.Windows;

//namespace Dev2.Integration.Tests.Mocks {
//    class MockPopup : IPopUp {

//        public bool isYesMessageBox {
//            get;
//            set;
//        }

//        public System.Windows.MessageBoxButton Buttons {
//            get;
//            set;
//        }

//        public string Description {
//            get;
//            set;
//        }

//        public string Header {
//            get;
//            set;
//        }

//        public System.Windows.MessageBoxImage ImageType {
//            get;
//            set;
//        }

//        public string Question {
//            get;
//            set;
//        }

//        public System.Windows.MessageBoxResult Show() {
//            if(isYesMessageBox) {
//                return MessageBoxResult.Yes;
//            }
//            else {
//                return MessageBoxResult.No;
//            }
//        }


//        #region Ctor

//        public MockPopup(MessageBoxResult resultType) {
//            if(resultType == MessageBoxResult.Yes) {
//                isYesMessageBox = true;
//            }

//        }

//        #endregion
//    }
//}
