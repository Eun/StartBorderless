using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace StartBorderless
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        
            public int Top;        
            public int Right;      
            public int Bottom;      
        }


        // constants
        const int GWL_STYLE = -16;
        const short SWP_NOMOVE = 0x0002;
        const short SWP_NOSIZE = 0x0001;
        const int SWP_FRAMECHANGED = 0x0020;

        const uint WS_THICKFRAME = 0x00040000;
        const uint WS_SYSMENU = 0x00080000;
        const uint WS_MAXIMIZE = 0x01000000;
        const uint WS_MINIMIZE = 0x20000000;
        const uint WS_CAPTION = 0x00C00000;

        static void Main(string[] args)
        {
            new Run(args);
        }


        class Run
        {
            public Run(string[] args)
            {
                if (args.Length < 1)
                {
                    MessageBox.Show(Path.GetFileName(Process.GetCurrentProcess().ProcessName) + " [D=1000] [X=0] [Y=0] <Programm>\n\nD: Delay in ms before removing the border\nX: X Position (leave it out to keep current position)\nY: Y Position (leave it out to keep current position)");
                    return;
                }

                Process process = Process.Start(args[args.Length-1]);

                int X = 0;
                int Y = 0;
                bool XSet, YSet;
                XSet = YSet = false;
                int Delay = 1000;
                if (args.Length > 1)
                {
                    for (int i = 0; i < args.Length-1; i++)
                    {
                        if (args[i].StartsWith("X="))
                        {
                            try
                            {
                                X = Convert.ToInt32(args[i].Substring(2));
                                XSet = true;
                            }
                            catch
                            {
                            }
                        }
                        else if (args[i].StartsWith("Y="))
                        {
                            try
                            {
                                Y = Convert.ToInt32(args[i].Substring(2));
                                YSet = true;
                            }
                            catch
                            {
                            }
                        }
                        else if (args[i].StartsWith("D="))
                        {
                            try
                            {
                                Delay = Convert.ToInt32(args[i].Substring(2));
                                
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                Thread.Sleep(Delay);

                uint lStyle = GetWindowLong(process.MainWindowHandle, GWL_STYLE);
                lStyle &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU);
                SetWindowLong(process.MainWindowHandle, GWL_STYLE, lStyle);

                if (!XSet && !YSet)
                {
                    SetWindowPos(process.MainWindowHandle, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);
                }
                else
                {
                    RECT rct;
                    if (!XSet || !YSet)
                    {
                        GetWindowRect(new HandleRef(this, process.MainWindowHandle), out rct);
                        if (!XSet)
                            X = rct.Left;
                        if (!YSet)
                            Y = rct.Top;
                    }
                    SetWindowPos(process.MainWindowHandle, 0, X, Y, 0, 0, SWP_NOSIZE | SWP_FRAMECHANGED);
                }
            }
        }
    }
}
