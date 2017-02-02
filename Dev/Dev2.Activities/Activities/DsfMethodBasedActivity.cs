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
                    string val = sai.Name;
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
            // ReSharper disable once RedundantJumpStatement
            return;
        }
    }
}