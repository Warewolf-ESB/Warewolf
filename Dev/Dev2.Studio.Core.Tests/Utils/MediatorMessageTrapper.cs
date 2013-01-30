using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core;

namespace Dev2.Core.Tests {
    public static class MediatorMessageTrapper {

        public static void DeregUserInterfaceLayoutProvider() {
            //2012-11-12, brendon.page replaced the hardcoded list with a loop, this makes it easier to maintain when new mediator messages are added
            foreach (MediatorMessages item in Enum.GetValues(typeof(MediatorMessages)))
            {
                Mediator.DeRegisterAllActionsForMessage(item);
            }
        }


        public static void RegisterMessageToTrash(MediatorMessages message, bool isObjectSentWithMessage) {
            if (isObjectSentWithMessage) {
                Mediator.RegisterToReceiveMessage(message, input => TrashMessage(input as object));
            }
            else {
                Mediator.RegisterToReceiveMessage(message, TrashMessage);
            }
        }

        private static void TrashMessage(object MessageBodyToTrash) {
            
        }

        private static void TrashMessage() {

        }
    }
}
