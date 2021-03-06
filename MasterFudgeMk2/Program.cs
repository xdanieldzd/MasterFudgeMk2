﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MasterFudgeMk2
{
    static class Program
    {
        public static readonly string UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.ProductName);
        public static readonly string XmlDocumentPath = Path.Combine(AppContext.BaseDirectory, "XML");
        public static readonly string NoIntroDatPath = Path.Combine(XmlDocumentPath, "NoIntro");

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
