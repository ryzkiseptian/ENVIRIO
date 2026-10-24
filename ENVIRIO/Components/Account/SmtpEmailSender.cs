using System.Net;
using System.Net.Mail;
using ENVIRIO.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ENVIRIO.Components.Account
{
    public class SmtpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpSettings> options, ILogger<SmtpEmailSender> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var htmlBody = GenerateEmailTemplate(
                title: "Aktivasi Akun Operator ENVIRIO",
                message: "Selamat datang di platform ENVIRIO. Silakan klik tombol di bawah untuk mengonfirmasi email dan mengaktifkan akses operator Anda:",
                actionUrl: confirmationLink,
                actionText: "Konfirmasi Akun Saya"
            );

            await SendEmailAsync(email, "Konfirmasi Akun Operator — ENVIRIO", htmlBody);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var htmlBody = GenerateEmailTemplate(
                title: "Permintaan Reset Password",
                message: "Kami menerima permintaan pengaturan ulang kata sandi untuk akun operator Anda. Silakan klik tombol di bawah untuk membuat kata sandi baru:",
                actionUrl: resetLink,
                actionText: "Reset Kata Sandi"
            );

            await SendEmailAsync(email, "Reset Kata Sandi — ENVIRIO", htmlBody);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var htmlBody = $@"
                <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; max-width: 580px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 28px;'>
                    <div style='text-align: center; margin-bottom: 24px;'>
                        <h1 style='color: #059669; margin: 0; font-size: 24px;'>ENVIRIO</h1>
                    </div>
                    <h2 style='color: #0f172a; font-size: 18px;'>Kode Verifikasi Reset Password</h2>
                    <p style='color: #475569; font-size: 14px;'>Gunakan kode verifikasi berikut untuk melanjutkan proses reset password:</p>
                    <div style='background-color: #f1f5f9; padding: 16px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                        <span style='font-family: monospace; font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #059669;'>{resetCode}</span>
                    </div>
                    <p style='color: #94a3b8; font-size: 12px;'>Kode ini bersifat rahasia dan berlaku dalam waktu terbatas. Abaikan jika Anda tidak meminta reset password.</p>
                </div>";

            await SendEmailAsync(email, "Kode Verifikasi Reset Kata Sandi — ENVIRIO", htmlBody);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host))
            {
                _logger.LogWarning("SMTP Host belum dikonfigurasi. Email ke {Email} tidak dapat dikirim.", toEmail);
                return;
            }

            try
            {
                var cleanPassword = _settings.Password?.Replace(" ", "") ?? "";
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.Username, cleanPassword)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email SMTP berhasil dikirim ke {Email} [Subjek: {Subject}]", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengirim email SMTP ke {Email}. Detail: {Message}", toEmail, ex.Message);
            }
        }

        private string GenerateEmailTemplate(string title, string message, string actionUrl, string actionText)
        {
            return $@"
            <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; max-width: 580px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>
                <div style='background: linear-gradient(135deg, #064e3b 0%, #065f46 100%); padding: 28px; text-align: center;'>
                    <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 1px;'>ENVIRIO</h1>
                </div>
                <div style='padding: 32px 28px;'>
                    <h2 style='color: #0f172a; margin-top: 0; font-size: 20px; font-weight: 600;'>{title}</h2>
                    <p style='color: #475569; font-size: 15px; line-height: 1.6;'>{message}</p>
                    <div style='text-align: center; margin: 32px 0;'>
                        <a href='{actionUrl}' style='background: linear-gradient(135deg, #059669 0%, #047857 100%); color: #ffffff !important; text-decoration: none; padding: 13px 30px; border-radius: 8px; font-weight: 600; font-size: 15px; display: inline-block; box-shadow: 0 3px 10px rgba(5,150,105,0.3);'>{actionText}</a>
                    </div>
                    <p style='color: #94a3b8; font-size: 13px; line-height: 1.5; margin-bottom: 0;'>
                        Jika tombol di atas tidak dapat diklik, salin tautan berikut ke peramban Anda:<br />
                        <a href='{actionUrl}' style='color: #059669; word-break: break-all;'>{actionUrl}</a>
                    </p>
                </div>
                <div style='background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 16px; text-align: center; color: #94a3b8; font-size: 12px;'>
                    &copy; {DateTime.Now.Year} ENVIRIO Smart IoT Platform. Hak Cipta Dilindungi.
                </div>
            </div>";
        }
    }
}
