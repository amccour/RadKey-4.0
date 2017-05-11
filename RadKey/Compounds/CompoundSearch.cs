using RadKey.Radicals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RadKey.Compounds
{
    public class CompoundSearch
    {
        private IJMDictFile _compoundDictionary;
        private IKradFile _kanjiToRadicalDictionary;

        public CompoundSearch(IJMDictFile compoundDictionary, IKradFile kanjiToRadicalDictionary)
        {
            _compoundDictionary = compoundDictionary;
            _kanjiToRadicalDictionary = kanjiToRadicalDictionary;
        }

        private bool HasReadingContaining(string searchReading, List<string> wordReadings)
        {
            foreach (string rebReading in wordReadings)
            {
                if (rebReading.Contains(searchReading))
                {
                    return true;
                }
            }

            return false;

        }

        private bool HasReadingStartingWith(string searchReading, List<string> wordReadings)
        {
            foreach (string wordReading in wordReadings)
            {
                if (wordReading.StartsWith(searchReading))
                {
                    return true;
                }
            }

            return false;
        }

        // Build wordlists in cases where only kana was used as input.
        // Matches any words with reading starting with the provided input.
        private List<Compound> BuildWordListByReading(string searchReading)
        {
            List<Compound> matchingWords = new List<Compound>();
            List<Compound> allWords = _compoundDictionary.Lookup();

            foreach (Compound word in allWords)
            {
                if (HasReadingStartingWith(searchReading, word.Readings()))
                {
                    matchingWords.Add(word);
                }
            }

            return matchingWords;
        }

        private List<Compound> BuildMatchingWordList(string matchText, List<Compound> lastMatchingWordSet, int posInWord, bool textIsBracketed)
        {
            List<Compound> matchingWords = new List<Compound>();

            foreach (Compound word in lastMatchingWordSet)
            {
                // Case 0: Throw out words shorter than the input string. 
                if (word.ToString().Length >= posInWord + 1)
                {
                    // Case 1: See if the search string matches the word outright.
                    if (word.ToString()[posInWord].ToString() == matchText)
                    {
                        matchingWords.Add(word);
                    }
                    // Case 2: Bracketed text. Either radicals or hiragana.
                    else if (textIsBracketed)
                    {
                        bool wordMatches = true;
                        // Case 2a: String only contains hiragana. 
                        if (IsHiraganaOnly(matchText))
                        {
                            wordMatches = wordMatches && HasReadingContaining(matchText, word.Readings());
                        }
                        // Case 2b: Assume it's radicals (if it's a mix of radicals, kanji, and kana, the match'll fail safely).
                        else
                        {
                            foreach (char radical in matchText)
                            {
                                // Checking the kanjiToRad dictionary is needed to stop the system from trying to look up a kana or symbol.
                                wordMatches = wordMatches && KanjiContainsRadical(word.CharacterAt(posInWord), radical.ToString());
                                // Word is not a kanji at this position. Does not match.
                            }
                        }

                        if (wordMatches)
                        {
                            matchingWords.Add(word);
                        }
                    }
                    // Case 3: Wildcard.
                    else if (matchText == "*")
                    {
                        matchingWords.Add(word);
                    }

                    // Case 4: character is a loose radical. See if word[pos] contains that radical.
                    // Checking the kanjiToRad dictionary is needed to stop the system from trying to look up a kana or symbol.
                    else if (KanjiContainsRadical(word.CharacterAt(posInWord), matchText))
                    {
                        matchingWords.Add(word);
                    }
                }
            }

            return matchingWords;
        }

        private bool KanjiContainsRadical(char kanji, string radical)
        {
            return _kanjiToRadicalDictionary.Lookup(kanji.ToString()).Contains(radical);
        }

        public List<Compound> Search(string searchInput)
        {
            List<Compound> matchingWordList = new List<Compound>();

            // Case 1: A blank search string was sent. Return an empty match set.
            if (searchInput == "")
            {
                return new List<Compound> { };
            }

            // Case 2: Only unbracketed hiragana were sent. Search for words by reading.
            if (IsHiraganaOnly(searchInput))
            {
                matchingWordList = BuildWordListByReading(searchInput);
            }

            // Case 3: Regular search based on radicals + hiragana.
            else
            {
                matchingWordList = _compoundDictionary.Lookup();

                int wordPos = 0; // NOTE: The position of a character in a given compound can be different from the position in the search string in cases where the text is bracketed.
                int closingBracketPos = 0;

                for (int searchPos = 0; 
                    searchPos < searchInput.Length; 
                    searchPos++)
                {
                    // 1. If the search text is in a bracket, all text within that bracket should be considered as maching a single character in a potentially matching word.
                    if (searchInput[searchPos] == '[')
                    {                        
                        if (searchPos + 1 != searchInput.Length) // Checks that the string doesn't end in a opening bracket.
                        {
                            closingBracketPos = FindClosingBracket(searchInput, searchPos);
                            matchingWordList = BuildMatchingWordList(BracketedTextAt(searchInput, searchPos, closingBracketPos), matchingWordList, wordPos, true);
                            searchPos = closingBracketPos;
                            wordPos++;
                        }
                    }
                    else
                    {
                        matchingWordList = BuildMatchingWordList(CharacterAt(searchInput, searchPos), matchingWordList, wordPos, false);
                        wordPos++;
                    }
                }
            }


            return matchingWordList;
        }

        private static string CharacterAt(string searchInput, int searchPos)
        {
            return searchInput[searchPos].ToString();
        }

        private static string BracketedTextAt(string searchInput, int openingBracketPos, int closingBracketPos)
        {
            return searchInput.Substring(openingBracketPos + 1, closingBracketPos - (openingBracketPos + 1));
        }

        private static int FindClosingBracket(string searchInput, int startPos)
        {
            int closingBracketPosition = searchInput.IndexOf(']', startPos);

            // If a closing bracket wasn't provided, just go to the end of the line.
            if (closingBracketPosition < 0)
            {
                closingBracketPosition = searchInput.Length;
            }

            return closingBracketPosition;
        }

        private static bool IsHiraganaOnly(string input)
        {
            return new Regex("[\\p{IsHiragana}]").IsMatch(input) &&
                            !new Regex("[\\p{IsCJKUnifiedIdeographs}\\p{IsKatakana}\\p{IsBasicLatin}]").IsMatch(input);
        }
    }
}
