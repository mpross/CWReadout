using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace CWReadout
{
    class EmailError
    {
        public static void emailAlert(Exception ex)
        {
            StringBuilder words = new StringBuilder();

            string[] emailList = ConfigurationManager.AppSettings["emailList"].Split(',');
            String location = ConfigurationManager.AppSettings["location"];
            String fromEmail = "coldwashalert@gmail.com";
            String user = "coldwashalert";
            String pass = "F=Gmm/r^2";

            words.Append("Message: ");
            words.Append(ex.Message);
            words.Append("\n ");
            words.Append("Source: ");
            words.Append(ex.Source);
            words.Append("\n ");
            words.Append("Stack Trace: ");
            words.Append(ex.StackTrace);
            words.Append("\n ");
            words.Append("Target: ");
            words.Append(ex.TargetSite);
            words.Append("\n ");

            
            try
            {
                string dir = Form1.curDirec + "\\CWLog.txt";
                StreamWriter w = File.AppendText(dir);
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  :");
                w.WriteLine("  :{0}", words.ToString());
                w.WriteLine("-------------------------------");
                w.Flush();
            }
            catch (Exception){ }
            foreach (string toEmail in emailList)
            {
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add(toEmail);
                message.Subject = location + " Error Alert";
                message.From = new System.Net.Mail.MailAddress(fromEmail);
                message.Body = words.ToString();
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
                smtp.Port = 25;
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential(user, pass);
                smtp.Send(message);
            }
        }
    }
}
