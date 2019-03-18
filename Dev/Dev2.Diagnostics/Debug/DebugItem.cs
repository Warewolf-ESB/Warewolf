#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
    [Serializable]
    public class DebugItem : IDebugItem
    {
        #region private fields

        static readonly string InvalidFileNameChars =
            new string(Path.GetInvalidFileNameChars())
            + new string(Path.GetInvalidPathChars());

        readonly string _tempPath;
        readonly Guid _itemId;
        readonly StringBuilder _stringBuilder;
        string _fileName;
        bool _isMoreLinkCreated;

        #endregion fieds

        #region public properties

        public static readonly int MaxItemDispatchCount = 10;
        public static readonly int MaxCharDispatchCount = 150;
        public static readonly int ActCharDispatchCount = 100;

        public List<IDebugItemResult> ResultsList { get; set; }
        public static List<DebugItem> EmptyList { get => emptyList; set => emptyList = value; }
        static List<DebugItem> emptyList = new List<DebugItem>();

        #endregion properties

        #region CTOR

        public DebugItem()
            : this(null)
        {
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)
        {
            ResultsList = new List<IDebugItemResult>();
            _tempPath = Path.Combine(GlobalConstants.TempLocation, "Warewolf", "Debug");
            if(!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            _itemId = Guid.NewGuid();
            _isMoreLinkCreated = false;
            _stringBuilder = new StringBuilder();
            _fileName = string.Empty;
            if(results != null)
            {
                AddRange(results.ToList());
            }
        }

        #endregion

        #region Contains

        public bool Contains(string filterText) => ResultsList.Any(r => r.Value.ContainsSafe(filterText) || r.GroupName.ContainsSafe(filterText));

        #endregion

        #region Public Methods

        public void Add(IDebugItemResult itemToAdd) => Add(itemToAdd, false);
        public void Add(IDebugItemResult itemToAdd, bool isDeserialize)
        {

            if (!string.IsNullOrWhiteSpace(itemToAdd.GroupName) && itemToAdd.GroupIndex > MaxItemDispatchCount && !isDeserialize)
            {
                _fileName = string.Format("{0}.txt", _itemId);
                if (itemToAdd.GroupIndex == MaxItemDispatchCount + 1 && !_isMoreLinkCreated)
                {
                    ClearFile(_fileName);
                    _stringBuilder.AppendLine(itemToAdd.GetMoreLinkItem());
                    ResultsList.Add(new DebugItemResult { MoreLink = SaveFile(_stringBuilder.ToString(), _fileName), GroupName = itemToAdd.GroupName, GroupIndex = itemToAdd.GroupIndex });
                    _stringBuilder.Clear();
                    _isMoreLinkCreated = true;
                    return;
                }

                _stringBuilder.AppendLine(itemToAdd.GetMoreLinkItem());
                if (itemToAdd.Type == DebugItemResultType.Value ||
                    itemToAdd.Type == DebugItemResultType.Variable)
                {
                    SaveFile(_stringBuilder.ToString(), _fileName);
                    _stringBuilder.Clear();
                }


                if (_stringBuilder.Length > 10000)
                {
                    SaveFile(_stringBuilder.ToString(), _fileName);
                    _stringBuilder.Clear();
                }

                return;
            }


            if (itemToAdd.Type == DebugItemResultType.Value ||
                itemToAdd.Type == DebugItemResultType.Variable)
            {
                TryCache(itemToAdd);
            }

            ResultsList.Add(itemToAdd);
        }

        public void AddRange(List<IDebugItemResult> itemsToAdd)
        {
            foreach(var debugItemResult in itemsToAdd)
            {
                if(string.IsNullOrEmpty(debugItemResult.Label) && string.IsNullOrEmpty(debugItemResult.Value) && string.IsNullOrEmpty(debugItemResult.Variable) && debugItemResult.Type == DebugItemResultType.Variable)
                {
                    continue;
                }
                Add(debugItemResult);
            }
        }

        public IList<IDebugItemResult> FetchResultsList() => ResultsList;

        #region TryCache

        public void TryCache(IDebugItemResult item)
        {
            if(item == null)
            {
                throw new ArgumentNullException("item");
            }

            if(!string.IsNullOrEmpty(item.Value) && item.Value.Length > MaxCharDispatchCount)
            {
                item.MoreLink = SaveFile(item.Value, string.Format("{0}-{1}.txt", DateTime.Now.ToString("s"), Guid.NewGuid()));
                item.Value = item.Value.Substring(0, ActCharDispatchCount);
            }
        }

        #endregion

        #region SaveFile

        public virtual string SaveFile(string contents, string fileName)
        {

            if(string.IsNullOrEmpty(contents))
            {
                throw new ArgumentNullException("contents");
            }
            contents = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(contents);
            fileName = InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c.ToString(CultureInfo.InvariantCulture), ""));

            var path = Path.Combine(_tempPath, fileName);
            File.AppendAllText(path, contents);
            var linkUri = string.Format(EnvironmentVariables.WebServerUri + "/Services/{0}?DebugItemFilePath={1}", "FetchDebugItemFileService", path);

            return linkUri;
        }

        #endregion

        #region DeleteFile

        public void ClearFile(string fileName)
        {

            var path = Path.Combine(_tempPath, fileName);
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        #endregion

        #region Flush String Builder

        public void FlushStringBuilder()
        {
            if(_stringBuilder.Length > 0 && !string.IsNullOrEmpty(_fileName))
            {
                SaveFile(_stringBuilder.ToString(), _fileName);
                _stringBuilder.Clear();
            }
        }

        #endregion

        #endregion
        
    }
}
