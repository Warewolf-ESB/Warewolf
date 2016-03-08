﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable ConvertToAutoProperty
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests
{

    class Region : IToolRegion
    {
        private bool _isVisible;

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            return this;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        public IList<string> Errors
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void HeightChangedNow()
        {
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    class ImplRegionBase : CustomToolWithRegionBase
    {
        public ImplRegionBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public ImplRegionBase(ModelItem modelItem, IList<IToolRegion> regions)
            : base(modelItem, regions)
        {
        }

        public ImplRegionBase(ModelItem modelItem, Action<Type> showExampleWorkflow, IList<IToolRegion> regions)
            : base(modelItem, showExampleWorkflow, regions)
        {
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        public override IList<IToolRegion> BuildRegions()
        {
            return new List<IToolRegion> { new Region() { IsVisible = true } };
        }

        #endregion
    }

    [TestClass]
    public class TestViewModelWithRegionsBase
    {
        [TestMethod]
        public void CustomToolWithRegionBase_Ctor()
        {
            CustomToolWithRegionBase b = new ImplRegionBase(ModelItemUtils.CreateModelItem(new DsfFileRead()));
            b.Regions = b.BuildRegions();
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CustomToolWithRegionBase_Ctor")]
        public void CustomToolWithRegionBase_Ctor_ValidRegions_HasCorrectHeights()
        {
            //------------Setup for test--------------------------
            var b = new ImplRegionBase(ModelItemUtils.CreateModelItem(new DsfFileRead()), new List<IToolRegion> { new Region() { IsVisible = true }, new Region() { IsVisible = true } });

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
