using oop_project.forms;
using System;
using System.Windows.Forms;

namespace oop_project
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Welcome());
        }
    }
}
