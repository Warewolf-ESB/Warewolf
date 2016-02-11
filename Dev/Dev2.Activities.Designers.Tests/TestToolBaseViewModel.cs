using System;
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

    class Region : IToolRegion {
        private double _minHeight;
        private double _currentHeight;
        private bool _isVisible;
        private double _maxHeight;

        #region Implementation of IToolRegion

        public double MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                _minHeight = value;
            }
        }
        public double CurrentHeight
        {
            get
            {
                return _currentHeight;
            }
            set
            {
                _currentHeight = value;
            }
        }
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
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;
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

        public event HeightChanged HeightChanged;


        public void HeightChangedNow()
        {
            if (HeightChanged != null)
                HeightChanged(this, this);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    class ImplRegionBase:CustomToolWithRegionBase
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
            return new List<IToolRegion>{new Region(){CurrentHeight = 25,IsVisible = true,MaxHeight = 33,MinHeight = 2}};
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
            b.Regions=b.BuildRegions();
            b.ReCalculateHeight();
            Assert.AreEqual(33,b.DesignMaxHeight);
            Assert.AreEqual(2, b.DesignMinHeight);
            Assert.AreEqual(25, b.DesignHeight);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CustomToolWithRegionBase_Ctor")]
        public void CustomToolWithRegionBase_Ctor_ValidRegions_HasCorrectHeights()
        {
            //------------Setup for test--------------------------
            var b = new ImplRegionBase(ModelItemUtils.CreateModelItem(new DsfFileRead()), new List<IToolRegion> { new Region() { CurrentHeight = 25, IsVisible = true, MaxHeight = 33, MinHeight = 2 }, new Region() { CurrentHeight = 69, IsVisible = true, MaxHeight = 85, MinHeight = 3 } });
            
            //------------Execute Test---------------------------
           
            Assert.AreEqual(118, b.DesignMaxHeight);
            Assert.AreEqual(5, b.DesignMinHeight);
            Assert.AreEqual(94, b.DesignHeight);

            //------------Assert Results-------------------------
        }
    }
}
