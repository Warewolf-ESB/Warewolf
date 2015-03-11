
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.DataList.Contract.Network
{
    public abstract class DataListMessage
    {
        public long Handle { get; set; }

        public bool HasError
        {
            get
            {
                return Errors.HasErrors();
            }
            // ReSharper disable ValueParameterNotUsed
            set
            // ReSharper restore ValueParameterNotUsed
            {

            }
        }

        public string ErrorMessage
        {
            get
            {
                return string.Join("\n", Errors.FetchErrors());
            }
            set
            {
                Errors.AddError(value);
            }
        }

        public ErrorResultTO Errors { get; set; }

        public abstract void Read(IByteReaderBase reader);

        public abstract void Write(IByteWriterBase writer);
    }
}
