
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.Data.Interfaces
{
    public interface IInputLanguageDefinition {

        #region Properties
        string Name { get; }

        string MapsTo { get; }

        string StartTagSearch { get; }

        string EndTagSearch { get; }

        string StartTagReplace { get; }

        string EndTagReplace { get; }

        bool IsEvaluated { get; }
        #endregion
    }
}
