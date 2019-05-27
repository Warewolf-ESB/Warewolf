#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities.WcfEndPoint
{
    [ToolDescriptorInfo("WcfEndPoint", "WCF", ToolType.Native, "6AEB1028-6332-46F9-8BED-641DE4EA038E", "Dev2.Activities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Resources_WCF")]
    public class DsfWcfEndPointActivity : DsfMethodBasedActivity,IEquatable<DsfWcfEndPointActivity>
    {
        public IWcfAction Method { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public WcfSource Source { get; set; }
        public DsfWcfEndPointActivity()
        {
            Type = "WCF Connector";
            DisplayName = "WCF Service";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            if (Method == null)
            {
                tmpErrors.AddError(ErrorResource.NoMethodSelected);
                return;
            }
            ExecuteService(update, out tmpErrors, Method, dataObject, OutputFormatterFactory.CreateOutputFormatter(OutputDescription));
        }
        
        protected void ExecuteService(int update, out ErrorResultTO errors, IWcfAction method, IDSFDataObject dataObject, IOutputFormatter formater)
        {
            errors = new ErrorResultTO();
            Source = ResourceCatalog.GetResource<WcfSource>(dataObject.WorkspaceID, SourceId);
            var itrs = new List<IWarewolfIterator>(5);
            IWarewolfListIterator itrCollection = new WarewolfListIterator();
            var methodParameters = Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList();
            BuildParameterIterators(update, methodParameters.ToList(),itrCollection,itrs,dataObject);
            try
            {
                while (itrCollection.HasMoreData())
                {
                    var pos = 0;
                    foreach (var itr in itrs)
                    {
                        var injectVal = itrCollection.FetchNextValue(itr);
                        var param = methodParameters.ToList()[pos];


                        param.Value = param.EmptyToNull &&
                                      (injectVal == null ||
                                       string.Compare(injectVal, string.Empty,
                                           StringComparison.InvariantCultureIgnoreCase) == 0)
                            ? null
                            : injectVal;

                        pos++;
                    }
                    var result = Source.ExecuteMethod(method);

                    if (result != null)
                    {
                        ResponseManager = new ResponseManager { OutputDescription = OutputDescription, Outputs = Outputs, IsObject = IsObject, ObjectName = ObjectName };
                        ResponseManager.PushResponseIntoEnvironment(result.ToString(), update, dataObject);
                    }
                }
            }
            catch (Exception e)
            {
                errors.AddError(e.Message);
            }
        }
        public IResponseManager ResponseManager { get; set; }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public bool Equals(DsfWcfEndPointActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && Equals(Method, other.Method) && Equals(OutputDescription, other.OutputDescription) && Equals(Source, other.Source);
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfWcfEndPointActivity dsfWcfEndPointActivity)
            {
                return this.Equals(dsfWcfEndPointActivity);
               
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Method != null ? Method.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputDescription != null ? OutputDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
