using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string server = "127.0.0.1";
            int port = 5000;

            try
            {
                using TcpClient client = new TcpClient(server, port);
                Console.WriteLine("Connected to the server.");
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string input;
                do
                {
                    Console.WriteLine("Enter currency pair (e.g., USD EURO) or type 'exit' to quit:");
                    input = Console.ReadLine();
                    if (input?.ToLower() == "exit") break;

                    writer.WriteLine(input);
                    string response = reader.ReadLine();
                    Console.WriteLine($"Server response: {response}");
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
