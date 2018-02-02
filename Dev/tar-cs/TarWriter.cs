using System;
using System.IO;

namespace tar_cs
{
    public class TarWriter : LegacyTarWriter
    {

        public TarWriter(Stream writeStream) : base(writeStream)
        {
        }

        protected override void WriteHeader(string name, DateTime lastModificationTime, long count, int userId, int groupId, int mode, EntryType entryType)
        {
            var tarHeader = new UsTarHeader()
            {
                FileName = name,
                LastModification = lastModificationTime,
                SizeInBytes = count,
                UserId = userId,
                UserName = Convert.ToString(userId,8),
                GroupId = groupId,
                GroupName = Convert.ToString(groupId,8),
                Mode = mode,
                EntryType = entryType
            };
            OutStream.Write(tarHeader.GetHeaderValue(), 0, tarHeader.HeaderSize);
        }

        protected virtual void WriteHeader(string name, DateTime lastModificationTime, long count, string userName, string groupName, int mode)
        {
            var tarHeader = new UsTarHeader()
            {
                FileName = name,
                LastModification = lastModificationTime,
                SizeInBytes = count,
                UserId = userName.GetHashCode(),
                UserName = userName,
                GroupId = groupName.GetHashCode(),
                GroupName = groupName,
                Mode = mode
            };
            OutStream.Write(tarHeader.GetHeaderValue(), 0, tarHeader.HeaderSize);
        }


        public virtual void Write(string name, long dataSizeInBytes, string userName, string groupName, int mode, DateTime lastModificationTime, WriteDataDelegate writeDelegate)
        {
            var writer = new DataWriter(OutStream,dataSizeInBytes);
            WriteHeader(name, lastModificationTime, dataSizeInBytes, userName, groupName, mode);
            while(writer.CanWrite)
            {
                writeDelegate(writer);
            }
            AlignTo512(dataSizeInBytes, false);
        }


        public void Write(Stream data, long dataSizeInBytes, string fileName, string userId, string groupId, int mode,
                          DateTime lastModificationTime)
        {
            WriteHeader(fileName,lastModificationTime,dataSizeInBytes,userId, groupId, mode);
            WriteContent(dataSizeInBytes,data);
            AlignTo512(dataSizeInBytes,false);
        }
    }
}