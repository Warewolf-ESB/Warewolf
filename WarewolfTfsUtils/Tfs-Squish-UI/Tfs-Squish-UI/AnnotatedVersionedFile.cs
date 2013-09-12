// Type: Microsoft.TeamFoundation.PowerTools.Client.VersionControl.AnnotatedVersionedFile
// Assembly: Microsoft.TeamFoundation.PowerTools.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Program Files (x86)\Microsoft Team Foundation Server 2012 Power Tools\Microsoft.TeamFoundation.PowerTools.Client.dll

using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Tfs.Squish
{
    internal class AnnotatedVersionedFile : AnnotatedFile, IDisposable
    {
        private AnnotatedVersionedFile.DownloadState m_downloadState = AnnotatedVersionedFile.DownloadState.Stopped;
        private VersionControlServer m_vcs;
        private string m_path;
        private VersionSpec m_version;
        private AnnotatedVersionedFile.AnnotationVersionList m_annList;
        private bool m_disposed;

        public AnnotatedFile.FileVersion TipVersionFile
        {
            get
            {
                if(m_annList.Count == 0)
                    throw new InvalidOperationException();
                else
                    return m_annList.m_fileVersions[0];
            }
        }

        public int EstimatedVersions
        {
            get
            {
                return m_annList.Count;
            }
        }

        public AnnotatedVersionedFile(VersionControlServer vcs, string path, VersionSpec version)
        {
            m_vcs = vcs;
            m_path = path;
            m_version = version;
            m_annList = new AnnotatedVersionedFile.AnnotationVersionList();
        }

        public override void Dispose()
        {
            lock(m_annList)
            {
                m_disposed = true;
                StopDownloads();
                for(int local_0 = 0; local_0 < m_annList.Count; ++local_0)
                {
                    if(m_annList.m_changesets[local_0] != null && m_annList.m_fileVersions[local_0] != null && m_annList.m_fileVersions[local_0].Name != null)
                        FileSpec.DeleteFileWithoutException(m_annList.m_fileVersions[local_0].Name);
                }
                m_annList.Clear();
            }
        }

        public Changeset GetAnnotatedVersion(int index)
        {
            if(index < 0 || index >= AnnotatedVersions)
                throw new IndexOutOfRangeException();
            else
                return m_annList.m_changesets[index];
        }

        public ChangesetState GetChangesetForLine(int line, out Changeset changeset)
        {
            int versionForLine = GetVersionForLine(line);
            if(versionForLine < 0)
            {
                changeset = null;
                return ChangesetState.Unknown;
            }
            else if(m_annList.m_changesets[versionForLine] == null)
            {
                changeset = null;
                return ChangesetState.Local;
            }
            else
            {
                changeset = m_annList.m_changesets[versionForLine];
                return ChangesetState.Committed;
            }
        }

        public override void AnnotateToLine(int line)
        {
            InitVersions(int.MaxValue);
            base.AnnotateToLine(line);
            StopDownloads();
        }

        public override void AnnotateToLine(int line, int count)
        {
            InitVersions(int.MaxValue);
            base.AnnotateToLine(line, count);
            StopDownloads();
        }

        public override void Annotate(int versions)
        {
            InitVersions(versions);
            base.Annotate(versions);
        }

        protected override bool AnnotateVersion()
        {
            int annotatedVersions = AnnotatedVersions;
            try
            {
                if(annotatedVersions == m_annList.Count || remainingLines <= 0)
                {
                    AnnotationComplete();
                    return false;
                }
                else
                {
                    try
                    {
                        AnnotateVersion(m_annList.GetFileVersion(annotatedVersions));
                    }
                    catch
                    {
                        StopDownloads();
                        throw;
                    }
                    return true;
                }
            }
            finally
            {
                if(annotatedVersions > 1 && m_annList.m_fileVersions[annotatedVersions - 1].Name != null)
                {
                    FileSpec.DeleteFileWithoutException(m_annList.m_fileVersions[annotatedVersions - 1].Name);
                    m_annList.m_fileVersions[annotatedVersions - 1].Name = (string)null;
                }
            }
        }

        private void InitVersions(int estimatedCount)
        {
            if(m_annList.Count == 0)
            {
                IEnumerable enumerable = m_vcs.QueryHistory(m_path, VersionSpec.Latest, 0, RecursionType.None, (string)null, (VersionSpec)null, m_version, int.MaxValue, true, false, true);
                Changeset changeset1 = null;
                foreach(Changeset changeset2 in enumerable)
                {
                    if(changeset1 == null)
                        changeset1 = changeset2;
                    if(changeset2.Changes[0].Item.ItemType != ItemType.File)
                        throw new ApplicationException(m_path);
                    else if((changeset2.Changes[0].ChangeType & ChangeType.Edit) != (ChangeType)0)
                        m_annList.Add(changeset2, null);
                }
                if(m_annList.Count == 0)
                {
                    if(changeset1 != null)
                        m_annList.Add(changeset1, null);
                    else
                        throw new ApplicationException(m_path);
                }
                if(!VersionControlPath.IsServerItem(m_path))
                {
                    Workspace workspace = m_vcs.GetWorkspace(m_path);
                    PendingChange[] pendingChanges = workspace.GetPendingChanges(m_path);
                    if(pendingChanges.Length == 1)
                    {
                        if((pendingChanges[0].ChangeType & ChangeType.Edit) != 0)
                        {
                            if(pendingChanges[0].Encoding == -1)
                            {
                                throw new BinaryEncodingException(pendingChanges[0].Version.ToString());
                            }
                            else
                            {
                                m_annList.Insert(0, null, new FileVersion(m_path, pendingChanges[0].Encoding));
                                --estimatedCount;
                            }
                        }
                        m_path = pendingChanges[0].ServerItem;
                    }
                    else
                        m_path = workspace.GetServerItemForLocalItem(m_path);
                }
            }
            BeginDownloadFiles(estimatedCount);
        }

        private void BeginDownloadFiles(int estimatedCount)
        {
            lock(m_annList)
            {
                while(m_downloadState != DownloadState.Stopped)
                    Monitor.Wait(m_annList);
                m_downloadState = DownloadState.InProgress;
            }
            m_annList.m_ex = null;
            ThreadPool.UnsafeQueueUserWorkItem(DownloadFiles, new DownloadArguments(estimatedCount));
        }

        private void DownloadFiles(object o)
        {
            DownloadArguments downloadArguments = (DownloadArguments)o;
            string str = null;
            try
            {
                for(int annotatedVersions = AnnotatedVersions; annotatedVersions < m_annList.Count && downloadArguments.m_estimatedCount > 0; ++annotatedVersions)
                {
                    if(m_annList.m_fileVersions[annotatedVersions] == null)
                    {
                        Changeset changeset = m_annList.m_changesets[annotatedVersions];
                        if(changeset.Changes[0].Item.Encoding == -1)
                        {
                            throw new BinaryEncodingException(changeset.ChangesetId.ToString());
                        }
                        else
                        {
                            str = Path.GetTempFileName();
                            changeset.Changes[0].Item.DownloadFile(str);
                            lock(m_annList)
                            {
                                if(m_disposed)
                                {
                                    FileSpec.DeleteFileWithoutException(str);
                                    break;
                                }
                                else
                                {
                                    m_annList.m_fileVersions[annotatedVersions] = new FileVersion(str, changeset.Changes[0].Item.Encoding);
                                    str = null;
                                    Monitor.PulseAll(m_annList);
                                    if(m_downloadState == DownloadState.StopPending)
                                        break;
                                    if(annotatedVersions - AnnotatedVersions > 2)
                                        Monitor.Wait(m_annList);
                                }
                            }
                        }
                    }
                    --downloadArguments.m_estimatedCount;
                }
            }
            catch(Exception ex)
            {
                lock(m_annList)
                {
                    m_annList.m_ex = ex;
                    Monitor.PulseAll(m_annList);
                }
                if(str != null)
                    FileSpec.DeleteFileWithoutException(str);
            }
            finally
            {
                lock(m_annList)
                {
                    m_downloadState = DownloadState.Stopped;
                    Monitor.PulseAll(m_annList);
                }
            }
        }

        private void StopDownloads()
        {
            lock(m_annList)
            {
                if(m_downloadState != DownloadState.InProgress)
                    return;
                m_downloadState = DownloadState.StopPending;
                Monitor.PulseAll(m_annList);
            }
        }

        public enum ChangesetState
        {
            Unknown,
            Local,
            Committed,
        }

        private enum DownloadState
        {
            Stopped,
            InProgress,
            StopPending,
        }

        private class AnnotationVersionList
        {
            internal List<Changeset> m_changesets = new List<Changeset>();
            internal List<FileVersion> m_fileVersions = new List<FileVersion>();
            internal Exception m_ex;

            public int Count
            {
                get
                {
                    return m_changesets.Count;
                }
            }

            public void Clear()
            {
                m_changesets.Clear();
                m_fileVersions.Clear();
            }

            public void Add(Changeset changeset, FileVersion annFile)
            {
                m_changesets.Add(changeset);
                m_fileVersions.Add(annFile);
            }

            public void Insert(int index, Changeset changeset, FileVersion annFile)
            {
                m_changesets.Insert(index, changeset);
                m_fileVersions.Insert(index, annFile);
            }

            public FileVersion GetFileVersion(int index)
            {
                lock(this)
                {
                    while(m_fileVersions[index] == null)
                    {
                        if(m_ex != null)
                            throw m_ex;
                        Monitor.Wait(this);
                    }
                    Monitor.PulseAll(this);
                }
                return m_fileVersions[index];
            }
        }

        private class DownloadArguments
        {
            internal int m_estimatedCount;

            public DownloadArguments(int estimatedCount)
            {
                m_estimatedCount = estimatedCount;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
        [Serializable]
        public class BinaryEncodingException : ApplicationException
        {
            public BinaryEncodingException(string message)
                : base(message)
            {
            }
        }
    }
}
