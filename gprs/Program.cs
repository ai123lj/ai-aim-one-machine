using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gprs
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //WinIOLab.Initialize(); // ע��
            Application.Run(new Form1());
            //KeyboardHook.Start();
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //WinIOLab.Shutdown(); // �����ע��
            //KeyboardHook.Stop();
            // �������д��Ҫ�ڳ���ر�ʱִ�еĴ���
            // Console.WriteLine("�����˳�ʱ�Ĵ���");

        }
    }
}
