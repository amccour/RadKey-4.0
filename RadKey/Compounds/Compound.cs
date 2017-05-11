using System;
using System.Collections.Generic;
using RadKey.Kanji;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadKey.Compounds
{
    public class Compound : IComparable
    {
        private string _keb;
        private List<string> _gloss;
        private List<string> _reb;

        // A cleaner alternative would be to store an array of KanjiData items/stroke and freq tuples on each compound but that would probably be bad for memory usage.
        private static IKanjidic _kanjiDictionary;

        public static void SetKanjiDictionary(IKanjidic kanjiDictionary)
        {
            _kanjiDictionary = kanjiDictionary;
        }

        public Compound(string keb, List<string> gloss, List<string> reb)
        {
            _keb = keb;
            _gloss = gloss;
            _reb = reb;
        }

        public override string ToString()
        {
            return _keb;
        }

        public List<string> definition()
        {
            return _gloss;
        }

        public string pronunciation()
        {
            string rebString;
            if (_reb.Count == 1)
            {
                rebString = _reb[0];
            }
            else
            {
                rebString = _reb[0];
                for (int x = 1; x < _reb.Count; x++)
                {
                    rebString = rebString + "; " + _reb[x];
                }
            }
            return rebString;
        }

        public void addDefinition(string newDef)
        {
            _gloss.Add(newDef);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is Compound)
            {
                Compound otherCompound = obj as Compound;

                string thisCompoundString = this.ToString();
                string otherCompoundString = otherCompound.ToString();

                KanjiData thisKanji = null;
                KanjiData otherKanji = null;

                // Check 0: Are they the same word?
                if (thisCompoundString == otherCompoundString)
                {
                    return string.Compare(thisCompoundString, otherCompoundString);
                }

                // Check 1: Shorter strings come first.
                if (thisCompoundString.Length != otherCompoundString.Length)
                {
                    return thisCompoundString.Length.CompareTo(otherCompoundString.Length);
                }
                else
                {
                    // Find the first character that doesn't match.
                    for (int x = 0; x < thisCompoundString.Length; x++)
                    {
                        if (thisCompoundString[x] != otherCompoundString[x])
                        {
                            // Check if it's in the kanjiDictionary. If not, assume it's a kana. If it is a kanji, do kanji comparison.                                                               
                            thisKanji = _kanjiDictionary.Lookup(thisCompoundString[x].ToString());
                            otherKanji = _kanjiDictionary.Lookup(otherCompoundString[x].ToString());

                            if (thisKanji != null && otherKanji != null)
                            {
                                return thisKanji.CompareTo(otherKanji);
                            }
                            else
                            {
                                return string.Compare(thisCompoundString, otherCompoundString);
                            }
                        }
                    }

                    return string.Compare(thisCompoundString, otherCompoundString);
                }

            }
            throw new ArgumentException("Object is not a User");
        }

        public bool HasReading(string reading)
        {
            if (_reb.Contains(reading))
            {
                return true;
            }

            return false;

        }

        public bool HasReadingStartingWith(string reading)
        {
            foreach (string rebReading in _reb)
            {
                if (rebReading.StartsWith(reading))
                {
                    return true;
                }
            }

            return false;

        }

        public bool HasReadingContaining(string reading)
        {
            foreach (string rebReading in _reb)
            {
                if (rebReading.Contains(reading))
                {
                    return true;
                }
            }

            return false;

        }

        public string Word()
        {
            return _keb;
        }

        public List<string> Readings()
        {
            return _reb;
        }
    }
}
