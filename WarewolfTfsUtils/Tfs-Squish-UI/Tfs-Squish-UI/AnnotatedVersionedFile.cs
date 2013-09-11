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
                if(this.m_annList.Count == 0)
                    throw new InvalidOperationException();
                else
                    return this.m_annList.m_fileVersions[0];
            }
        }

        public int EstimatedVersions
        {
            get
            {
                return this.m_annList.Count;
            }
        }

        public AnnotatedVersionedFile(VersionControlServer vcs, string path, VersionSpec version)
        {
            this.m_vcs = vcs;
            this.m_path = path;
            this.m_version = version;
            this.m_annList = new AnnotatedVersionedFile.AnnotationVersionList();
        }

        public override void Dispose()
        {
            lock(this.m_annList)
            {
                this.m_disposed = true;
                this.StopDownloads();
                for(int local_0 = 0; local_0 < this.m_annList.Count; ++local_0)
                {
                    if(this.m_annList.m_changesets[local_0] != null && this.m_annList.m_fileVersions[local_0] != null && this.m_annList.m_fileVersions[local_0].Name != null)
                        FileSpec.DeleteFileWithoutException(this.m_annList.m_fileVersions[local_0].Name);
                }
                this.m_annList.Clear();
            }
        }

        public Changeset GetAnnotatedVersion(int index)
        {
            if(index < 0 || index >= this.AnnotatedVersions)
                throw new IndexOutOfRangeException();
            else
                return this.m_annList.m_changesets[index];
        }

        public AnnotatedVersionedFile.ChangesetState GetChangesetForLine(int line, out Changeset changeset)
        {
            int versionForLine = this.GetVersionForLine(line);
            if(versionForLine < 0)
            {
                changeset = (Changeset)null;
                return AnnotatedVersionedFile.ChangesetState.Unknown;
            }
            else if(this.m_annList.m_changesets[versionForLine] == null)
            {
                changeset = (Changeset)null;
                return AnnotatedVersionedFile.ChangesetState.Local;
            }
            else
            {
                changeset = this.m_annList.m_changesets[versionForLine];
                return AnnotatedVersionedFile.ChangesetState.Committed;
            }
        }

        public override void AnnotateToLine(int line)
        {
            this.InitVersions(int.MaxValue);
            base.AnnotateToLine(line);
            this.StopDownloads();
        }

        public override void AnnotateToLine(int line, int count)
        {
            this.InitVersions(int.MaxValue);
            base.AnnotateToLine(line, count);
            this.StopDownloads();
        }

        public override void Annotate(int versions)
        {
            this.InitVersions(versions);
            base.Annotate(versions);
        }

        protected override bool AnnotateVersion()
        {
            int annotatedVersions = this.AnnotatedVersions;
            try
            {
                if(annotatedVersions == this.m_annList.Count || this.m_remainingLines <= 0)
                {
                    this.AnnotationComplete();
                    return false;
                }
                else
                {
                    try
                    {
                        this.AnnotateVersion(this.m_annList.GetFileVersion(annotatedVersions));
                    }
                    catch
                    {
                        this.StopDownloads();
                        throw;
                    }
                    return true;
                }
            }
            finally
            {
                if(annotatedVersions > 1 && this.m_annList.m_fileVersions[annotatedVersions - 1].Name != null)
                {
                    FileSpec.DeleteFileWithoutException(this.m_annList.m_fileVersions[annotatedVersions - 1].Name);
                    this.m_annList.m_fileVersions[annotatedVersions - 1].Name = (string)null;
                }
            }
        }

        private void InitVersions(int estimatedCount)
        {
            if(this.m_annList.Count == 0)
            {
                IEnumerable enumerable = this.m_vcs.QueryHistory(this.m_path, VersionSpec.Latest, 0, RecursionType.None, (string)null, (VersionSpec)null, this.m_version, int.MaxValue, true, false, true);
                Changeset changeset1 = (Changeset)null;
                foreach(Changeset changeset2 in enumerable)
                {
                    if(changeset1 == null)
                        changeset1 = changeset2;
                    if(changeset2.Changes[0].Item.ItemType != ItemType.File)
                        throw new ApplicationException(this.m_path);
                    else if((changeset2.Changes[0].ChangeType & ChangeType.Edit) != (ChangeType)0)
                        this.m_annList.Add(changeset2, (AnnotatedFile.FileVersion)null);
                }
                if(this.m_annList.Count == 0)
                {
                    if(changeset1 != null)
                        this.m_annList.Add(changeset1, (AnnotatedFile.FileVersion)null);
                    else
                        throw new ApplicationException(this.m_path);
                }
                if(!VersionControlPath.IsServerItem(this.m_path))
                {
                    Workspace workspace = this.m_vcs.GetWorkspace(this.m_path);
                    PendingChange[] pendingChanges = workspace.GetPendingChanges(this.m_path);
                    if(pendingChanges.Length == 1)
                    {
                        if((pendingChanges[0].ChangeType & ChangeType.Edit) != (ChangeType)0)
                        {
                            if(pendingChanges[0].Encoding == -1)
                            {
                                throw new BinaryEncodingException(pendingChanges[0].Version.ToString());
                            }
                            else
                            {
                                this.m_annList.Insert(0, null, new FileVersion(this.m_path, pendingChanges[0].Encoding));
                                --estimatedCount;
                            }
                        }
                        this.m_path = pendingChanges[0].ServerItem;
                    }
                    else
                        this.m_path = workspace.GetServerItemForLocalItem(this.m_path);
                }
            }
            this.BeginDownloadFiles(estimatedCount);
        }

        private void BeginDownloadFiles(int estimatedCount)
        {
            lock(this.m_annList)
            {
                while(this.m_downloadState != DownloadState.Stopped)
                    Monitor.Wait((object)this.m_annList);
                this.m_downloadState = DownloadState.InProgress;
            }
            this.m_annList.m_ex = (Exception)null;
            ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.DownloadFiles), (object)new AnnotatedVersionedFile.DownloadArguments(estimatedCount));
        }

        private void DownloadFiles(object o)
        {
            AnnotatedVersionedFile.DownloadArguments downloadArguments = (AnnotatedVersionedFile.DownloadArguments)o;
            string str = (string)null;
            try
            {
                for(int annotatedVersions = this.AnnotatedVersions; annotatedVersions < this.m_annList.Count && downloadArguments.m_estimatedCount > 0; ++annotatedVersions)
                {
                    if(this.m_annList.m_fileVersions[annotatedVersions] == null)
                    {
                        Changeset changeset = this.m_annList.m_changesets[annotatedVersions];
                        if(changeset.Changes[0].Item.Encoding == -1)
                        {
                            throw new AnnotatedVersionedFile.BinaryEncodingException(changeset.ChangesetId.ToString());
                        }
                        else
                        {
                            str = Path.GetTempFileName();
                            changeset.Changes[0].Item.DownloadFile(str);
                            lock(this.m_annList)
                            {
                                if(this.m_disposed)
                                {
                                    FileSpec.DeleteFileWithoutException(str);
                                    break;
                                }
                                else
                                {
                                    this.m_annList.m_fileVersions[annotatedVersions] = new AnnotatedFile.FileVersion(str, changeset.Changes[0].Item.Encoding);
                                    str = (string)null;
                                    Monitor.PulseAll((object)this.m_annList);
                                    if(this.m_downloadState == AnnotatedVersionedFile.DownloadState.StopPending)
                                        break;
                                    if(annotatedVersions - this.AnnotatedVersions > 2)
                                        Monitor.Wait((object)this.m_annList);
                                }
                            }
                        }
                    }
                    --downloadArguments.m_estimatedCount;
                }
            }
            catch(Exception ex)
            {
                lock(this.m_annList)
                {
                    this.m_annList.m_ex = ex;
                    Monitor.PulseAll((object)this.m_annList);
                }
                if(str != null)
                    FileSpec.DeleteFileWithoutException(str);
            }
            finally
            {
                lock(this.m_annList)
                {
                    this.m_downloadState = AnnotatedVersionedFile.DownloadState.Stopped;
                    Monitor.PulseAll((object)this.m_annList);
                }
            }
        }

        private void StopDownloads()
        {
            lock(this.m_annList)
            {
                if(this.m_downloadState != AnnotatedVersionedFile.DownloadState.InProgress)
                    return;
                this.m_downloadState = AnnotatedVersionedFile.DownloadState.StopPending;
                Monitor.PulseAll((object)this.m_annList);
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
            internal List<AnnotatedFile.FileVersion> m_fileVersions = new List<AnnotatedFile.FileVersion>();
            internal Exception m_ex;

            public int Count
            {
                get
                {
                    return this.m_changesets.Count;
                }
            }

            public void Clear()
            {
                this.m_changesets.Clear();
                this.m_fileVersions.Clear();
            }

            public void Add(Changeset changeset, AnnotatedFile.FileVersion annFile)
            {
                this.m_changesets.Add(changeset);
                this.m_fileVersions.Add(annFile);
            }

            public void Insert(int index, Changeset changeset, AnnotatedFile.FileVersion annFile)
            {
                this.m_changesets.Insert(index, changeset);
                this.m_fileVersions.Insert(index, annFile);
            }

            public AnnotatedFile.FileVersion GetFileVersion(int index)
            {
                lock(this)
                {
                    while(this.m_fileVersions[index] == null)
                    {
                        if(this.m_ex != null)
                            throw this.m_ex;
                        Monitor.Wait((object)this);
                    }
                    Monitor.PulseAll((object)this);
                }
                return this.m_fileVersions[index];
            }
        }

        private class DownloadArguments
        {
            internal int m_estimatedCount;

            public DownloadArguments(int estimatedCount)
            {
                this.m_estimatedCount = estimatedCount;
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
