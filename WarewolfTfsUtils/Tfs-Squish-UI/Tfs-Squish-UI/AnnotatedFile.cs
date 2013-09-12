using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Tfs.Squish
{

    internal abstract class AnnotatedFile : IDisposable
    {
        
        protected int remainingLines = int.MaxValue;
        private AnnotatedVersionAvailableEventHandler AnnotatedVersionAvailable;
        private DiffOptions _diffOptions;
        private int _index;
        private FileVersion _lastFile;
        private List<LineMapping> _lineMapping;
        private int[] _lines;
        private readonly Queue<FileVersion> tempFiles;

        public DiffOptions DiffOptions
        {
            get
            {
                return _diffOptions;
            }
            set
            {
                _diffOptions = value;
            }
        }

        public bool AllLinesAnnotated
        {
            get
            {
                return remainingLines == 0;
            }
        }

        public int LineCount
        {
            get
            {
                if(_lines == null)
                    InitTipVersion();
                return _lines.Length;
            }
        }

        public int AnnotatedVersions
        {
            get
            {
                return _index;
            }
        }

        protected AnnotatedFile()
        {
            _diffOptions = new DiffOptions();
            _diffOptions.Flags = DiffOptionFlags.IgnoreEndOfLineDifference | DiffOptionFlags.EnablePreambleHandling;
            tempFiles = new Queue<FileVersion>();
        }

        public virtual void Dispose()
        {
            foreach(FileVersion fileVersion in tempFiles)
                FileSpec.DeleteFileWithoutException(fileVersion.Name);
            tempFiles.Clear();
        }

        public int GetVersionForLine(int line)
        {
            if(_lines == null)
                return -1;
            else
                return _lines[line] - 1;
        }

        public virtual void AnnotateToLine(int line)
        {
            do
            {
            } while((_lines == null || _lines[line] == 0) && AnnotateVersion());
        }

        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
        public virtual void AnnotateToLine(int line, int count)
        {
            while(count > 0)
            {
                do
                {
                } while((_lines == null || _lines[line] == 0) && AnnotateVersion());
                --count;
                ++line;
            }
        }

        public virtual void Annotate(int versions)
        {
            while(versions > 0 && AnnotateVersion())
                --versions;
        }

        public void AnnotateAll()
        {
            Annotate(int.MaxValue);
        }

        protected virtual void OnVersionAvailable(int index, FileVersion file)
        {
            AnnotatedVersionAvailableEventHandler availableEventHandler = AnnotatedVersionAvailable;
            if(availableEventHandler == null)
                return;
            AnnotatedVersionAvailableEventArgs e = new AnnotatedVersionAvailableEventArgs(index, file);
            availableEventHandler((object)this, e);
            if(e.Canceled)
                throw new OperationCanceledException();
        }

        protected abstract bool AnnotateVersion();

        protected virtual void AnnotateVersion(FileVersion file)
        {
            if(remainingLines <= 0)
                return;
            if(_index != 0)
            {
                if(file.CodePage != _lastFile.CodePage || !DiffSupportsCodePage(file.CodePage))
                {
                    if(_lastFile.CodePage != Encoding.Unicode.CodePage)
                    {
                        string tempFileName = Path.GetTempFileName();
                        TFCommonUtil.ConvertFileEncoding(_lastFile.Name, tempFileName, Encoding.GetEncoding(_lastFile.CodePage), Encoding.Unicode, true);
                        tempFiles.Enqueue(_lastFile = new FileVersion(tempFileName, Encoding.Unicode.CodePage));
                    }
                    if(file.CodePage != Encoding.Unicode.CodePage)
                    {
                        string tempFileName = Path.GetTempFileName();
                        TFCommonUtil.ConvertFileEncoding(file.Name, tempFileName, Encoding.GetEncoding(file.CodePage), Encoding.Unicode, true);
                        tempFiles.Enqueue(file = new FileVersion(tempFileName, Encoding.Unicode.CodePage));
                    }
                }
                DiffSegment segment = Difference.DiffFiles(file.Name, file.CodePage, _lastFile.Name, _lastFile.CodePage, _diffOptions);
                if(_lines == null)
                    InitTipVersion(segment);
                List<LineMapping> list = new List<LineMapping>();
                int num1 = 0;
                int nextMapping = 0;
                for(; segment != null; segment = segment.Next)
                {
                    int num2 = segment.ModifiedStart - num1;
                    int num3 = 0;
                    LineMapping mapping1;
                    int mapping2;
                    while(num3 < num2 && (mapping2 = GetMapping(0, num1 + num3, num2 - num3, out mapping1, ref nextMapping)) != 0)
                    {
                        for(int index = 0; index < mapping1.count; ++index)
                            _lines[mapping1.tgtLine + index] = _index;
                        remainingLines -= mapping1.count;
                        num3 += mapping2;
                    }
                    num1 = segment.ModifiedStart + segment.ModifiedLength;
                    int num4 = 0;
                    while(num4 < segment.ModifiedLength && (mapping2 = GetMapping(segment.OriginalStart + num4, segment.ModifiedStart + num4, segment.ModifiedLength - num4, out mapping1, ref nextMapping)) != 0)
                    {
                        list.Add(mapping1);
                        num4 += mapping2;
                    }
                }
                list.Add(new LineMapping(1073741823, 1073741823, 0));
                _lineMapping = list;
            }

            if(tempFiles.Count > 0 && tempFiles.Peek() == _lastFile)
            {
                tempFiles.Dequeue();
                FileSpec.DeleteFileWithoutException(_lastFile.Name);
            }

            _lastFile = file;
            ++_index;
            OnVersionAvailable(_index - 1, file);
        }

        protected void AnnotationComplete()
        {
            if(_lines == null)
                InitTipVersion();
            for(int index = 0; index < _lines.Length; ++index)
            {
                if(_lines[index] == 0)
                    _lines[index] = _index;
            }
            remainingLines = 0;
            _lineMapping = null;
            _lastFile = null;
        }

        private int GetMapping(int srcStartLine, int tgtStartLine, int count, out AnnotatedFile.LineMapping mapping, ref int nextMapping)
        {
            while(_lineMapping[nextMapping].srcLine + _lineMapping[nextMapping].count <= tgtStartLine)
                ++nextMapping;
            int num1 = _lineMapping[nextMapping].srcLine - tgtStartLine;
            int num2 = 0;
            if(num1 < 0)
            {
                num2 = -num1;
                num1 = 0;
            }
            mapping = new LineMapping(srcStartLine + num1, _lineMapping[nextMapping].tgtLine + num2, Math.Min(count - num1, _lineMapping[nextMapping].count - num2));
            if(mapping.count <= 0)
                return 0;
            else
                return mapping.count + num1;
        }

        private void InitTipVersion(DiffSegment segment)
        {
            int count = 0;
            for(; segment != null; segment = segment.Next)
            {
                int num = segment.ModifiedStart + segment.ModifiedLength;
                if(num > count)
                    count = num;
            }
            InitLineInfo(count);
        }

        private void InitTipVersion()
        {
            if(_lastFile == null)
                throw new InvalidOperationException();
            int count = 0;
            using(DiffLineReader diffLineReader = new DiffLineReader(new StreamReader(File.OpenRead(_lastFile.Name), Encoding.GetEncoding(_lastFile.CodePage))))
            {
                while(diffLineReader.ReadLine() != null)
                    ++count;
            }
            InitLineInfo(count);
        }

        private void InitLineInfo(int count)
        {
            _lines = new int[count];
            _lineMapping = new List<LineMapping>();
            _lineMapping.Add(new LineMapping(0, 0, count));
            _lineMapping.Add(new LineMapping(1073741823, 1073741823, 0));
            remainingLines = _lines.Length;
        }

        private bool DiffSupportsCodePage(int codePage)
        {
            return codePage != 54936 && codePage != Encoding.UTF32.CodePage && codePage != Encoding.UTF8.CodePage && codePage != Encoding.UTF7.CodePage;
        }

        public class FileVersion
        {
            public string Name { get; internal set; }

            public int CodePage { get; internal set; }

            public FileVersion(string name, int codePage)
            {
                Name = name;
                CodePage = codePage;
            }
        }

        private struct LineMapping
        {
            public readonly int srcLine;
            public readonly int tgtLine;
            public readonly int count;

            public LineMapping(int srcLine, int tgtLine, int count)
            {
                this.srcLine = srcLine;
                this.tgtLine = tgtLine;
                this.count = count;
            }
        }
    }
}
