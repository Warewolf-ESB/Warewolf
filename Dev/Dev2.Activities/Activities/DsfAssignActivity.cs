
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
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // TODO: DELETE UNUSED
    public class DsfAssignActivity : DsfActivityAbstract<string>
    {

        public string FieldName { get; set; }
        public string FieldValue { get; set; }

        public bool UpdateAllOccurrences { get; set; }
        public bool CreateBookmark { get; set; }
        public string ServiceHost { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public DsfAssignActivity()
            : base("Assign")
        {
        }

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember



        protected override void OnExecute(NativeActivityContext context)
        {
            throw new NotImplementedException("Nothing here...");
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return DsfForEachItem.EmptyList;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(FieldValue);
        }

        #endregion

    }
}
