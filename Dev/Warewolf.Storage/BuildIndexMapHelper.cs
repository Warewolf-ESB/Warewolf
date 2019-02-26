/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Warewolf.Storage
{
    internal interface IBuildIndexMap
    {
        void Build(LanguageAST.JsonIdentifierExpression var, string exp, List<string> indexMap, JContainer container);
    }
    internal class BuildIndexMapHelper : IBuildIndexMap
    {
        readonly DataStorage.WarewolfEnvironment _env;
        public BuildIndexMapHelper(DataStorage.WarewolfEnvironment env)
        {
            _env = env;
        }
        public void Build(LanguageAST.JsonIdentifierExpression var, string exp, List<string> indexMap, JContainer container)
        {
            var jsonIdentifierExpression = var;
            if (jsonIdentifierExpression == null)
            {
                return;
            }
            if (jsonIdentifierExpression is LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression nameExpression)
            {
                var objectName = nameExpression.Item.ObjectName;
                JContainer obj;
                JArray arr = null;
                if (container == null)
                {
                    obj = _env.JsonObjects[objectName];
                    arr = obj as JArray;
                }
                else
                {
                    var props = container.FirstOrDefault(token => token.Type == JTokenType.Property && ((JProperty)token).Name == objectName);
                    if (props != null)
                    {
                        obj = props.First as JContainer;
                        arr = obj as JArray;
                    }
                    else
                    {
                        obj = container;
                    }
                }

                if (arr != null)
                {
                    BuildHelper(exp, indexMap, nameExpression, objectName, arr);
                }
                else
                {
                    if (!nameExpression.Item.Next.IsTerminal)
                    {
                        Build(nameExpression.Item.Next, exp, indexMap, obj);
                    }
                }
            }
            else
            {
                if (jsonIdentifierExpression is LanguageAST.JsonIdentifierExpression.NestedNameExpression nestedNameExpression)
                {
                    JContainer obj;
                    var objectName = nestedNameExpression.Item.ObjectName;
                    if (container == null)
                    {
                        obj = _env.JsonObjects[objectName];
                    }
                    else
                    {
                        var props = container.FirstOrDefault(token => token.Type == JTokenType.Property && ((JProperty)token).Name == objectName);
                        obj = props != null ? props.First as JContainer : container;
                    }
                    Build(nestedNameExpression.Item.Next, exp, indexMap, obj);
                }
            }
        }

        private void BuildHelper(string exp, List<string> indexMap, LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression nameExpression, string objectName, JArray arr)
        {
            var indexToInt = AssignEvaluation.indexToInt(LanguageAST.Index.Star, arr).ToList();
            foreach (var i in indexToInt)
            {
                if (!string.IsNullOrEmpty(exp))
                {
                    var indexed = objectName + @"(" + i + @")";
                    var updatedExp = exp.Replace(objectName + @"(*)", indexed);
                    indexMap.Add(updatedExp);
                    Build(nameExpression.Item.Next, updatedExp, indexMap, arr[i - 1] as JContainer);
                }
            }
        }
    }
}
