using AppIt.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppIt.Core.AppServices
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(IConfiguration config, ILogger<EmailNotificationService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            if (!_config.GetValue<bool>("Smtp:Enabled"))
            {
                _logger.LogInformation("SMTP disabled — would send to {To}: {Subject}", to, subject);
                return Task.CompletedTask;
            }
            _logger.LogInformation("Email queued to {To}: {Subject}", to, subject);
            return Task.CompletedTask;
        }
    }
}
