using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Server
{
    internal class Program
    {
        private static readonly ConcurrentDictionary<string, double> exchangeRates = new()
        {
            ["USD_EURO"] = 0.92,
            ["EURO_USD"] = 1.09
        };

        private static readonly string logFile = "server_log.txt";
        private static readonly object logLock = new();

        static void Main(string[] args)
        {
            int port = 5000;
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server started on port {port}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            string clientEndPoint = client.Client.RemoteEndPoint.ToString();
            LogConnection(clientEndPoint, "connected");

            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            try
            {
                while (true)
                {
                    string request = reader.ReadLine();
                    if (string.IsNullOrEmpty(request)) break;

                    Console.WriteLine($"Request from {clientEndPoint}: {request}");
                    string response = ProcessRequest(request);
                    writer.WriteLine(response);
                    Console.WriteLine($"Response to {clientEndPoint}: {response}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {clientEndPoint}: {ex.Message}");
            }
            finally
            {
                LogConnection(clientEndPoint, "disconnected");
                client.Close();
            }
        }

        private static string ProcessRequest(string request)
        {
            string[] parts = request.ToUpper().Split(' ');
            if (parts.Length != 2)
                return "Invalid request format. Use: <CURRENCY1> <CURRENCY2>";

            string key = $"{parts[0]}_{parts[1]}";
            if (exchangeRates.TryGetValue(key, out double rate))
                return $"1 {parts[0]} = {rate} {parts[1]}";
            else
                return "Currency pair not supported.";
        }

        private static void LogConnection(string clientEndPoint, string status)
        {
            string logEntry = $"{DateTime.Now}: Client {clientEndPoint} {status}";
            Console.WriteLine(logEntry);

            lock (logLock)
            {
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
        }
    }
}
