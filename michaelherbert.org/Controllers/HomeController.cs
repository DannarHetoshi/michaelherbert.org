using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using michaelherbert.org.ViewModels;
using System.Net.Mail;
using System.Web.Configuration;
using System.Net;
using System.Threading;

namespace michaelherbert.org.Controllers
{
    public class HomeController : Controller
    {
        
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(ContactViewModel vm)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var fromName = vm.Name;
                    var toAddress = "michael@michaelherbert.org";
                    var fromAddress = vm.Email.ToString();
                    var subject = vm.Subject;
                    var message = vm.Message;

                    var tEmail = new Thread(() => SendEmail(fromName, toAddress, fromAddress, subject, message));
                    tEmail.Start();
                } 
                catch (Exception Ex)
                {
                    ModelState.Clear();
                    ViewBag.Message = $" An error has occurred.  {Ex.Message}";
                }
            }

            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public void SendEmail(string fromName, string toAddress, string fromAddress, string subject, string message)
        {
            try
            {
                using (var mail = new MailMessage())
                {
                    const string email = "michael@michaelherbert.org";
                    string password = WebConfigurationManager.AppSettings.Get("emailPassword").ToString();

                    var loginInfo = new NetworkCredential("michael.herbert.84@gmail.com", password);

                    mail.From = new MailAddress(fromAddress);
                    mail.To.Add(new MailAddress(toAddress));
                    mail.Subject = subject;
                    mail.Body += ("You've received a new email through michaelherbert.org from: <br><br>");
                    mail.Body += ("Name: " + fromName + "<br><br>");
                    mail.Body += ("Email: " + fromAddress + "<br><br>");
                    mail.Body += message;
                    mail.IsBodyHtml = true;

                    try
                    {
                        using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtpClient.EnableSsl = true;
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = loginInfo;
                            smtpClient.Send(mail);
                        }
                    }

                    finally
                    {
                        mail.Dispose();
                    }
                }
            }
            catch (SmtpFailedRecipientsException ex)
            {
                foreach(SmtpFailedRecipientException t in ex.InnerExceptions)
                {
                    var status = t.StatusCode;
                    if (status == SmtpStatusCode.MailboxBusy || status == SmtpStatusCode.MailboxUnavailable)
                    {
                        Response.Write("Delivery Failed - Retrying in 5 seconds.");
                        System.Threading.Thread.Sleep(5000);
                        //resend
                        //smtpClient.Send(message);
                    }
                    else
                    {
                        
                    }
                }
            }
            catch (SmtpException Se)
            {
                Response.Write(Se.ToString());
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        

    }

    
}