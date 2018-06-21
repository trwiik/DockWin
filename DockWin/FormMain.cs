using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DockWin
{
    public partial class FormMain : Form
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);
        [DllImport("user32.dll")]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        private const int HIDE = 0;
        //private const int MINIMIZED = 1;
        private const int MINIMIZED = 2; // Show minimized
        //private const int MAXIMIZED = 2;
        private const int MAXIMIZED = 3; // Show maximized
        //private const int RESTORE = 9;
        private const int RESTORE = 1; // Show normal

        //private static List<IntPtr> hWindList;
        private static List<WindowParams> mWindowsParams;


        /*TEST*/
        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll")]
        protected static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        protected static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
        {
            int size = GetWindowTextLength(hWnd);
            if (size++ > 0 && IsWindowVisible(hWnd))
            {
                StringBuilder sb = new StringBuilder(size);
                GetWindowThreadProcessId(hWnd, out uint processId);
                GetWindowText(hWnd, sb, size);
                String title = sb.ToString();
                //Console.WriteLine("PID=" + processId + ": " + sb.ToString());
                //hWindList.Add(hWnd);
                Process p = Process.GetProcessById((int)processId);
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(hWnd, ref placement);
                int windowState = RESTORE; // Normal
                switch (placement.showCmd)
                {
                    case 0:
                        windowState = HIDE;
                        break;
                    case 2:
                        windowState = MINIMIZED;
                        break;
                    case 3:
                        windowState = MAXIMIZED;
                        break;
                }
                GetWindowRect(hWnd, out Rectangle rect);
                mWindowsParams.Add(new WindowParams()
                {
                    hWndPtr = hWnd,
                    processName = "",
                    title = title,
                    processId = processId,
                    rect = rect,
                    windowState = windowState
                });
            }
            return true;
        }
        /*TEST END*/


        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            HideApp();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveWindows();
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            RestoreWindows();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            ExitApp();
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                HideApp();
            }
            else if (WindowState == FormWindowState.Normal)
            {
                RestoreApp();
            }
        }

        private void HideApp()
        {
            NotifyIcon.Visible = true;
            //NotifyIcon.ShowBalloonTip(250);
            Hide();
        }

        private void RestoreApp()
        {
            Show();
            WindowState = FormWindowState.Normal;
            NotifyIcon.Visible = false;
        }

        private void notMenuSave_Click(object sender, EventArgs e)
        {
            SaveWindows();
        }
    
        private void notMenuRestore_Click(object sender, EventArgs e)
        {
            RestoreWindows();
        }

        private void notMenuExit_Click(object sender, EventArgs e)
        {
            ExitApp();
        }

        private void SaveWindows()
        {
            mWindowsParams = new List<WindowParams>();
            EnumWindows(new EnumWindowsProc(EnumTheWindows), IntPtr.Zero);

            // Has now hWindList

            TxtLog.Clear();
//            mWindowsParams = new List<WindowParams>();

            String txt = "SAVE:" + Environment.NewLine;
            txt += "Num windows: " + mWindowsParams.Count + Environment.NewLine;
            //var processlist = Process.GetProcesses()
            //    .Where(p => p.MainWindowHandle != IntPtr.Zero);

            //foreach (Process process in processlist)
            //{
            foreach (WindowParams winParam in mWindowsParams)
            { 
                //if (!String.IsNullOrEmpty(process.MainWindowTitle))
                //{
                    //IntPtr hwnd = process.MainWindowHandle;
                    //GetWindowRect(winParam.hWndPtr, out Rectangle rect);
                    //if (winParam.rect.Top == 0 && winParam.rect.Left == 0)
                    //    continue;

                    //if (process.ProcessName == "ApplicationFrameHost")
                    //    continue;

                    //if (winParam.rect.Top > -1000) // Else assumed minimized
                    //{
                    //txt += String.Format("Saving process '{0}' (id {1})(title '{2}': ",
                    //    process.ProcessName,
                    //    process.Id,
                    //    process.MainWindowTitle);

                        txt += "Title '" + winParam.title + "': ";
                        txt += "Top=" + winParam.rect.Top;
                        txt += ",Left=" + winParam.rect.Left;
                        txt += ",Height=" + winParam.rect.Height;
                        txt += ",Width=" + winParam.rect.Width;
                        txt += ",winState=" + winParam.windowState;
                        txt += Environment.NewLine;

                        //mWindowsParams.Add(new WindowParams()
                        //{
                        //    hWndPtr = hwnd,
                        //    processName = "",
                        //    processId = 0,
                        //    top = rect.Top,
                        //    left = rect.Left,
                        //    width = rect.Width,
                        //    height = rect.Height
                        //});
                    //}
                //}
            }

            TxtLog.Text = txt;
        }

        private void RestoreWindows()
        {
            String txt = Environment.NewLine + "RESTORE:" + Environment.NewLine;
            if (mWindowsParams == null || mWindowsParams.Count == 0)
            {
                txt += "No windows saved";
                TxtLog.Text += txt;
                return;
            }

            //Process[] processlist = Process.GetProcesses();
            //foreach (Process process in processlist)
            foreach (WindowParams winParam in mWindowsParams)
            {
                //if (String.IsNullOrEmpty(process.MainWindowTitle))
                //    continue;

                //WindowParams winParam = mWindowsParams.Find(param => param.processName == process.ProcessName);
                //WindowParams winParam = mWindowsParams.Find(param => param.processId == process.Id);
                //WindowParams winParam = mWindowsParams.Find(param => param.hWndPtr == hwnd);
                //if (winParam == null)
                //{
                    //txt += String.Format("Process'{0}' not found" + Environment.NewLine, process.ProcessName);
                //    continue;
                //}

                txt += String.Format("Title '{0}' restoring back to {1}, {2}, {3}, {4}, {5}" + Environment.NewLine,
                    winParam.title,
                    winParam.rect.Left,
                    winParam.rect.Top,
                    winParam.rect.Width - winParam.rect.Left,
                    winParam.rect.Height - winParam.rect.Top,
                    winParam.windowState);

                //IntPtr hwnd = process.MainWindowHandle;
                if (winParam.windowState == HIDE)
                    continue;

                MoveWindow(winParam.hWndPtr, winParam.rect.Left, winParam.rect.Top, winParam.rect.Width - winParam.rect.Left, winParam.rect.Height - winParam.rect.Top, true);
                ShowWindow(winParam.hWndPtr, winParam.windowState);

                //if (winParam.windowState == RESTORE)
                //{
                //    MoveWindow(winParam.hWndPtr, winParam.rect.Left, winParam.rect.Top, winParam.rect.Width - winParam.rect.Left, winParam.rect.Height - winParam.rect.Top, true);
                //    ShowWindow(winParam.hWndPtr, winParam.windowState);
                //}
                //else if (winParam.windowState == MAXIMIZED)
                //{
                //    ShowWindow(winParam.hWndPtr, winParam.windowState);
                //    MoveWindow(winParam.hWndPtr, winParam.rect.Left, winParam.rect.Top, 1280, 720, false);
                //}
                //else if (winParam.windowState == MINIMIZED)
                //{
                //    ShowWindow(winParam.hWndPtr, winParam.windowState);
                //}
            }
            TxtLog.Text += txt;
        }

        private void ExitApp()
        {
            Application.Exit();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreApp();
        }

        private void notMenuOpenApp_Click(object sender, EventArgs e)
        {
            RestoreApp();
        }
    }
}
