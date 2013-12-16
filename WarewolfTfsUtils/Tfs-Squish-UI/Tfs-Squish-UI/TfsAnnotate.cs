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
        
        private string _serverURI;

        public TfsAnnotate(string serverURI)
        {
            _serverURI = serverURI;
        }

        private VersionControlServer FetchVersionControlServer()
        {

            string serverName = _serverURI;
            TeamFoundationServer tfs = new TeamFoundationServer(serverName);
            VersionControlServer version = (VersionControlServer) tfs.GetService(typeof (VersionControlServer));
            return version;
            //var result = version.GetWorkspace(_workspace, version.AuthenticatedUser);

            //return result;

        }

        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="dumpCode">if set to <c>true</c> [dump code].</param>
        public void MyInvoke(string file, bool dumpCode)
        {
            MyInvoke(file, Console.Out, string.Empty, string.Empty, dumpCode);
        }
        
        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="dumpCode">if set to <c>true</c> [dump code].</param>
        public void MyInvoke(string file, TextWriter outputStream, bool dumpCode)
        {
            MyInvoke(file, outputStream, string.Empty, string.Empty, dumpCode);
        }


        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="user">The user.</param>
        /// <param name="pass">The pass.</param>
        /// <param name="dumpCode">if set to <c>true</c> [dump code].</param>
        /// <exception cref="Microsoft.TeamFoundation.Client.CommandLine.Command.ArgumentListException">AnnotateFileRequired</exception>
        public void MyInvoke(string file, string user, string pass, bool dumpCode)
        {
            try
            {
                var ws = FetchVersionControlServer();

                if (string.IsNullOrEmpty(file))
                {
                    throw new Command.ArgumentListException("AnnotateFileRequired");
                }

                VersionSpec version = VersionSpec.Latest;

                using (AnnotatedVersionedFile annotatedVersionedFile = new AnnotatedVersionedFile(ws, file, version))
                {
                    annotatedVersionedFile.AnnotateAll();
                    DumpToStream(annotatedVersionedFile, Console.Out, dumpCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

        /// <summary>
        /// Invoke the annotatation fetch for the file ;)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="user">The user.</param>
        /// <param name="pass">The pass.</param>
        /// <param name="dumpCode">if set to <c>true</c> [dump code].</param>
        /// <exception cref="Microsoft.TeamFoundation.Client.CommandLine.Command.ArgumentListException">AnnotateFileRequired</exception>
        public void MyInvoke(string file, TextWriter outputStream, string user, string pass, bool dumpCode)
        {
            var ws = FetchVersionControlServer();

            if(string.IsNullOrEmpty(file))
            {
                throw new Command.ArgumentListException("AnnotateFileRequired");
            }

            VersionSpec version = VersionSpec.Latest;

            using(AnnotatedVersionedFile annotatedVersionedFile = new AnnotatedVersionedFile(ws, file, version))
            {
                annotatedVersionedFile.AnnotateAll();
                DumpToStream(annotatedVersionedFile, outputStream, dumpCode);
            }

        }

        /// <summary>
        /// Dumps the output to the console
        /// </summary>
        /// <param name="annFile">The ann file.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="dumpCode">if set to <c>true</c> [dump code].</param>
        private void DumpToStream(AnnotatedVersionedFile annFile, TextWriter outputStream, bool dumpCode)

        {

            AnnotatedFile.FileVersion versionFile = annFile.TipVersionFile;

            var writer = outputStream;

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
                        //writer.Write("{0,-8} ", line);
                        writer.Write("{0,-8} ", changeset.ChangesetId);
                        writer.Write("{0,-8} ", changeset.Owner);
                        writer.Write("{0, -10} ", changeset.CreationDate.ToShortDateString());
                    }

                    if (dumpCode)
                    {
                        writer.WriteLine(str);
                    }
                    else
                    {
                        writer.WriteLine();
                    }
                    ++line;
                }
            }
        }

    }
}
