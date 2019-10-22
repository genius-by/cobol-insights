using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace Kbg.NppPluginNET
{
    class Main
    {
        internal const string PluginName = "COBOLInsights";
        static string iniFilePath = null;
        static bool ShowVerticalLines = false;
        static List<VerticalLine> VerticalLines = new List<VerticalLine>();
        static Bitmap ToggleVerticalLinesBmp = COBOLInsights.Properties.Resources.toggle_vertical_lines;
        static int ToggleVerticalLinesCommandId =-1;
        static Bitmap AddVerticalLineBmp = COBOLInsights.Properties.Resources.add_vertical_line;
        static int AddVerticalLineCommandId = -1;
        static Bitmap ClearVerticalLinesBmp = COBOLInsights.Properties.Resources.clear_vertical_lines;
        static int ClearVerticalLinesCommandId = -1;
        static DockableFormCommandStruct SNDialogStruct = new DockableFormCommandStruct()
        {
            Form = null,
            FormCommandId = -1,
            ToolbarButtonBmp = COBOLInsights.Properties.Resources.list_icon,
            DockingTabIconBmp = COBOLInsights.Properties.Resources.list_icon,
            DockingTabIcon = null
        };

        static IScintillaGateway Editor1 = new ScintillaGateway(PluginBase.nppData._scintillaMainHandle);
        static IScintillaGateway Editor2 = new ScintillaGateway(PluginBase.nppData._scintillaSecondHandle);

        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
            // use eg. as
            if (notification.Header.Code == (uint)NppMsg.NPPN_BUFFERACTIVATED && SNDialogStruct.Form != null)
            {
                ((FrmSNDlg)SNDialogStruct.Form).UpdateSNListBox();
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
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");
            ShowVerticalLines = (Win32.GetPrivateProfileInt("COBOLInsights", "ShowVerticalLines", 0, iniFilePath) != 0);
            StringBuilder sbVerticalLinesParam = new StringBuilder(150);
            Win32.GetPrivateProfileString("COBOLInsights", "VerticalLines", "0|0", sbVerticalLinesParam, 150, iniFilePath);
            VerticalLines = ParseIniVerticalLinesParam(sbVerticalLinesParam.ToString());

            PluginBase.SetCommand(0, "Show vertical lines", ShowMultipleVerticalLines, new ShortcutKey(false, false, false, Keys.None), ShowVerticalLines);
            ToggleVerticalLinesCommandId = 0;

            PluginBase.SetCommand(1, "Add vertical line...", AddVerticalLine, new ShortcutKey(false, false, false, Keys.None));
            AddVerticalLineCommandId = 1;
            PluginBase.SetCommand(2, "Clear all vertical lines", ClearAllVerticalLines, new ShortcutKey(false, false, false, Keys.None));
            ClearVerticalLinesCommandId = 2;
            PluginBase.SetCommand(3, "---", null);
            PluginBase.SetCommand(4, "Source Navigation Panel", SourceNavigationDialog); SNDialogStruct.FormCommandId = 4;
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
            
        }

        /// <summary>
        /// Method used by NPP, when by closing to run any necessary code before plugin is unloaded (i.e. save settings).
        /// </summary>
        internal static void PluginCleanUp()
        {
            Win32.WritePrivateProfileString("COBOLInsights", "ShowVerticalLines", ShowVerticalLines ? "1" : "0", iniFilePath);
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
                SetToolbarCommandActivatedState(0, true);
            }
            else
            {
                Editor1.MultiEdgeClearAll();
                Editor2.MultiEdgeClearAll();
                SetToolbarCommandActivatedState(0, false);
            }
        }

        /// <summary>
        /// Create and show form for adding a single vertical line.
        /// </summary>
        private static void AddVerticalLine()
        {
            FrmAddVerticalLine frmAddVerticalLine = new FrmAddVerticalLine();
            var res = frmAddVerticalLine.ShowDialog();
            if (res == DialogResult.OK && frmAddVerticalLine.Position > 0)
            {
                VerticalLine line = new VerticalLine() { Position = frmAddVerticalLine.Position, Color = frmAddVerticalLine.Colour.Value };
                VerticalLines.Add(line);
                Editor1.MultiEdgeAddLine(line.Position, new Colour(line.Color));
                Editor2.MultiEdgeAddLine(line.Position, new Colour(line.Color));
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
        /// <param name="dialogId">Index od plugin command to control dialog.</param>
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