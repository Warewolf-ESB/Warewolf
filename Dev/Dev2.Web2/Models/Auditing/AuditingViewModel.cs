#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Auditing;
using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Dev2.Web2.Models.Auditing
{
    public class AuditingViewModel : AuditLog
    {
        public AuditingViewModel()
        {
            Protocols = new List<SelectListItem>
            {
                new SelectListItem{Text = "https",Value="https"}
                ,
                new SelectListItem{Text = "http",Value="http" }
            };
        }
        public List<SelectListItem> Protocols { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public string SortBy { get; set; }

        public string ComputerName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }
    }
    
}