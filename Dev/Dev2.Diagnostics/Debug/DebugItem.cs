
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Utils;

// ReSharper disable CheckNamespace
namespace Dev2.Diagnostics
// ReSharper restore CheckNamespace
{
    [Serializable]
    public class DebugItem : IDebugItem
    {
        #region private fields

        static readonly string InvalidFileNameChars =
            new string(Path.GetInvalidFileNameChars())
            + new string(Path.GetInvalidPathChars());

        readonly string _tempPath;
        private readonly Guid _itemId;
        private readonly StringBuilder _stringBuilder;
        string _fileName;
        bool _isMoreLinkCreated;

        #endregion fieds

        #region public properties

        public const int MaxItemDispatchCount = 10;
        public const int MaxCharDispatchCount = 150;
        public const int ActCharDispatchCount = 100;

        public static List<DebugItem> EmptyList = new List<DebugItem>();
        public List<IDebugItemResult> ResultsList { get; set; }

        #endregion properties

        #region CTOR

        public DebugItem()
            : this(null)
        {
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)
        {
            ResultsList = new List<IDebugItemResult>();
            _tempPath = Path.Combine(Path.GetTempPath(), "Warewolf", "Debug");
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

        public bool Contains(string filterText)
        {
            return ResultsList.Any(r => r.Value.ContainsSafe(filterText) || r.GroupName.ContainsSafe(filterText));
        }

        #endregion

        #region Public Methods

        public void Add(IDebugItemResult itemToAdd, bool isDeserialize = false)
        {
            if(!string.IsNullOrWhiteSpace(itemToAdd.GroupName) && itemToAdd.GroupIndex > MaxItemDispatchCount)
            {

                if(!isDeserialize)
                {
                    _fileName = string.Format("{0}.txt", _itemId);
                    if(itemToAdd.GroupIndex == (MaxItemDispatchCount + 1) && !_isMoreLinkCreated)
                    {
                        ClearFile(_fileName);
                        _stringBuilder.AppendLine(itemToAdd.GetMoreLinkItem());
                        ResultsList.Add(new DebugItemResult { MoreLink = SaveFile(_stringBuilder.ToString(), _fileName), GroupName = itemToAdd.GroupName, GroupIndex = itemToAdd.GroupIndex });
                        _stringBuilder.Clear();
                        _isMoreLinkCreated = true;
                        return;
                    }

                    _stringBuilder.AppendLine(itemToAdd.GetMoreLinkItem());
                    if(itemToAdd.Type == DebugItemResultType.Value ||
                        itemToAdd.Type == DebugItemResultType.Variable)
                    {
                        SaveFile(_stringBuilder.ToString(), _fileName);
                        _stringBuilder.Clear();
                    }


                    if(_stringBuilder.Length > 10000)
                    {
                        SaveFile(_stringBuilder.ToString(), _fileName);
                        _stringBuilder.Clear();
                    }

                    return;
                }

            }

            if(itemToAdd.Type == DebugItemResultType.Value ||
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
                Add(debugItemResult);
            }
        }

        public IList<IDebugItemResult> FetchResultsList()
        {
            return ResultsList;
        }

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
            string linkUri = string.Format(EnvironmentVariables.WebServerUri + "/Services/{0}?DebugItemFilePath={1}", "FetchDebugItemFileService", path);

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

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            if(reader.ReadToDescendant("DebugItemResults"))
            {
                ResultsList = new List<IDebugItemResult>();
                reader.ReadStartElement();
                while(reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugItemResult")
                {
                    var item = new DebugItemResult();
                    item.ReadXml(reader);
                    ResultsList.Add(item);
                }

                if(reader.NodeType == XmlNodeType.EndElement && reader.Name == "DebugItemResults")
                {
                    reader.ReadEndElement();
                }
            }

            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("DebugItemResults");
            writer.WriteAttributeString("Count", ResultsList.Count.ToString(CultureInfo.InvariantCulture));

            var resultSer = new XmlSerializer(typeof(DebugItemResult));
            foreach(var other in ResultsList)
            {
                resultSer.Serialize(writer, other);
            }
            writer.WriteEndElement();
        }

        #endregion
    }
}
