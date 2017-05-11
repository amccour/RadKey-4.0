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
                            foreach (char rad in matchText)
                            {
                                // Checking the kanjiToRad dictionary is needed to stop the system from trying to look up a kana or symbol.
                                wordMatches = wordMatches && _kanjiToRadicalDictionary.Lookup(word.ToString()[posInWord].ToString()).Contains(rad);
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
                    else if (_kanjiToRadicalDictionary.Lookup(word.ToString()[posInWord].ToString()).Contains(matchText))
                    {
                        matchingWords.Add(word);
                    }
                }
            }

            return matchingWords;
        }

        public List<Compound> Search(string searchInput)
        {
            List<Compound> matchingWordList = new List<Compound>();

            if (searchInput == "")
            {
                return new List<Compound> { };
            }
            // Special case for searching by reading.
            if (IsHiraganaOnly(searchInput))
            {
                matchingWordList = BuildWordListByReading(searchInput);
            }
            else
            {
                // Standard multi-radical/multi-kanji wordlist generation
                matchingWordList = _compoundDictionary.Lookup();

                int wordPos = 0; // NOTE: The position of a character in a given compound can be different from the position in the search string in cases where the text is bracketed.
                int closingBracketPos = 0;

                for (int searchPos = 0; 
                    searchPos < searchInput.Length; 
                    searchPos++)
                {
                    // Bracketed text found. Get a substring.
                    if (searchInput[searchPos] == '[')
                    {
                        // Check that the string doesn't end in a opening bracket.
                        if (searchPos + 1 != searchInput.Length)
                        {
                            closingBracketPos = FindClosingBracket(searchInput, searchPos);

                            matchingWordList = BuildMatchingWordList(searchInput.Substring(searchPos + 1, closingBracketPos - (searchPos + 1)), matchingWordList, wordPos, true);
                            searchPos = closingBracketPos;
                            wordPos++;
                        }
                    }
                    else
                    {
                        matchingWordList = BuildMatchingWordList(searchInput[searchPos].ToString(), matchingWordList, wordPos, false);
                        wordPos++;
                    }
                }
            }


            return matchingWordList;
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
