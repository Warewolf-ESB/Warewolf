
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

namespace DummyNamespaceForTest
{
    [Serializable]
    public class DummyClassForPluginTest
    {
        public string Name { get; set; }

        public DummyClassForPluginTest DummyMethod()
        {
            return new DummyClassForPluginTest
            {
                Name = "test data"
            };
        }

        public string Echo(string text)
        {
            return string.Format("<root><echo>{0}</echo><hack>wtf</hack></root>", text);
        }

        public string NoEcho()
        {
            return "<root><echo>None</echo><hack>wtf</hack></root>";
        }
    }
}
