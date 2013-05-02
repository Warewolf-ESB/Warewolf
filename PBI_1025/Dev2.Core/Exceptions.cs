using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2;

namespace Dev2 {
    public static class Exceptions {
        public static void ThrowArgumentNullExceptionIfObjectIsNull(string objectName, object objectValue) {
            if (objectValue == null) {
                throw new ArgumentNullException(objectName, FrameworkResources.Exception_ArgumentCannotBeNull);
            }
        }

        public static void ThrowArgumentExceptionIfObjectIsNullOrIsEmptyString(string objectName, object objectValue) {
            ThrowArgumentNullExceptionIfObjectIsNull(objectName, objectValue);

            if (objectValue is string) {
                if (string.IsNullOrEmpty(objectValue.ToString())) {
                    throw new ArgumentException(FrameworkResources.Exception_ArgumentCannotBeNullOrEmpty, objectName);
                }
            }
        }

    }
}
