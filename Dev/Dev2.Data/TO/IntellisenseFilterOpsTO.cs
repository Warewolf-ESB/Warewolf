/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Data.Interfaces;

namespace Dev2.Data.TO
{
    public class IntellisenseFilterOpsTO : IIntellisenseFilterOpsTO
    {
        public enIntellisensePartType FilterType { get; set; }
        public string FilterCondition { get; set; }
    }
}
