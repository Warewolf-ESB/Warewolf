
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.FileActivityDesigner
{
    public class TestFileActivityDesignerViewModel : FileActivityDesignerViewModel
    {
        public TestFileActivityDesignerViewModel(ModelItem modelItem, string inputPathLabel, string outputPathLabel)
            : base(modelItem, inputPathLabel, outputPathLabel)
        {
        }

        public int ValidateInputPathHitCount { get; private set; }
        protected override void ValidateInputPath()
        {
            ValidateInputPathHitCount++;
            base.ValidateInputPath();
        }

        public int ValidateOutputPathHitCount { get; private set; }
        protected override void ValidateOutputPath()
        {
            ValidateOutputPathHitCount++;
            base.ValidateOutputPath();
        }

        public int ValidatePathHitCount { get; private set; }
        public bool ValidatePathIsRequired { get; private set; }
        protected override string ValidatePath(string label, string path, Action onError, bool pathIsRequired)
        {
            ValidatePathHitCount++;
            ValidatePathIsRequired = pathIsRequired;
            return base.ValidatePath(label, path, onError, pathIsRequired);
        }

        public override void Validate()
        {
            throw new NotImplementedException();
        }

        public string TestValidatePath(string label, string path, Action onError, bool pathIsRequired)
        {
            return base.ValidatePath(label, path, onError, pathIsRequired);
        }

        public void TestValidateInputPath()
        {
            base.ValidateInputPath();
        }

        public void TestValidateOutputPath()
        {
            base.ValidateOutputPath();
        }

        public void TestValidateFileContent(string content, string label)
        {
            base.ValidateFileContent(content, label);
        }

        public void TestValidateArchivePassword(string password, string label)
        {
            base.ValidateArchivePassword(password, label);
        }

        public void TestValidateInputAndOutputPaths()
        {
            base.ValidateInputAndOutputPaths();
        }



        public string TestValidateFileContent(string content, string label, Action onError, bool contentIsRequired)
        {
            return base.ValidateFileContent(content, label, onError, contentIsRequired);
        }

        public int ValidateFileContentHitCount { get; private set; }
        public void TestValidateBaseFileContent(string content, string label)
        {
            ValidateFileContentHitCount++;
            base.ValidateFileContent(content, label);
        }


        public string TestValidateArchivePassword(string password, string label, Action onError, bool contentIsRequired)
        {
            return base.ValidateArchivePassword(password, label, onError, contentIsRequired);
        }


        public int ValidateArchivePasswordCount { get; private set; }
        public void TestValidateBaseArchivePassword(string password, string label)
        {
            ValidateArchivePasswordCount++;
            base.ValidateArchivePassword(password, label);
        }


    }
}
