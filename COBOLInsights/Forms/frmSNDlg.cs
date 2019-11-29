using COBOLInsights.SNClasses;
using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kbg.NppPluginNET
{
    public partial class FrmSNDlg : Form
    {
        private readonly SynchronizationContext synchronizationContext;
        private IScintillaGateway Editor;

        public FrmSNDlg()
        {
            InitializeComponent();
            SNListBox.DisplayMember = "Name";
            synchronizationContext = SynchronizationContext.Current;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            UpdateSNListBox();
        }

        private void SNListBox_DoubleClick(object sender, EventArgs e)
        {
            GotoSelectedSection();
        }

        private void GotoSelectedSection()
        {
            Editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            SourceNavigationItem selectedItem = (SourceNavigationItem)SNListBox.SelectedItem;
            int line = selectedItem.LineNumber;
            int linesOnScreen = Editor.LinesOnScreen();
            Editor.GotoLine(line);
            Editor.SetFirstVisibleLine(line - linesOnScreen / 2 + 1 < 0 ? 0 : line - linesOnScreen / 2 + 1);
            Editor.GrabFocus();
        }

        private void FrmSNDlg_Shown(object sender, EventArgs e)
        {
            UpdateSNListBox();
        }

        public async void UpdateSNListBox()
        {
            SNListBox.Enabled = false;
            await Task.Run(UpdateSNList);
            SNListBox.Enabled = true;
        }

        private void UpdateSNList()
        {
            Editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            List<SourceNavigationItem> SNList = new List<SourceNavigationItem>();
            SNList.Clear();
            SNList.Add(new SourceNavigationItem
            {
                Name = "<TOP>",
                LineNumber = 0
            });
            string text = Editor.GetText(Editor.GetTextLength());
            string search = @"^[\s]*([\w|-]+)[\s]+(SECTION|DIVISION)[\s]*\.[\s]*$";
            MatchCollection matches = Regex.Matches(text, search, RegexOptions.Multiline);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    SNList.Add(new SourceNavigationItem
                    {
                        Name = (match.Groups[2].Value == "SECTION" ? " " : "") + match.Groups[1].Value + " " + match.Groups[2].Value,
                        LineNumber = Editor.LineFromPosition(new Position(match.Index))
                    });
                }
            }
            PostDataToSNListBox(SNList);
        }

        private void PostDataToSNListBox(List<SourceNavigationItem> snlist)
        {
            synchronizationContext.Post(new SendOrPostCallback(o =>
            {
                SNListBox.Items.Clear();
                SNListBox.Items.AddRange(((List<SourceNavigationItem>)o).ToArray());
            }), snlist);
        }

        private void frmSNDlg_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible==true)
            {
                UpdateSNListBox();
            }
        }

        private void toolStripToTop_Click(object sender, EventArgs e)
        {
            SNListBox.TopIndex = 0;
            SNListBox.SelectedIndex = 0;
        }

        private void FindSectionInProcedureDivision(string itemName)
        {
            int index = 0;
            if (itemName != null)
            {
                index = SNListBox.FindString(itemName, SNListBox.FindString("procedure"));
            }
            SNListBox.TopIndex = index;
            SNListBox.SelectedIndex = index;
        }

        private void toolStripESections_Click(object sender, EventArgs e)
        {
            FindSectionInProcedureDivision(" e");
        }

        private void toolStripRSections_Click(object sender, EventArgs e)
        {
            FindSectionInProcedureDivision(" r");
        }

        private void toolStripUSections_Click(object sender, EventArgs e)
        {
            FindSectionInProcedureDivision(" u");
        }

        private void toolStripToBottom_Click(object sender, EventArgs e)
        {
            SNListBox.TopIndex = SNListBox.Items.Count - 1;
            SNListBox.SelectedIndex = SNListBox.Items.Count - 1;
        }

        public List<SourceNavigationItem> GetStoredSectionsList()
        {
            List<SourceNavigationItem> result = new List<SourceNavigationItem>();
            foreach (SourceNavigationItem item in SNListBox.Items)
            {
                result.Add(item);
            }
            return result;
        }
    }
}
