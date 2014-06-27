using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ionic.Zip;
namespace Gui.Utility
{
    public class UpdateExampleResources
    {
        public void SetupSamples(string sourceZip,string outDir,string destinationDir)
        {
           using(var zip = ZipFile.Read(sourceZip))
           {
               zip.ExtractAll(outDir,ExtractExistingFileAction.OverwriteSilently);
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
               UpdateExamples(outDir, destinationDir).ToList();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
           }
        }

        public IEnumerable<Resource> UpdateExamples(string sourceDir, string destinationDir,bool flatStructure= false)
        {
            var destFiles = FetchFiles(destinationDir);
            var sourceFiles = FetchFiles(sourceDir);
            return  sourceFiles.OrderBy(a=>a.Path).Select(a => MoveFile(a, destFiles,destinationDir,flatStructure));
        }

        Resource MoveFile(Resource resource, IEnumerable<Resource> destFiles,string destinationDir,bool flatStructure=false)
        {
            try
            {


                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);
                var fileToDelete = destFiles.FirstOrDefault(a => a == resource);
                var destFileName = Path.Combine(destinationDir, resource.Path + ".xml");
                if( flatStructure  &&  resource.Path.LastIndexOf("\\", StringComparison.Ordinal)>=0)
                  destFileName = Path.Combine(destinationDir, resource.Path.Substring(1+resource.Path.LastIndexOf("\\", StringComparison.Ordinal)) + ".xml");
                   
                if (File.Exists(destFileName))
                    File.Delete(destFileName);
                if (fileToDelete != null)
                {
                    File.Delete(fileToDelete.FileName);
                }
                FileInfo f = new FileInfo(destFileName);
                var dir = f.Directory;
                if (dir != null && !dir.Exists)
                    dir.Create();
                File.Copy(resource.FileName, destFileName,true);
                return new Resource(destFileName, resource.ID, resource.Path);
            }
// ReSharper disable EmptyGeneralCatchClause
            catch 
// ReSharper restore EmptyGeneralCatchClause
            {

            }
            return new Resource(resource.FileName, resource.ID, resource.Path);
        }

        public IEnumerable<Resource> FetchFiles(string dir)
        {
            if(Directory.Exists(dir))
            {
                var root =  Directory.GetFiles(dir,"*.xml").Select(CreateResource).ToList();
                root.AddRange((Directory.GetDirectories(dir).Where(a=>Directory.GetFileSystemEntries(a).Length>0).SelectMany(FetchFiles)));
                return root;
            }
// ReSharper disable RedundantIfElseBlock
            else
// ReSharper restore RedundantIfElseBlock
            {
                return new List<Resource>();
            }
        }

        Resource CreateResource(string arg)
        {
            var element = XElement.Load(File.OpenText(arg));
            var id = Guid.Parse( element.Attribute("ID").Value);
            var cat = element.Descendants("Category").First().Value;
            return   new Resource(arg,id,cat);
        }

        public void SetupSamplesFlat(string sourceZip, string outDir, string destinationDir)
        {
            using (var zip = ZipFile.Read(sourceZip))
            {
                zip.ExtractAll(outDir, ExtractExistingFileAction.OverwriteSilently);
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
                UpdateExamples(outDir, destinationDir,true).ToList();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
            }
        }
    }

    public class Resource : IEquatable<Resource>
    {
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Resource other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return ID.Equals(other.ID);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(Resource left, Resource right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Resource left, Resource right)
        {
            return !Equals(left, right);
        }

        #endregion

        public  Resource(string fileName,Guid id,string path)
        {
            FileName = fileName;
            ID = id;
            Path = path;
        }

        public string Path { get; private set; }

        public string FileName { get; private set; }
        public Guid ID { get; private set; }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Resource)obj);
        }
    }
}
