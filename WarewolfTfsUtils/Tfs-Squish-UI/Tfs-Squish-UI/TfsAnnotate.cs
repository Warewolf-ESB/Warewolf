using System;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Client.CommandLine;
using Microsoft.TeamFoundation.VersionControl.Client;

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

    }
}
