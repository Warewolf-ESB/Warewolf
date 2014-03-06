using System;
using System.IO;
using System.Text;

namespace Infragistics.SpellChecker
{

	internal class DictFile  
	{

		///<summary>WordList or ReverseList</summary>
		public static int WordList = 1, ReverseList = 2;

		internal Stream dictFile;
		internal String dictFilePath;
		internal String dictFileExtension;
		internal byte[] dictFileBytes;
		Int32 listLength;
		Int32 listBytesLength;
		int listStart;

        UTF8Encoding unicodeEncoding = new UTF8Encoding();
		Decoder uniEnc;

        /// <summary>
        /// Creates a new DictFile object working on the file in dictFilePath.
        /// </summary>
		public DictFile(String dictFilePath)
		{

			dictFile = new FileStream(dictFilePath, FileMode.Open, FileAccess.Read);
			this.dictFilePath = dictFilePath;

			// MD 10/5/11 - TFS84743/TFS33353
			// The init method needs to know the file path and will store the dictFilePath member variable.
			//init();
			init(dictFilePath);
		}

        /// <summary>
        /// Creates a new DictFile object working on the file in dictFilePath.
        /// </summary>
		// MD 10/5/11 - TFS84743/TFS33353
		//public DictFile(Stream stream)//, byte[] bytes)
		public DictFile(Stream stream, string originalFilePath = "")
		{
			dictFile = stream;
            dictFileBytes = new byte[(int)stream.Length];
            stream.Read(dictFileBytes, 0, (int)stream.Length);
            stream.Position = 0;
			
			// MD 10/5/11 - TFS84743/TFS33353
			// The init method needs to know the extension.
			init(originalFilePath);
		}

		// MD 10/5/11 - TFS84743/TFS33353
		// The init method needs to know the extension.
		//void init()
		void init(string originalFilePath)
		{
			// MD 10/5/11 - TFS91391/TFS84743/TFS33353
			// If the Dictionary file is a .txt file then do not read the stream(bypass the logic used for reading .dict files)
			// set the start position and the length of the list and return to the calling method
			if (originalFilePath != null)
			{
				this.dictFileExtension = Path.GetExtension(originalFilePath);
				if (string.Equals(".txt", this.dictFileExtension, StringComparison.CurrentCultureIgnoreCase))
				{
					listStart = 0;
					listLength = (int)dictFile.Length;
					return;
				}
			}

			//read the header info
			int b;		//the current byte

			//the byte array to hold the length in bytes of the word list (25 digits)
			byte[] lengthBytes = new byte[50];
			byte lengthBytesPtr = 0;

			//Read the header, up until the first ':'
			while ( (b = dictFile.ReadByte()) != -1 && b != 58 ) listStart++;
			//because listStart wasnt shifted along when we reached last byte of header, do it now
			listStart++;


			//read word list length from header
			while ( (b = dictFile.ReadByte()) != -1 && b != 58 ) 
			{
				lengthBytes[lengthBytesPtr] = (byte) b;	
				listStart++;
				lengthBytesPtr++;
			}
			listStart++;
			



			//decode bytes into unicode, and get forward list length (bytes) from lengthBytes
			uniEnc = unicodeEncoding.GetDecoder();
			char[] chars = new char[lengthBytesPtr];
			uniEnc.GetChars(lengthBytes, 0, lengthBytesPtr, chars, 0);
			listLength = Int32.Parse(new String(chars));


			//read  list length in bytes
			lengthBytesPtr=0;
			while ( (b = dictFile.ReadByte()) != -1 &&  b != 58 ) 
			{
				lengthBytes[lengthBytesPtr] = (byte) b;	
				listStart++;
				lengthBytesPtr++;		
			}
			listStart++;

			//get unicode decoder and grab reverse list length from header
			uniEnc = unicodeEncoding.GetDecoder();
			chars = new char[lengthBytesPtr];
			uniEnc.GetChars(lengthBytes, 0, lengthBytesPtr, chars, 0);
			listBytesLength = Int32.Parse(new String(chars));
	
			//if we read from a file, close the stream
			//if (dictFilePath!=null) dictFile.Close();
            dictFile.Close();

		}

        /// <summary>
        /// Returns a byte array of the words in the list of the specified type.
        /// </summary>
		public byte[] ReadList(int type)
		{
			Int32 start=0, length=0;
			byte[] bytes;
			if (type == WordList)
			{
				start = listStart;
				length = listLength;
			} 
			else if (type==ReverseList)
			{
				start = listStart + listLength;
				length = listLength;
			}
			bytes = new byte[length];
			dictFile.Seek(start, SeekOrigin.Begin);
			dictFile.Read(bytes, 0, length);
			return bytes;
		}


        /// <summary>
        /// Return the decoded file stream.
        /// </summary>
        public Stream GetDecodedFileStream()
        {
            return new DecodedFileStream(dictFilePath, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Gets the start and length of the wordlist.
        /// </summary>
		public int[] GetStreamDimensions(int type)
		{
			Int32 start=0, length=0;
			if (type == WordList)
			{
				start = listStart;
				length = listLength;
			} 
			else if (type==ReverseList)
			{
				start = listStart + listBytesLength;
				length = listLength;
			}	
			int[] pos = {start, length};
			return pos;
		}
	}



	internal class DecodedFileStream : FileStream
	{
		public DecodedFileStream(string path,  FileMode mode,  FileAccess access) : base(path, mode, access){}
		public override int Read(byte[] array, int offset, int count)
		{
			int r = base.Read(array, offset, count);
			for(int i=offset; i<count; i++)
			{
				array[i] = (byte)((((int)array[i]) - 48) %256);
			}
			return r;
		}

		public override int ReadByte()
		{
			int r = base.ReadByte();
			if (r != -1)
			{
				return (((int)r) - 48) %256;
			} 
			else 
			{
				return -1;
			}
		}
	}

	internal class DecodedMemoryStream : MemoryStream
	{
		public DecodedMemoryStream(byte[] bytearray) : base(bytearray){}

		public override int Read(byte[] array, int offset, int count)
		{
			int r = base.Read(array, offset, count);
			for(int i=offset; i<count; i++)
			{
				array[i] = (byte)((((int)array[i]) - 48) %256);
			}
			return r;
		}

		public override int ReadByte()
		{
			int r = base.ReadByte();
			if (r != -1)
			{
				return (((int)r) - 48) %256;
			} 
			else 
			{
				return -1;
			}
		}
	}
}



#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved