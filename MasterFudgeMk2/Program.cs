using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MasterFudgeMk2
{
    static class Program
    {
        static bool tempNewUi = true;

        public static readonly string UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.ProductName + (tempNewUi ? "_test" : string.Empty));

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(tempNewUi ? (new NewMainForm() as Form) : (new MainForm() as Form));
        }
    }
}
