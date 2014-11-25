
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common.Interfaces.ComponentModel;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    public class WorkflowDescriptor : PropertyChangedBase, IWorkflowDescriptor
    {
        private bool _isSelected;

        public string ResourceID { get; set; }
        public string ResourceName { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if(_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        #region WorkflowDescriptor

        public WorkflowDescriptor()
        {
        }

        public WorkflowDescriptor(XElement xml)
        {
            if(xml == null)
            {
                return;
            }

            ResourceName = xml.AttributeSafe("ResourceName");
            ResourceID = xml.AttributeSafe("ResourceID");
            IsSelected = true;
        }

        #endregion

        #region ToXml

        public XElement ToXml()
        {
            var result = new XElement("Workflow",
                new XAttribute("ResourceID", ResourceID ?? string.Empty),
                new XAttribute("ResourceName", ResourceName ?? string.Empty)
                );
            return result;
        }

        #endregion
    }
}
