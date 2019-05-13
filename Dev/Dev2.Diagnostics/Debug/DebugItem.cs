/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Utils;

namespace Dev2.Diagnostics
{
    //TODO: Issues with the way large results are stored in SaveFile.
    [Serializable]
    public class DebugItem : IDebugItem
    {
        static readonly string _invalidFileNameChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        readonly string _tempFolder;

        readonly Guid _itemId;
        readonly StringBuilder _stringBuilder;
        string _fileName;
        bool _isMoreLinkCreated;

        public static readonly int MaxItemDispatchCount = 10;
        public static readonly int MaxCharDispatchCount = 150;
        public static readonly int ActCharDispatchCount = 100;

        public List<IDebugItemResult> ResultsList { get; set; }
        public static List<DebugItem> EmptyList { get; set; } = new List<DebugItem>();

        public DebugItem()
            : this(null)
        {
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)
        {
            ResultsList = new List<IDebugItemResult>();
            _tempFolder = EnvironmentVariables.DebugItemTempPath;
            _itemId = Guid.NewGuid();
            _isMoreLinkCreated = false;
            _stringBuilder = new StringBuilder();
            _fileName = string.Empty;
            if (results != null)
            {
                AddRange(results.ToList());
            }
        }

        public bool Contains(string filterText) => ResultsList.Any(r => r.Value.ContainsSafe(filterText) || r.GroupName.ContainsSafe(filterText));

        public void Add(IDebugItemResult itemToAdd) => Add(itemToAdd, false);

        public void Add(IDebugItemResult itemToAdd, bool isDeserialize)
        {
            AddResultItemOrAddToItemResultListLog(itemToAdd, isDeserialize);
        }

        private void AddResultItemOrAddToItemResultListLog(IDebugItemResult itemToAdd, bool isDeserialize)
        {
            if (!string.IsNullOrWhiteSpace(itemToAdd.GroupName) && itemToAdd.GroupIndex > MaxItemDispatchCount && !isDeserialize)
            {
                _fileName = string.Format("{0}.txt", _itemId);
                var shouldCreateMoreLink = itemToAdd.GroupIndex == MaxItemDispatchCount + 1 && !_isMoreLinkCreated;
                if (shouldCreateMoreLink)
                {
                    ClearFile(_fileName);
                    _stringBuilder.AppendLine(itemToAdd.GetMoreLinkItem());
                    var clonedItem = itemToAdd.Clone();
                    var moreLink = SaveFile(_stringBuilder.ToString(), _fileName);
                    _stringBuilder.Clear();
                    clonedItem.MoreLink = moreLink;
                    ResultsList.Add(clonedItem);
                    _isMoreLinkCreated = true;
                    return;
                }

                _stringBuilder.AppendLine(itemToAdd.GetMoreLinkItem());

                FlushResultListLogIfNeeded(itemToAdd);
                return;
            }

            TruncateItemResultIfNeeded(itemToAdd);

            ResultsList.Add(itemToAdd);
        }

        private void FlushResultListLogIfNeeded(IDebugItemResult itemToAdd)
        {
            if (itemToAdd.Type == DebugItemResultType.Value || itemToAdd.Type == DebugItemResultType.Variable || _stringBuilder.Length > 10000)
            {
                SaveFile(_stringBuilder.ToString(), _fileName);
                _stringBuilder.Clear();
            }
        }

        internal void TruncateItemResultIfNeeded(IDebugItemResult itemToAdd)
        {
            var valueOrVariable = itemToAdd.Type == DebugItemResultType.Value || itemToAdd.Type == DebugItemResultType.Variable;
            var valueLengthTooLong = !string.IsNullOrEmpty(itemToAdd.Value) && itemToAdd.Value.Length > MaxCharDispatchCount;
            if (valueOrVariable && valueLengthTooLong)
            {
                itemToAdd.MoreLink = SaveFile(itemToAdd.Value, string.Format("{0}-{1}.txt", DateTime.Now.ToString("s"), Guid.NewGuid()));
                itemToAdd.Value = itemToAdd.Value.Substring(0, ActCharDispatchCount);
            }
        }

        public void AddRange(List<IDebugItemResult> itemsToAdd)
        {
            foreach (var debugItemResult in itemsToAdd)
            {
                var invalidVariableResult = string.IsNullOrEmpty(debugItemResult.Label) && string.IsNullOrEmpty(debugItemResult.Value) && string.IsNullOrEmpty(debugItemResult.Variable) && debugItemResult.Type == DebugItemResultType.Variable;
                if (invalidVariableResult)
                {
                    continue;
                }
                Add(debugItemResult);
            }
        }

        public IList<IDebugItemResult> FetchResultsList() => ResultsList;

        //This method saves a file to C:\ProgramData\Warewolf\Temp\Warewolf\Debug
        public virtual string SaveFile(string contents, string fileName)
        {
            var updateContents = contents;
            var updateFileName = fileName;

            if (string.IsNullOrEmpty(updateContents))
            {
                throw new ArgumentNullException(nameof(updateContents));
            }
            updateContents = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(updateContents);
            updateFileName = _invalidFileNameChars.Aggregate(updateFileName, (current, c) => current.Replace(c.ToString(CultureInfo.InvariantCulture), ""));

            var path = Path.Combine(_tempFolder , updateFileName);
            File.AppendAllText(path, updateContents);
            var linkUri = string.Format(EnvironmentVariables.WebServerUri + "/Services/{0}?DebugItemFilePath={1}", "FetchDebugItemFileService", path);

            return linkUri;
        }

        public void ClearFile(string fileName)
        {
            var path = Path.Combine(_tempFolder , fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void FlushStringBuilder()
        {
            if (_stringBuilder.Length > 0 && !string.IsNullOrEmpty(_fileName))
            {
                SaveFile(_stringBuilder.ToString(), _fileName);
                _stringBuilder.Clear();
            }
        }
    }
}
