using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press any keys to connect...");
            Console.ReadLine();
            using (ClientWebSocket client = new ClientWebSocket())
            {
                Uri serviceUri = new Uri("ws://localhost:5000/send");
                var cTs = new CancellationTokenSource();
                cTs.CancelAfter(TimeSpan.FromSeconds(120));
                try
                {
                    await client.ConnectAsync(serviceUri,cTs.Token);
                    while(client.State == WebSocketState.Open)
                    {
                        Console.WriteLine("Please enter message:");
                        string message = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            ArraySegment<byte> sendMessage = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                            await client.SendAsync(sendMessage,WebSocketMessageType.Text,true,cTs.Token);
                            var response = new byte[1024];
                            var offset = 0;
                            var packetSize = 1024;
                            while (true)
                            {
                                ArraySegment<byte> msgReceived = new ArraySegment<byte>(response, offset, packetSize);
                                WebSocketReceiveResult result = await client.ReceiveAsync(msgReceived, cTs.Token);
                                var responseMessage = Encoding.UTF8.GetString(response,offset,packetSize);
                                Console.WriteLine(responseMessage);
                                if (result.EndOfMessage)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.ReadLine();
        }
    }
}
