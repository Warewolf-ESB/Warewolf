
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Windows;
using Dev2.TO;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Activities.Designers2.Decision
{
    public partial class Large:IView
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = LargeDataGrid;
        }

        #region Overrides of ActivityDesignerTemplate

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid;
        }

        #endregion

        public void VerifyInputsAvailable()
        {
            if (LargeDataGrid.Items.Count <= 1)
           {
               throw  new Exception("Row count should be at least 2");
           }
           if(!FalseArmText.IsEnabled)
               throw new Exception("False arm not enabled");
           if (!TrueArmText.IsEnabled)
               throw new Exception("True arm not enabled");
           if (!DisplayText.IsEnabled)
               throw new Exception("Display arm not enabled");
        }

        public void VerifyEmptyRow()
        {
           var item = (DecisionTO) LargeDataGrid.Items[LargeDataGrid.Items.Count - 1];
           if(!String.IsNullOrEmpty( item.SearchCriteria) || !string.IsNullOrEmpty(item.MatchValue) || !string.IsNullOrEmpty(item.From) || !string.IsNullOrEmpty(item.To))
           {
               throw new Exception("no empty");
           }
           
        }

        public void VerifyOption(string option)
        {
            var vm = DataContext as DecisionDesignerViewModel;
            if(!DecisionTO.Whereoptions.Select(a=>a.HandlesType().ToLower()).Contains(option.ToLower()))
            {
                throw  new Exception("invalid match option: "+option);
            }
        }

        public void SetAllTrue(bool b)
        {
            And.IsChecked = b;
        }

        public void ClickDone()
        {

        }

        public bool GetAllTrue()
        {
            return And.IsChecked != null && And.IsChecked.Value;
        }

        public void DoneAction()
        {
        }

        public string GetDisplayName()
        {
            return DisplayText.Text;
        }
    }
}
