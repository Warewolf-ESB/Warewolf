using System;
using System.Collections.Generic;
using System.Windows;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Workflow;
using Unlimited.Framework;
using System.Activities.Presentation;
using System.Activities.Presentation.Debug;
using System.Activities;
using System.Activities.Presentation.Services;
using System.IO;
using System.Activities.XamlIntegration;
using System.Activities.Debugger;
using System.Activities.Core.Presentation;
using System.Activities.Tracking;
using System.Windows.Threading;
using System.Threading;
using System.Activities.Presentation.Metadata;
using Dev2.Studio.Core.Models;
using System.Xml.Linq;
using Dev2.DataList.Contract;

namespace Dev2.Studio.Views.Workflow {
    /// <summary>
    /// Interaction logic for WorkflowDebuggerWindow.xaml
    /// </summary>
    public partial class WorkflowDebuggerWindow {

        IFrameworkDataChannel _dsfChannel;
        IApplicationMessage _applicationMessage;
        private readonly string _dataList;
        private readonly string _workflowName;
        private readonly string _workflowDef;
        private readonly string _inputData;

       
        public WorkflowDebuggerWindow() {
            InitializeComponent();
        }

        public WorkflowDebuggerWindow(IFrameworkDataChannel dsfChannel, string workflowName, string workflowDefinition, string inputData, string dataList) : this() {
            _dsfChannel = dsfChannel;
            _applicationMessage = new ActivityMessage();
            _applicationMessage.MessageReceived += _applicationMessage_MessageReceived;
            _workflowName = workflowName;
            _workflowDef = workflowDefinition;
            _dataList = dataList;
            _inputData = inputData;
            RegisterMetadata();
        }

        void _applicationMessage_MessageReceived(string message) {
            Dispatcher.Invoke(DispatcherPriority.Normal,
                  new Action(
                    delegate() {
                        if (OnOutputMessageReceived != null) {
                            OnOutputMessageReceived(message);
                        }
                    }
                  )
            );
        }

        public event StringMessageEventHandler OnOutputMessageReceived;
        

        #region Properties
        public WorkflowDesigner WorkflowDesigner { get; set; }
        public IDesignerDebugView DebuggerService { get; set; }
        #endregion

        #region Debugger Methods
        private void RegisterMetadata() {
            (new DesignerMetadata()).Register();
            WorkflowDesigner = new WorkflowDesigner();
            AttributeTableBuilder builder = new AttributeTableBuilder();
            MetadataStore.AddAttributeTable(builder.CreateTable());

            DebuggerService = this.WorkflowDesigner.DebugManagerView;
            WorkflowDesigner.Text = _workflowDef;
            WorkflowDesigner.Load();
            DebugWorkflowView.Children.Add(this.WorkflowDesigner.View);
        }

        private void ShowDebug(SourceLocation sourceLocation) {
            this.Dispatcher.Invoke(DispatcherPriority.Render
                , (Action)(() => {
                    this.WorkflowDesigner.DebugManagerView.CurrentLocation = sourceLocation;
                }));
        }

        private Dictionary<object, SourceLocation> UpdateSourceLocationMappingInDebuggerService() {
            object rootInstance = GetRootInstance();
            Dictionary<object, SourceLocation> sourceLocationMapping = new Dictionary<object, SourceLocation>();
            Dictionary<object, SourceLocation> designerSourceLocationMapping = new Dictionary<object, SourceLocation>();

            if (rootInstance != null) {
                Activity documentRootElement = GetRootWorkflowElement(rootInstance);
                SourceLocationProvider.CollectMapping(GetRootRuntimeWorkflowElement(), documentRootElement, sourceLocationMapping, "");
                SourceLocationProvider.CollectMapping(documentRootElement, documentRootElement, designerSourceLocationMapping, "");
            }

            // Notify the DebuggerService of the new sourceLocationMapping.
            // When rootInstance == null, it'll just reset the mapping.
            if (DebuggerService != null) {
                ((DebuggerService)DebuggerService).UpdateSourceLocations(designerSourceLocationMapping);
            }

            return sourceLocationMapping;
        }

        private object GetRootInstance() {
            ModelService modelService = WorkflowDesigner.Context.Services.GetService<ModelService>();
            if (modelService != null) {
                return modelService.Root.GetCurrentValue();
            }
            return null;
        }

        private Activity GetRootWorkflowElement(object rootModelObject) {
            System.Diagnostics.Debug.Assert(rootModelObject != null, "Cannot pass null as rootModelObject");

            Activity rootWorkflowElement;
            IDebuggableWorkflowTree debuggableWorkflowTree = rootModelObject as IDebuggableWorkflowTree;
            if (debuggableWorkflowTree != null) {
                rootWorkflowElement = debuggableWorkflowTree.GetWorkflowRoot();
            }
            else  {
                rootWorkflowElement = rootModelObject as Activity;
            }
            return rootWorkflowElement;
        }

        private Activity GetRootRuntimeWorkflowElement() {
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(_workflowDef);

            MemoryStream m = new MemoryStream(data);
            Activity root = ActivityXamlServices.Load(m);
            WorkflowInspectionServices.CacheMetadata(root);

            IEnumerator<Activity> enumerator1 = WorkflowInspectionServices.GetActivities(root).GetEnumerator();
            //Get the first child of the x:class
            enumerator1.MoveNext();
            root = enumerator1.Current;
            return root;
        }

        Activity GetRuntimeExecutionRoot() {
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(_workflowDef);
            MemoryStream m = new MemoryStream(data);
            Activity root = ActivityXamlServices.Load(m);
            WorkflowInspectionServices.CacheMetadata(root);

            return root;
        }

        private Dictionary<string, Activity> BuildActivityIdToWfElementMap(Dictionary<object, SourceLocation> wfElementToSourceLocationMap) {
            Dictionary<string, Activity> map = new Dictionary<string, Activity>();

            Activity wfElement;
            foreach (object instance in wfElementToSourceLocationMap.Keys) {
                wfElement = instance as Activity;
                if (wfElement != null) {
                    map.Add(wfElement.Id, wfElement);
                }
            }

            return map;
        }
        #endregion

        #region Public Methods

        public void RunDebugger(double transitionPeriod) {
            WorkflowInvoker instance = new WorkflowInvoker(GetRuntimeExecutionRoot());
            Dictionary<object, SourceLocation> wfElementToSourceLocationMap = UpdateSourceLocationMappingInDebuggerService();
            Dictionary<string, Activity> activityIdToWfElementMap = BuildActivityIdToWfElementMap(wfElementToSourceLocationMap);

            string all = "*";

            CustomTrackingParticipant debugger = new CustomTrackingParticipant() {
                TrackingProfile = new TrackingProfile() {
                    Name = string.Format("{0}-Debug", _workflowName),
                    Queries = {
                        new CustomTrackingQuery(){ Name = all, ActivityName = all},
                        new WorkflowInstanceQuery(){States = { WorkflowInstanceStates.Started, WorkflowInstanceStates.Completed}},
                        new ActivityStateQuery() {ActivityName = all, States = {all}, Variables = {all} }
                    }
                }
            };

            debugger.ActivityIdToWorkflowElementMap = activityIdToWfElementMap;

            debugger.TrackingRecordReceived += (trackingParticipant, trackingEventArgs) => {
                
                if (trackingEventArgs.TrackingActivity != null) {
                    ShowDebug(wfElementToSourceLocationMap[trackingEventArgs.TrackingActivity]);
                    int transition = 0;
                    int.TryParse(transitionPeriod.ToString(), out transition);

                    Thread.Sleep(new TimeSpan(0,0,0,0,transition));
                }
            };

            var dataObject = new DsfDebuggerDataObject {XmlData = _inputData, DataList = _dataList};
            var binder = new DataListBinder();
            IDataListCompiler dlCompiler = DataListFactory.CreateDataListCompiler();

            instance.Extensions.Add(debugger);
            instance.Extensions.Add(_dsfChannel);
            instance.Extensions.Add(_applicationMessage);
            instance.Extensions.Add(dataObject);
            instance.Extensions.Add(binder);
            instance.Extensions.Add(dlCompiler);

            ThreadPool.QueueUserWorkItem((context) => {
                                             Dictionary<string, object> arguments = new Dictionary<string, object>();
                                             arguments.Add("AmbientDataList", new List<string> { _inputData, "<BDSDebugMode>true</BDSDebugMode>" });
                                             IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                                             try {
                                                 instance.Invoke(arguments);
                                             }
                                             catch(Exception ex) {
                    
                                                 MessageBox.Show(string.Format("Error occurred executing worflow \nPlease check the workflow definition\r\n{0}", new UnlimitedObject(ex).XmlString), "Error in workflow", MessageBoxButton.OK, MessageBoxImage.Error);
                                                 return;
                                             }
                                             // Emit Debug data 

                                             string result = dataObject.XmlData;

                                             SendDebugMessage(Environment.NewLine);
                                             SendDebugMessage("<DebugResult>" + Environment.NewLine);
                                             SendDebugMessage(XElement.Parse(result).ToString());
                                             SendDebugMessage(Environment.NewLine+"</DebugResult>");
                                             SendDebugMessage("</DebugData>");
                                         });

            //This is to remove the final debug adornment
            Dispatcher.Invoke(DispatcherPriority.Render
                , (Action)(() => {
                    WorkflowDesigner.DebugManagerView.CurrentLocation = new SourceLocation("", 1, 1, 1, 10);
                }));
        }

        internal void SendDebugMessage(string debugMessage) {
            if (!string.IsNullOrEmpty(debugMessage)) {
                Dispatcher.Invoke(DispatcherPriority.Normal,
                                  new Action(
                                      delegate() {
                                          if (OnOutputMessageReceived != null) {
                                              OnOutputMessageReceived(debugMessage);
                                          }
                                      }
                                      )
                    );
            }
        }

        #endregion
    }
}
