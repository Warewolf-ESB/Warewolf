
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.DataList.Contract;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph;
using System.Xml.Linq;

namespace Dev2.AnytingToXmlHook.Plugin
{
    /// <summary>
    /// Used as an entry point to the databrowser map functionality and to emit any given string data
    /// </summary>
    public class AnythignToXmlHookPlugin
    {
        /// <summary>
        /// Emit the data given in the StringData field of the payload
        /// </summary>
        public string EmitStringData(string data)
        {
            string stringData = string.Empty;

            try
            {
                XElement xelement = XElement.Parse(data).DescendantsAndSelf("StringData").Last();

                if (xelement.HasElements)
                {
                    stringData = String.Join("", xelement.Nodes().Select(x => x.ToString()).ToArray());
                }
                else
                {
                    stringData = xelement.Value;
                }
            }
            catch
            {
                // JSON data passed in deal with it correctly ;)
                stringData = data;
            }
            return "" + stringData + "";
        }

        /// <summary>
        /// Emit complex data
        /// </summary>
        public object EmitComplex()
        {
            Company company = new Company
            {
                Name = "Dev2",
                Departments = new List<Department>()
                {
                    new Department()
                    {
                        Name = "Dev",
                        Employees = new List<Person>
                        {
                            new Person()
                            {
                                Name = "Brendon",
                            },
                            new Person()
                            {
                                Name = "Jayd",
                            }
                        }
                    },
                    new Department()
                    {
                        Name = "Accounts",
                        Employees = new List<Person>
                        {
                            new Person()
                            {
                                Name = "Bob",
                            },
                            new Person()
                            {
                                Name = "Jo",
                            }
                        }
                    }
                }
            };

            return company;
        }

        /// <summary>
        /// Map the data given in the StringData field of the payload using the DataBrowser
        /// </summary>
        public string MapStringData(string data)
        {
            string stringData = ValueExtractor.GetValueFromDataList("StringData", data);

            IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();
            IEnumerable<IPath> mappedPaths = dataBrowser.Map(stringData);

            string pathsString = string.Join(Environment.NewLine, mappedPaths.Select(p => p.DisplayPath));

            return "<ADL><PathData>" + pathsString + "</PathData></ADL>";
        }
    }
}
