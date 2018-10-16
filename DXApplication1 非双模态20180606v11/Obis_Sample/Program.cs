using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using System.IO;

namespace Obis_Sample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string result = Environment.GetEnvironmentVariable("USERPROFILE");
            string patient_path = result + "\\Documents\\orbis\\Data\\";//检查data文件夹
            if (!Directory.Exists(patient_path))
            {
                Directory.CreateDirectory(patient_path);
            }
            patient_path = result + "\\Documents\\orbis\\Doctor\\";//检查doctor文件夹
            if (!Directory.Exists(patient_path))
            {
                Directory.CreateDirectory(patient_path);
            }
            patient_path = result + "\\Documents\\orbis\\Log\\";
            if (!Directory.Exists(patient_path))
            {
                Directory.CreateDirectory(patient_path);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            BonusSkins.Register();
            SkinManager.EnableFormSkins();
            Application.Run(new Mainform());
        }
    }
}
