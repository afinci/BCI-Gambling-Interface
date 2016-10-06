using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace deneme1
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(String DllName);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, String ProcName);

        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool InitializeWinIoType();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SetPortValType(UInt16 PortAddr, UInt32 PortVal, UInt16 Size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ShutdownWinIoType();

        IntPtr hMod;

        UInt16 address = 16376; //Port address of Lenovo T530 for PCI Parallel Port, check EEG Based BCI Experiments Manual for further info

        int trial = 100;             // total number of trials
        int balance = 10000;          // $500
        int clickLimit = 2;         // 2 sec wait after click - timer1
        int clickCounter = 0;
        int clickEvent = 0;         // which card is chosen 1-2-3-4
        int showLimit = 2;          // 2 sec show selected card response  - timer2
        int showCounter = 0;
        int balanceLimit = 2;          // 2 sec show balance  - timer3
        int balanceCounter = 0;

        public Form1()
        {
            InitializeComponent();

            var height = Screen.PrimaryScreen.Bounds.Height;
            var width = Screen.PrimaryScreen.Bounds.Width;

            pictureBox2.Height = height / 2;
            pictureBox4.Height = height / 2;
            pictureBox6.Height = height / 2;
            pictureBox8.Height = height / 2;

            pictureBox2.Width = 5 * width / 27;
            pictureBox4.Width = 5 * width / 27;
            pictureBox6.Width = 5 * width / 27;
            pictureBox8.Width = 5 * width / 27;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Check pointer size to determine dll type
            if (IntPtr.Size == 4)
            {
                hMod = LoadLibrary("WinIo32.dll");
            }
            else if (IntPtr.Size == 8)
            {
                hMod = LoadLibrary("WinIo64.dll");
            }
            //Error Message
            if (hMod == IntPtr.Zero)
            {
                MessageBox.Show("Can't find WinIo dll");
                this.Close();
            }
            IntPtr pFunc = GetProcAddress(hMod, "InitializeWinIo");
            if (pFunc != IntPtr.Zero)
            {
                InitializeWinIoType InitializeWinIo = (InitializeWinIoType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitializeWinIoType));
                bool Result = InitializeWinIo();
                if (!Result)
                {
                    MessageBox.Show("Error returned from InitializeWinIo.\nMake sure you are running with administrative privileges and that the WinIo library files are located in the same directory as your executable file.");
                    FreeLibrary(hMod);
                    this.Close();
                }
            }

            label1.Visible = true;
            label1.Text = "BALANCE: $" + balance + " TRIAL: " + trial;

            IntPtr send = GetProcAddress(hMod, "SetPortVal");
            SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
            SetPortVal(address, 120, 1);

            timer3.Start();
        }

        private void pictureBox6_Click(object sender, EventArgs e) // 25 
        {
            IntPtr send = GetProcAddress(hMod, "SetPortVal");
            SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
            SetPortVal(address, 50, 1);

            clickEvent = 2;
            timer1.Start();
        }

        private void pictureBox8_Click(object sender, EventArgs e) // 100
        {
            IntPtr send = GetProcAddress(hMod, "SetPortVal");
            SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
            SetPortVal(address, 100, 1);

            clickEvent = 4;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            clickCounter = clickCounter + 1;

            if (clickCounter >= clickLimit)
            {
                timer1.Stop();
                clickCounter = 0;

                Random rnd = new Random();
                int result = rnd.Next(10);
                trial--;

                if (clickEvent == 2)
                {
                    if (result <= 6) // win %70
                    {
                        IntPtr send = GetProcAddress(hMod, "SetPortVal");
                        SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                        SetPortVal(address, 150, 1);

                        pictureBox2.BackColor = System.Drawing.Color.Green;
                        pictureBox6.Visible = false;
                        balance = balance + 25;
                    }
                    else // lose
                    {
                        IntPtr send = GetProcAddress(hMod, "SetPortVal");
                        SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                        SetPortVal(address, 200, 1);

                        pictureBox2.BackColor = System.Drawing.Color.Red;
                        pictureBox6.Visible = false;
                        balance = balance - 25;
                    }
                }

                else if (clickEvent == 4)
                {
                    if (result <= 6) // win %70
                    {
                        IntPtr send = GetProcAddress(hMod, "SetPortVal");
                        SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                        SetPortVal(address, 180, 1);

                        pictureBox4.BackColor = System.Drawing.Color.Green;
                        pictureBox8.Visible = false;
                        balance = balance + 100;
                    }
                    else // lose
                    {
                        IntPtr send = GetProcAddress(hMod, "SetPortVal");
                        SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                        SetPortVal(address, 230, 1);

                        pictureBox4.BackColor = System.Drawing.Color.Red;
                        pictureBox8.Visible = false;
                        balance = balance - 100;
                    }
                }

                timer2.Start();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            showCounter = showCounter + 1;

            if (showCounter >= showLimit)
            {
                timer2.Stop();
                showCounter = 0;

                pictureBox6.Visible = true;
                pictureBox8.Visible = true;

                label1.Visible = true;
                label1.Text = "BALANCE: $" + balance + " TRIAL: " + trial;
                
                IntPtr send = GetProcAddress(hMod, "SetPortVal");
                SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                SetPortVal(address, 120, 1);

                timer3.Start();
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            balanceCounter = balanceCounter + 1;

            if (balanceCounter >= balanceLimit)
            {
                timer3.Stop();
                balanceCounter = 0;

                label1.Visible = false;

                IntPtr send = GetProcAddress(hMod, "SetPortVal");
                SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                SetPortVal(address, 10, 1);

                if (trial<=0)
                {
                    SetPortVal(address, 250, 1);
                    Application.Exit();
                }
            }
        }
    }
}