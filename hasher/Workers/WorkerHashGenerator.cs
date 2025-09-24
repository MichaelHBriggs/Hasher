using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace hasher.Workers
{
    public class WorkerHashGenerator (ILogger<WorkerHashGenerator> logger) : IWorker<Tuple<string, float>, string>
    {

        public async Task<string> DoWork(Tuple<string, float> arg)
        {
            string? macAddress = NetworkInterface.GetAllNetworkInterfaces()
                 .Where(x => x.OperationalStatus == OperationalStatus.Up)
                 .Select(x => x.GetPhysicalAddress().ToString())
                 .FirstOrDefault();
            if (!string.IsNullOrEmpty(macAddress))
            {
                string filename = arg.Item1;
                float takePercent = arg.Item2;
                logger.LogDebug($"Hashing file: {filename} with chunk size percent: {takePercent}");
                long size = new FileInfo(filename).Length;
                string currentHash = await MakeHashAsync(filename, macAddress, Math.Max(8192, (int)Math.Floor( size * takePercent)));
                return currentHash;

            }
            return string.Empty;
        }

        private async Task<string> MakeHashAsync(string filePath, string hashKey, int bufferSize = 8192)
        {
            logger.LogDebug($"MakeHashAsync: {filePath}, {hashKey}, {bufferSize}");
            using (var stream = new BufferedStream(File.OpenRead(filePath), bufferSize))
            {
                HMACSHA256 sha = new HMACSHA256(Encoding.ASCII.GetBytes(hashKey));
                byte[] checksum = await sha.ComputeHashAsync(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
            }
        }
    }
}
