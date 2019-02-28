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
        IList<string> Build(LanguageAST.JsonIdentifierExpression var, string exp);
    }
    internal class BuildJObjectArrayIndexMapHelper : IBuildIndexMap
    {
        readonly ExecutionEnvironment _env;
        readonly List<string>  _indexMap = new List<string>();
        public BuildJObjectArrayIndexMapHelper(ExecutionEnvironment env)
        {
            _env = env;
        }

        public IList<string> Build(LanguageAST.JsonIdentifierExpression var, string exp)
        {
            Build(var, exp, null);
            return _indexMap;
        }

        public void Build(LanguageAST.JsonIdentifierExpression var, string exp, JContainer container)
        {
            var jsonIdentifierExpression = var;
            if (jsonIdentifierExpression is null)
            {
                return;
            }

            if (jsonIdentifierExpression is LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression nameExpression)
            {
                BuildFromIndexNestedNameExpression(exp, container, nameExpression);
            }
            else
            {
                if (jsonIdentifierExpression is LanguageAST.JsonIdentifierExpression.NestedNameExpression nestedNameExpression)
                {
                    BuildFromNestedNameExpression(exp, container, nestedNameExpression);
                }
            }
            return;
        }

        private void BuildFromIndexNestedNameExpression(string exp, JContainer container, LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression nameExpression)
        {
            var objectName = nameExpression.Item.ObjectName;
            JArray arr = null;
            JContainer obj;
            obj = TryGetObjectOrArray(container, objectName, ref arr);

            if (arr != null)
            {
                BuildIndexFromJArray(exp, nameExpression, objectName, arr);
            }
            else
            {
                if (!nameExpression.Item.Next.IsTerminal)
                {
                    Build(nameExpression.Item.Next, exp, obj);
                }
            }
        }

        private void BuildIndexFromJArray(string exp, LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression nameExpression, string objectName, JArray arr)
        {
            var indexToInt = AssignEvaluation.indexToInt(LanguageAST.Index.Star, arr).ToList();
            foreach (var i in indexToInt)
            {
                if (!string.IsNullOrEmpty(exp))
                {
                    var indexed = objectName + @"(" + i + @")";
                    var updatedExp = exp.Replace(objectName + @"(*)", indexed);
                    _indexMap.Add(updatedExp);
                    Build(nameExpression.Item.Next, updatedExp, arr[i - 1] as JContainer);
                }
            }
        }

        private JContainer TryGetObjectOrArray(JContainer container, string objectName, ref JArray arr)
        {
            JContainer obj;
            if (container == null)
            {
                obj = _env.GetObject(objectName);
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

            return obj;
        }

        private void BuildFromNestedNameExpression(string exp, JContainer container, LanguageAST.JsonIdentifierExpression.NestedNameExpression nestedNameExpression)
        {
            JContainer obj;
            var objectName = nestedNameExpression.Item.ObjectName;
            if (container == null)
            {
                obj = _env.GetObject(objectName);
            }
            else
            {
                var props = container.FirstOrDefault(token => token.Type == JTokenType.Property && ((JProperty)token).Name == objectName);
                obj = props != null ? props.First as JContainer : container;
            }
            Build(nestedNameExpression.Item.Next, exp, obj);
        }
    }
}
