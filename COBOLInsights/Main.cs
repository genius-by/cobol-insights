using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using COBOLInsights.SNClasses;

namespace Kbg.NppPluginNET
{
    class Main
    {
        internal const string PluginName = "COBOLInsights";
        static int commandIndexCounter = 0;
        static string iniFilePath = null;
        static bool ShowVerticalLines = false;
        static bool CobolLikeWords = false;
        static List<VerticalLine> VerticalLines = new List<VerticalLine>();
        static readonly Bitmap CobolWordsCharsBmp = COBOLInsights.Properties.Resources.cobol_words_chars;
        static readonly Bitmap ToggleVerticalLinesBmp = COBOLInsights.Properties.Resources.toggle_vertical_lines;
        static int ToggleVerticalLinesCommandId = -1;
        static readonly Bitmap AddVerticalLineBmp = COBOLInsights.Properties.Resources.add_vertical_line;
        static int AddVerticalLineCommandId = -1;
        static readonly Bitmap ClearVerticalLinesBmp = COBOLInsights.Properties.Resources.clear_vertical_lines;
        static int ClearVerticalLinesCommandId = -1;
        static int ToggleCobolWordsCommandId = -1;
        static string sectionName;
        static int CurrentSearchOffset = 0;
        static DockableFormCommandStruct SNDialogStruct = new DockableFormCommandStruct()
        {
            Form = null,
            FormCommandId = -1,
            ToolbarButtonBmp = COBOLInsights.Properties.Resources.list_icon,
            DockingTabIconBmp = COBOLInsights.Properties.Resources.list_icon,
            DockingTabIcon = null
        };

        static readonly IScintillaGateway Editor1 = new ScintillaGateway(PluginBase.nppData._scintillaMainHandle);
        static readonly IScintillaGateway Editor2 = new ScintillaGateway(PluginBase.nppData._scintillaSecondHandle);

        static readonly List<ISnippet> Snippets = new List<ISnippet>();

        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
            // use eg. as
            if (notification.Header.Code == (uint)NppMsg.NPPN_BUFFERACTIVATED)
            {
                if (SNDialogStruct.Form != null)
                {
                    ((FrmSNDlg)SNDialogStruct.Form).UpdateSNListBox();
                }
                SetCobolLikeWords(CobolLikeWords);
            }

            if (notification.Header.Code == (uint)NppMsg.NPPN_READY)
            {
                LoadMultipleVerticalLines(ShowVerticalLines);
            }
            // or
            //
            //if (notification.Header.Code == (uint)SciMsg.SCN_MODIFIED &&
            //    (notification.ModificationType == (int)SciMsg.SC_MOD_INSERTTEXT ||
            //     notification.ModificationType == (int)SciMsg.SC_MOD_DELETETEXT))
            //{
            //    UpdateSNList();
            //}
        }

        internal static void CommandMenuInit()
        {
            InitPluginIniParameters();

            PluginBase.SetCommand(commandIndexCounter, "Show vertical lines", ShowMultipleVerticalLines, new ShortcutKey(false, false, false, Keys.None), ShowVerticalLines);
            ToggleVerticalLinesCommandId = commandIndexCounter++;
            PluginBase.SetCommand(commandIndexCounter, "Add vertical line...", AddVerticalLine, new ShortcutKey(false, false, false, Keys.None));
            AddVerticalLineCommandId = commandIndexCounter++;
            PluginBase.SetCommand(commandIndexCounter, "Clear all vertical lines", ClearAllVerticalLines, new ShortcutKey(false, false, false, Keys.None));
            ClearVerticalLinesCommandId = commandIndexCounter++;

            PluginBase.SetCommand(commandIndexCounter++, "---", null);
            PluginBase.SetCommand(commandIndexCounter, "Source Navigation Panel", SourceNavigationDialog);
            SNDialogStruct.FormCommandId = commandIndexCounter++;
            PluginBase.SetCommand(commandIndexCounter++, "Go to SECTION/PERFORM", GotoSectionOrPerform, new ShortcutKey(false, false, false, Keys.F9));

            PluginBase.SetCommand(commandIndexCounter++, "---", null);
            PluginBase.SetCommand(commandIndexCounter, "Toggle COBOL-like words", ToggleCobolLikeWords, new ShortcutKey(false, false, false, Keys.None));
            ToggleCobolWordsCommandId = commandIndexCounter++;

            PluginBase.SetCommand(commandIndexCounter++, "---", null);
            PopulateMenuSnippetsCommands();
        }

        private static void ToggleCobolLikeWords()
        {
            SetCobolLikeWords(!CobolLikeWords);
        }

        private static void SetCobolLikeWords(bool activate)
        {
            if (activate)
            {
                Editor1.SetWordChars("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_");
                Editor2.SetWordChars("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_");
                CobolLikeWords = true;
                SetToolbarCommandActivatedState(ToggleCobolWordsCommandId, true);
            }
            else
            {
                Editor1.SetCharsDefault();
                Editor2.SetCharsDefault();
                CobolLikeWords = false;
                SetToolbarCommandActivatedState(ToggleCobolWordsCommandId, false);
            }
            
        }

        private static void GotoSectionOrPerform()
        {
            if (SNDialogStruct.Form == null)
            {
                SourceNavigationDialog();
            }
            var editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            if (!editor.GetWordChars().Contains("-"))
            {
                SetCobolLikeWords(true);
            }
            editor.SetSelection(editor.WordEndPosition(editor.GetCurrentPos(), true), editor.WordStartPosition(editor.GetCurrentPos(), true));

            if (editor.GetSelectionLength() == 0)
            {
                return;
            }
            // If new search
            if (sectionName != editor.GetSelText())
            {
                sectionName = editor.GetSelText();
                int sectionImplementationLine = GetSectionImplementationLine(sectionName);
                if (sectionImplementationLine >= 0)
                {
                    if (editor.GetCurrentLineNumber() == sectionImplementationLine)
                    {
                        if (!SearchNextSectionOrPerform(sectionName, editor.GetCurrentPos().Value))
                        {
                            SearchNextSectionOrPerform(sectionName, 0);
                        }
                            
                    }
                    else
                    {
                        ScrollToLine(sectionImplementationLine);
                        CurrentSearchOffset = sectionImplementationLine;
                    }
                }
                else
                {
                    sectionName = "";
                }
            }
            //If continuing search
            else
            {
                if (!SearchNextSectionOrPerform(sectionName, CurrentSearchOffset))
                {
                    SearchNextSectionOrPerform(sectionName, 0);
                }
            }
        }

        private static bool SearchNextSectionOrPerform(string sectionName, int offset)
        {
            var editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            using (TextToFind textToFind = new TextToFind(offset, editor.GetTextLength() - 1, sectionName))
            {
                Position sectionPosition = editor.FindText(0, textToFind);
                if (sectionPosition.Value >= 0)
                {
                    if (editor.GetLine(editor.LineFromPosition(sectionPosition)).StartsWith("*"))
                    {
                        CurrentSearchOffset = sectionPosition.Value + sectionName.Length;
                        return SearchNextSectionOrPerform(sectionName, CurrentSearchOffset);
                    }
                    ScrollToLine(editor.LineFromPosition(sectionPosition));
                    CurrentSearchOffset = sectionPosition.Value + sectionName.Length;
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
        }

        private static void ScrollToLine(int sectionImplementationLine)
        {
            var editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            int linesOnScreen = editor.LinesOnScreen();
            editor.SetFirstVisibleLine(sectionImplementationLine - linesOnScreen / 2 + 1 < 0 ? 0 : sectionImplementationLine - linesOnScreen / 2 + 1);
        }

        private static int GetSectionImplementationLine(string name)
        {
            if (SNDialogStruct.Form != null)
            {
                foreach (var item in ((FrmSNDlg)SNDialogStruct.Form).GetStoredSectionsList())
                {
                    if (item.Name.Trim().StartsWith(name))
                    {
                        return item.LineNumber;
                    }
                }
            }
            return -1;
        }

        private static void InitPluginIniParameters()
        {
            iniFilePath = GetPluginIniFilePath();

            ShowVerticalLines = Win32.GetPrivateProfileInt("COBOLInsights", "ShowVerticalLines", 0, iniFilePath) != 0;

 //           CobolLikeWords = Win32.GetPrivateProfileInt("COBOLInsights", "CobolLikeWords", 0, iniFilePath) != 0;

            StringBuilder sbVerticalLinesParam = new StringBuilder(150);
            Win32.GetPrivateProfileString("COBOLInsights", "VerticalLines", "0|0", sbVerticalLinesParam, 150, iniFilePath);
            VerticalLines = ParseIniVerticalLinesParam(sbVerticalLinesParam.ToString());

            InitSnippetList();
        }

        private static string GetPluginIniFilePath()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            return Path.Combine(iniFilePath, PluginName + ".ini");
        }

        private static void InitSnippetList()
        {
            Snippets.Add(new SimpleSnippet(name: "New section",
                                           snippet: "*-------------------------------------------------------------------------------\n" +
                                                    " {*} SECTION.\n" +
                                                    "*-------------------------------------------------------------------------------\n" +
                                                    "*/ Description\n" +
                                                    "*-------------------------------------------------------------------------------\n" +
                                                    "\n    ." +
                                                    "\n\n" +
                                                    " {*} - EXIT.\n" +
                                                    "    EXIT."));
            Snippets.Add(new LineSurroundingSnippet(name: "Marke #999#",
                                                    snippetBodyBefore: "*#999# Anfang:",
                                                    snippetBodyAfter: "*#999# Ende.",
                                                    keepIndent: false));
            Snippets.Add(new LineSurroundingSnippet(name: "IF THEN * END-IF",
                                                    snippetBodyBefore: "IF {*}\nTHEN",
                                                    snippetBodyAfter: "END-IF",
                                                    keepIndent: true,
                                                    indentSelection: 3));
            Snippets.Add(new LineSurroundingSnippet(name: "IF THEN * ELSE * END-IF",
                                                    snippetBodyBefore: "IF {*}\nTHEN",
                                                    snippetBodyAfter: "ELSE\n   \nEND-IF",
                                                    keepIndent: true,
                                                    indentSelection: 3));
        }

        private static void PopulateMenuSnippetsCommands()
        {
            foreach (ISnippet snippet in Snippets)
            {
                PluginBase.SetCommand(commandIndexCounter++, snippet.GetCommandName(), snippet.InsertSnippet, new ShortcutKey(false, false, false, Keys.None));
            }
        }


        /// <summary>
        /// Method used by NPP to add plugin commands to toolbar and menu.
        /// </summary>
        internal static void SetToolBarIcon()
        {
            AddNppToolbarIcon(ToggleVerticalLinesBmp, ToggleVerticalLinesCommandId);
            AddNppToolbarIcon(AddVerticalLineBmp, AddVerticalLineCommandId);
            AddNppToolbarIcon(ClearVerticalLinesBmp, ClearVerticalLinesCommandId);
            AddNppToolbarIcon(SNDialogStruct);
            AddNppToolbarIcon(CobolWordsCharsBmp, ToggleCobolWordsCommandId);
        }

        /// <summary>
        /// Method used by NPP, when by closing to run any necessary code before plugin is unloaded (i.e. save settings).
        /// </summary>
        internal static void PluginCleanUp()
        {
            Win32.WritePrivateProfileString("COBOLInsights", "ShowVerticalLines", ShowVerticalLines ? "1" : "0", iniFilePath);
            Editor1.SetCharsDefault();
            Editor2.SetCharsDefault();
//            Win32.WritePrivateProfileString("COBOLInsights", "CobolLikeWords", CobolLikeWords ? "1" : "0", iniFilePath);
            Win32.WritePrivateProfileString("COBOLInsights", "VerticalLines", GetVerticalLinesParamString(VerticalLines), iniFilePath);
        }


        /// <summary>
        /// Toggles show of coloured vertical lines and state of corresponding menu command.
        /// </summary>
        internal static void ShowMultipleVerticalLines()
        {
            LoadMultipleVerticalLines(!ShowVerticalLines);
            ShowVerticalLines = !ShowVerticalLines;
        }

        /// <summary>
        /// Draws predefined coloured vertical lines.
        /// </summary>
        /// <param name="show">When set to false clears all currently drawed lines.</param>
        private static void LoadMultipleVerticalLines(bool show)
        {
            if (show)
            {
                Editor1.MultiEdgeClearAll();
                Editor2.MultiEdgeClearAll();
                Editor1.SetEdgeMode((int)SciMsg.EDGE_MULTILINE);
                Editor2.SetEdgeMode((int)SciMsg.EDGE_MULTILINE);
                foreach (var line in VerticalLines)
                {
                    Editor1.MultiEdgeAddLine(line.Position, new Colour(line.Color));
                    Editor2.MultiEdgeAddLine(line.Position, new Colour(line.Color));
                }
                SetToolbarCommandActivatedState(ToggleVerticalLinesCommandId, true);
            }
            else
            {
                Editor1.MultiEdgeClearAll();
                Editor2.MultiEdgeClearAll();
                SetToolbarCommandActivatedState(ToggleVerticalLinesCommandId, false);
            }
        }

        /// <summary>
        /// Create and show form for adding a single vertical line.
        /// </summary>
        private static void AddVerticalLine()
        {
            using (FrmAddVerticalLine frmAddVerticalLine = new FrmAddVerticalLine())
            {
                var res = frmAddVerticalLine.ShowDialog();
                if (res == DialogResult.OK && frmAddVerticalLine.Position > 0)
                {
                    VerticalLine line = new VerticalLine() { Position = frmAddVerticalLine.Position, Color = frmAddVerticalLine.Colour.Value };
                    VerticalLines.Add(line);
                    Editor1.MultiEdgeAddLine(line.Position, new Colour(line.Color));
                    Editor2.MultiEdgeAddLine(line.Position, new Colour(line.Color));
                }
            }
        }

        /// <summary>
        /// Removes all vertical lines from screen and clears their settings.
        /// </summary>
        private static void ClearAllVerticalLines()
        {
            VerticalLines.Clear();
            Editor1.MultiEdgeClearAll();
            Editor2.MultiEdgeClearAll();
        }

        /// <summary>
        /// Controls show of Source Navigation dialog.
        /// </summary>
        internal static void SourceNavigationDialog()
        {
            if (SNDialogStruct.Form == null)
            {
                StartNewDockedSourceNavigationPanel();
            }
            else
            {
                ToggleSourceNavigationPanelVisibility();
            }
        }

        /// <summary>
        /// Toggles show of Source Navigation dialog.
        /// </summary>
        private static void ToggleSourceNavigationPanelVisibility()
        {
            if (!SNDialogStruct.Form.Visible)
            {
                SetDockableDialogVisibility(SNDialogStruct.Form.Handle, true);
                SetToolbarCommandActivatedState(SNDialogStruct.FormCommandId, true);
            }
            else
            {
                SetDockableDialogVisibility(SNDialogStruct.Form.Handle, false);
                SetToolbarCommandActivatedState(SNDialogStruct.FormCommandId, false);
            }
        }

        /// <summary>
        /// Create and show dockable Source Navigation dialog.
        /// </summary>
        private static void StartNewDockedSourceNavigationPanel()
        {
            SNDialogStruct.Form = new FrmSNDlg();
            SNDialogStruct.DockingTabIcon = InitializeDockableDialogTabIcon(SNDialogStruct.DockingTabIconBmp);

            IntPtr _ptrNppTbData = CreateNppToolbarDataPointer(SNDialogStruct.Form,
                                                               "Source Navigation",
                                                               SNDialogStruct.FormCommandId,
                                                               NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                                                               SNDialogStruct.DockingTabIcon);
            RegisterNppDockableDialog(_ptrNppTbData);
            SetToolbarCommandActivatedState(SNDialogStruct.FormCommandId, true);
            ((FrmSNDlg)SNDialogStruct.Form).UpdateSNListBox();
        }

        /// <summary>
        /// Send message to Npp to add dockable-form-starting command to toolbar.
        /// </summary>
        /// <param name="form">Contains parameters for menu command starting dockable form.</param>
        private static void AddNppToolbarIcon(DockableFormCommandStruct form)
        {
            toolbarIcons tbIcons = new toolbarIcons
            {
                hToolbarBmp = form.ToolbarButtonBmp.GetHbitmap()
            };
            IntPtr pTbIcons = GetStructPointer(tbIcons);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[form.FormCommandId]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        /// <summary>
        /// Send message to Npp to add command to toolbar.
        /// </summary>
        /// <param name="toolbarButtonBmp">Bitmap for toolbar icon.</param>
        /// <param name="commandId">Index of plugin command.</param>
        private static void AddNppToolbarIcon(Bitmap toolbarButtonBmp, int commandId)
        {
            toolbarIcons tbIcons = new toolbarIcons
            {
                hToolbarBmp = toolbarButtonBmp.GetHbitmap()
            };
            IntPtr pTbIcons = GetStructPointer(tbIcons);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[commandId]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        /// <summary>
        /// Send message to Npp to set Visible property of control.
        /// </summary>
        /// <param name="handle">Handle of control.</param>
        /// <param name="visible">Visibility value.</param>
        private static void SetDockableDialogVisibility(IntPtr handle, bool visible)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, visible ? (uint)NppMsg.NPPM_DMMSHOW : (uint)NppMsg.NPPM_DMMHIDE, 0, handle);
        }

        /// <summary>
        /// Send message to Npp to set checked/unchecked state of menu and toolbar command.
        /// </summary>
        /// <param name="dialogId">Index of plugin command to control button.</param>
        /// <param name="pressed">Checked state value.</param>
        private static void SetToolbarCommandActivatedState(int dialogId, bool pressed)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[dialogId]._cmdID, pressed ? 1 : 0);
        }

        /// <summary>
        /// Send message to Npp to register dockable dialog.
        /// </summary>
        /// <param name="_ptrNppTbData">Pointer to structure with dialog parameters.</param>
        private static void RegisterNppDockableDialog(IntPtr _ptrNppTbData)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
        }

        /// <summary>
        /// Creates structure with dialog parameters and returns pointer on that structure.
        /// </summary>
        /// <param name="form">Form to describe</param>
        /// <param name="shownName">Name to show at form caption</param>
        /// <param name="dialogId">Plugin command index</param>
        /// <param name="settings">Bit-mask with window settings</param>
        private static IntPtr CreateNppToolbarDataPointer(Form form, string shownName, int dialogId, NppTbMsg settings, Icon icon)
        {
            NppTbData _nppTbData = new NppTbData
            {
                hClient = form.Handle,
                pszName = shownName,
                dlgID = dialogId,
                uMask = settings,
                hIconTab = (uint)icon.Handle,
                pszModuleName = PluginName
            };
            return GetStructPointer(_nppTbData);
        }

        /// <summary>
        /// Returns pointer on structure.
        /// </summary>
        /// <param name="structure">Structure, to get pointer to.</param>
        private static IntPtr GetStructPointer(object structure)
        {
            IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, pointer, false);
            return pointer;
        }

        /// <summary>
        /// Sets up and returns icon object based on passed bitmap.
        /// </summary>
        /// <param name="bitmap">Bitmap object fot icon.</param>
        private static Icon InitializeDockableDialogTabIcon(Bitmap bitmap)
        {
            using (Bitmap newBmp = new Bitmap(16, 16))
            {
                Graphics g = Graphics.FromImage(newBmp);
                ColorMap[] colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap
                {
                    OldColor = Color.Fuchsia,
                    NewColor = Color.FromKnownColor(KnownColor.ButtonFace)
                };
                ImageAttributes attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);
                g.DrawImage(bitmap, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                return Icon.FromHandle(newBmp.GetHicon());
            }
        }

        /// <summary>
        /// Parse string-setting received from plugin INI-file into list of VerticalLine objects.
        /// </summary>
        /// <param name="param">String with space-splitted values "position|color".</param>
        private static List<VerticalLine> ParseIniVerticalLinesParam(string param)
        {
            List<VerticalLine> result = new List<VerticalLine>();
            string[] linesStrings = param.Split(' ');
            foreach (string line in linesStrings)
            {
                if (line != "0|0")
                {
                    string[] lineParam = line.Split('|');
                    result.Add(new VerticalLine() { Position = int.Parse(lineParam[0]), Color = int.Parse(lineParam[1]) });
                }
                
            }
            return result;
        }

        /// <summary>
        /// Convert list of VerticalLine objects into string (space-splitted values "position|color").
        /// </summary>
        /// <param name="verticalLines">List of VerticalLine objects.</param>
        private static string GetVerticalLinesParamString(List<VerticalLine> verticalLines)
        {
            string res = "";
            foreach (var line in verticalLines)
            {
                res += line.Position + "|" + line.Color + " ";
            }
            return res.Trim();
        }
    }

    /// <summary>
    /// Represents settings list, required for creating dockable dialog in Npp.
    /// </summary>
    public struct DockableFormCommandStruct
    {
        public int FormCommandId;
        public Form Form;
        public Bitmap ToolbarButtonBmp;
        public Bitmap DockingTabIconBmp;
        public Icon DockingTabIcon;
    }

    /// <summary>
    /// Describes minimal set of data for a vertical line, required for its drawing (position and color).
    /// </summary>
    public struct VerticalLine
    {
        public int Position;
        public int Color;
    }
}