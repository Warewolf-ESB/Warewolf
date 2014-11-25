
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Data.SystemTemplates.Models
{

    /// <summary>
    /// Used to figure out the correct model type
    /// </summary>
    public class Dev2ModelTypeCheck
    {
        public Dev2ModelType ModelName { get; set; }

    }

    /// <summary>
    /// The model types ;)
    /// </summary>
    public enum Dev2ModelType
    {
        Dev2DecisionStack,
        Dev2Decision,
        Dev2Switch,
        Dev2SwitchCase
    }
}
