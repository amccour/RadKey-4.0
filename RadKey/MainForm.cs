using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MovablePython;
using RadKey.Compounds;
using RadKey.Radicals;
using RadKey.Kanji;
using RadKey.SearchHandler;

namespace RadKey
{    
    public partial class RadKey : Form
    {        
        // Tray icon and hotkey.
        private System.Windows.Forms.NotifyIcon RadKeyNotifyIcon;
        Hotkey restoreHotkey;

        const string radicalNameFile = "radicalNames.txt";

        private KanjiSearchHandler kanjiSearchManager = null;
        private CompoundSearchHandler compoundSearchHandler = null;

        public RadKey()
        {
            InitializeComponent();                     

            // Initialize OS-specific components.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Windows_InitializeComponent();
                restoreHotkey = Windows_CreateRestoreHotkey();
            }
            else
            {
                Linux_InitializeComponent();
            }

            KanjiDictionary kanjiDictionary = new KanjiDictionary("kanjidic"); //loadKanjiDic(); // Making this non-static would mean KanjiData would need to store a static reference to it for sorting?
            Compound.SetKanjiDictionary(kanjiDictionary);

            KanjiToRadicalDictionary kanjiToRadicalDictionary = new KanjiToRadicalDictionary("kradfile");
            SelectionInfoHandler.SetKanjiToRadicalDictionary(kanjiToRadicalDictionary, kanjiDictionary);
            TextConverter.SetKanjiToRadicalDictionary(kanjiToRadicalDictionary);

            kanjiSearchManager = new KanjiSearchHandler(
                new KanjiSearch(
                    new RadkfileDictionary("radkfile")), kanjiDictionary);

            compoundSearchHandler = new CompoundSearchHandler(
                new CompoundSearch(
                    new CompoundDictionary("JMdict_e"), kanjiToRadicalDictionary));

            
            

            messageBox.Text = NameToRadicalDictionary.Load(radicalNameFile) + Environment.NewLine + messageBox.Text;
        }        






        
        
        
        




        private void RadKey_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+F? Sort by freq/strokes.
            if (e.Control && e.KeyCode == Keys.F)
            {
                toggleSortByFreq();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+J: Hide/show infrequent kanji.
            else if (e.Control && e.KeyCode == Keys.J)
            {
                toggleNoLowFreq();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+shift+C: Copy the contents of the Selected Kanji box.
            else if (e.Control && e.Shift && e.KeyCode == Keys.C)
            {
                if(selectedKanjiBox.Text.Length > 0)
                {
                    TextConverter.ConvertToKana(selectedKanjiBox);
                    Clipboard.SetText(selectedKanjiBox.Text);
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+shift+E: Copy the contents of the Selected Kanji box as shift-jis bytes.
            else if (e.Control && e.Shift && e.KeyCode == Keys.E)
            {
                if(selectedKanjiBox.Text.Length > 0)
                { 
                    Clipboard.SetText(TextConverter.ConvertToShiftJISBytes(selectedKanjiBox.Text));
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+Shift+V: Append the contents of the buffer onto the Selected Kanji box.
            else if (e.Control && e.Shift && e.KeyCode == Keys.V)
            {
                selectedKanjiBox.Text = string.Concat(selectedKanjiBox.Text, Clipboard.GetText());
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // +W? Give focus to the stroke count box.
            else if (e.Control && e.KeyCode == Keys.W)
            {
                strokeBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Pin the application.
            else if(e.Control && e.KeyCode == Keys.P)
            {
                this.TopMost = this.TopMost == false;
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // F8 -- Open radicalNames.txt for reference/editing.
            else if (e.KeyCode == Keys.F8)
            {
                // THIS MAY HAVE ISSUES ON LINUX.
                NameToRadicalDictionary.ShowRadicalNamesFile();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // F9 -- Reload radicalNames.txt
            else if (e.KeyCode == Keys.F9)
            {
                messageBox.Text = NameToRadicalDictionary.Reload();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
        


        private void entryField_KeyDown(object sender, KeyEventArgs e)
        {           
            // Did the user hit space?
            if (e.KeyCode == Keys.Space)
            {
                // May need to return toUpdate and set entryfield to that?
                TextConverter.ConvertToRadical(entryField);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                // Replace any remaining things.
                TextConverter.ConvertToRadical(entryField);

                // The actual focus updating for this is handled in searcKanji. Consider changing that.
                // TODO: This should really take a TextBox or something as an argument.
                // Or have separate functions for getting a kanji list and poulating it into the field.
                kanjiSearchManager.Search(entryField, strokeBox, resultList, messageBox, readingBox, meaningBox);

                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // Decompose kanji into radicals.
            else if (e.Control && e.KeyCode == Keys.D)
            {
                // Limit this to single kanji decomposition.
                if(entryField.Text.Length == 1)
                {
                    string result = TextConverter.ConvertKanjiToRadicals(entryField.Text);
                    if(result != "")
                    {
                        entryField.Text = result;
                    }
                    else
                    {
                        messageBox.Text = "Unable to break this character into radicals.";
                    }
                }
                else
                {
                    messageBox.Text = "A single kanji needs to be in the Entry Field for this to work.";
                }

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit down?
            else if (e.KeyCode == Keys.Down)
            {
                TextConverter.ConvertToRadical(entryField);
                selectedKanjiBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Up: Go to Compound Entry.
            else if (e.KeyCode == Keys.Up)
            {
                TextConverter.ConvertToRadical(entryField);
                compoundEntry.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

        }

        private void resultList_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift enter to copy to compound box.
            if (e.Shift && e.KeyCode == Keys.Enter)
            {
                int atPos = compoundEntry.Text.IndexOf("@");
                if (atPos > -1)
                {
                    string tText = compoundEntry.Text;
                    compoundEntry.Text = tText.Substring(0, atPos) + resultList.Text + tText.Substring(atPos + 1, tText.Length - atPos - 1);
                    
                    // Move the cursor to one past where the kanji was inserted.
                    compoundEntry.SelectionStart = atPos + 1;

                    // See if there are other @s.
                    atPos = compoundEntry.Text.IndexOf("@");
                }
                else
                {
                    compoundEntry.AppendText(resultList.Text);
                    compoundEntry.SelectionStart = selectedKanjiBox.Text.Length;
                }

                // If there are more @s, give focus back to entry box instead.
                if (atPos > -1)
                {
                    entryField.Clear();
                    strokeBox.Clear();
                    entryField.Focus();
                }
                else
                {
                    entryField.Clear();
                    strokeBox.Clear();
                    compoundEntry.Focus();
                }                              
                
                e.SuppressKeyPress = true;
                e.Handled = true;
            }            
            // User hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                // Grab the selected kanji and store the last result focus.
                // Also we want to try to replace any @ characters first.
                int atPos = selectedKanjiBox.Text.IndexOf("@");
                if(atPos > -1)
                {
                    string tText = selectedKanjiBox.Text;
                    selectedKanjiBox.Text = tText.Substring(0, atPos) + resultList.Text + tText.Substring(atPos+1, tText.Length - atPos-1);

                    // Move the cursor to one past where the kanji was inserted.
                    selectedKanjiBox.SelectionStart = atPos + 1;
                }
                else 
                {
                    selectedKanjiBox.AppendText(resultList.Text);
                    selectedKanjiBox.SelectionStart = selectedKanjiBox.Text.Length;
                }
                // Clear out the radical entry and give focus to it.
                entryField.Clear();
                strokeBox.Clear();
                entryField.Focus();
            }
            // User hit space?
            else if (e.KeyCode == Keys.Space)
            {
                entryField.Focus();
            }

            // Decompose kanji into radicals.
            // Give focus to the stroke count box.
            else if (e.Control && e.KeyCode == Keys.D)
            {
                entryField.Text = TextConverter.ConvertKanjiToRadicals(entryField.Text);
                entryField.Focus();
                entryField.SelectionStart = entryField.Text.Length;
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Shift+Up goes back to Selected Kanji.
            else if (e.Shift && e.KeyCode == Keys.Up)
            {
                selectedKanjiBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

        }

        private void strokeBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Space)
            {
                entryField.Focus();
                e.Handled = true;
            }
            // Did the user hit space? Populate the kanji.
            else if (e.KeyChar == (char)Keys.Enter)
            {
                TextConverter.ConvertToRadical(entryField);
                kanjiSearchManager.Search(entryField, strokeBox, resultList, messageBox, readingBox, meaningBox);
                e.Handled = true;
            }
            else if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void resultList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectionInfoHandler.ShowKanjiSelectionInfo(resultList.Text, messageBox, readingBox, meaningBox);
        }

        // Minimize to system tray.
        private void RadKey_Resize(object sender, EventArgs e)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.Visible = true;
                }
                else if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Visible = false;
                }
            }
        }

        // Unregister the hotkey.
        private void RadKey_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Only do this stuff on Windows.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (restoreHotkey.Registered)
                {
                    restoreHotkey.Unregister();
                }
            }
        }             

     
        
        private void compoundEntry_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift + Enter - Conver to hiragana without submitting.
            if (e.Shift && e.KeyCode == Keys.Enter)
            {
                // Convert text to hiragana.
                TextConverter.ConvertToKana(compoundEntry);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // Shift + Space  - Also convert to hiragana without submitting.
            else if (e.Shift && e.KeyCode == Keys.Space)
            {
                // Convert text to hiragana.
                TextConverter.ConvertToKana(compoundEntry);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            
            
            // User hit space: Insert brackets, surround text in brackets, and also do radical conversion.
            // Space is not allowed to do kana conversions.
            else if (e.KeyCode == Keys.Space)
            {
                // If only some text is highlighted, surround it in brackets.
                if(compoundEntry.SelectionLength > 0)
                {
                    int newSelectionStart = compoundEntry.SelectionStart + compoundEntry.SelectedText.Length + 1;
                    compoundEntry.SelectedText = "[" + compoundEntry.SelectedText + "]";
                    compoundEntry.SelectionStart = newSelectionStart;
                }
                else
                {
                    string originalCompoundEntryText = compoundEntry.Text;
                    int originalSelectionStart = compoundEntry.SelectionStart;

                    TextConverter.ConvertToRadical(compoundEntry);

                    // If the text didn't change and the cursor is at a ], space past the ].
                    if (originalCompoundEntryText == compoundEntry.Text)
                    {
                        if(compoundEntry.SelectionStart == compoundEntry.Text.Length)
                        {
                            compoundEntry.Text = compoundEntry.Text + "[]";
                            compoundEntry.SelectionStart = compoundEntry.Text.Length-1;
                        }

                        // Space pressed at end of entry box; add a [].
                        else if (compoundEntry.Text[compoundEntry.SelectionStart] == ']')
                        {
                            compoundEntry.SelectionStart = compoundEntry.SelectionStart + 1;
                        }
                    }

                    // Otherwise do normal radical conversion.
                    else
                    {                        
                        if (InBrackets(originalCompoundEntryText, originalSelectionStart))
                        {
                            // If it WAS originally within brackets, the radical search moved it past the start of the brackets.
                            // Put it back in brackets.
                            if (compoundEntry.Text[compoundEntry.SelectionStart] == '[')
                            {
                                compoundEntry.SelectionStart = compoundEntry.SelectionStart + 1;
                            }
                        }

                    }
                }

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Handle input of [ and ].
            else if ((e.Shift && e.KeyCode == Keys.OemOpenBrackets)
            || e.Shift && e.KeyCode == Keys.OemCloseBrackets)
            {
                e.Handled = true;
            }
            // Did the user enter an opening bracket? Add the closing bracket.
            else if (e.KeyCode == Keys.OemOpenBrackets)
            {
                // Clear any highlighted text. Opening bracket will replace it.
                compoundEntry.SelectedText = "";

                // Insert opening bracket and matchin closing bracket if necessary.
                InsertOpeningBracket(compoundEntry);               
                ClosingBracketAutocompletion(compoundEntry);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // Did the user enter a closing bracket?
            else if (e.KeyCode == Keys.OemCloseBrackets)
            {
                // Clear any highlighted text. Closing bracket will replace it.
                compoundEntry.SelectedText = "";
                InsertClosingBracket(compoundEntry);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                compoundSearchHandler.Search(compoundEntry, compoundsResults, meaningBox, readingBox); // TODO.
          
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Shift+Down goes back to results.
            else if (e.Shift && e.KeyCode == Keys.Down)
            {
                if (compoundsResults.Items.Count > 0)
                {
                    compoundsResults.Focus();
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }            
            // Did the user hit down?
            else if (e.KeyCode == Keys.Down)
            {                
                entryField.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit up?
            else if (e.KeyCode == Keys.Up)
            {
                selectedKanjiBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit Ctrl+H?
            else if (e.Control && e.KeyCode == Keys.H)
            {
                // Convert text to hiragana.
                TextConverter.ConvertToKana(compoundEntry);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // History keys.
            else if(e.KeyCode == Keys.PageDown)
            {
                compoundSearchHandler.GetNextFromHistory(compoundEntry);
            }
            else if(e.KeyCode == Keys.PageUp)
            {
                compoundSearchHandler.GetLastFromHistory(compoundEntry);
            }

        }

        private void compoundResults_KeyDown(object sender, KeyEventArgs e)
        {
            // Did the user hit space?
            if (e.KeyCode == Keys.Space)
            {
                compoundEntry.Focus();

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                int atPos = selectedKanjiBox.Text.IndexOf("@");
                if (atPos > -1)
                {
                    string tText = selectedKanjiBox.Text;
                    selectedKanjiBox.Text = tText.Substring(0, atPos) + compoundsResults.Text + tText.Substring(atPos + 1, tText.Length - atPos - 1);

                    // Set The cursor in selectedKanjiBox on past the text that was added.
                    selectedKanjiBox.SelectionStart = atPos + compoundsResults.Text.Length;

                    // See if there are other @s.
                    atPos = selectedKanjiBox.Text.IndexOf("@");           
                }
                else
                {
                    selectedKanjiBox.AppendText(compoundsResults.Text);
                    selectedKanjiBox.SelectionStart = selectedKanjiBox.Text.Length;
                }

                // If there are more @s, give focus back to entry box instead.
                if (atPos > -1)
                {
                    // Clear out the compound field entry and give focus to it.
                    compoundEntry.Clear();
                    compoundEntry.Focus();
                }
                else
                {
                    compoundEntry.Clear();
                    selectedKanjiBox.Focus();
                }                

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Shift+Up goes back to Selected Kanji.
            else if (e.Shift && e.KeyCode == Keys.Up)
            {
                compoundEntry.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void compoundsResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectionInfoHandler.ShowCompoundSelectionInfo((Compound)compoundsResults.SelectedItem, meaningBox, readingBox);
        }
    
        private void entryField_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Tab))
            {
                TextConverter.ConvertToRadical(entryField);
            }
        }

        

        // These next parts convert Japanese special characterss to English ones.
        private void compoundEntry_Leave(object sender, EventArgs e)
        {
            int originalSelectionStart = compoundEntry.SelectionStart;
            TextConverter.ConvertPunctuation(compoundEntry);

            // Length can't change so this is safe.
            compoundEntry.SelectionStart = originalSelectionStart;
        }

        

        private void includeLowFreqCB_Click(object sender, EventArgs e)
        {
            toggleNoLowFreq();
            entryField.Focus();
        }

        private void frequencySortCB_Click(object sender, EventArgs e)
        {
            toggleSortByFreq();
            entryField.Focus();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void compoundsResults_Enter(object sender, EventArgs e)
        {
            Resizer.ExpandReadingBox(this);
        }

        private void resultList_Enter(object sender, EventArgs e)
        {
            Resizer.OriginalFieldSize(this);
        }
    }
}