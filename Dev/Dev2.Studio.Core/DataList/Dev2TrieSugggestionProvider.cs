#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Gma.DataStructures.StringSearch;



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
                var languageExpression = EvaluationFunctions.parseLanguageExpression(a, 0);
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