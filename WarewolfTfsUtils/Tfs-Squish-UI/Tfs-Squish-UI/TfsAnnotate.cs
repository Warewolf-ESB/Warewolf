using System;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Client.CommandLine;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Tfs.Squish
{
    internal class TfsAnnotate
    {
        private string _workspace;
        private string _serverURI;

        public TfsAnnotate(string serverURI, string workspace)
        {
            _workspace = workspace;
            _serverURI = serverURI;
        }

        private Workspace FetchWorkspace()
        {

            string serverName = _serverURI;
            TeamFoundationServer tfs = new TeamFoundationServer(serverName);
            VersionControlServer version = (VersionControlServer) tfs.GetService(typeof (VersionControlServer));
            var result = version.GetWorkspace(_workspace, version.AuthenticatedUser);

            return result;

        }

        public void MyInvoke(string workspace, string file, string ver, string user, string pass)
        {
            var ws = FetchWorkspace();

            if(string.IsNullOrEmpty(file))
            {
                throw new Command.ArgumentListException("AnnotateFileRequired");
            }

            VersionSpec version;

            if(VersionControlPath.IsServerItem(file))
            {
                version = VersionSpec.Latest;
            }
            else
            {
                var str = Path.GetFullPath(file);
                file = ws.GetServerItemForLocalItem(str);
                version = new WorkspaceVersionSpec(ws);
            }

            using(AnnotatedVersionedFile annFile = new AnnotatedVersionedFile(ws.VersionControlServer, file, version))
            {
                annFile.AnnotateAll();
                DumpToConsole(annFile);
            }

        }

        /// <summary>
        /// Dumps the output to the console
        /// </summary>
        /// <param name="annFile">The ann file.</param>
        private void DumpToConsole(AnnotatedVersionedFile annFile)
        {

            AnnotatedFile.FileVersion versionFile = annFile.TipVersionFile;

            var writer = Console.Out;

            using(DiffLineReader diffLineReader = new DiffLineReader(new StreamReader(File.OpenRead(versionFile.Name), Encoding.GetEncoding(versionFile.CodePage))))
            {
                int line = 0;
                string str;
                while((str = diffLineReader.ReadLine()) != null)
                {
                    Changeset changeset;
                    AnnotatedVersionedFile.ChangesetState changesetForLine = annFile.GetChangesetForLine(line, out changeset);

                    if (changesetForLine == AnnotatedVersionedFile.ChangesetState.Committed)
                    {
                        writer.Write("{0,-8} ", changeset.ChangesetId);
                        writer.Write("{0,-8}", changeset.Owner);
                    }
                    else if (changesetForLine == AnnotatedVersionedFile.ChangesetState.Local)
                    {
                        writer.Write("{0,-8} ", "Local");
                    }
                    else
                    {
                        writer.Write("{0,-8} ", "Unknown");
                    }

                    writer.Write("{0, -10}", changeset.CreationDate.ToShortDateString());
                    writer.WriteLine(str);
                    ++line;
                }
            }
        }

        //public void Invoke(string file, string ver)
        //{
        //    WorkspaceInfo workspace1 = this.GetWorkspace((IList<string>)this.m_arguments.FreeArguments);
        //    this.SetTeamFoundationServer(workspace1.ServerUri);
        //    Workspace workspace2 = workspace1.GetWorkspace(this.Tfs);
            
        //    if (string.IsNullOrEmpty(file))
        //    {
        //        throw new Command.ArgumentListException("AnnotateFileRequired");
        //    }

        //    //VersionSpec version;
        //    //if(VersionControlPath.IsServerItem(file))
        //    //{
        //    //    version = VersionSpec.Latest;
        //    //}
        //    //else
        //    //{
        //    //    var str = Path.GetFullPath(file);
        //    //    path = workspace2.GetServerItemForLocalItem(str);
        //    //    version = (VersionSpec)new WorkspaceVersionSpec(workspace2);
        //    //}

        //    if(!string.IsNullOrEmpty(versionSpec))
        //        version = VersionSpec.ParseSingleSpec(versionSpec, workspace1.OwnerName);

        //    using(AnnotatedVersionedFile annFile = new AnnotatedVersionedFile(this.VersionControlServer, path, version))
        //    {
        //        if(!flag)
        //        {
        //            this.m_display.Write("Computing differences");
        //            annFile.AnnotatedVersionAvailable += new AnnotatedVersionAvailableEventHandler(this.annFile_AnnotatedVersionAvailable);
        //        }
        //        annFile.AnnotateAll();
        //        this.m_display.WriteLine();
        //        if(flag)
        //        {
        //            this.PrintInterleavedOutput(annFile, Console.Out);
        //        }
        //        else
        //        {
        //            this.m_display.WriteLine("Launching viewer");
        //            this.DisplayVisualOutput(annFile, str);
        //        }
        //    }
        //}

        //private void annFile_AnnotatedVersionAvailable(object sender, AnnotatedVersionAvailableEventArgs e)
        //{
        //    this.m_display.Write(".");
        //}

        //private void PrintInterleavedOutput(AnnotatedVersionedFile annFile, TextWriter writer)
        //{
        //    bool flag1 = false;
        //    bool flag2 = true;
        //    bool flag3 = false;
        //    AnnotatedFile.FileVersion tipVersionFile = annFile.TipVersionFile;
        //    using(DiffLineReader diffLineReader = new DiffLineReader((TextReader)new StreamReader((Stream)File.OpenRead(tipVersionFile.Name), Encoding.GetEncoding(tipVersionFile.CodePage))))
        //    {
        //        int line = 0;
        //        string str;
        //        while((str = diffLineReader.ReadLine()) != null)
        //        {
        //            Changeset changeset;
        //            AnnotatedVersionedFile.ChangesetState changesetForLine = annFile.GetChangesetForLine(line, out changeset);
        //            if(flag2)
        //            {
        //                if(changesetForLine == AnnotatedVersionedFile.ChangesetState.Committed)
        //                    writer.Write("{0,-8} ", (object)changeset.ChangesetId);
        //                else if(changesetForLine == AnnotatedVersionedFile.ChangesetState.Local)
        //                    writer.Write("{0,-8} ", (object)"Local");
        //                else
        //                    writer.Write("{0,-8} ", (object)"Unknown");
        //            }
        //            if(flag1)
        //                writer.Write("{0,-8}", (object)changeset.Owner);
        //            if(flag3)
        //                writer.Write("{0, -10}", (object)changeset.CreationDate.ToShortDateString());
        //            writer.WriteLine(str);
        //            ++line;
        //        }
        //    }
        //}
    }
}
