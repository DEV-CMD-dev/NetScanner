using System.Net.NetworkInformation;

namespace NetScanner
{
    public class PingResultDetailed
    {
        public string Host { get; set; }
        public string IPAddress { get; set; }
        public int Bytes { get; set; }
        public List<PingReply> Replies { get; set; } = new List<PingReply>();
    }

    public static class NetworkTools
    {
        public static List<string> Ping(string host, int count = 4, int timeout = 1000)
        {
            var results = new List<string>();
            var ping = new Ping();
            PingOptions options = new PingOptions { DontFragment = true };
            byte[] buffer = new byte[32];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0x65;

            // Get ip
            var ipHost = System.Net.Dns.GetHostEntry(host).AddressList[0].ToString();

            results.Add($"Pinging {host} [{ipHost}] with {buffer.Length} bytes of data:");

            int sent = 0, received = 0;
            List<long> times = new List<long>();

            for (int i = 0; i < count; i++)
            {
                sent++;
                PingReply reply;
                try
                {
                    reply = ping.Send(ipHost, timeout, buffer, options);
                }
                catch (Exception ex)
                {
                    results.Add("Ping error: " + ex.Message);
                    break;
                }

                if (reply.Status == IPStatus.Success)
                {
                    received++;
                    times.Add(reply.RoundtripTime);
                    results.Add($"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl}");
                }
                else
                {
                    results.Add($"Request timed out.");
                }
            }

            int lost = sent - received;
            int lossPercent = (int)((lost / (double)sent) * 100);

            results.Add("");
            results.Add($"Ping statistics for {ipHost}:");
            results.Add($"    Packets: Sent = {sent}, Received = {received}, Lost = {lost} ({lossPercent}% loss),");

            if (times.Count > 0)
            {
                long min = long.MaxValue;
                long max = long.MinValue;
                long sum = 0;
                foreach (var t in times)
                {
                    if (t < min) min = t;
                    if (t > max) max = t;
                    sum += t;
                }
                long avg = sum / times.Count;

                results.Add("Approximate round trip times in milli-seconds:");
                results.Add($"    Minimum = {min}ms, Maximum = {max}ms, Average = {avg}ms");
            }

            return results;
        }
    }
}
