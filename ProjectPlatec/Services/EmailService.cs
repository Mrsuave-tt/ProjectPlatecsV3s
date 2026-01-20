using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ProjectPlatec.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendStudentCredentialsAsync(
            string toEmail,
            string studentName,
            string studentId,
            string password,
            string websiteUrl)
        {
            try
            {
                // 🔐 Force Gmail TLS
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

                var fromAddress = new MailAddress(senderEmail, senderName);
                var toAddress = new MailAddress(toEmail, studentName);

                const string subject = "Your Student Account Credentials - ProjectPlatec";

                var body = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f6f8; padding: 20px;'>

  <div style='max-width: 600px; margin: auto; background: #ffffff; padding: 25px; border-radius: 8px;'>

    <h2 style='color: #2c3e50; text-align: center;'>
      🎓 Student Account Created
    </h2>

    <p style='font-size: 15px; color: #333;'>
      Hello <b>{studentName}</b>,
    </p>

    <p style='font-size: 15px; color: #333;'>
      An account has been created for you by the administrator. Below are your login details:
    </p>

    <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
      <tr>
        <td style='padding: 8px; font-weight: bold;'>Student ID</td>
        <td style='padding: 8px;'>{studentId}</td>
      </tr>
      <tr>
        <td style='padding: 8px; font-weight: bold;'>Temporary Password</td>
        <td style='padding: 8px;'>{password}</td>
      </tr>
    </table>

    <div style='text-align: center; margin: 25px 0;'>
      <a href='{websiteUrl}/Account/Login'
         style='background-color: #007bff; color: #ffffff; padding: 12px 20px;
                text-decoration: none; border-radius: 5px; font-weight: bold;'>
        🔐 Login to Your Account
      </a>
    </div>

    <p style='font-size: 14px; color: #555;'>
      For security reasons, please change your password after logging in.
    </p>

    <hr style='margin: 25px 0;' />

    <p style='font-size: 13px; color: #777; text-align: center;'>
      Attendance Management System<br>
      This is an automated message. Please do not reply.
    </p>

  </div>

</body>
</html>";


                using var smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error");
                throw new Exception("Email authentication failed. Make sure you are using a Gmail App Password.", ex);
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // 🔐 Force Gmail TLS
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

                var fromAddress = new MailAddress(senderEmail, senderName);
                var toAddress = new MailAddress(toEmail);

                using var smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error");
                throw new Exception("Email authentication failed. Make sure you are using a Gmail App Password.", ex);
            }
        }
    }
}
