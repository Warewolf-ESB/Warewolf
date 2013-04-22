using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Unlimited.Framework;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core {
    [Export(typeof(IFileNameProvider))]
    public class FileNameProviderImpl : IFileNameProvider {
        public UnlimitedObject GetFileName() {
            dynamic returnData = new UnlimitedObject();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            returnData.FileName = dialog.SafeFileName;
            returnData.LocalFilePath = dialog.FileName;

            return returnData;
        }
    }
}
