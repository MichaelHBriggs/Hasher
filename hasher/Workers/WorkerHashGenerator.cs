using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace hasher.Workers
{
    public class WorkerHashGenerator : IWorker<string, string>
    {

        public async Task<string> DoWork(string arg)
        {
            string? macAddress = NetworkInterface.GetAllNetworkInterfaces()
                 .Where(x => x.OperationalStatus == OperationalStatus.Up)
                 .Select(x => x.GetPhysicalAddress().ToString())
                 .FirstOrDefault();
            if (!string.IsNullOrEmpty(macAddress))
            {

                string currentHash = await MakeHashAsync(arg, macAddress);
                return currentHash;

            }
            return string.Empty;
        }

        private async Task<string> MakeHashAsync(string filePath, string hashKey, int bufferSize = 8192)
        {
            using (var stream = new BufferedStream(File.OpenRead(filePath), bufferSize))
            {
                HMACSHA256 sha = new HMACSHA256(Encoding.ASCII.GetBytes(hashKey));
                byte[] checksum = await sha.ComputeHashAsync(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
            }
        }
    }
}
