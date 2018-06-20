using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
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
        private const int MINIMIZED = 1;
        private const int MAXIMIZED = 2;
        private const int RESTORE = 9;


        private List<WindowParams> mWindowsParams;

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
            TxtLog.Clear();
            mWindowsParams = new List<WindowParams>();

            String txt = "SAVE:" + Environment.NewLine;
            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    IntPtr hwnd = process.MainWindowHandle;
                    GetWindowRect(hwnd, out Rectangle rect);
                    if (rect.Top == 0 && rect.Left == 0)
                        continue;

                    if (process.ProcessName == "ApplicationFrameHost")
                        continue;

                    if (rect.Top > -1000) // Else assumed minimized
                    {
                        txt += String.Format("Saving process '{0}' (id {1})(title '{2}': ",
                            process.ProcessName,
                            process.Id,
                            process.MainWindowTitle);

                        txt += "Top=" + rect.Top;
                        txt += ",Left=" + rect.Left;
                        txt += ",Height=" + rect.Height;
                        txt += ",Width=" + rect.Width;
                        txt += Environment.NewLine;

                        mWindowsParams.Add(new WindowParams()
                        {
                            processName = process.ProcessName,
                            processId = process.Id,
                            top = rect.Top,
                            left = rect.Left,
                            width = rect.Width,
                            height = rect.Height
                        });
                    }
                }
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

            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (String.IsNullOrEmpty(process.MainWindowTitle))
                    continue;

                WindowParams winParam = mWindowsParams.Find(param => param.processName == process.ProcessName);
                //WindowParams winParam = mWindowsParams.Find(param => param.processId == process.Id);
                if (winParam == null)
                {
                    //txt += String.Format("Process'{0}' not found" + Environment.NewLine, process.ProcessName);
                    continue;
                }

                txt += String.Format("Process'{0}' restoring back to {1}, {2}, {3}, {4}" + Environment.NewLine,
                    winParam.processName,
                    winParam.left,
                    winParam.top,
                    winParam.width - winParam.left,
                    winParam.height - winParam.top);

                IntPtr hwnd = process.MainWindowHandle;
                ShowWindow(hwnd, RESTORE);
                MoveWindow(hwnd, winParam.left, winParam.top, winParam.width - winParam.left, winParam.height - winParam.top, true);
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
