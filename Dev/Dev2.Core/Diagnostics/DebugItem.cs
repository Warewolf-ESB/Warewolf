using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;

namespace Dev2.Diagnostics
{
    public class DebugItem : IDebugItem
    {
        private List<IDebugItemResult> _resultsList = new List<IDebugItemResult>();

        static readonly string InvalidFileNameChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        readonly string _tempPath;
        private Guid _itemId;
        private StringBuilder stringBuilder;
        string fileName;
        bool _isMoreLinkCreated;

        public const int MaxItemDispatchCount = 10;
        public const int MaxCharDispatchCount = 150;
        public const int ActCharDispatchCount = 100;

        public static IDebugItem[] EmptyList = new IDebugItem[0];

        #region CTOR

        public DebugItem()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), "Dev2", "Debug");
            _itemId = Guid.NewGuid();
            _isMoreLinkCreated = false;
            stringBuilder = new StringBuilder();
            fileName = string.Empty;
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)           
        {
            _tempPath = Path.Combine(Path.GetTempPath(), "Dev2", "Debug");
            _itemId = Guid.NewGuid();
            _isMoreLinkCreated = false;
            stringBuilder = new StringBuilder();
            fileName = string.Empty;
            AddRange(results.ToList());            
        }

        #endregion

        #region Contains

        public bool Contains(string filterText)
        {
            return _resultsList.Any(r => r.Value.ContainsSafe(filterText) || r.GroupName.ContainsSafe(filterText));
        }

        #endregion

        #region Public Methods

        public void Add(IDebugItemResult itemToAdd,bool isDeserialize = false)
        {
            if (!string.IsNullOrWhiteSpace(itemToAdd.GroupName) && itemToAdd.GroupIndex > MaxItemDispatchCount)
            {
                
                if (!isDeserialize)
                {
                    fileName = string.Format("{0}-{1}.txt", DataListUtil.ExtractRecordsetNameFromValue(itemToAdd.GroupName), _itemId);                
                    if (itemToAdd.GroupIndex == (MaxItemDispatchCount + 1) && !_isMoreLinkCreated)
                    {
                        ClearFile(fileName);
                        _resultsList.Add(new DebugItemResult { MoreLink = SaveFile(itemToAdd.Value, fileName), GroupName = itemToAdd.GroupName, GroupIndex = itemToAdd.GroupIndex });
                        _isMoreLinkCreated = true;
                        return;
                    }
                    else
                    {
                        stringBuilder.Append(itemToAdd.Value);                            
                        if (itemToAdd.Type == DebugItemResultType.Value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                        }
                        if(stringBuilder.Length > 10000)
                        {
                            SaveFile(stringBuilder.ToString(), fileName);
                            stringBuilder.Clear();
                        }
                        
                        return;
                    }        
                }
                                        
            }

            if(itemToAdd.Type == DebugItemResultType.Value)
            {
                TryCache(itemToAdd);                
            }

            _resultsList.Add(itemToAdd);
        }        

        public void AddRange(IList<IDebugItemResult> itemsToAdd)
        {
            foreach(IDebugItemResult debugItemResult in itemsToAdd)
            {
                Add(debugItemResult);    
            }            
        }

        public IList<IDebugItemResult> FetchResultsList()
        {
            return _resultsList;
        }

        #region TryCache

        public void TryCache(IDebugItemResult item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (!string.IsNullOrEmpty(item.Value) && item.Value.Length > MaxCharDispatchCount)
            {
                item.MoreLink = SaveFile(item.Value, string.Format("{0}-{1}.txt", DateTime.Now.ToString("s"), Guid.NewGuid()));
                item.Value = item.Value.Substring(0, ActCharDispatchCount);            
            }                
        }

        #endregion

        #region SaveFile

        public virtual string SaveFile(string contents,string fileName)
        {
            if (string.IsNullOrEmpty(contents))
            {
                throw new ArgumentNullException("contents");
            }
            
            fileName = InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c.ToString(CultureInfo.InvariantCulture), ""));

            var path = Path.Combine(_tempPath, fileName);
            File.AppendAllText(path, contents);

            return new Uri(path).AbsoluteUri;
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
            if(stringBuilder.Length > 0 && !string.IsNullOrEmpty(fileName))
            {
                SaveFile(stringBuilder.ToString(), fileName);
                stringBuilder.Clear();    
            }            
        }

        #endregion

        #endregion

    }
}
