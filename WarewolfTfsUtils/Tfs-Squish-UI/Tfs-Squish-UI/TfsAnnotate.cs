using System;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Client.CommandLine;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Tfs.Squish
{
    public class TfsAnnotate
    {
        private string _workspace;
        private string _serverURI;
        private string _localMappedPath;

        // , string localMappedPath
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

        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="outputStream">The output stream.</param>
        public void MyInvoke(string file, TextWriter outputStream, bool delimit)
        {
            MyInvoke(file, outputStream, string.Empty, string.Empty, delimit);
        }

        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="ver">The ver.</param>
        public void MyInvoke(string file, string ver)
        {
            MyInvoke(file, Console.Out, string.Empty, string.Empty);
        }

        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="ver">The ver.</param>
        /// <param name="outputStream">The output stream.</param>
        public void MyInvoke(string file, string ver, TextWriter outputStream)
        {
            MyInvoke(file, outputStream, string.Empty, string.Empty);
        }

        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="user">The user.</param>
        /// <param name="pass">The pass.</param>
        /// <param name="delimit">if set to <c>true</c> [delimit].</param>
        /// <exception cref="Microsoft.TeamFoundation.Client.CommandLine.Command.ArgumentListException">AnnotateFileRequired</exception>
        public void MyInvoke(string file, TextWriter outputStream, string user, string pass, bool delimit = false)
        {
            var ws = FetchWorkspace();

            if(string.IsNullOrEmpty(file))
            {
                throw new Command.ArgumentListException("AnnotateFileRequired");
            }

            VersionSpec version = VersionSpec.Latest;

            using(AnnotatedVersionedFile annotatedVersionedFile = new AnnotatedVersionedFile(ws.VersionControlServer, file, version))
            {
                annotatedVersionedFile.AnnotateAll();
                DumpToStream(annotatedVersionedFile, outputStream, delimit);
            }

        }

        /// <summary>
        /// Dumps the output to the console
        /// </summary>
        /// <param name="annFile">The ann file.</param>
        /// <param name="outputStream"></param>
        private void DumpToStream(AnnotatedVersionedFile annFile, TextWriter outputStream, bool delimit)
        {

            AnnotatedFile.FileVersion versionFile = annFile.TipVersionFile;

            var writer = outputStream;

            var token = " ";

            if (delimit)
            {
                token = "|";
            }

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
                        writer.Write("{0,-8}"+token, line);
                        writer.Write("{0,-8}"+token, changeset.ChangesetId);
                        writer.Write("{0,-8}"+token, changeset.Owner);
                        writer.Write("{0, -10}"+token, changeset.CreationDate.ToShortDateString());
                    }

                    writer.WriteLine(str);
                    ++line;
                }
            }
        }

        //public void Invoke(string file, string ver)
        //{
        //    WorkspaceInfo workspace1 = GetWorkspace((IList<string>)m_arguments.FreeArguments);
        //    SetTeamFoundationServer(workspace1.ServerUri);
        //    Workspace workspace2 = workspace1.GetWorkspace(Tfs);
            
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

        //    using(AnnotatedVersionedFile annFile = new AnnotatedVersionedFile(VersionControlServer, path, version))
        //    {
        //        if(!flag)
        //        {
        //            m_display.Write("Computing differences");
        //            annFile.AnnotatedVersionAvailable += new AnnotatedVersionAvailableEventHandler(annFile_AnnotatedVersionAvailable);
        //        }
        //        annFile.AnnotateAll();
        //        m_display.WriteLine();
        //        if(flag)
        //        {
        //            PrintInterleavedOutput(annFile, Console.Out);
        //        }
        //        else
        //        {
        //            m_display.WriteLine("Launching viewer");
        //            DisplayVisualOutput(annFile, str);
        //        }
        //    }
        //}

        //private void annFile_AnnotatedVersionAvailable(object sender, AnnotatedVersionAvailableEventArgs e)
        //{
        //    m_display.Write(".");
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
