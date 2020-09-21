/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Web_Service_Get_With_Base64;


namespace Dev2.Activities.Designers2.WebServiceGetWithBase64
{
    // Interaction logic for WebServiceGetWithBase64Designer.xaml
    public partial class WebServiceGetWithBase64Designer
    {
        public WebServiceGetWithBase64Designer()
        {
            InitializeComponent();
        }
        protected override WebServiceGetViewModel CreateViewModel() => new WebServiceGetViewModel(ModelItem);
    }
}
