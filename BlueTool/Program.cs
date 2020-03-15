using System;
using System.Reflection;
using System.Windows.Forms;

namespace com.DanTheMan827.BlueTool
{
    static class Program
    {
        public static Version AppVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static string AppDisplayVersion
        {
            get
            {
                var version = AppVersion;

                return $"{version.Major}.{version.Minor}";
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
