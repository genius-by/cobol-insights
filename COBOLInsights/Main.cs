using System;
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
        //static bool someSetting = false;
        static frmSNDlg frmSNDlg = null;
        static int idSNDlg = -1;
        static Bitmap tbBmp = COBOLInsights.Properties.Resources.list_icon;
        static Bitmap tbBmp_tbTab = COBOLInsights.Properties.Resources.list_icon;
        static Icon tbIcon = null;
        

        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
            // use eg. as
            if (notification.Header.Code == (uint)NppMsg.NPPN_BUFFERACTIVATED && frmSNDlg != null)
            {
                frmSNDlg.UpdateSNListBox();
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
            //someSetting = (Win32.GetPrivateProfileInt("SomeSection", "SomeKey", 0, iniFilePath) != 0);

            PluginBase.SetCommand(0, "MyMenuCommand", MyMenuFunction, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "Source Navigation", SourceNavigationDialog); idSNDlg = 1;
        }

        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons
            {
                hToolbarBmp = tbBmp.GetHbitmap()
            };
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idSNDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        internal static void PluginCleanUp()
        {
            //Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        }


        internal static void MyMenuFunction()
        {
            _ = MessageBox.Show("Hello N++!");
        }

        internal static void SourceNavigationDialog()
        {
            if (frmSNDlg == null)
            {
                frmSNDlg = new frmSNDlg();
                InitializeDockableDialogTabIcon(tbBmp_tbTab);

                IntPtr _ptrNppTbData = CreateNppToolbarDataPointer(frmSNDlg,
                                                                   "Source Navigation",
                                                                   idSNDlg,
                                                                   NppTbMsg.DWS_DF_CONT_LEFT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                                                                   tbIcon);
                RegisterNppDockableDialog(_ptrNppTbData);
                SetToolbarCommandActivatedState(idSNDlg, true);
                frmSNDlg.UpdateSNListBox();
            }
            else
            {
                if (!frmSNDlg.Visible)
                {
                    SetDockableDialogVisibility(frmSNDlg.Handle, true);
                    SetToolbarCommandActivatedState(idSNDlg, true);
                }
                else
                {
                    SetDockableDialogVisibility(frmSNDlg.Handle, false);
                    SetToolbarCommandActivatedState(idSNDlg, false);
                }
            }
        }

        private static void SetDockableDialogVisibility(IntPtr handle, bool visible)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, visible ? (uint)NppMsg.NPPM_DMMSHOW : (uint)NppMsg.NPPM_DMMHIDE, 0, handle);
        }

        private static void SetToolbarCommandActivatedState(int dialogId, bool pressed)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[dialogId]._cmdID, pressed ? 1 : 0);
        }

        private static void RegisterNppDockableDialog(IntPtr _ptrNppTbData)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
        }

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
            IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
            Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);
            return _ptrNppTbData;
        }

        private static void InitializeDockableDialogTabIcon(Bitmap bitmap)
        {
            using (Bitmap newBmp = new Bitmap(16, 16))
            {
                Graphics g = Graphics.FromImage(newBmp);
                ColorMap[] colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = Color.Fuchsia;
                colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                ImageAttributes attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);
                g.DrawImage(bitmap, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                tbIcon = Icon.FromHandle(newBmp.GetHicon());
            }
        }
    }
}