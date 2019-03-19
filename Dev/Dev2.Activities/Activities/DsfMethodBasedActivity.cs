#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfMethodBasedActivity : DsfActivity
    {
        protected void BuildParameterIterators(int update, List<MethodParameter> inputs, IWarewolfListIterator itrCollection, List<IWarewolfIterator> itrs, IDSFDataObject dataObject)
        {
            if(inputs != null)
            {
                foreach(var sai in inputs)
                {
                    var val = sai.Name;
                    string toInject = null;

                    if(val != null)
                    {
                        toInject = sai.Value;
                    }

                    var paramIterator = new WarewolfIterator(dataObject.Environment.Eval(toInject, update));
                    itrCollection.AddVariableToIterateOn(paramIterator);
                    itrs.Add(paramIterator);
                }
            }
        }
    }
}