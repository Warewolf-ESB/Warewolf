
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
using System.Text;

namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a concrete impl of the IActivityIOPath interface
    /// </summary>
    [Serializable]
    public class Dev2ActivityIOPath : IActivityIOPath
    {

        internal Dev2ActivityIOPath(enActivityIOPathType type, string path, string user, string pass,bool isNotCertVerifiable)
        {
            PathType = type;
            Path = path;
            Username = user;
            Password = pass;
            IsNotCertVerifiable = isNotCertVerifiable;

        }

        /// <summary>
        /// Convert the object to XML
        /// </summary>
        /// <returns></returns>
        public string ToXML()
        {
            StringBuilder result = new StringBuilder("<AcitivityIOPath>");

            result.Append("<TypeOf>" + PathType + "</TypeOf>");
            result.Append("<Path>" + Path + "</Path>");
            result.Append("<Username>" + Username + "</Username>");
            result.Append("<Password>" + Password + "</Password>");

            result.Append("</AcitivityIOPath>");

            return result.ToString();
        }

        public enActivityIOPathType PathType
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public bool IsNotCertVerifiable
        {
            get;
            set;
        }
    }
}
