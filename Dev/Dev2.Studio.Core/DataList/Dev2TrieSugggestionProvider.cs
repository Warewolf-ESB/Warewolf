using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Gma.DataStructures.StringSearch;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Studio.Core.DataList
{
    public class Dev2TrieSugggestionProvider : ISuggestionProvider
    {
        #region Implementation of ISuggestionProvider

        private readonly char[] _tokenisers = "!#$%^&-=_+{}|:\"?><`~<>?:'{}| [](".ToCharArray();
        private ITrie<string> PatriciaTrie { get; set; }
        private ObservableCollection<string> _variableList;

        public ObservableCollection<string> VariableList
        {
            get
            {
                return _variableList;
            }
            set
            {
                _variableList = value;
                PatriciaTrie = new SuffixTrie<string>(1);

                PatriciaTrieScalars = new SuffixTrie<string>(1);
                var vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsScalarExpression);
                foreach (var var in vars)
                {
                    var currentVar = var as LanguageAST.LanguageExpression.ScalarExpression;
                    if (currentVar != null)
                    {
                        PatriciaTrieScalars.Add(DataListUtil.AddBracketsToValueIfNotExist(currentVar.Item), DataListUtil.AddBracketsToValueIfNotExist(currentVar.Item));
                    }
                }
                PatriciaTrieRecsets = new SuffixTrie<string>(1);
                vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsRecordSetNameExpression);
                foreach (var var in vars)
                {
                    var currentVar = var as LanguageAST.LanguageExpression.RecordSetNameExpression;
                    if (currentVar != null)
                    {
                        var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(currentVar.Item.Name, Equals(currentVar.Item.Index, LanguageAST.Index.Star)));
                        PatriciaTrieRecsets.Add(name, name);
                    }
                }
                PatriciaTrieRecsetsFields = new SuffixTrie<string>(1);
                vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsRecordSetExpression || a.IsRecordSetNameExpression);
                foreach (var var in vars)
                {
                    var currentVar = var as LanguageAST.LanguageExpression.RecordSetExpression;
                    if (currentVar != null)
                    {
                        var index = "";
                        if (currentVar.Item.Index.IsStar)
                        {
                            index = "*";
                        }
                        var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(currentVar.Item.Name, currentVar.Item.Column, index));
                        PatriciaTrieRecsetsFields.Add(name, name);
                    }
                }
                PatriciaTrieJsonObjects = new SuffixTrie<string>(1);
                vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsJsonIdentifierExpression);
                foreach (var var in vars)
                {
                    var jsonIdentifierExpression = var as LanguageAST.LanguageExpression.JsonIdentifierExpression;
                    if (jsonIdentifierExpression != null)
                    {
                        AddJsonVariables(jsonIdentifierExpression.Item, null);
                    }
                }
                vars = _variableList.Select(ParseExpression).OrderBy(a => a);
                foreach (var var in vars)
                {
                    var recordSetExpression = var as LanguageAST.LanguageExpression.RecordSetExpression;
                    if (recordSetExpression != null)
                    {
                        var index = "";
                        if (recordSetExpression.Item.Index.IsStar)
                        {
                            index = "*";
                        }
                        var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(recordSetExpression.Item.Name, recordSetExpression.Item.Column, index));
                        PatriciaTrie.Add(name, name);
                    }
                    else
                    {
                        var recordSetNameExpression = var as LanguageAST.LanguageExpression.RecordSetNameExpression;
                        if (recordSetNameExpression != null)
                        {
                            var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(recordSetNameExpression.Item.Name, Equals(recordSetNameExpression.Item.Index, LanguageAST.Index.Star)));
                            PatriciaTrie.Add(name, name);
                        }
                        else
                        {
                            var scalarExpression = var as LanguageAST.LanguageExpression.ScalarExpression;
                            if (scalarExpression != null)
                            {
                                PatriciaTrie.Add(DataListUtil.AddBracketsToValueIfNotExist(scalarExpression.Item), DataListUtil.AddBracketsToValueIfNotExist(scalarExpression.Item));
                            }
                        }
                    }
                }
            }
        }

        private static LanguageAST.LanguageExpression ParseExpression(string a)
        {
            try
            {
                var languageExpression = EvaluationFunctions.parseLanguageExpression(a, 0);
                return languageExpression;
            }
            catch(Exception)
            {
                //
            }
            return LanguageAST.LanguageExpression.NewWarewolfAtomExpression(DataStorage.WarewolfAtom.Nothing);
        }

        private LanguageAST.JsonIdentifierExpression AddJsonVariables(LanguageAST.JsonIdentifierExpression currentVar, string parentName)
        {
            if (currentVar != null)
            {

                var namedExpression = currentVar as LanguageAST.JsonIdentifierExpression.NameExpression;
                if (namedExpression != null)
                {
                    var name = namedExpression.Item.Name;
                    var objectName = parentName == null ? name : parentName + "." + name;
                    if (!objectName.Contains("@"))
                    {
                        objectName = "@" + objectName;
                    }
                    PatriciaTrieJsonObjects.Add(DataListUtil.AddBracketsToValueIfNotExist(objectName), DataListUtil.AddBracketsToValueIfNotExist(objectName));
                    return null;
                }

                var indexNestedExpression = currentVar as LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression;
                if (indexNestedExpression != null)
                {
                    var name = Equals(indexNestedExpression.Item.Index, LanguageAST.Index.Star) ? indexNestedExpression.Item.ObjectName + "(*)" : indexNestedExpression.Item.ObjectName + "()";
                    var objectName = parentName == null ? name : parentName + "." + name;
                    if (!objectName.Contains("@"))
                    {
                        objectName = "@" + objectName;
                    }
                    PatriciaTrieJsonObjects.Add(DataListUtil.AddBracketsToValueIfNotExist(objectName), DataListUtil.AddBracketsToValueIfNotExist(objectName));
                    return AddJsonVariables(indexNestedExpression.Item.Next, objectName);
                }

                var nestedNameExpression = currentVar as LanguageAST.JsonIdentifierExpression.NestedNameExpression;
                if (nestedNameExpression != null)
                {
                    var objectName = parentName == null ? nestedNameExpression.Item.ObjectName : parentName + "." + nestedNameExpression.Item.ObjectName;
                    if (!objectName.Contains("@"))
                    {
                        objectName = "@" + objectName;
                    }
                    PatriciaTrieJsonObjects.Add(DataListUtil.AddBracketsToValueIfNotExist(objectName), DataListUtil.AddBracketsToValueIfNotExist(objectName));
                    var next = nestedNameExpression.Item.Next;
                    return AddJsonVariables(next, objectName);
                }
            }
            return null;
        }

        private SuffixTrie<string> PatriciaTrieRecsetsFields { get; set; }
        private SuffixTrie<string> PatriciaTrieRecsets { get; set; }
        private SuffixTrie<string> PatriciaTrieScalars { get; set; }
        private SuffixTrie<string> PatriciaTrieJsonObjects { get; set; }
        public int Level { get; set; }

        public Dev2TrieSugggestionProvider()
        {
            VariableList = new ObservableCollection<string>();
            PatriciaTrie = new PatriciaTrie<string>();
        }

      
        public IEnumerable<string> GetSuggestions(string orignalText, int caretIndex, bool tokenise, enIntellisensePartType type)
        {
            if (caretIndex < 0) return new List<string>();
            string filter;
            if (tokenise)
            {
                if (caretIndex > orignalText.Length) caretIndex = orignalText.Length;
                string texttrimmedRight = orignalText.Substring(0, caretIndex);
                int start = texttrimmedRight.LastIndexOf(texttrimmedRight.Split(_tokenisers).Last(), StringComparison.Ordinal);
                filter = texttrimmedRight.Substring(start);
            }
            else
            {
                filter = orignalText;
            }
            var trie = PatriciaTrie;
            switch (type)
            {
                case enIntellisensePartType.RecordsetsOnly:
                    if (orignalText.Contains("(") && orignalText.IndexOf("(", StringComparison.Ordinal) < caretIndex)
                        trie = PatriciaTrie;
                    else
                        trie = PatriciaTrieRecsets;
                    break;

                case enIntellisensePartType.ScalarsOnly:
                    trie = PatriciaTrieScalars;
                    break;

                case enIntellisensePartType.JsonObject:
                    trie = PatriciaTrieJsonObjects;
                    break;

                case enIntellisensePartType.RecordsetFields:
                    if (orignalText.Contains("(") && orignalText.IndexOf("(", StringComparison.Ordinal) < caretIndex)
                        trie = PatriciaTrie;
                    else
                        trie = PatriciaTrieRecsetsFields;
                    break;
            }
            if (filter.EndsWith("[["))
                return trie.Retrieve("[[");
            if (!filter.StartsWith("[[") && filter.Contains("[["))
                return trie.Retrieve(filter.Substring(filter.LastIndexOf("[[", StringComparison.Ordinal)));
            if (filter == "]" || filter == "]]")
            {
                return new string[0];
            }
            var suggestions = trie.Retrieve(filter);
            return suggestions;
        }

        #endregion Implementation of ISuggestionProvider
    }
}