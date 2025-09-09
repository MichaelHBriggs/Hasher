using HasherDataObjects.Configuration;
using HasherDataObjects.Models;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Text;

#nullable enable

namespace hasher.Workers
{
    public class WorkerEmailerData
    {
        public IDictionary<string, Tuple<string, string>>? ChangedHashes { get; set; }
        public IList<string>? MissingFileList { get; set; }
        public RunResults? RunResults { get; set; }
    }

    public class WorkerEmailer(Settings settings, ILogger<WorkerEmailer> logger) : IWorker<WorkerEmailerData, bool>
    {
        public Task<bool> DoWork(WorkerEmailerData arg)
        {
            _ = arg.ChangedHashes ?? throw new ArgumentNullException(nameof(arg.ChangedHashes), "Changed hashes cannot be null");
            _ = arg.MissingFileList ?? throw new ArgumentNullException(nameof(arg.MissingFileList), "Missing file list cannot be null");

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            MailMessage mailMessage = new()
            {
                From = new MailAddress(settings.Email.EmailFrom),
                Subject = "Hasher Notification",
                IsBodyHtml = true,
                To = { settings.Email.EmailTo }
            };
            mailMessage.Headers.Add("Message-Id", $"{Guid.NewGuid()}-{settings.Email.EmailFrom}");
            StringBuilder body = new StringBuilder();

            if (arg.RunResults != null)
            {
                body.AppendLine("<h2>Run Summary</h2>");
                body.AppendLine("<table border='1'>");
                body.AppendLine($"<tr><td>Total Files Scanned</td><td>{arg.RunResults.TotalFiles}</td></tr>");
                body.AppendLine($"<tr><td>Total Files Updated</td><td>{arg.RunResults.UpdatedFiles}</td></tr>");
                body.AppendLine($"<tr><td>Total Files Added</td><td>{arg.RunResults.AddedFiles}</td></tr>");
                body.AppendLine($"<tr><td>Total Files Deleted</td><td>{arg.RunResults.DeletedFiles}</td></tr>");
                body.AppendLine($"<tr><td>Total Unchanged Files</td><td>{arg.RunResults.UnchangedFiles}</td></tr>");
                body.AppendLine("</table>");
            }

            if (arg.ChangedHashes.Any() || arg.MissingFileList.Any())
            {
                
                if (arg.ChangedHashes.Any())
                {
                    body.AppendLine("<h2>Changed Hashes</h2>");
                    body.AppendLine("<table border='1'><tr><th>File</th><th>Old Hash</th><th>New Hash</th></tr>");
                    foreach (var changedHash in arg.ChangedHashes)
                    {
                        body.AppendLine($"<tr><td>{changedHash.Key}</td><td>{changedHash.Value.Item1}</td><td>{changedHash.Value.Item2}</td></tr>");
                    }
                    body.AppendLine("</table>");
                }
                if (arg.ChangedHashes.Any() && arg.MissingFileList.Any())
                {
                    body.AppendLine("<hr>");
                }
                if (arg.MissingFileList.Any())
                {
                    body.AppendLine("<h2>Missing Files</h2>");
                    body.AppendLine("<ul>");
                    foreach (var missingFile in arg.MissingFileList)
                    {
                        body.AppendLine($"<li>{missingFile}</li>");
                    }
                    body.AppendLine("</ul>");
                }
            }
            mailMessage.Body = body.ToString();
            try
            {
                SmtpClient smtp = new SmtpClient(settings.Email.SMTPServerName, settings.Email.SmtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(settings.Email.Username, settings.Email.AppPassword),
                    EnableSsl = true
                };
                smtp.Send(mailMessage);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                logger.LogError($"Error sending email: {ex.Message}");
                return Task.FromResult(false);
            }
            return Task.FromResult(false);
        }
    }
}
