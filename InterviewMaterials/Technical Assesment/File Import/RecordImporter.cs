using System;
using System.IO;
using Technical_Assesment.Value_Objects;

namespace Technical_Assesment.File_Import
{
    public class RecordImporter : IRecordImporter
    {

        public BTree<T> ImportRecords<T>(string filePath, char token, bool skipHeader, ImportBuilder<T> voBuilder, BTree<T> seed)
        {

            if (string.IsNullOrEmpty(filePath))
            {
                throw new NullReferenceException("filePath");
            }

            string[] lines = File.ReadAllLines(filePath);

            // ensure we skip the header ;)
            if (skipHeader)
            {
                lines[0] = string.Empty;
            }

            foreach (string line in lines)
            {
                string[] parts = line.Split(token); 


                if (voBuilder.TokenCnt() == parts.Length)
                {
                    T obj = voBuilder.FromImportTokens(parts);
                    seed.Add(obj);
                }

                
            }

            return seed;
        }
    }
}
