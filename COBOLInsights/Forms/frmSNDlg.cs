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
        private List<SourceNavigationItem> SNList = new List<SourceNavigationItem>();

        public FrmSNDlg()
        {
            InitializeComponent();
            SNListBox.DisplayMember = "Name";
            synchronizationContext = SynchronizationContext.Current;
        }

        //private void toolStripButton1_Click(object sender, EventArgs e)
        //{
        //    Main.UpdateSNList();
        //}

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
            int charNumber = selectedItem.CharNumber;
            Editor.EnsureVisible(line - 1);
            Editor.GotoLine(line - 1);
            Editor.ScrollRange(new Position(charNumber + 1000), new Position(charNumber - 1000));
            Editor.GrabFocus();
        }

        //private void frmSNDlg_Shown(object sender, EventArgs e)
        //{
        //    Main.UpdateSNList();
        //}

        private void frmSNDlg_Shown(object sender, EventArgs e)
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
            SNList.Clear();
            string text = Editor.GetText(Editor.GetTextLength());
            string search = @"[\s]([\w|-]+)[\s]+(SECTION|DIVISION)[\s]*\.[\s]*";
            MatchCollection matches = Regex.Matches(text, search);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    SNList.Add(new SourceNavigationItem
                    {
                        Name = match.Groups[2].Value == "SECTION" ? " " + match.Groups[0].Value : match.Groups[0].Value,
                        LineNumber = LineFromPos(text, match.Index),
                        CharNumber = match.Index
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

        private int LineFromPos(string input, int indexPosition)
        {
            int lineNumber = 1;
            for (int i = 0; i < indexPosition; i++)
            {
                if (input[i] == '\n') lineNumber++;
            }
            return lineNumber;
        }

        private void frmSNDlg_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible==true)
            {
                UpdateSNListBox();
            }
        }
    }
}
