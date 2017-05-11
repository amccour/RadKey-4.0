using RadKey.Radicals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace RadKey
{
    public static class TextConverter
    {
        private static KanjiToRadicalDictionary kanjiToRadicalDictionary = null;

        public static void SetKanjiToRadicalDictionary(KanjiToRadicalDictionary _kanjiToRadicalDictionary)
        {
            kanjiToRadicalDictionary = _kanjiToRadicalDictionary;
        }

        /// <summary>
        /// Given a string containing radicals and English text, find the first occurence of English text, 
        /// look up a radical associated with it, replace the English text with the radical, and return a Tuple
        /// containing the new string and position where the text was updated.
        /// </summary>
        /// <param name="input">Test.</param>
        /// <param name="originalStart"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        private static Tuple<string, int> ConvertToRadical(string inputText, int originalSelectionStart, out bool success)
        {
            // If the input is empty, return it as is.
            if (inputText == "")
            {
                success = false;
                return Tuple.Create(inputText, 0);
            }

            // Parse the input string to find the first occurence of English text, where English text is
            // defined as text NOT in the dictionary.
            int start = -1;
            int end = -1;

            // First, get the start position.
            for (int x = 0; x < inputText.Length; x++)
            {
                // See if the character at the current position is English text or *.
                if (!Regex.IsMatch(inputText[x].ToString(), RegexPatterns.isSpecialPattern))
                {
                    if (Regex.IsMatch(inputText[x].ToString(), RegexPatterns.isLatinPattern))
                    {
                        start = x;
                        end = x; // End is set here too. This will stop one-character radical names from being ignored.
                        break;
                    }
                }
            }

            // If the user only sent radicals/reserved characters, start will still be -1.
            // Return the original start position.
            if (start == -1)
            {
                success = false;
                return Tuple.Create(inputText, originalSelectionStart);
            }

            // Next, get the end position.
            // NOTE TO SELF: Is there a more elegant way to do this?
            for (end = start; end < inputText.Length; end++)
            {

                if (Regex.IsMatch(inputText[end].ToString(), RegexPatterns.isNotLatinPattern))
                {
                    // The charcter at position 'end' is in the dictionary. Return one past the last valid English letter.
                    break;
                }

                else if (Regex.IsMatch(inputText[end].ToString(), RegexPatterns.isSpecialPattern))
                {
                    break;
                }

            }

            // Store the substring we want to use to look up the radical.
            // Need to add 1 here to get the correct length.
            string radicalLookup = inputText.Substring(start, end - start);

            // Attempt to look up that radical. This returns "" if nothing is found.
            string radical = NameToRadicalDictionary.Lookup(radicalLookup);

            // Set success flag indicating whether the radical was found.
            if (radical != "")
            {
                success = true;
            }
            else
            {
                success = false;
            }


            // Then, insert it into original string and return that.
            // If a radical was not found, this just removes the input text.
            string temp = string.Concat(inputText.Substring(0, start), radical);

            // If end = the last character of input, we need to handle it differently.
            int newSelectionStart;


            // If the cursor was before the characters being replaced, no need to change it.
            if (originalSelectionStart < start)
            {
                newSelectionStart = originalSelectionStart;
            }
            else
            {
                // Explanation: originalSelectionStart-(end-start) accounts for:
                // rad|* <- rad starts at 0, ends at 2, the cursor is at 3.
                // rad*| <- rad starts at 0, ends at 2, the cursor is at 4.
                // *rad*| <- rad starts at 1, ends at 3, the cursor is at 5.                
                newSelectionStart = (originalSelectionStart-(end-start)) + radical.Length;
                // When no matching radical is found, newSelectionStart can go negative. Move it back to the starting position of the replaced text.
                if (newSelectionStart < 0)
                {
                    newSelectionStart = start;
                }
            }

            if (end == inputText.Length)
            {
                return Tuple.Create(temp, newSelectionStart);
            }
            else
            {
                return Tuple.Create(string.Concat(temp, inputText.Substring(end, inputText.Length - end)), newSelectionStart);
            }

        }

        // Overloaded variant that doesn't need to track success/failure.
        private static Tuple<string, int> ConvertToRadical(string inputText, int originalSelectionStart)
        {
            bool ignore;
            return ConvertToRadical(inputText, originalSelectionStart, out ignore);
        }

        // Convert a string to hiragana while preserving the intended cursor position.
        private static Tuple<string, int> ConvertToKanaWithCursorPos(string inputText, int originalSelectionStart, string cutoffRegex)
        {
            while (originalSelectionStart > 0)
            {
                if (Regex.IsMatch(inputText[originalSelectionStart - 1].ToString(), cutoffRegex))
                {
                    break;
                }
                originalSelectionStart--;
            }

            // Break this into two parts to see where the cursor needs to go.
            string first = ConvertToKana(inputText.Substring(0, originalSelectionStart));
            string rest = ConvertToKana(inputText.Substring(originalSelectionStart));
            return new Tuple<string, int>(first + rest, first.Length);
        }

        // Attempt to convert to radical. Converts to kana instead on fail.
        private static Tuple<string, int> ConvertToRadicalOrKana(string inputText, int originalSelectionStart, string cutoffRegex)
        {
            // input: String that radical or kana conversions are being done on.
            // kanaPattern: Kana conversion regex.
            // pos: SelectionStart.

            // Step 1: Attempt radical conversion.
            bool radicalUpdateSuccessFlag;
            Tuple<string, int> result = ConvertToRadical(inputText, originalSelectionStart, out radicalUpdateSuccessFlag);
            if (!radicalUpdateSuccessFlag)
            {
                // Step 2: Radical not found; do kana conversion.
                result = TextConverter.ConvertToKanaWithCursorPos(inputText,                    
                    originalSelectionStart,
                    cutoffRegex);
            }

            return result;
        }

        private static string ConvertToKana(string temp)
        {
            // Good candidate for conversion to stringbuilder.

            // Added tiny tsu for double characters (this has to happen first!).
            temp = temp.Replace("kk", "っk"); temp = temp.Replace("tt", "っt"); temp = temp.Replace("ss", "っs"); // Should also pick up sshi.
            temp = temp.Replace("gg", "っg"); temp = temp.Replace("dd", "っd"); temp = temp.Replace("zz", "っz");
            temp = temp.Replace("nn", "んn"); temp = temp.Replace("rr", "っr"); temp = temp.Replace("jj", "っj");
            temp = temp.Replace("pp", "っp"); temp = temp.Replace("bb", "っb"); temp = temp.Replace("cc", "っc");

            // Replace glides (this needs to happen before other y-line replacements)
            temp = temp.Replace("ky", "き/y"); temp = temp.Replace("gy", "ぎ/y");
            temp = temp.Replace("chi", "ち"); temp = temp.Replace("di", "ぢ"); temp = temp.Replace("ch", "ち/y"); temp = temp.Replace("dy", "ぢ/y");
            temp = temp.Replace("shi", "し"); temp = temp.Replace("ji", "じ"); temp = temp.Replace("sh", "し/y"); temp = temp.Replace("jy", "じ/y");
            temp = temp.Replace("ja", "じ/ya"); temp = temp.Replace("ju", "じ/yu"); temp = temp.Replace("jo", "じ/yo");
            temp = temp.Replace("ny", "に/y"); temp = temp.Replace("my", "み/y");
            temp = temp.Replace("hy", "ひ/y"); temp = temp.Replace("by", "び/y"); temp = temp.Replace("py", "ぴ/y");
            temp = temp.Replace("ry", "り/y");

            // Replace actual glide characters now.
            temp = temp.Replace("/ya", "ゃ"); temp = temp.Replace("/yu", "ゅ"); temp = temp.Replace("/yo", "ょ");

            // Replace tsu. Moving it here because if you replace su first, this breaks :P
            temp = temp.Replace("tsu", "つ"); temp = temp.Replace("dzu", "づ");

            // Replace consonant ones.
            temp = temp.Replace("ka", "か"); temp = temp.Replace("ki", "き"); temp = temp.Replace("ku", "く"); temp = temp.Replace("ke", "け"); temp = temp.Replace("ko", "こ");
            temp = temp.Replace("ga", "が"); temp = temp.Replace("gi", "ぎ"); temp = temp.Replace("gu", "ぐ"); temp = temp.Replace("ge", "げ"); temp = temp.Replace("go", "ご");
            temp = temp.Replace("sa", "さ"); temp = temp.Replace("su", "す"); temp = temp.Replace("se", "せ"); temp = temp.Replace("so", "そ");
            temp = temp.Replace("za", "ざ"); temp = temp.Replace("zu", "ず"); temp = temp.Replace("ze", "ぜ"); temp = temp.Replace("zo", "ぞ");
            temp = temp.Replace("ta", "た"); temp = temp.Replace("chi", "ち"); temp = temp.Replace("te", "て"); temp = temp.Replace("to", "と");
            temp = temp.Replace("da", "だ"); temp = temp.Replace("di", "ぢ"); temp = temp.Replace("de", "で"); temp = temp.Replace("do", "ど");
            temp = temp.Replace("na", "な"); temp = temp.Replace("ni", "に"); temp = temp.Replace("nu", "ぬ"); temp = temp.Replace("ne", "ね"); temp = temp.Replace("no", "の");
            temp = temp.Replace("ma", "ま"); temp = temp.Replace("mi", "み"); temp = temp.Replace("mu", "む"); temp = temp.Replace("me", "め"); temp = temp.Replace("mo", "も");
            temp = temp.Replace("ha", "は"); temp = temp.Replace("hi", "ひ"); temp = temp.Replace("fu", "ふ"); temp = temp.Replace("he", "へ"); temp = temp.Replace("ho", "ほ");
            temp = temp.Replace("ba", "ば"); temp = temp.Replace("bi", "び"); temp = temp.Replace("bu", "ぶ"); temp = temp.Replace("be", "べ"); temp = temp.Replace("bo", "ぼ");
            temp = temp.Replace("pa", "ぱ"); temp = temp.Replace("pi", "ぴ"); temp = temp.Replace("pu", "ぷ"); temp = temp.Replace("pe", "ぺ"); temp = temp.Replace("po", "ぽ");
            temp = temp.Replace("ra", "ら"); temp = temp.Replace("ri", "り"); temp = temp.Replace("ru", "る"); temp = temp.Replace("re", "れ"); temp = temp.Replace("ro", "ろ");
            temp = temp.Replace("ya", "や"); temp = temp.Replace("yu", "ゆ"); temp = temp.Replace("yo", "よ");
            temp = temp.Replace("wa", "わ"); temp = temp.Replace("wo", "を");

            // Get any remaining loose n's.
            temp = temp.Replace("n", "ん");

            // Replace vowels (this has to happen VERY near the end, after every other full syllable has been replaced!)
            temp = temp.Replace("a", "あ"); temp = temp.Replace("i", "い"); temp = temp.Replace("u", "う"); temp = temp.Replace("e", "え"); temp = temp.Replace("o", "お");

            // Replace any stray ts's with a small tsu. Needs to happen after normal tsu-line replacement.
            temp = temp.Replace("ts", "っ");

            // Katakana section.
            temp = temp.Replace("KK", "ッK"); temp = temp.Replace("TT", "ッT"); temp = temp.Replace("SS", "ッS"); // Should also pick up sshi.
            temp = temp.Replace("GG", "ッG"); temp = temp.Replace("DD", "ッD"); temp = temp.Replace("ZZ", "ッZ");
            temp = temp.Replace("NN", "ンN"); temp = temp.Replace("RR", "ッR"); temp = temp.Replace("JJ", "ッJ");
            temp = temp.Replace("PP", "ッP"); temp = temp.Replace("BB", "ッB"); temp = temp.Replace("CC", "ッC");
            temp = temp.Replace("FF", "ッF"); // Katakana apparently allows this.

            // Replace glides (this needs to happen before other y-line replacements)
            temp = temp.Replace("KY", "キ/Y"); temp = temp.Replace("GY", "ギ/Y");
            temp = temp.Replace("CHI", "チ"); temp = temp.Replace("DI", "ディ"); temp = temp.Replace("CH", "チ/Y"); temp = temp.Replace("DY", "ヂ/Y");
            temp = temp.Replace("CHE", "チェ");
            temp = temp.Replace("SHI", "シ"); temp = temp.Replace("JI", "ジ"); temp = temp.Replace("SH", "シ/Y"); temp = temp.Replace("JY", "ジ/Y");
            temp = temp.Replace("SHE", "シェ");
            temp = temp.Replace("JA", "ジ/YA"); temp = temp.Replace("JU", "ジ/YU"); temp = temp.Replace("JO", "ジ/YO");
            temp = temp.Replace("JE", "ジェ");
            temp = temp.Replace("NY", "ニ/Y"); temp = temp.Replace("MY", "ミ/Y");
            temp = temp.Replace("HY", "ヒ/Y"); temp = temp.Replace("BY", "ビ/Y"); temp = temp.Replace("PY", "ピ/Y");
            temp = temp.Replace("RY", "リ/Y");
            temp = temp.Replace("FY", "フ/Y"); temp = temp.Replace("VY", "ヴ/Y");
            temp = temp.Replace("TY", "テ/Y");
            temp = temp.Replace("KW", "ク/W"); temp = temp.Replace("GW", "グ/W");

            // Replace actual glide characters now.
            temp = temp.Replace("/YA", "ャ"); temp = temp.Replace("/YU", "ュ"); temp = temp.Replace("/YO", "ョ");
            temp = temp.Replace("/YE", "ェ"); temp = temp.Replace("/YI", "ィ");
            temp = temp.Replace("/WA", "ヮ"); temp = temp.Replace("/WI", "ィ"); temp = temp.Replace("/WE", "ェ"); temp = temp.Replace("/WO", "ォ");

            // Replace tsu. Moving it here because if you replace su first, this breaks :P
            temp = temp.Replace("TSU", "ツ"); temp = temp.Replace("DZU", "ヅ");
            temp = temp.Replace("TSA", "ツァ"); temp = temp.Replace("TSI", "ツィ"); temp = temp.Replace("TSE", "ツェ"); temp = temp.Replace("TSO", "ツォ");

            // Replace consonant ones.
            temp = temp.Replace("KA", "カ"); temp = temp.Replace("KI", "キ"); temp = temp.Replace("KU", "ク"); temp = temp.Replace("KE", "ケ"); temp = temp.Replace("KO", "コ");
            temp = temp.Replace("GA", "ガ"); temp = temp.Replace("GI", "ギ"); temp = temp.Replace("GU", "グ"); temp = temp.Replace("GE", "ゲ"); temp = temp.Replace("GO", "ゴ");
            temp = temp.Replace("SA", "サ"); temp = temp.Replace("SU", "ス"); temp = temp.Replace("SE", "セ"); temp = temp.Replace("SO", "ソ");
            temp = temp.Replace("ZA", "ザ"); temp = temp.Replace("ZU", "ズ"); temp = temp.Replace("ZE", "ゼ"); temp = temp.Replace("ZO", "ゾ");
            temp = temp.Replace("TA", "タ"); temp = temp.Replace("TE", "テ"); temp = temp.Replace("TO", "ト");
            temp = temp.Replace("DA", "ダ"); temp = temp.Replace("DI", "ヂ"); temp = temp.Replace("DE", "デ"); temp = temp.Replace("DO", "ド");
            temp = temp.Replace("NA", "ナ"); temp = temp.Replace("NI", "ニ"); temp = temp.Replace("NU", "ヌ"); temp = temp.Replace("NE", "ネ"); temp = temp.Replace("NO", "ノ");
            temp = temp.Replace("MA", "マ"); temp = temp.Replace("MI", "ミ"); temp = temp.Replace("MU", "ム"); temp = temp.Replace("ME", "メ"); temp = temp.Replace("MO", "モ");
            temp = temp.Replace("HA", "ハ"); temp = temp.Replace("HI", "ヒ"); temp = temp.Replace("FU", "フ"); temp = temp.Replace("HE", "ヘ"); temp = temp.Replace("HO", "ホ");
            temp = temp.Replace("BA", "バ"); temp = temp.Replace("BI", "ビ"); temp = temp.Replace("BU", "ブ"); temp = temp.Replace("BE", "ベ"); temp = temp.Replace("BO", "ボ");
            temp = temp.Replace("PA", "パ"); temp = temp.Replace("PI", "ピ"); temp = temp.Replace("PU", "プ"); temp = temp.Replace("PE", "ペ"); temp = temp.Replace("PO", "ポ");
            temp = temp.Replace("RA", "ラ"); temp = temp.Replace("RI", "リ"); temp = temp.Replace("RU", "ル"); temp = temp.Replace("RE", "レ"); temp = temp.Replace("RO", "ロ");
            temp = temp.Replace("YA", "ヤ"); temp = temp.Replace("YU", "ユ"); temp = temp.Replace("YO", "ヨ");
            temp = temp.Replace("WA", "ワ"); temp = temp.Replace("WO", "ウォ"); temp = temp.Replace("WI", "ウィ"); temp = temp.Replace("WE", "ウェ");
            temp = temp.Replace("VA", "ヴァ"); temp = temp.Replace("VI", "ヴィ"); temp = temp.Replace("VU", "ヴ"); temp = temp.Replace("VE", "ヴェ"); temp = temp.Replace("VO", "ヴォ");
            temp = temp.Replace("FA", "ファ"); temp = temp.Replace("FI", "フィ"); temp = temp.Replace("FE", "フェ"); temp = temp.Replace("FO", "フォ");
            temp = temp.Replace("TI", "ティ"); temp = temp.Replace("TU", "トゥ"); temp = temp.Replace("DU", "ドゥ");


            // Get any remaining loose n's.
            temp = temp.Replace("N", "ン");

            // Replace vowels (this has to happen VERY near the end, after every other full syllable has been replaced!)
            temp = temp.Replace("A", "ア"); temp = temp.Replace("I", "イ"); temp = temp.Replace("U", "ウ"); temp = temp.Replace("E", "エ"); temp = temp.Replace("O", "オ");
            temp = temp.Replace("-", "ー");

            // Replace any stray ts's with a small tsu. Needs to happen after normal tsu-line replacement.
            temp = temp.Replace("TS", "ッ");

            return temp;
        }        




        public static void ConvertToRadical(TextBox toUpdate)
        {
            Tuple<string, int> result = ConvertToRadical(toUpdate.Text, toUpdate.SelectionStart);
            toUpdate.Text = result.Item1;
            toUpdate.SelectionStart = result.Item2;
        }

        public static void ConvertToKana(TextBox toUpdate)
        {
            Tuple<string, int> result = ConvertToKanaWithCursorPos(toUpdate.Text, toUpdate.SelectionStart, RegexPatterns.MainKanaConversionPattern);
            toUpdate.Text = result.Item1;
            toUpdate.SelectionStart = result.Item2;
        }

        public static void ConvertToRadicalOrKana(TextBox toUpdate)
        {
            Tuple<string, int> result = ConvertToRadicalOrKana(toUpdate.Text, toUpdate.SelectionStart, RegexPatterns.MainKanaConversionPattern);
            toUpdate.Text = result.Item1;
            toUpdate.SelectionStart = result.Item2;
        }

        public static void ConvertPunctuation(TextBox toUpdate)
        {
            // Replace any Japanese punctuation with English ones.
            toUpdate.Text = toUpdate.Text.Replace('＊', '*');
            toUpdate.Text = toUpdate.Text.Replace('「', '[');
            toUpdate.Text = toUpdate.Text.Replace('」', ']');

            // Also handle different wildcard configurations.
            toUpdate.Text = toUpdate.Text.Replace("[]", "*");
            toUpdate.Text = toUpdate.Text.Replace("[*]", "*");
        }

        public static string ConvertToShiftJISBytes(string toConvert)
        {
            byte[] selectedString = System.Text.Encoding.GetEncoding("shift-jis").GetBytes(TextConverter.ConvertToKana(toConvert));
            return BitConverter.ToString(selectedString).Replace("-", "");
        }        

        public static string ConvertKanjiToRadicals(string toConvert)
        {
            if (toConvert.Length == 1)
            {
                return kanjiToRadicalDictionary.Lookup(toConvert);
            }

            return "";
        }
    }
}