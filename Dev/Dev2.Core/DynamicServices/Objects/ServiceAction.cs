#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.DynamicServices.Objects.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.DynamicServices.Objects
{
    #region Service Action Class - Represents a single action within a service

    public class ServiceAction : DynamicServiceObjectBase, IDisposable
    {
        #region Private Fields

        const int _generation = 0;
        readonly object _poolGuard = new object();
        bool _disposing;
        bool _resultsToClient = true;
        bool _terminateServiceOnFault = true;
        Activity _workflowActivity;
        Queue<PooledServiceActivity> _workflowPool = new Queue<PooledServiceActivity>();
        StringBuilder _xamlDefinition;
        Stream _xamlStream;

        #endregion

        #region Public Properties

        public int? CommandTimeout { get; set; }

        public Stream XamlStream => _xamlStream;

        [JsonConverter(typeof(StringEnumConverter))]
        public enActionType ActionType { get; set; }

        public string SourceName { get; set; }

        public string SourceMethod { get; set; }

        public Source Source { get; set; }

        public StringBuilder XamlDefinition
        {
            get => _xamlDefinition;
            set
            {
                lock (_poolGuard)
                {
                    // Default operation
                    _xamlDefinition = value;
                    if (_xamlStream != null)
                    {
                        _xamlStream.Close();
                        _xamlStream.Dispose();
                        _xamlStream = new MemoryStream();
                    }

                    // now load it all up ;)
                    // extracted to avoid memory leak in MS utilities and root references 
                    var xamlLoader = new Dev2XamlLoader();
                    xamlLoader.Load(value, ref _xamlStream, ref _workflowPool, ref _workflowActivity);
                }
            }
        }

        public Activity WorkflowActivity => _workflowActivity;

        public List<ServiceActionInput> ServiceActionInputs { get; set; }

        public string ServiceName { get; set; }

        public Guid ServiceID { get; set; }

        public DynamicService Service { get; set; }

        public bool ResultsToClient
        {
            get => _resultsToClient;
            set => _resultsToClient = value;
        }

        public bool TerminateServiceOnFault
        {
            get => _terminateServiceOnFault;
            set => _terminateServiceOnFault = value;
        }

        public dynamic Parent { get; set; }

        public IList<IDev2Definition> ServiceActionOutputs { get; set; }

        public string OutputDescription { get; set; }

        public AppDomain PluginDomain { get; set; }

        #endregion

        #region Constructors

        public ServiceAction()
            : base(enDynamicServiceObjectType.ServiceAction)
        {
            ServiceActionInputs = new List<ServiceActionInput>();
            ServiceActionOutputs = new List<IDev2Definition>(); // Travis.Frisinger
            ActionType = enActionType.Unknown;
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
        }

        public PooledServiceActivity PopActivity()
        {
            PooledServiceActivity result;

            lock (_poolGuard)
            {
                if (_workflowPool.Count == 0)
                {
                    if (_xamlStream == null)
                    {
                        result = new PooledServiceActivity(_generation, _workflowActivity);
                    }
                    else
                    {
                        var activity = ActivityXamlServices.Load(_xamlStream);
                        _xamlStream.Seek(0, SeekOrigin.Begin);
                        result = new PooledServiceActivity(_generation, activity);
                    }
                }
                else
                {
                    result = _workflowPool.Dequeue();
                }
            }

            return result;
        }

        public override bool Compile()
        {
            base.Compile();

            foreach (ServiceActionInput sai in ServiceActionInputs)
            {
                sai.Compile();
                sai.CompilerErrors.ToList().ForEach(c => CompilerErrors.Add(c));
            }

            if (CompilerErrors.Count > 0)
            {
                return IsCompiled;
            }

            return IsCompiled;
        }


        public void SetActivity(Activity activity)
        {
            _workflowActivity = activity;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposing && disposing && _xamlStream != null)
            {
                _xamlStream.Close();
                _xamlStream.Dispose();
            }
            _disposing = true;
        }
    }

    #endregion

    public sealed class PooledServiceActivity
    {
        readonly int _generation;
        readonly Activity _value;

        internal PooledServiceActivity(int generation, Activity value)
        {
            _generation = generation;
            _value = value;
        }

        public int Generation => _generation;

        public Activity Value => _value;
    }
}