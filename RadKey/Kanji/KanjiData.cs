using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RadKey.Kanji
{
    public class KanjiData : IComparable
    {
        // The actual kanji name is stored in the dictionary's key.
        private string kanjiString;
        private int strokes;
        private int frequency;
        private int grade;
        public string onReads;
        public string kunReads;
        public string meanings;
        private static bool sortByFrequencyOnly = false;

        public KanjiData(string _kanjiString, int _strokes, int _frequency, int _grade, string _onReads, string _kunReads, string _meanings)
        {
            kanjiString = _kanjiString;
            strokes = _strokes;
            frequency = _frequency;
            grade = _grade;
            onReads = _onReads;
            kunReads = _kunReads;
            meanings = _meanings;
        }

        public int Strokes()
        {
            return strokes;
        }

        public int Frequency()
        {
            return frequency;
        }

        public int Grade()
        {
            return grade;
        }

        public static class SortingMethod
        {
            public const bool FrequencyOnly = true;
            public const bool StrokesAndFrequency = false;
        }

        public static bool ToggleSortingMethod()
        {
            sortByFrequencyOnly = sortByFrequencyOnly ^ true;
            return sortByFrequencyOnly;
        }


        public override string ToString()
        {
            return kanjiString;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is KanjiData)
            {
                KanjiData otherKanjiData = obj as KanjiData;

                // Sort differently depending on what sortByFreq is set to.
                if (sortByFrequencyOnly == false)
                {

                    // If the stroke counts are different, compare on that; otherwise, if the stroke counts are the same, compare on frequency.
                    if (this.strokes.CompareTo(otherKanjiData.strokes) == 0)
                    {
                        return this.frequency.CompareTo(otherKanjiData.frequency);
                    }
                    return this.strokes.CompareTo(otherKanjiData.strokes);
                }
                else
                {
                    // Sort by hex value if freq == 9999. 
                    if(this.frequency == 9999 && otherKanjiData.frequency == 9999)
                    {
                        return string.Compare(this.ToString(), otherKanjiData.ToString());
                    }
                    else
                    { 
                        return this.frequency.CompareTo(otherKanjiData.frequency);
                    }
                }
            }
            throw new ArgumentException("Object is not a User");
        }
    }


    
}

