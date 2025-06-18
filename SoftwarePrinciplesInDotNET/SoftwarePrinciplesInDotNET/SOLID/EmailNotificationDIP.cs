using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.SOLID
{
    //GOOD: Both depend on abstraction
    public interface IEmailSender
    {
        void Send(string emailAddress, string message);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private SmtpClient _smtpClient;

        public SmtpEmailSender()
        {
            _smtpClient = new SmtpClient();
        }

        //ignore
        public void Email(string document)
        {
            throw new NotImplementedException();
        }

        public void Send(string emailAddress, string message)
        {
            _smtpClient.Send(emailAddress, message, "", "");
        }
    }

    public class EmailNotification
    {
        private readonly IEmailSender _emailSender;

        public EmailNotification(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public void Send(string emailAddress, string message)
        {
            _emailSender.Send(emailAddress, message);
        }
    }
}
