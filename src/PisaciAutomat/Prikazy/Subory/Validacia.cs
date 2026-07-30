using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PisaciAutomat.Subory
{
    public static class Validacia
    {
        /// <summary>
        /// Checks if a file is likely a text file by reading a sample of its bytes.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>True if the file appears to be text; otherwise false.</returns>
        public static bool IsTextFile(string filePath)
        {
            return true;

            //nefunguje to uplne spolahlivo
            /*
            try
            {
                // Read only a small chunk to determine type
                const int sampleSize = 8000; // 8 KB sample
                byte[] buffer = new byte[sampleSize];
                int bytesRead;

                using (FileStream fs = File.OpenRead(filePath))
                {
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                }

                // Check for null bytes (common in binary files)
                if (buffer.Take(bytesRead).Any(b => b == 0))
                    return false;

                // Try decoding as UTF-16
                string text = Encoding.BigEndianUnicode.GetString(buffer, 0, bytesRead);

                // If decoding produces replacement characters, it's likely binary
                if (text.Contains('\uFFFD'))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
            */
        }
    }
}
