/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Activities.Designers2.Web_Service_Get;

namespace Dev2.Activities.Designers2.WebServiceGet
{

    [Obsolete("DsfWebGetActivity is deprecated. It will be deleted in future releases along with this view designer.")]
    // Interaction logic for WebServiceGetDesigner.xaml
    public partial class WebServiceGetDesigner
    {
        public WebServiceGetDesigner()
        {
            InitializeComponent();
        }
        protected override WebServiceGetViewModel CreateViewModel() => new WebServiceGetViewModel(ModelItem);
    }
}
