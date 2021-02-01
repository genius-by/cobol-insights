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
            SNListBox.DrawMode = DrawMode.OwnerDrawFixed;
            synchronizationContext = SynchronizationContext.Current;
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
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
            string sectionName = selectedItem.Name.Trim().Split(' ')[0];
            string lineText = Editor.GetLine(line).ToUpper();
            Editor.GotoLine(line);
            Editor.SetFirstVisibleLine(line - linesOnScreen / 2 + 1 < 0 ? 0 : line - linesOnScreen / 2 + 1);
            Editor.SetSelection(Editor.PositionFromLine(line).Value + lineText.IndexOf(sectionName) + sectionName.Length, Editor.PositionFromLine(line).Value + lineText.IndexOf(sectionName));
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
            if (Editor.GetTextLength()<=999999)
            {
                string text = Editor.GetText(Editor.GetTextLength());
                string search = @"^[\s]*([\w|-]+)[\s]+(SECTION|DIVISION)[\s]*\.[\s]*$";
                MatchCollection matches = Regex.Matches(text, search, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        SNList.Add(new SourceNavigationItem
                        {
                            Name = ((match.Groups[2].Value.StartsWith("SECTION", StringComparison.OrdinalIgnoreCase) ? " " : "") + match.Groups[1].Value + " " + match.Groups[2].Value).ToUpper(),
                            LineNumber = Editor.LineFromPosition(new Position(match.Index))
                        });
                    }
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

        private void FrmSNDlg_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible==true)
            {
                UpdateSNListBox();
            }
        }

        private void ToolStripToTop_Click(object sender, EventArgs e)
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

        private void ToolStripESections_Click(object sender, EventArgs e)
        {
            FindSectionInProcedureDivision(" e");
        }

        private void ToolStripRSections_Click(object sender, EventArgs e)
        {
            FindSectionInProcedureDivision(" r");
        }

        private void ToolStripUSections_Click(object sender, EventArgs e)
        {
            FindSectionInProcedureDivision(" u");
        }

        private void ToolStripToBottom_Click(object sender, EventArgs e)
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

        private void SNListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            SNListBox.BackColor = SystemColors.Window;
            e.DrawBackground();
            e.DrawFocusRectangle();

            Color color = Color.Black;
            FontStyle fs = FontStyle.Regular;

            

            if (!((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" "))
            {
                fs = FontStyle.Bold;
            }
            else if (((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" E", StringComparison.OrdinalIgnoreCase) && !((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" EXTENDED", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.DarkGreen;
            }
            else if (((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" S", StringComparison.OrdinalIgnoreCase) ||
                ((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" A", StringComparison.OrdinalIgnoreCase) ||
                ((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" B", StringComparison.OrdinalIgnoreCase) ||
                ((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" Z", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.Gray;
                fs = FontStyle.Italic;
            }
            else if (((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" R", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.DarkBlue;
            }
            else if (((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" U", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.DarkOrange;
            }
            else if (((SourceNavigationItem)SNListBox.Items[e.Index]).Name.StartsWith(" Y", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.DarkSlateGray;
            }
            using (FontFamily fontFamily = new FontFamily("Consolas"))
            {
                if (fontFamily.Name == "Consolas")
                {
                    TextRenderer.DrawText(e.Graphics, ((SourceNavigationItem)SNListBox.Items[e.Index]).Name, new Font(fontFamily, 9.75F, fs, GraphicsUnit.Point), e.Bounds, color, TextFormatFlags.Default);
 //                   e.Graphics.DrawString(((SourceNavigationItem)SNListBox.Items[e.Index]).Name, new Font(fontFamily, 9.75F, fs, GraphicsUnit.Point), new SolidBrush(color), e.Bounds);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, ((SourceNavigationItem)SNListBox.Items[e.Index]).Name, new Font("Lucida Console", 9.75F, fs, GraphicsUnit.Point), e.Bounds, color, TextFormatFlags.Default);
                }

            }
        }

        private void SNListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
                ContextMenuStrip menuStrip = new ContextMenuStrip();
                int index = SNListBox.IndexFromPoint(e.X, e.Y);
                SNListBox.SetSelected(index, true);
                string text = Editor.GetText(Editor.GetTextLength());
                string search = @"^[ ]*PERFORM[\s]*" + ((SourceNavigationItem)SNListBox.Items[index]).Name.Trim().Split(' ')[0] + @"[\s]*[\.]{0,1}[\s]*$";
                MatchCollection matches = Regex.Matches(text, search, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        string itemName = "";
                        foreach (SourceNavigationItem item in GetStoredSectionsList())
                        {
                            int line = Editor.LineFromPosition(new Position(match.Index));
                            string currentText = "";
                            if (item.LineNumber > line)
                            {
                                break;
                            }
                            else
                            {
                                currentText = item.Name.Trim().Split(' ')[0];
                            }
                            if (currentText != "")
                            {
                                itemName = (line + 1) + ": " + currentText;

                            }
                            Editor.LineFromPosition(new Position(match.Index));
                        }

                        menuStrip.Items.Add(itemName).Click += FrmSNDlg_SNListBox_Context_Click;
                    }
                }
                if (menuStrip.Items.Count > 0)
                {
                    menuStrip.Show(SNListBox, e.X, e.Y);
                }
            }
        }

        private void FrmSNDlg_SNListBox_Context_Click(object sender, EventArgs e)
        {
            Editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            int line = int.Parse(((ToolStripItem)sender).Text.Split(':')[0]) - 1;
            int linesOnScreen = Editor.LinesOnScreen();
            string lineText = Editor.GetLine(line);
            Editor.GotoLine(line);
            Editor.SetFirstVisibleLine(line - linesOnScreen / 2 + 1 < 0 ? 0 : line - linesOnScreen / 2 + 1);
            string sectionName = lineText.Trim().Split(' ')[1];
            Editor.SetSelection(Editor.WordEndPosition(new Position(Editor.PositionFromLine(line).Value + lineText.IndexOf(sectionName)), true), Editor.PositionFromLine(line).Value + lineText.IndexOf(sectionName));
            Editor.GrabFocus();
            ((ToolStripItem)sender).Owner.Dispose();
        }
    }
}
