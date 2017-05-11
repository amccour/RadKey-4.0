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
    public class KanjiSearch
    {
        private IRadkFile _radicaltoKanjiDictionary = null;

        public KanjiSearch(RadkfileDictionary radicaltoKanjiDictionary)
        {
            _radicaltoKanjiDictionary = radicaltoKanjiDictionary;
        }

        // Get the first pass list of all kanji matching any of the input radicals.
        private List<string> BuildMatchingKanjiList(string inputRadicals)
        {
            List<string> matchList = new List<string>();

            string result = "";

            // Iterate over the radical input text string, and get all kanji matching any of those radicals.
            for (int x = 0; x < inputRadicals.Length; x++)
            {
                result = _radicaltoKanjiDictionary.Lookup(inputRadicals[x].ToString());
                // Make sure the input character exists in the radToKanji map.
                if (result != "")
                {
                    matchList.Add(result);
                }
            }

            return matchList;
        }

        // Filter the potentially matching kanji to only include those matching ALL of the radicals.
        private string FilterMatchingKanjiList(List<string> matchList)
        {
            string filteredResults = matchList.First();

            // If we had more than one radical provided, do intersects on all of the matching kanji sets.
            if (matchList.Count > 1)
            {
                foreach (string element in matchList)
                {
                    filteredResults = new string(filteredResults.Intersect(element).ToArray());
                }
            }

            return filteredResults;
        }

        // Populates the kanji result list. Possibly clean this up in the future if I really need to.
        public List<char> Search(string inputRadicals)
        {            
            // Get a list of all kanji matching all of the input radicals.
            List<string> matchList = BuildMatchingKanjiList(inputRadicals);

            if(matchList.Count == 0)
            {
                return null;
            }
            else
            {
                return FilterMatchingKanjiList(matchList).ToList();
            }
        }
    }

}




// Updated functions. Put these back in later.
// Populates the kanji result list. Possibly clean this up in the future if I really need to.
/*private static Tuple<KanjiSearchErrors, List<string>> __GetMatchingKanjiList(string inputRadicals)
{
    // Get a list of all kanji matching all of the input radicals.
    List<string> matchList = BuildMatchingKanjiList(inputRadicals);

    if (matchList.Count == 0)
    {
        // Box isn't empty; treat this like quick search.   
        if (lastResultFocus > -1)
        {
            resultList.Focus();
            resultList.SelectedIndex = lastResultFocus;
            return;
        }

        return new Tuple<KanjiSearchErrors, List<string>>(KanjiSearchErrors.NO_SEARCHABLE_RADICALS, matchList);
    }
    else
    {
        string filteredResults = filterKanji(matchList);

        // Convert result string to a list of kanji.
        // First, make sure we actually HAVE matching kanji.
        if (filteredResults == "")
        {                                        
            return new Tuple<KanjiSearchErrors, List<string>>(KanjiSearchErrors.NO_MATCHING_KANJI, matchList);
        }
        else
        {
            List<KanjiData> kanjiDataList = buildKanjiDataList(filteredResults);

            // If a stroke count was given, filter by that.
            // Make sure there are still kanji left.
            if (kanjiDataList.Count == 0)
            {
                return new Tuple<KanjiSearchErrors, List<string>>(KanjiSearchErrors.NO_MATCHING_KANJI, matchList);
            }
        }
    }
}*/
