using System.Collections.Generic;
using System.Linq;

namespace RadKey.Radicals
{
    public class RadkfileDictionary : IRadkFile
    {
        private Dictionary<string, string> _radToKanji;

        public RadkfileDictionary(string inputFilename)
        {
            _radToKanji = new Dictionary<string, string>();

            string[] radkfileLines = System.IO.File.ReadAllLines(@inputFilename, System.Text.Encoding.GetEncoding(20932)); // 20932 is the EUC-JP encoding.

            // REMOVE THE HARDCODING ON THESE AT SOME POINT.
            string currentRad = "";
            string kanjiSet = "";

            foreach (string entry in radkfileLines)
            {
                // Ignore blank lines/lines shorter than one character.
                if (entry.Length > 1)
                {
                    // Ignore lines commented out with #.
                    if (entry[0].ToString() != "#")
                    {
                        // If the line starts with $, we have a new radical.
                        if (entry[0].ToString() == "$")
                        {
                            // Write the old radical and kanji set to the dictionary.
                            _radToKanji.Add(currentRad, kanjiSet);
                            // Store the new radical.
                            currentRad = entry[2].ToString();
                            // Clear the kanji set.
                            kanjiSet = "";
                        }
                        // Otherwise, it's an existing list of kanjiine of kanji.
                        else
                        {
                            // Append it to the current kanjiset.
                            kanjiSet = string.Concat(kanjiSet, entry);
                        }
                    }
                }
            }
        }

        public string Lookup(string toLookup)
        {
            string results;
            _radToKanji.TryGetValue(toLookup, out results);

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
