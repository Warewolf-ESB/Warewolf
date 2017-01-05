/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Common.Interfaces.Data
{
    public interface IDev2Definition
    {
        #region Properties

        string Name { get; }

        string MapsTo { get; }

        string Value { get; }

        bool IsRecordSet { get; }

        string RecordSetName { get; }

        bool IsEvaluated { get; }

        string DefaultValue { get; }

        bool IsRequired { get; }

        string RawValue { get; }

        bool EmptyToNull { get; }

        bool IsTextResponse { get; }
        bool IsObject { get; set; }

        #endregion
    }
}