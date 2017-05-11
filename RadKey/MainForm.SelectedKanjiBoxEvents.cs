using System;
using System.Windows.Forms;

namespace RadKey
{
    public partial class RadKey
    {
        private void selectedKanjiBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextConverter.ConvertToKana(selectedKanjiBox);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            if (e.Shift && e.KeyCode == Keys.Space)
            {
                TextConverter.ConvertToKana(selectedKanjiBox);

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                TextConverter.ConvertToKana(selectedKanjiBox);

                entryField.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit Ctrl+H?
            else if (e.Control && e.KeyCode == Keys.H)
            {
                TextConverter.ConvertToKana(selectedKanjiBox);

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Shift+Down goes back to results.
            else if (e.Shift && e.KeyCode == Keys.Down)
            {
                if (resultList.Items.Count > 0)
                {
                    TextConverter.ConvertToKana(selectedKanjiBox);

                    resultList.Focus();
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit down?
            else if (e.KeyCode == Keys.Down)
            {
                TextConverter.ConvertToKana(selectedKanjiBox);

                compoundEntry.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void selectedKanjiBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Tab))
            {
                TextConverter.ConvertToKana(selectedKanjiBox);
            }
        }

        private void selectedKanjiBox_Leave(object sender, EventArgs e)
        {
            int currentSelection = selectedKanjiBox.SelectionStart;
            selectedKanjiBox.Text = selectedKanjiBox.Text.Replace('＠', '@');
            selectedKanjiBox.SelectionStart = currentSelection;
        }
    }
}