using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Gma.DataStructures.StringSearch;

namespace Dev2
{
    public class Dev2TrieSugggestionProvider : ISuggestionProvider
    {
        #region Implementation of ISuggestionProvider
        readonly char[] _tokenisers = "!@#$%^&-=_+{}|:\"?><`~<>?:'{}| [](".ToCharArray();
        public ITrie<string> PatriciaTrie { get; private set; }
        ObservableCollection<string> _variableList;
        public ObservableCollection<string> VariableList
        {
            get
            {
                return _variableList;
            }
            set
            {
                if(_variableList == null || ( value.Union(_variableList).Count() !=_variableList.Count))
                _variableList = value;
               

                PatriciaTrie = new SuffixTrie<string>(1);

                var vars = IntellisenseStringProvider.getOptions(_variableList.Select(a => WarewolfDataEvaluationCommon.parseLanguageExpression(a, 0)).OrderBy(a => a), Level, IntellisenseStringProvider.FilterOption.All);
                foreach (var @var in vars)
                {
                    PatriciaTrie.Add(@var, @var);
                }
                PatriciaTrieScalars = new SuffixTrie<string>(1);
                vars = IntellisenseStringProvider.getOptions(_variableList.Select(a => WarewolfDataEvaluationCommon.parseLanguageExpression(a, 0)).OrderBy(a => a).Where(a => a.IsScalarExpression), Level, IntellisenseStringProvider.FilterOption.Scalars);
                foreach (var @var in vars)
                {
                    PatriciaTrieScalars.Add(@var, @var);
                }
                PatriciaTrieRecsets = new SuffixTrie<string>(1);
                vars = IntellisenseStringProvider.getOptions(_variableList.Select(a => WarewolfDataEvaluationCommon.parseLanguageExpression(a, 0)).OrderBy(a => a).Where(a => a.IsRecordSetNameExpression), Level, IntellisenseStringProvider.FilterOption.RecordSetNames);
                foreach (var @var in vars)
                {
                    PatriciaTrieRecsets.Add(@var, @var);
                }
                PatriciaTrieRecsetsFields = new SuffixTrie<string>(1);
                vars = IntellisenseStringProvider.getOptions(_variableList.Select(a => WarewolfDataEvaluationCommon.parseLanguageExpression(a, 0)).OrderBy(a => a).Where(a => a.IsRecordSetExpression || a.IsRecordSetNameExpression), Level, IntellisenseStringProvider.FilterOption.Recordsets);
                foreach (var @var in vars)
                {
                    PatriciaTrieRecsetsFields.Add(@var, @var);
                }
            }
        }
        public SuffixTrie<string> PatriciaTrieRecsetsFields { get; set; }
        public SuffixTrie<string> PatriciaTrieRecsets { get; set; }
        public SuffixTrie<string> PatriciaTrieScalars { get; set; }
        public int Level { get; set; }

        public Dev2TrieSugggestionProvider(int level)
        {
            VariableList = new ObservableCollection<string>();
            PatriciaTrie = new PatriciaTrie<string>();
            Level = level;
         
        }
   

        public IEnumerable<string> GetSuggestions(string orignalText, int caretIndex, bool tokenise, enIntellisensePartType type)
        {
            if(caretIndex<0) return  new List<string>();
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
                case enIntellisensePartType.ScalarsOnly: trie = PatriciaTrieScalars;
                    break;
                case enIntellisensePartType.RecordsetFields: if (orignalText.Contains("(") && orignalText.IndexOf("(", StringComparison.Ordinal) < caretIndex)
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
            return trie.Retrieve(filter);
        }

        #endregion
    }
}