﻿using System;
using System.Drawing;
using System.Windows.Forms;
using OSIRT.UI.Attachment;
using OSIRT.UI.AuditLog;
using OSIRT.UI.CaseNotes;
using OSIRT.VideoCapture;

namespace OSIRT.UI
{

    public partial class BrowserPanel : UserControl
    {

        public event EventHandler CaseClosing;


        public BrowserPanel()
        {
            InitializeComponent();
            uiTabbedBrowserControl.SetAddressBar(uiURLComboBox);
        }


        private void BrowserPanel_Load(object sender, EventArgs e)
        {
            ConfigureUi();
            AddNewTab();
            uiTabbedBrowserControl.ScreenshotComplete += UiTabbedBrowserControl_ScreenshotComplete;
        }

        private void UiTabbedBrowserControl_ScreenshotComplete(object sender, EventArgs e)
        {
            uiScreenshotButton.Enabled = true;
        }

        private void ConfigureUi()
        {
            Dock = DockStyle.Fill;
            uiBrowserToolStrip.ImageScalingSize = new Size(32, 32);
            uiURLComboBox.KeyDown += UiURLComboBox_KeyDown;

        }

        private void UiURLComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                uiTabbedBrowserControl.Navigate(uiURLComboBox.Text);
                e.SuppressKeyPress = true; //stops "ding" sound when enter is pushed
            }
        }

        private void uiAddTabButton_Click(object sender, EventArgs e)
        {
            //AddNewTab();
        }


        private void uiScreenshotButton_Click(object sender, EventArgs e)
        {
            uiScreenshotButton.Enabled = false;
            uiTabbedBrowserControl.FullPageScreenshot();
            //uiScreenshotButton.Enabled = true;
        }

        private void AddNewTab()
        {
            //uiTabbedBrowserControl.NewTab(UserSettings.Load().Homepage, uiURLComboBox);
        }

        private void uiBrowserMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void uiCaseNotesButton_Click(object sender, EventArgs e)
        {
            CaseNotesForm caseNotes = new CaseNotesForm();
            caseNotes.Show();
        }

        private void auditLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(AuditLogForm audit = new AuditLogForm())
            {
                audit.ShowDialog();
            }
        }

        private void uiAttachmentToolStripButton_Click(object sender, EventArgs e)
        {
            using(AttachmentForm attachment = new AttachmentForm())
            {
                attachment.ShowDialog();
            }
        }

        private void uiLBackButton_Click(object sender, EventArgs e)
        {
            //TODO: check if can back
            uiTabbedBrowserControl.CurrentTab.Browser.GoBack();
        }

        private void uiForwardButton_Click(object sender, EventArgs e)
        {
            //TODO: check if can go forward
            uiTabbedBrowserControl.CurrentTab.Browser.GoForward();
        }

        private void uiRefreshButton_Click(object sender, EventArgs e)
        {
            uiTabbedBrowserControl.CurrentTab.Browser.Refresh();
        }

        private void uiTabbedBrowserControl_Load(object sender, EventArgs e)
        {

        }

        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void closeCaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaseClosing?.Invoke(this, e);
        }

        private void uiBrowserPanel_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void uiVideoCaptureButton_Click(object sender, EventArgs e)
        {
            if (!OsirtVideoCapture.IsRecording())
            {
               
                OsirtVideoCapture.StartCapture(Width, Height);
                uiVideoCaptureButton.Image = Properties.Resources.stop_rec;
                uiVideoCaptureButton.ToolTipText = "Stop screen capture";
            }
            else
            {
                OsirtVideoCapture.StopCapture();
                uiVideoCaptureButton.Image = Properties.Resources.start_rec;
                uiVideoCaptureButton.ToolTipText = "Start screen capture";
            }
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new VideoPreviewer().ShowDialog();
        }

    }
}
