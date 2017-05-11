using RadKey.Compounds;
using RadKey.Kanji;
using RadKey.Radicals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadKey
{
    static class SelectionInfoHandler
    {
        private static KanjiToRadicalDictionary kanjiToRadicalDictionary;

        private static KanjiDictionary kanjiDictionary;

        public static void SetKanjiToRadicalDictionary(KanjiToRadicalDictionary _kanjiToRadicalDictionary, KanjiDictionary _kanjiDictionary)
        {
            kanjiToRadicalDictionary = _kanjiToRadicalDictionary;
            kanjiDictionary = _kanjiDictionary;
        }

        // Shows info about the selected kanji in the message box.
        // TODO: Move this out of this class and into something more relevant.
        public static void ShowKanjiSelectionInfo(string toLookup, TextBox outputMessage, TextBox outputReading, TextBox outputMeaning)
        {
            KanjiData selected = kanjiDictionary.Lookup(toLookup);
            if (selected != null)
            {
                outputMessage.Text = string.Concat("Strokes: ", selected.Strokes().ToString(), " :: Freq: ", selected.Frequency().ToString(),
                            " :: Rads: ", kanjiToRadicalDictionary.Lookup(toLookup));
                outputReading.Text = string.Concat("On: ", selected.onReads, " :: Kun: ", selected.kunReads);
                outputMeaning.Text = selected.meanings;
            }
            else
            {
                outputMessage.Text = "No data found for the selected kanji.";
            }
        }

        public static void ShowCompoundSelectionInfo(Compound toDisplay, TextBox outputMessage, TextBox outputReading)
        {
            // NOTE: For compounds, the definition goes in the readingBox and the pronunciations go in the meaningBox.       
            // This also changes the font sizes in addition to updating the text.


            outputMessage.Text = toDisplay.pronunciation();
            outputReading.Text = "";
            int defcount = 1;
            foreach (string def in toDisplay.definition())
            {
                outputReading.Text = outputReading.Text
                    + defcount.ToString() + ". "
                    + def
                    + Environment.NewLine;
                defcount++;
            }
        }

        // Use something like this eventually, and move it somewhere else:
        /*
         *
            public static void ShowKanjiSelectionInfo(ListBox kanjiResults, TextBox outputMessage, TextBox outputReading, TextBox outputMeaning)
            {
                KanjiData selected = null;
                if(kanjiResults.Items.Count > 0)
                {
                    selected = KanjiDictionary.Lookup(toLookup);
                }
                
                if (selected != null)
                {
                    outputMessage.Text = string.Concat("Strokes: ", selected.strokes.ToString(), " :: Freq: ", selected.freq.ToString(),
                                " :: Rads: ", KanjiToRadicalDictionary.Lookup(toLookup));
                    outputReading.Text = string.Concat("On: ", selected.onReads, " :: Kun: ", selected.kunReads);
                    outputMeaning.Text = selected.meanings;
                }
                else
                {
                    outputMessage.Text = "No data found for the selected kanji.";
                }
            }
         */
    }
}
