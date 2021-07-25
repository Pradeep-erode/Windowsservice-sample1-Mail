using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Mail_sending_service
{
    public partial class Service1 : ServiceBase
    {
        private string timeString;

        int getCallType = 0;
        Timer Timer1 = new Timer();
        public Service1()
        {
            InitializeComponent();
            int strTime = Convert.ToInt32(ConfigurationManager.AppSettings["callDuration"]);
            getCallType = Convert.ToInt32(ConfigurationManager.AppSettings["CallType"]);
            if (getCallType == 1)
            {
                Timer Timer1 = new System.Timers.Timer();
                double inter = (double)GetNextInterval();
                Timer1.Interval = inter;
                Timer1.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
            }
            else
            {
                Timer1 = new System.Timers.Timer();
                Timer1.Interval = strTime * 1000;
                Timer1.Elapsed += new ElapsedEventHandler(ServiceTimer_Tick);
            }
        }

        public Timer Timer { get; }
        private double GetNextInterval()
        {
            timeString = ConfigurationManager.AppSettings["StartTime"];
            DateTime t = DateTime.Parse(timeString);
            TimeSpan ts = new TimeSpan();
            int x;
            ts = t - System.DateTime.Now;
            if (ts.TotalMilliseconds < 0)
            {
                ts = t.AddDays(1) - System.DateTime.Now;//Here you can increase the timer interval based on your requirments.   
            }
            return ts.TotalMilliseconds;
        }
        private void SetTimer()
        {
            try
            {
                double inter = (double)GetNextInterval();
                Timer1.Interval = inter;
                Timer1.Start();
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnStart(string[] args)
        {
            Timer1.AutoReset = true;
            Timer1.Enabled = true;
            WriteErrorLog("Daily Reporting service started");
        }

        protected override void OnStop()
        {
            Timer1.AutoReset = false;
            Timer1.Enabled = false;
            WriteErrorLog("Daily Reporting service stopped");
        }
        public static void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }
        public static void SendEmail(String ToEmail, string cc, string bcc, String Subj, string Message)
        {
            //Reading sender Email credential from web.config file  

            string HostAdd = ConfigurationManager.AppSettings["Host"].ToString();
            string FromEmailid = ConfigurationManager.AppSettings["FromMail"].ToString();
            string Pass = ConfigurationManager.AppSettings["Password"].ToString();

            //creating the object of MailMessage  
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(FromEmailid); //From Email Id  
            mailMessage.Subject = Subj; //Subject of Email  
            mailMessage.Body = Message; //body or message of Email  
            mailMessage.IsBodyHtml = true;

            string[] ToMuliId = ToEmail.Split(',');
            foreach (string ToEMailId in ToMuliId)
            {
                mailMessage.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id  
            }


            string[] CCId = cc.Split(',');

            foreach (string CCEmail in CCId)
            {
                mailMessage.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id  
            }

            string[] bccid = bcc.Split(',');

            foreach (string bccEmailId in bccid)
            {
                mailMessage.Bcc.Add(new MailAddress(bccEmailId)); //Adding Multiple BCC email Id  
            }
            SmtpClient smtp = new SmtpClient();  // creating object of smptpclient  
            smtp.Host = HostAdd;              //host of emailaddress for example smtp.gmail.com etc  

            //network and security related credentials  

            smtp.EnableSsl = false;
            NetworkCredential NetworkCred = new NetworkCredential();
            NetworkCred.UserName = mailMessage.From.Address;
            NetworkCred.Password = Pass;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;
            smtp.Port = 3535;
            smtp.Send(mailMessage); //sending Email  
        }
        private void ServiceTimer_Tick(object sender,ElapsedEventArgs e)
        {
            string Msg = "Hi ! This is DailyMailSchedulerService mail.";//whatever msg u want to send write here.  
                                                                        // Here you can write the   
            SendEmail("---To mail---", "----cc mail----", "---bcc mail----", "Daily Report of DailyMailSchedulerService on " + DateTime.Now.ToString("dd-MMM-yyyy"), Msg);

            if (getCallType == 1)
            {
                Timer.Stop();
                System.Threading.Thread.Sleep(1000000);
                SetTimer();
            }
        }

    }
}
