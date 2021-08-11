using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

/*
 *  This class writes down data to the main run ouput file
 */
class DataWriting
    {
        const string DataFilePreFix = "BRS_";
        const string DataFileSufFix = ".dat";

        bool DataWritingOn = false;
        System.IO.FileStream gfdata;
        String name;
        DateTime CurTime;
        double curday;
        int openday;
        char ch;
        public DataWriting()
        {
            string DataDir = Path.GetDirectoryName(Application.ExecutablePath);
            DataDir = DataDir + "\\Data";
            if (!Directory.Exists(DataDir))
            {
                Directory.CreateDirectory(DataDir);
            }
            if (DataWritingOn == true) return;
            DataWritingOn = true;
            CurTime = DateTime.Now;
            curday = DayNr(CurTime);
            openday = (int)curday;
            ch = 'a';

            name = DataDir + "\\" + DataFilePreFix + CurTime.Year.ToString() + "_" + openday.ToString() + ch.ToString() + DataFileSufFix;
            while (System.IO.File.Exists(name))
            {
                ch++;
                name = DataDir + "\\" + DataFilePreFix + CurTime.Year.ToString() + "_" + openday.ToString() + ch.ToString() + DataFileSufFix;
            }
            gfdata = new FileStream(name, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        }


        public void Write(double time, double[] data)
        {
            String ds3;
            byte[] info;
            int i;
            if (DataWritingOn == true)
            {

                ds3 = time.ToString("F9");
                for (i = 0; i< data.Length;i++) {
                    ds3 = ds3 + " " + data[i].ToString();
                }
                info = new System.Text.UTF8Encoding(true).GetBytes(ds3);
                gfdata.Write(info, 0, info.Length);
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                gfdata.Write(newline, 0, newline.Length);
                gfdata.Flush();
            }

        }
        public void stopit()
        {
            DataWritingOn = false;
            gfdata.Close();
           
        }
    




        double DayNr(DateTime Input)
        {
            double a, frac;
            int mon;
            int leapyear;
            mon = Input.Month;
            a = 0;
            leapyear = 0;
            if ((Input.Year % 4 == 0) && (Input.Year % 100 != 0)) leapyear = 1;
            if (mon > 1) a = a + 31;
            if ((mon > 2) && (leapyear == 0)) a = a + 28;
            if ((mon > 2) && (leapyear == 1)) a = a + 29;
            if (mon > 3) a = a + 31;
            if (mon > 4) a = a + 30;
            if (mon > 5) a = a + 31;
            if (mon > 6) a = a + 30;
            if (mon > 7) a = a + 31;
            if (mon > 8) a = a + 31;
            if (mon > 9) a = a + 30;
            if (mon > 10) a = a + 30;
            if (mon > 11) a = a + 31;
            a = a + Input.Day;
            frac = (Input.Hour + Input.Minute / 60.0 + Input.Second / 3600.0) / 24.0;
            return a + frac;
        }

    }


    

