using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Security;
using System.Text;
using System.IO.IsolatedStorage;

namespace Infragistics.SpellChecker
{
    ///	<summary>Represents the UserDictionary</summary>
    internal class UserDictionary
    {

        internal Stream dictFile;
        /// Whether the file is valid 
        private bool valid;

        /// array holding all lines in dict.
        private List<String> wordList;

        // MD 11/13/07 - BR27454
        // Added sorted words collection for faster searching
        private List<string> sortedWords;

        // MD 11/13/07 - BR27454
        // Added a property to get the sorted words collection
        internal List<string> SortedWords
        {
            get { return this.sortedWords; }
        }


        /// <summary>
        /// Constructs an invalid UserDictionary.
        /// </summary>
        public UserDictionary()
        {

        }

        /// <summary>Construct a UserDictionary based on a stream.</summary>
        ///	<param name="userDictionaryFile">The file used for a user dictionary.</param>
        // MD 8/15/07 - BR25752
        // There should be no word limit
        public UserDictionary(Stream userDictionaryFile)
        {
            wordList = new List<String>();
            // MD 11/13/07 - BR27454
            // Added sorted words collection for faster searching
            sortedWords = new List<string>(); dictFile = userDictionaryFile;

            //load the dictionary
            ReadDict();
        }

        /// <summary>Returns whether the dictionary file is valid.</summary>
        public virtual bool IsValid()
        {
            return valid;
        }



        /// <summary>Tries to add <c>word</c> to this dictionary</summary>
        ///	<param name="word">String containing the new word</param>
        ///	<returns>true if added successfully, false otherwise.</returns>
        public virtual bool AddWord(String word)
        {
            lock (this)
            {
                if (IsValid() && word.Length > 0)
                {
                    // MD 8/15/07 - BR25752
                    // There should be no word limit
                    //if(wordList.Count < wordLimit){
                    //add word
                    // MD 11/13/07 - BR27454
                    // Moved below, only add the word if we know it is not in the list already
                    //wordList.Add(word);

                    // MD 11/13/07 - BR27454
                    // Add the word to the new sorted word list too
                    int index = this.sortedWords.BinarySearch(word, SpellChecker.InvariantComparer.Instance);

                    if (index >= 0)
                        return false;

                    this.sortedWords.Insert(~index, word);

                    // MD 11/13/07 - BR27454
                    // Moved from above below, we only add the word if we know it is not in the list already
                    this.wordList.Add(word);

                    return true;
                    //if (WriteDict()) return true; else return false;
                    // MD 8/15/07 - BR25752
                    // There should be no word limit
                    //} else {
                    //    return false;
                    //}

                }
                else
                {
                    return false;
                }
            }
        }



        // MD 11/13/07 - BR27454
        // This is no longer needed. Instead of getting a copy of the word list, 
        // callers will now just use the word list exposed by the user dictionary.
        ///// <summary>Read the dictionary word list into <c>list</c>.</summary>
        /////	<param name="list">the String array that the word list will be read into.</param>
        /////	<returns>number of words in list.</returns>
        //public virtual int ReadAll(ArrayList list){
        //    lock(list){
        //        list.Clear();
        //        for(int ptr=0; ptr<wordList.Count; ptr++){
        //            list.Add(wordList[ptr]);
        //        }
        //        return wordList.Count;
        //    }
        //}

        /// <summary>
        /// Tries to remove a word from this dictionary.
        /// </summary>
        /// <param name="word">String contianing the word to remove.</param>
        /// <returns>True if the word was removed successfully; False otherwise</returns>
        public virtual bool RemoveWord(String word)
        {
            lock (this)
            {
                if (this.IsValid() && word.Length > 0)
                {
                    // MD 11/13/07 - BR27454
                    // Remove the word from the new sorted word list too
                    int index = this.sortedWords.BinarySearch(word, SpellChecker.InvariantComparer.Instance);

                    if (index < 0)
                        return false;

                    this.sortedWords.RemoveAt(index);

                    wordList.Remove(word);

                    //if (this.WriteDict())
                        return true;
                }

                return false;
            }
        }



        /// <summary>
        /// Read dictionary a file.
        /// </summary>
        private bool ReadDict()
        {
            lock (this)
            {
                String l = "";
                int ptr = 0;

                //read word list
                try
                {
                    // MD 9/19/07 - BR26620
                    // For Office programs to be able to read the custom dictionaries, they must be written in unicode, 
                    // but autodetect when reading just incase they open a file in another format
                    //StreamReader objReader = new StreamReader(dictFile, new System.Text.UTF8Encoding());
                    StreamReader objReader = new StreamReader(dictFile, true);

                    while (l != null)
                    {
                        l = objReader.ReadLine();
                        if (l != null)
                        {
                            wordList.Add(l);
                            // MD 11/13/07 - BR27454
                            // Also add the word to the sorted words list
                            this.sortedWords.Add(l);
                            ptr++;
                        }
                    }
                    //objReader.Close();
                    valid = true;

                    // MD 11/13/07 - BR27454
                    // Sort the sorted words list
                    this.sortedWords.Sort(SpellChecker.InvariantComparer.Instance);

                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("Can't find wordlist: " + e);
                    valid = false;
                }
                catch (IOException e)
                {
                    Console.WriteLine("Can't open wordlist: " + e);
                    valid = false;
                }


                return valid;
            }
        }


        /// Write dictionary file ////
        internal bool WriteDict(string fileName)
        {
            lock (this)
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Create, store))
                    {
                        using (StreamWriter writer = new StreamWriter(isfs))
                        {
                            for (int ptr = 0; ptr < wordList.Count; ptr++)
                            {
                                writer.WriteLine(wordList[ptr]);
                            }
                            writer.Flush();
                            writer.Close();
                        }
                    }
                }

                return true;
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