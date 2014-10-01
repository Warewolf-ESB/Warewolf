
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Rename;

namespace Dev2.Activities.Designers.Tests.Rename
{
    public class TestRenameDesignerViewModel : RenameDesignerViewModel
    {
        public TestRenameDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
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

        public int ValidateInputAndOutputPathHitCount { get; private set; }
        protected override void ValidateInputAndOutputPaths()
        {
            ValidateInputAndOutputPathHitCount++;
            base.ValidateInputAndOutputPaths();
        }

        public int ValidateUserNameAndPasswordHitCount { get; private set; }
        protected override void ValidateUserNameAndPassword()
        {
            ValidateUserNameAndPasswordHitCount++;
            base.ValidateUserNameAndPassword();
        }

        public int ValidateDestinationUsernameAndPasswordHitCount { get; private set; }
        protected override void ValidateDestinationUsernameAndPassword()
        {
            ValidateDestinationUsernameAndPasswordHitCount++;
            base.ValidateDestinationUsernameAndPassword();
        }


    }

}
