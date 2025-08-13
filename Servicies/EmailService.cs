using System.Net;
using System.Net.Mail;

namespace GestionVentas.Servicies
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, byte[] pdfBytes,string NombreArchivo)
        {
            try
            {
               var smtpClient = new SmtpClient("smtp.gmail.com")
               {
                   Port = 587,
                   Credentials = new NetworkCredential(_configuration["Email:User"],
                   _configuration["Email:Password"]),
                   EnableSsl = true,
               };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:User"]),
                    Subject = "Factura electronica",
                    Body = "Adjunto encontrara su factura",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);
                var attachment = new Attachment(new MemoryStream(pdfBytes), NombreArchivo, "application/pdf");
                mailMessage.Attachments.Add(attachment);
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

    }
}
