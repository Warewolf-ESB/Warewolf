using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Warewolf.Storage
{
    public class IndexMapBuilder : IIndexMapBuilder
    {
        private readonly DataStorage.WarewolfEnvironment _env;
        public IndexMapBuilder(DataStorage.WarewolfEnvironment env)
        {
            _env = env;
        }
        public void BuildIndexMap(LanguageAST.JsonIdentifierExpression var, string exp, List<string> indexMap, JContainer container)
        {
            var jsonIdentifierExpression = var;
            if (jsonIdentifierExpression == null) return;
            var nameExpression = jsonIdentifierExpression as LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression;
            if (nameExpression != null)
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
                    var indexToInt = AssignEvaluation.indexToInt(LanguageAST.Index.Star, arr).ToList();
                    foreach (var i in indexToInt)
                    {
                        if (string.IsNullOrEmpty(exp)) continue;
                        var indexed = objectName + "(" + i + ")";
                        var updatedExp = exp.Replace(objectName + "(*)", indexed);
                        indexMap.Add(updatedExp);
                        BuildIndexMap(nameExpression.Item.Next, updatedExp, indexMap, arr[i - 1] as JContainer);
                    }
                }
                else
                {
                    if (!nameExpression.Item.Next.IsTerminal)
                    {
                        BuildIndexMap(nameExpression.Item.Next, exp, indexMap, obj);
                    }
                }
            }
            else
            {
                var nestedNameExpression = jsonIdentifierExpression as LanguageAST.JsonIdentifierExpression.NestedNameExpression;
                if (nestedNameExpression == null) return;
                JContainer obj;
                var objectName = nestedNameExpression.Item.ObjectName;
                if (container == null)
                {
                    obj = _env.JsonObjects[objectName];
                }
                else
                {
                    var props = container.FirstOrDefault(token => token.Type == JTokenType.Property && ((JProperty)token).Name == objectName);
                    if (props != null)
                    {
                        obj = props.First as JContainer;
                    }
                    else
                    {
                        obj = container;
                    }
                }
                BuildIndexMap(nestedNameExpression.Item.Next, exp, indexMap, obj);
            }
        }
    }
}