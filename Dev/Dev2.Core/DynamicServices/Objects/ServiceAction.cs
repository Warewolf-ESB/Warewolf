/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

        private const int _generation = 0;
        private readonly object _poolGuard = new object();
        private int _commandTimeout = 30;
        private bool _disposing;
        private bool _resultsToClient = true;
        private bool _terminateServiceOnFault = true;
        private Activity _workflowActivity;
        private Queue<PooledServiceActivity> _workflowPool = new Queue<PooledServiceActivity>();
        private StringBuilder _xamlDefinition;

        private Stream _xamlStream;

        #endregion

        #region Public Properties

        public int CommandTimeout
        {
            get { return _commandTimeout; }

            set { _commandTimeout = value; }
        }

        public Stream XamlStream => _xamlStream;
        
        [JsonConverter(typeof (StringEnumConverter))]
        public enActionType ActionType { get; set; }
        
        public string SourceName { get; set; }
        
        public string SourceMethod { get; set; }
        
        public Source Source { get; set; }
        
        public StringBuilder XamlDefinition
        {
            get { return _xamlDefinition; }
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
            get { return _resultsToClient; }
            set { _resultsToClient = value; }
        }
        
        public bool TerminateServiceOnFault
        {
            get { return _terminateServiceOnFault; }
            set { _terminateServiceOnFault = value; }
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
                        Activity activity = ActivityXamlServices.Load(_xamlStream);
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
            if (!_disposing)
            {
                if (disposing)
                {
                    _xamlStream.Close();
                    _xamlStream.Dispose();
                }
            }
            _disposing = true;
        }
    }

    #endregion

    public sealed class PooledServiceActivity
    {
        private readonly int _generation;
        private readonly Activity _value;

        internal PooledServiceActivity(int generation, Activity value)
        {
            _generation = generation;
            _value = value;
        }

        public int Generation => _generation;

        public Activity Value => _value;
    }
}