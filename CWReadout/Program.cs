/**     CWReadout
 **     
 **     Michael Ross
 **     Eot-Wash Group
 **     Center for Experimental Nuclear Physics and Astrophysics (CENPA)
 **     University of Washington
 **
 **     Developed 2021
 *
 **/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace CWReadout
{
    
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Application.Run(new Form1());
        }
        public static void Main2()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            var graphForm= new Form2();
            Application.Run();
        }
    }
}
