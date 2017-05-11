using MovablePython;
using System;
using System.Windows.Forms;

namespace RadKey
{
    partial class RadKey
    {
        private Hotkey Windows_CreateRestoreHotkey()
        {
            // Hotkey section.
            Hotkey toRegister = new Hotkey(Keys.Back, true, true, false, false);
            toRegister.Pressed += delegate
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.Visible = false;
                }
                else if (this.WindowState == FormWindowState.Minimized)
                {
                    // Need to make it visible first, otherwise it ignores the WindowState change.
                    this.Visible = true;
                    this.WindowState = FormWindowState.Normal;
                }
            };

            if (!toRegister.GetCanRegister(this))
            {
                // TODO
            }
            else
            {
                toRegister.Register(this);
            }

            return toRegister;
        }

        private void Windows_InitializeComponent()
        {
            // Tray section.
            this.ShowInTaskbar = false;

            // Duplicates the component registration code from Form1.Designer since it gets mad if I try to modify that file directly.
            if (this.components == null)
            {
                this.components = new System.ComponentModel.Container();
            }

            this.RadKeyNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);

            this.RadKeyNotifyIcon.BalloonTipText = "RadKey";
            this.RadKeyNotifyIcon.Icon = ((System.Drawing.Icon)(GlobeIcon.ResourceManager.GetObject("globe")));
            this.RadKeyNotifyIcon.Text = "RadKey";
            this.RadKeyNotifyIcon.Visible = true;
            this.RadKeyNotifyIcon.Click += new System.EventHandler(this.Windows_RKNIClick);
        }

        private void Windows_RKNIClick(object sender, System.EventArgs e)
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.Visible = false;
                }
                else if (this.WindowState == FormWindowState.Minimized)
                {
                    // Need to make it visible first, otherwise it ignores the WindowState change.
                    this.Visible = true;
                    this.WindowState = FormWindowState.Normal;
                }
            }
        }

        public void Linux_InitializeComponent()
        {
            this.messageBox.Font = new System.Drawing.Font("MS Gothic", (float)9);
            this.meaningBox.Font = new System.Drawing.Font("MS Gothic", (float)8.5);
            this.readingBox.Font = new System.Drawing.Font("MS Gothic", (float)11.5);
        }
    }
}
