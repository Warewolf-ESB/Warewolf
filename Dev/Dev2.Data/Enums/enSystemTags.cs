
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


/*
 *  List of system tags, please update as required
 */
namespace Dev2.DataList.Contract
{

    /*
     * 17.07.2012 - Travis.Frisinger 
     * 
     * Please note any additional tags added here, but be added to the 
     * DataListUtil class's IsSystemTag method as this avoids any nasty
     * issues where a tag below is seen as a recordset with nested data
     * causing all sorts of havoc with the Intellisense parsing
     * 
     * 20.09.2013 - Travis.Frisinger
     * 
     * Removed a ton of useless tags ;)
     */
    public enum enSystemTag {
        InstanceId,
        ParentWorkflowInstanceId,
        ParentServiceName,
        ParentInstanceID,
        Dev2Error,
        SystemModel,
        Service,
        WorkflowInstanceId,
        ManagmentServicePayload,
        wid,
        PostData
    }
}
