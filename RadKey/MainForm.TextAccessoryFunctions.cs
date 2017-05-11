using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadKey
{
    partial class RadKey
    {
        /// <summary>
        /// Determine if the specified position in a string is within a pair of brackets.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool InBrackets(string text, int pos)
        {
            int nextClosed = text.IndexOf(']', pos);
            int nextOpen = text.IndexOf('[', pos);

            // Supress this if there's an unmatched closed bracket to the right of SelectionStart.

            // Special case where string is a single ].
            if (text == "]" && pos == 0)
            {
                return true;
            }
            else if ((nextClosed > 0 && nextOpen < 0)
                || (nextClosed > 0 && nextOpen > 0 && nextOpen > nextClosed))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if the cursor in a textbox is within a pair of brackets.
        /// </summary>
        /// <param name="toTest"></param>
        /// <returns></returns>
        private bool InBrackets(TextBox toTest)
        {
            return InBrackets(toTest.Text, toTest.SelectionStart);
        }

        /// <summary>
        /// Inserts (or doesn't insert) closing brackets into the provided TextBox based on autocompletion rules.
        /// </summary>
        /// <param name="toUpdate"></param>
        private void ClosingBracketAutocompletion(TextBox toUpdate)
        {
            // Track the initial SelectionStart.
            // Needs to be taken first -- removing highlighted text changes the selection start.
            int originalSelectionStart = toUpdate.SelectionStart;

            if (!InBrackets(toUpdate))
            {
                // Trying something different: only adding a closing bracket if the next character is a kana, a [, or the end of the textbox.
                if (toUpdate.SelectionStart == toUpdate.Text.Length)
                {
                    toUpdate.Text = toUpdate.Text.Insert(originalSelectionStart, "]");
                }

                else if (Regex.IsMatch(toUpdate.Text[toUpdate.SelectionStart].ToString(), "[\\p{IsHiragana}\\p{IsKatakana}\\[]"))
                {
                    toUpdate.Text = toUpdate.Text.Insert(originalSelectionStart, "]");
                }
            }

            // Split text -- text after cursor goes into Remainder, text prior to cursor stays in the textbox.
            string Remainder = toUpdate.Text.Substring(originalSelectionStart);
            toUpdate.Text = toUpdate.Text.Substring(0, originalSelectionStart);

            // Reset the selection start for the Convert function, which preserves the selection start.
            toUpdate.SelectionStart = originalSelectionStart;

            TextConverter.ConvertToRadicalOrKana(toUpdate);

            // Store any new returned selection start, as it would get lost when appending the text.
            int updatedSelectionStart = toUpdate.SelectionStart;
            toUpdate.Text = toUpdate.Text + Remainder;

            // Restore the selection start.
            toUpdate.SelectionStart = updatedSelectionStart;
        }

        /// <summary>
        /// Inserts a closing bracket at the cursor or moves past an existing closing bracket if one is already at the cursor in the provided TextBox.
        /// </summary>
        /// <param name="toUpdate"></param>
        private void InsertClosingBracket(TextBox toUpdate)
        {
            int originalTextLength = toUpdate.Text.Length;
            int originalSelectionStart = toUpdate.SelectionStart;
            string originalText = toUpdate.Text;

            

            // Only add another ] if there wasn't already one present.
            // Also some special handling if trying to enter a ] as the first character/last character.
            if (originalSelectionStart == originalTextLength)
            {
                toUpdate.Text = toUpdate.Text + "]";
            }
            else if (originalText[originalSelectionStart] != ']')
            {
                toUpdate.Text = toUpdate.Text.Insert(toUpdate.SelectionStart, "]");
            }

            // Reset the selection start for the Convert function, which preserves the selection start.
            toUpdate.SelectionStart = originalSelectionStart;
            TextConverter.ConvertToRadicalOrKana(toUpdate);
            int updatedSelectionStart = toUpdate.SelectionStart;

            // In all cases add 1 to Item2's value since a ] was added that needs to be moved past.
            toUpdate.SelectionStart = updatedSelectionStart + 1;
        }

        /// <summary>
        /// Inserts an opening bracket at the cursor in the provided TextBox, and moves the cursor past it.
        /// </summary>
        /// <param name="toUpdate"></param>
        private void InsertOpeningBracket(TextBox toUpdate)
        {
            int originalSelectionStart = toUpdate.SelectionStart;
            toUpdate.Text = toUpdate.Text.Insert(originalSelectionStart, "[");
            toUpdate.SelectionStart = originalSelectionStart + 1;
        }
    }
}
