using System;
using System.Windows.Forms;

namespace Comparer {
    static class Program {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            formMain mainForm = new formMain();
            if (args.Length > 0) mainForm.File1 = args[0];
            if (args.Length > 1) mainForm.File2 = args[1];
            if (args.Length > 2) {
                for (int i = 2; i < args.Length; i++) {
                    string arg = args[i];
                    if (arg == "-i") mainForm.IgnoreSpaces = true;
                    if (arg == "-n") mainForm.NoChangesInSpaces = true;
                    if (arg == "-s") mainForm.Semantic = true;
                }
            }
            Application.Run(mainForm);
        }
    }
}
