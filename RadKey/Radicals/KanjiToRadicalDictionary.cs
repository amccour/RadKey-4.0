using System.Collections.Generic;
using System.Linq;

namespace RadKey.Radicals
{
    public class KanjiToRadicalDictionary : IKradFile
    {
        private static Dictionary<string, string> _kanjiToRad;

        public KanjiToRadicalDictionary(string inputFilename)
        {
            string[] kradfileLines = System.IO.File.ReadAllLines(@inputFilename, System.Text.Encoding.GetEncoding(20932)); // 20932 is the EUC-JP encoding.

            _kanjiToRad = new Dictionary<string, string>();

            string tKanji = "";
            string tRads = "";

            int line = 0;

            // Loop through the file to find the first entry. Non-entries are commented out with an #.
            tKanji = kradfileLines[0][0].ToString();
            while(tKanji == "#")
            {
                line++;
                tKanji = kradfileLines[line][0].ToString();                
            }

            foreach(string entry in kradfileLines)
            {
                // Ignore blank lines and those with only one character.
                if (entry.Length > 1)
                {
                    // Grab the character in the kanji position.
                    tKanji = entry[0].ToString();

                    // Ignore commented out lines (primarily the header portion).
                    if(tKanji != "#")
                    {
                        // Get the radicals. These start at position 4.
                        tRads = entry.Substring(4);
                        // Remove the spaces.
                        tRads = tRads.Replace(" ", "");
                        // Add the kanji info to the dictionary.
                        _kanjiToRad.Add(tKanji, tRads);

                    }
                }
            }
        }

        public string Lookup(string toLookup)
        {
            string results;
            _kanjiToRad.TryGetValue(toLookup, out results);

            if (results != null)
            {
                return results;
            }
            else
            {
                return "";
            }
        }
    }
}
