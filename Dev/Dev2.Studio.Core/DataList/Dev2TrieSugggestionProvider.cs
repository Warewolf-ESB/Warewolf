#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Gma.DataStructures.StringSearch;
using static DataStorage;

namespace Dev2.Studio.Core.DataList
{
    public class Dev2TrieSugggestionProvider : ISuggestionProvider
    {
        #region Implementation of ISuggestionProvider

        readonly char[] _tokenisers = "!#$%^&-=_+{}|:\"?><`~<>?:'{}| [](".ToCharArray();
        ITrie<string> PatriciaTrie { get; set; }
        ObservableCollection<string> _variableList;

        public ObservableCollection<string> VariableList
        {
            get => _variableList;
            set
            {
                _variableList = value;
                PatriciaTrie = new SuffixTrie<string>(1);
                AddScalars();
                AddRecsets();
                AddFields();
                AddJsonObjects();
                AddMutations();
            }
        }

        void AddMutations()
        {
            var vars = _variableList.Select(ParseExpression).OrderBy(a => a);
            foreach (var var in vars)
            {
                if (var is LanguageAST.LanguageExpression.RecordSetExpression recordSetExpression)
                {
                    AddIndexAsStarMutation(recordSetExpression);
                }
                else
                {
                    AddPermutations(var);
                }
            }
        }

        void AddPermutations(LanguageAST.LanguageExpression var)
        {
            if (var is LanguageAST.LanguageExpression.RecordSetNameExpression recordSetNameExpression)
            {
                var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(recordSetNameExpression.Item.Name, Equals(recordSetNameExpression.Item.Index, LanguageAST.Index.Star)));
                foreach (var permutation in PermuteCapitalizations(name))
                {
                    PatriciaTrie.Add(permutation, name);
                }
            }
            else
            {
                if (var is LanguageAST.LanguageExpression.ScalarExpression scalarExpression)
                {
                    var key = DataListUtil.AddBracketsToValueIfNotExist(scalarExpression.Item);
                    foreach (var permutation in PermuteCapitalizations(key))
                    {
                        PatriciaTrie.Add(permutation, DataListUtil.AddBracketsToValueIfNotExist(scalarExpression.Item));
                    }
                }
            }
        }

        void AddIndexAsStarMutation(LanguageAST.LanguageExpression.RecordSetExpression recordSetExpression)
        {
            var index = "";
            if (recordSetExpression.Item.Index.IsStar)
            {
                index = "*";
            }
            var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(recordSetExpression.Item.Name, recordSetExpression.Item.Column, index));
            foreach (var permutation in PermuteCapitalizations(name))
            {
                PatriciaTrie.Add(permutation, name);
            }
        }

        void AddJsonObjects()
        {
            PatriciaTrieJsonObjects = new SuffixTrie<string>(1);
            var vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsJsonIdentifierExpression);
            foreach (var var in vars)
            {
                if (var is LanguageAST.LanguageExpression.JsonIdentifierExpression jsonIdentifierExpression)
                {
                    AddJsonVariables(jsonIdentifierExpression.Item, null);
                }
            }
        }

        void AddFields()
        {
            PatriciaTrieRecsetsFields = new SuffixTrie<string>(1);
            var vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsRecordSetExpression || a.IsRecordSetNameExpression);
            foreach (var var in vars)
            {
                if (var is LanguageAST.LanguageExpression.RecordSetExpression currentVar)
                {
                    var index = "";
                    if (currentVar.Item.Index.IsStar)
                    {
                        index = "*";
                    }
                    var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(currentVar.Item.Name, currentVar.Item.Column, index));
                    foreach (var permutation in PermuteCapitalizations(name))
                    {
                        PatriciaTrieRecsetsFields.Add(permutation, name);
                    }
                }
            }
        }

        void AddRecsets()
        {
            IEnumerable<LanguageAST.LanguageExpression> vars;
            PatriciaTrieRecsets = new SuffixTrie<string>(1);
            vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsRecordSetNameExpression);
            foreach (var var in vars)
            {
                if (var is LanguageAST.LanguageExpression.RecordSetNameExpression currentVar)
                {
                    var name = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(currentVar.Item.Name, Equals(currentVar.Item.Index, LanguageAST.Index.Star)));
                    foreach (var permutation in PermuteCapitalizations(name))
                    {
                        PatriciaTrieRecsets.Add(permutation, name);
                    }
                }
            }
        }

        void AddScalars()
        {
            PatriciaTrieScalars = new SuffixTrie<string>(1);
            var vars = _variableList.Select(ParseExpression).OrderBy(a => a).Where(a => a.IsScalarExpression);
            foreach (var var in vars)
            {
                if (var is LanguageAST.LanguageExpression.ScalarExpression currentVar)
                {
                    var key = DataListUtil.AddBracketsToValueIfNotExist(currentVar.Item);
                    foreach (var permutation in PermuteCapitalizations(key))
                    {
                        PatriciaTrieScalars.Add(permutation, DataListUtil.AddBracketsToValueIfNotExist(currentVar.Item));
                    }
                }
            }
        }

        static LanguageAST.LanguageExpression ParseExpression(string a)
        {
            try
            {
                var languageExpression = EvaluationFunctions.parseLanguageExpression(a, 0, ShouldTypeCast.Yes);
                return languageExpression;
            }
            catch (Exception)
            {
                //
            }
            return LanguageAST.LanguageExpression.NewWarewolfAtomExpression(DataStorage.WarewolfAtom.Nothing);
        }

        LanguageAST.JsonIdentifierExpression AddJsonVariables(LanguageAST.JsonIdentifierExpression currentVar, string parentName)
        {
            if (currentVar != null)
            {

                if (currentVar is LanguageAST.JsonIdentifierExpression.NameExpression namedExpression)
                {
                    var name = namedExpression.Item.Name;
                    var objectName = parentName == null ? name : parentName + "." + name;
                    if (!objectName.Contains("@"))
                    {
                        objectName = "@" + objectName;
                    }
                    var key = DataListUtil.AddBracketsToValueIfNotExist(objectName);
                    foreach (var permutation in PermuteCapitalizations(key))
                    {
                        PatriciaTrieJsonObjects.Add(permutation, DataListUtil.AddBracketsToValueIfNotExist(objectName));
                    }
                    return null;
                }

                if (currentVar is LanguageAST.JsonIdentifierExpression.IndexNestedNameExpression indexNestedExpression)
                {
                    var name = Equals(indexNestedExpression.Item.Index, LanguageAST.Index.Star) ? indexNestedExpression.Item.ObjectName + "(*)" : indexNestedExpression.Item.ObjectName + "()";
                    var objectName = parentName == null ? name : parentName + "." + name;
                    if (!objectName.Contains("@"))
                    {
                        objectName = "@" + objectName;
                    }
                    var key = DataListUtil.AddBracketsToValueIfNotExist(objectName);
                    foreach (var permutation in PermuteCapitalizations(key))
                    {
                        PatriciaTrieJsonObjects.Add(permutation, DataListUtil.AddBracketsToValueIfNotExist(objectName));
                    }
                    return AddJsonVariables(indexNestedExpression.Item.Next, objectName);
                }

                if (currentVar is LanguageAST.JsonIdentifierExpression.NestedNameExpression nestedNameExpression)
                {
                    var objectName = parentName == null ? nestedNameExpression.Item.ObjectName : parentName + "." + nestedNameExpression.Item.ObjectName;
                    if (!objectName.Contains("@"))
                    {
                        objectName = "@" + objectName;
                    }
                    var key = DataListUtil.AddBracketsToValueIfNotExist(objectName);
                    foreach (var permutation in PermuteCapitalizations(key))
                    {
                        PatriciaTrieJsonObjects.Add(permutation, DataListUtil.AddBracketsToValueIfNotExist(objectName));
                    }
                    var next = nestedNameExpression.Item.Next;
                    return AddJsonVariables(next, objectName);
                }
            }
            return null;
        }

        SuffixTrie<string> PatriciaTrieRecsetsFields { get; set; }
        SuffixTrie<string> PatriciaTrieRecsets { get; set; }
        SuffixTrie<string> PatriciaTrieScalars { get; set; }
        SuffixTrie<string> PatriciaTrieJsonObjects { get; set; }
        public int Level { get; set; }

        public Dev2TrieSugggestionProvider()
        {
            VariableList = new ObservableCollection<string>();
            PatriciaTrie = new PatriciaTrie<string>();
        }

       
        public IEnumerable<string> GetSuggestions(string orignalText, int caretPosition, bool tokenise, enIntellisensePartType type)
        {
            if (caretPosition < 0)
            {
                return new List<string>();
            }

            string filter;
            if (tokenise)
            {
                if (caretPosition > orignalText.Length)
                {
                    caretPosition = orignalText.Length;
                }

                var texttrimmedRight = orignalText.Substring(0, caretPosition);
                var start = texttrimmedRight.LastIndexOf(texttrimmedRight.Split(_tokenisers).Last(), StringComparison.Ordinal);
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
                    if (orignalText.Contains("(") && orignalText.IndexOf("(", StringComparison.Ordinal) < caretPosition)
                    {
                        trie = PatriciaTrie;
                    }
                    else
                    {
                        trie = PatriciaTrieRecsets;
                    }

                    break;

                case enIntellisensePartType.ScalarsOnly:
                    trie = PatriciaTrieScalars;
                    break;

                case enIntellisensePartType.JsonObject:
                    trie = PatriciaTrieJsonObjects;
                    break;

                case enIntellisensePartType.RecordsetFields:
                    if (orignalText.Contains("(") && orignalText.IndexOf("(", StringComparison.Ordinal) < caretPosition)
                    {
                        trie = PatriciaTrie;
                    }
                    else
                    {
                        trie = PatriciaTrieRecsetsFields;
                    }

                    break;
                case enIntellisensePartType.None:
                    break;
                default:
                    break;
            }
            if (filter.EndsWith("[["))
            {
                return trie.Retrieve("[[");
            }

            if (!filter.StartsWith("[[") && filter.Contains("[["))
            {
                return trie.Retrieve(filter.Substring(filter.LastIndexOf("[[", StringComparison.Ordinal)));
            }

            if (filter == "]" || filter == "]]")
            {
                return new string[0];
            }
            return trie.Retrieve(filter);
        }

        List<string> PermuteCapitalizations(string key)
        {
            var suffixes = new List<string>();
            suffixes.Add(key);
            suffixes.Add(key.ToLower());
            suffixes.Add(key.ToUpper());
            suffixes.Add(TitleCase(key));
            suffixes.Add(ReverseCase(key));
            return suffixes;
        }

        string TitleCase(string input) => input?[0].ToString().ToUpper() + input?.Substring(1).ToLower();

        string ReverseCase(string input)
        {
            var array = input?.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)) : c).ToArray();
            var reversedCase = new string(array);
            return reversedCase;
        }

        #endregion Implementation of ISuggestionProvider
    }
}