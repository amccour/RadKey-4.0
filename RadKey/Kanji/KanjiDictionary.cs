using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadKey.Kanji
{
    public class KanjiDictionary : IKanjidic
    {
        private Dictionary<string, KanjiData> _kanjiDataDictionary = null;

        // NOTE: Currently only works with kanjidic, and not the XML-based kanjidic2.
        public KanjiDictionary(string inputFilename)
        {
            _kanjiDataDictionary = new Dictionary<string, KanjiData>();
            
            string[] kanjidicLines = System.IO.File.ReadAllLines(@inputFilename, System.Text.Encoding.GetEncoding(20932)); // 20932 is the EUC-JP encoding.

            string buffer = "";
            string tKanji = "";
            string[] tokens;

            int meaningStart = -1;
            string onReads;
            string kunReads;

            int tStrokes = 0;
            int tFreq = 0;
            int tGrade = 0;

            // Parse the file, starting at line 2. Line 1 is a header.
            foreach(string entry in kanjidicLines)
            {
                // Ignore lines commented out with a #.
                if(entry[0].ToString() != "#")
                { 
                    // Load and tokenize the current string.
                    onReads = "";
                    kunReads = "";
                    buffer = entry.Trim();
                    tokens = buffer.Split(' ');
                    // Get the kanji.
                    tKanji = tokens[0];
                    // Ideally, Freq always comes after Stroke in kanjidic. However, I can't actually prove this.
                    // So, I need to search for them separately.
                    // Set tStrokes to 99 in case the stroke count is undefined.
                    tStrokes = 99;
                    // I think it is safe to assume that strokes will be within the first ten tokens. Same for frequency. 
                    for (int y = 1; y < 11; y++)
                    {
                        // Found the stroke token (the stroken).
                        if (tokens[y][0] == 'S')
                        {
                            tStrokes = Int32.Parse(tokens[y].Substring(1));
                            break;
                        }
                    }
                    // Same approach for frequency.
                    tFreq = 9999;
                    for (int y = 1; y < tokens.Count(); y++)
                    {
                        // Found the stroke token (the stroken).
                        if (tokens[y][0] == 'F')
                        {
                            tFreq = Int32.Parse(tokens[y].Substring(1));
                            break;
                        }
                    }

                    // Same approach for grade.
                    tGrade = 99;
                    for (int y = 1; y < tokens.Count(); y++)
                    {
                        // Found the stroke token (the stroken).
                        if (tokens[y][0] == 'G')
                        {
                            tGrade = Int32.Parse(tokens[y].Substring(1));
                            break;
                        }
                    }

                    // Find the ON- or KUN- readings in the tokens.
                    foreach (string element in tokens)
                    {
                        // See if the strting chracter is in the katakana range. If so, it's on.
                        if ((element[0] >= 0x30A0) && (element[0] <= 0x30FF))
                        {
                            onReads = string.Concat(element, " / ", onReads);
                        }
                        // See if the strting chracter is in the hiragana range. If so, it's kun.
                        else if ((element[0] >= 0x3040) && (element[0] <= 0x309F))
                        {
                            kunReads = string.Concat(element, " / ", kunReads);
                        }
                    }

                    // Find the meanings start position.
                    meaningStart = buffer.IndexOf('{');

                    // Add the kanji info to the dictionary.
                    _kanjiDataDictionary.Add(tKanji, new KanjiData(tKanji, tStrokes, tFreq, tGrade, onReads, kunReads, buffer.Substring(meaningStart, buffer.Length - meaningStart)));
                }
            }
        }

        public KanjiData Lookup(string toLookup)
        {
            KanjiData resultKanji;
            _kanjiDataDictionary.TryGetValue(toLookup, out resultKanji);

            return resultKanji;
        }
    }
}
