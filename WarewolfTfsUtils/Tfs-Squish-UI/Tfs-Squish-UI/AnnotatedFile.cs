// Type: Microsoft.TeamFoundation.PowerTools.Client.VersionControl.AnnotatedFile
// Assembly: Microsoft.TeamFoundation.PowerTools.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Program Files (x86)\Microsoft Team Foundation Server 2012 Power Tools\Microsoft.TeamFoundation.PowerTools.Client.dll

using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;

namespace Tfs.Squish
{

    internal abstract class AnnotatedFile : IDisposable
    {
        
        protected int m_remainingLines = int.MaxValue;
        private AnnotatedVersionAvailableEventHandler AnnotatedVersionAvailable;
        private DiffOptions m_diffOptions;
        private int m_index;
        private AnnotatedFile.FileVersion m_lastFile;
        private List<AnnotatedFile.LineMapping> m_lineMapping;
        private int[] m_lines;
        private Queue<AnnotatedFile.FileVersion> m_tempFiles;

        public DiffOptions DiffOptions
        {
            get
            {
                return this.m_diffOptions;
            }
            set
            {
                this.m_diffOptions = value;
            }
        }

        public bool AllLinesAnnotated
        {
            get
            {
                return this.m_remainingLines == 0;
            }
        }

        public int LineCount
        {
            get
            {
                if(this.m_lines == null)
                    this.InitTipVersion();
                return this.m_lines.Length;
            }
        }

        public int AnnotatedVersions
        {
            get
            {
                return this.m_index;
            }
        }

        //public event AnnotatedVersionAvailableEventHandler AnnotatedVersionAvailable
        //{
        //    add
        //    {
        //        AnnotatedVersionAvailableEventHandler availableEventHandler = this.AnnotatedVersionAvailable;
        //        AnnotatedVersionAvailableEventHandler comparand;
        //        do
        //        {
        //            comparand = availableEventHandler;
        //            availableEventHandler = Interlocked.CompareExchange<AnnotatedVersionAvailableEventHandler>(ref this.AnnotatedVersionAvailable, comparand + value, comparand);
        //        }
        //        while(availableEventHandler != comparand);
        //    }
        //    remove
        //    {
        //        AnnotatedVersionAvailableEventHandler availableEventHandler = this.AnnotatedVersionAvailable;
        //        AnnotatedVersionAvailableEventHandler comparand;
        //        do
        //        {
        //            comparand = availableEventHandler;
        //            availableEventHandler = Interlocked.CompareExchange<AnnotatedVersionAvailableEventHandler>(ref this.AnnotatedVersionAvailable, comparand - value, comparand);
        //        }
        //        while(availableEventHandler != comparand);
        //    }
        //}

        protected AnnotatedFile()
        {
            this.m_diffOptions = new DiffOptions();
            this.m_diffOptions.Flags = DiffOptionFlags.IgnoreEndOfLineDifference | DiffOptionFlags.EnablePreambleHandling;
            this.m_tempFiles = new Queue<AnnotatedFile.FileVersion>();
        }

        public virtual void Dispose()
        {
            foreach(AnnotatedFile.FileVersion fileVersion in this.m_tempFiles)
                FileSpec.DeleteFileWithoutException(fileVersion.Name);
            this.m_tempFiles.Clear();
        }

        public int GetVersionForLine(int line)
        {
            if(this.m_lines == null)
                return -1;
            else
                return this.m_lines[line] - 1;
        }

        public virtual void AnnotateToLine(int line)
        {
            do
                ;
            while((this.m_lines == null || this.m_lines[line] == 0) && this.AnnotateVersion());
        }

        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
        public virtual void AnnotateToLine(int line, int count)
        {
            while(count > 0)
            {
                do
                    ;
                while((this.m_lines == null || this.m_lines[line] == 0) && this.AnnotateVersion());
                --count;
                ++line;
            }
        }

        public virtual void Annotate(int versions)
        {
            while(versions > 0 && this.AnnotateVersion())
                --versions;
        }

        public void AnnotateAll()
        {
            this.Annotate(int.MaxValue);
        }

        protected virtual void OnVersionAvailable(int index, AnnotatedFile.FileVersion file)
        {
            AnnotatedVersionAvailableEventHandler availableEventHandler = this.AnnotatedVersionAvailable;
            if(availableEventHandler == null)
                return;
            AnnotatedVersionAvailableEventArgs e = new AnnotatedVersionAvailableEventArgs(index, file);
            availableEventHandler((object)this, e);
            if(e.Canceled)
                throw new OperationCanceledException();
        }

        protected abstract bool AnnotateVersion();

        protected virtual void AnnotateVersion(AnnotatedFile.FileVersion file)
        {
            if(this.m_remainingLines <= 0)
                return;
            if(this.m_index != 0)
            {
                if(file.CodePage != this.m_lastFile.CodePage || !this.DiffSupportsCodePage(file.CodePage))
                {
                    if(this.m_lastFile.CodePage != Encoding.Unicode.CodePage)
                    {
                        string tempFileName = Path.GetTempFileName();
                        TFCommonUtil.ConvertFileEncoding(this.m_lastFile.Name, tempFileName, Encoding.GetEncoding(this.m_lastFile.CodePage), Encoding.Unicode, true);
                        this.m_tempFiles.Enqueue(this.m_lastFile = new AnnotatedFile.FileVersion(tempFileName, Encoding.Unicode.CodePage));
                    }
                    if(file.CodePage != Encoding.Unicode.CodePage)
                    {
                        string tempFileName = Path.GetTempFileName();
                        TFCommonUtil.ConvertFileEncoding(file.Name, tempFileName, Encoding.GetEncoding(file.CodePage), Encoding.Unicode, true);
                        this.m_tempFiles.Enqueue(file = new AnnotatedFile.FileVersion(tempFileName, Encoding.Unicode.CodePage));
                    }
                }
                DiffSegment segment = Difference.DiffFiles(file.Name, file.CodePage, this.m_lastFile.Name, this.m_lastFile.CodePage, this.m_diffOptions);
                if(this.m_lines == null)
                    this.InitTipVersion(segment);
                List<AnnotatedFile.LineMapping> list = new List<AnnotatedFile.LineMapping>();
                int num1 = 0;
                int nextMapping = 0;
                for(; segment != null; segment = segment.Next)
                {
                    int num2 = segment.ModifiedStart - num1;
                    int num3 = 0;
                    AnnotatedFile.LineMapping mapping1;
                    int mapping2;
                    while(num3 < num2 && (mapping2 = this.GetMapping(0, num1 + num3, num2 - num3, out mapping1, ref nextMapping)) != 0)
                    {
                        for(int index = 0; index < mapping1.m_count; ++index)
                            this.m_lines[mapping1.m_tgtLine + index] = this.m_index;
                        this.m_remainingLines -= mapping1.m_count;
                        num3 += mapping2;
                    }
                    num1 = segment.ModifiedStart + segment.ModifiedLength;
                    int num4 = 0;
                    while(num4 < segment.ModifiedLength && (mapping2 = this.GetMapping(segment.OriginalStart + num4, segment.ModifiedStart + num4, segment.ModifiedLength - num4, out mapping1, ref nextMapping)) != 0)
                    {
                        list.Add(mapping1);
                        num4 += mapping2;
                    }
                }
                list.Add(new AnnotatedFile.LineMapping(1073741823, 1073741823, 0));
                this.m_lineMapping = list;
            }

            if(this.m_tempFiles.Count > 0 && this.m_tempFiles.Peek() == this.m_lastFile)
            {
                this.m_tempFiles.Dequeue();
                FileSpec.DeleteFileWithoutException(this.m_lastFile.Name);
            }

            this.m_lastFile = file;
            ++this.m_index;
            this.OnVersionAvailable(this.m_index - 1, file);
        }

        protected void AnnotationComplete()
        {
            if(this.m_lines == null)
                this.InitTipVersion();
            for(int index = 0; index < this.m_lines.Length; ++index)
            {
                if(this.m_lines[index] == 0)
                    this.m_lines[index] = this.m_index;
            }
            this.m_remainingLines = 0;
            this.m_lineMapping = (List<AnnotatedFile.LineMapping>)null;
            this.m_lastFile = (AnnotatedFile.FileVersion)null;
        }

        private int GetMapping(int srcStartLine, int tgtStartLine, int count, out AnnotatedFile.LineMapping mapping, ref int nextMapping)
        {
            while(this.m_lineMapping[nextMapping].m_srcLine + this.m_lineMapping[nextMapping].m_count <= tgtStartLine)
                ++nextMapping;
            int num1 = this.m_lineMapping[nextMapping].m_srcLine - tgtStartLine;
            int num2 = 0;
            if(num1 < 0)
            {
                num2 = -num1;
                num1 = 0;
            }
            mapping = new AnnotatedFile.LineMapping(srcStartLine + num1, this.m_lineMapping[nextMapping].m_tgtLine + num2, Math.Min(count - num1, this.m_lineMapping[nextMapping].m_count - num2));
            if(mapping.m_count <= 0)
                return 0;
            else
                return mapping.m_count + num1;
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
            this.InitLineInfo(count);
        }

        private void InitTipVersion()
        {
            if(this.m_lastFile == null)
                throw new InvalidOperationException();
            int count = 0;
            using(DiffLineReader diffLineReader = new DiffLineReader((TextReader)new StreamReader((Stream)File.OpenRead(this.m_lastFile.Name), Encoding.GetEncoding(this.m_lastFile.CodePage))))
            {
                while(diffLineReader.ReadLine() != null)
                    ++count;
            }
            this.InitLineInfo(count);
        }

        private void InitLineInfo(int count)
        {
            this.m_lines = new int[count];
            this.m_lineMapping = new List<AnnotatedFile.LineMapping>();
            this.m_lineMapping.Add(new AnnotatedFile.LineMapping(0, 0, count));
            this.m_lineMapping.Add(new AnnotatedFile.LineMapping(1073741823, 1073741823, 0));
            this.m_remainingLines = this.m_lines.Length;
        }

        private bool DiffSupportsCodePage(int codePage)
        {
            return codePage != 54936 && codePage != Encoding.UTF32.CodePage && codePage != Encoding.UTF8.CodePage && codePage != Encoding.UTF7.CodePage;
        }

        public class FileVersion
        {
            private string m_name;
            private int m_codePage;

            public string Name
            {
                get
                {
                    return this.m_name;
                }
                internal set
                {
                    this.m_name = value;
                }
            }

            public int CodePage
            {
                get
                {
                    return this.m_codePage;
                }
                internal set
                {
                    this.m_codePage = value;
                }
            }

            public FileVersion(string name, int codePage)
            {
                this.m_name = name;
                this.m_codePage = codePage;
            }
        }

        private struct LineMapping
        {
            public int m_srcLine;
            public int m_tgtLine;
            public int m_count;

            public LineMapping(int srcLine, int tgtLine, int count)
            {
                this.m_srcLine = srcLine;
                this.m_tgtLine = tgtLine;
                this.m_count = count;
            }
        }
    }
}
