/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

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

        internal Dev2ActivityIOPath(enActivityIOPathType type, string path, string user, string pass, bool isNotCertVerifiable,string privateKeyFile)
        {
            PathType = type;
            Path = path;
            Username = user;
            Password = pass;
            IsNotCertVerifiable = isNotCertVerifiable;
            PrivateKeyFile = privateKeyFile;
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
        public string PrivateKeyFile
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
