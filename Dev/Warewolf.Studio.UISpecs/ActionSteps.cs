﻿using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;
using Warewolf.Studio.UISpecs.OutsideWorkflowDesignSurfaceUIMapClasses;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public class ActionSteps
    {
        [Given(@"I '(.*)'")]
        [When(@"I '(.*)'")]
        [Then(@"I '(.*)'")]
        public void ThenTheRecordedActionIsPerformed(string p0)
        {
            Type workflowDesignerMapType = Uimap.GetType();
            Type outsideWorkflowDesignerMapType = OutsideWorkflowDesignSurfaceUiMap.GetType();
            MethodInfo workflowDesignerAction = workflowDesignerMapType.GetMethod(p0);
            MethodInfo outsideWorkflowDesignerAction = outsideWorkflowDesignerMapType.GetMethod(p0);
            if (workflowDesignerAction != null && outsideWorkflowDesignerAction != null)
            {
                throw new InvalidOperationException("Cannot distinguish between duplicated action recordings, both named '" + p0 + "' in different UI maps.");
            }
            else
            {
                if (outsideWorkflowDesignerAction != null)
                {
                    outsideWorkflowDesignerAction.Invoke(OutsideWorkflowDesignSurfaceUiMap, new object[] { });
                }
                if (workflowDesignerAction != null)
                {
                    workflowDesignerAction.Invoke(Uimap, new object[] { });
                }
            }
        }

        #region Properties and Fields

        UIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        OutsideWorkflowDesignSurfaceUIMap OutsideWorkflowDesignSurfaceUiMap
        {
            get
            {
                if ((_outsideWorkflowDesignSurfaceUiMap == null))
                {
                    _outsideWorkflowDesignSurfaceUiMap = new OutsideWorkflowDesignSurfaceUIMap();
                }

                return _outsideWorkflowDesignSurfaceUiMap;
            }
        }

        private OutsideWorkflowDesignSurfaceUIMap _outsideWorkflowDesignSurfaceUiMap;

        #endregion
    }
}
