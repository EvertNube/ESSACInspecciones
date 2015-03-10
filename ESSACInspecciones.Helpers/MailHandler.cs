using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ESSACInspecciones.Helpers
{
    public static class MailHandler
    {
        public static void Send(string to, string copy, string subject, string body)
        {
            try
            {
                string[] listCopy = copy == string.Empty ? new string[0] : copy.Split(',');
                MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["MailFrom"], to, subject, body);
                foreach (var item in listCopy)
                    mail.CC.Add(new MailAddress(item));
                //mail.Bcc.Add(new MailAddress(ConfigurationManager.AppSettings["MailFrom"]));
                //mail.Bcc.Add(new MailAddress("boris@nube.la"));
                mail.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                //client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                //client.UseDefaultCredentials = false;
                client.Send(mail);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //Funcion de prueba para enviar correo
        public static void sendEmailTest(string emailBody)
        {
            try
            {
                MailMessage mailMessage = new MailMessage("evert@go3studios.com", "77saber77@gmail.com");
                mailMessage.Subject = "Exception";
                mailMessage.Body = emailBody;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.Credentials = new System.Net.NetworkCredential()
                {
                    UserName = "evert@go3studios.com",
                    Password = "BladeKnight7"
                };
                smtpClient.EnableSsl = true;
                smtpClient.Send(mailMessage);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
