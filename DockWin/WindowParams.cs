using System;
using System.Drawing;

namespace DockWin
{
    class WindowParams
    {
        public IntPtr hWndPtr;
        public String processName;
        public String title;
        public uint processId;
        public Rectangle rect;
        //public int top;
        //public int left;
        //public int width;
        //public int height;
        public int windowState;
    }
}
