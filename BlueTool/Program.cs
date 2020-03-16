using System;
using System.Reflection;
using System.Windows.Forms;

/*
    Copyright 2020 DanTheMan827

    This file is part of BlueTool.

    BlueTool is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BlueTool is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BlueTool.  If not, see <https://www.gnu.org/licenses/>.
*/

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
