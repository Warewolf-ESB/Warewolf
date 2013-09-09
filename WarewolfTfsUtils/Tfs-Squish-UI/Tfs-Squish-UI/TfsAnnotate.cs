// Type: Microsoft.TeamFoundation.PowerTools.CommandLine.CommandAnnotate
// Assembly: TFPT, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Program Files (x86)\Microsoft Team Foundation Server 2012 Power Tools\TFPT.EXE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Tfs.Squish
{
    internal class CommandAnnotate
    {

        public CommandAnnotate()
        {
        }


        public void Run()
        {
            WorkspaceInfo workspace1 = this.GetWorkspace((IList<string>)this.m_arguments.FreeArguments);
            this.SetTeamFoundationServer(workspace1.ServerUri);
            Workspace workspace2 = workspace1.GetWorkspace(this.Tfs);
            bool flag = this.m_arguments.Contains(Options.ID.NoPrompt);
            if(this.m_arguments.FreeArguments.Count != 1)
                throw new Command.ArgumentListException(TfptResources.Get("AnnotateFileRequired"));
            string str = this.m_arguments.FreeArguments[0];
            string versionSpec = (string)null;
            int length = str.IndexOf(';');
            if(length != -1)
            {
                versionSpec = str.Substring(length + 1);
                str = str.Substring(0, length);
            }
            string path;
            VersionSpec version;
            if(VersionControlPath.IsServerItem(str))
            {
                path = str;
                version = VersionSpec.Latest;
            }
            else
            {
                str = Path.GetFullPath(str);
                path = workspace2.GetServerItemForLocalItem(str);
                version = (VersionSpec)new WorkspaceVersionSpec(workspace2);
            }
            if(!string.IsNullOrEmpty(versionSpec))
                version = VersionSpec.ParseSingleSpec(versionSpec, workspace1.OwnerName);
            using(AnnotatedVersionedFile annFile = new AnnotatedVersionedFile(this.VersionControlServer, path, version))
            {
                if(!flag)
                {
                    this.m_display.Write("Computing differences");
                    annFile.AnnotatedVersionAvailable += new AnnotatedVersionAvailableEventHandler(this.annFile_AnnotatedVersionAvailable);
                }
                annFile.AnnotateAll();
                this.m_display.WriteLine();
                if(flag)
                {
                    this.PrintInterleavedOutput(annFile, Console.Out);
                }
                else
                {
                    this.m_display.WriteLine("Launching viewer");
                    this.DisplayVisualOutput(annFile, str);
                }
            }
        }

        private void annFile_AnnotatedVersionAvailable(object sender, AnnotatedVersionAvailableEventArgs e)
        {
            this.m_display.Write(".");
        }

        private void PrintInterleavedOutput(AnnotatedVersionedFile annFile, TextWriter writer)
        {
            bool flag1 = false;
            bool flag2 = true;
            bool flag3 = false;
            AnnotatedFile.FileVersion tipVersionFile = annFile.TipVersionFile;
            using(DiffLineReader diffLineReader = new DiffLineReader((TextReader)new StreamReader((Stream)File.OpenRead(tipVersionFile.Name), Encoding.GetEncoding(tipVersionFile.CodePage))))
            {
                int line = 0;
                string str;
                while((str = diffLineReader.ReadLine()) != null)
                {
                    Changeset changeset;
                    AnnotatedVersionedFile.ChangesetState changesetForLine = annFile.GetChangesetForLine(line, out changeset);
                    if(flag2)
                    {
                        if(changesetForLine == AnnotatedVersionedFile.ChangesetState.Committed)
                            writer.Write("{0,-8} ", (object)changeset.ChangesetId);
                        else if(changesetForLine == AnnotatedVersionedFile.ChangesetState.Local)
                            writer.Write("{0,-8} ", (object)"Local");
                        else
                            writer.Write("{0,-8} ", (object)"Unknown");
                    }
                    if(flag1)
                        writer.Write("{0,-8}", (object)changeset.Owner);
                    if(flag3)
                        writer.Write("{0, -10}", (object)changeset.CreationDate.ToShortDateString());
                    writer.WriteLine(str);
                    ++line;
                }
            }
        }
    }
}
