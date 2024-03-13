using DTO;
using Interfaces;
using NetMQ;
using NetMQ.Sockets;

namespace NetMQLibrary
{
    public class NetMqClient : IMessageSourceClient
    {
        public string? Name { get; private set; }

        public async Task Run()
        {
            Name = GetUserName();

            var netMQClient = new DealerSocket();

            try
            {
                netMQClient.Connect("tcp://127.0.0.1:55555");

                _ = Task.Run(() => ReceiveMessageAsync(netMQClient));

                await Task.Run(() => SendMessageAsync(netMQClient));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                netMQClient?.Dispose();
            }
        }

        private string GetUserName()
        {
            while (true)
            {
                Console.Write("Введите своё имя: ");
                string name = Console.ReadLine()!;

                if (name is null || name.Length == 0)
                {
                    Console.WriteLine("Вы не ввели ваше имя!");
                }
                else
                {
                    Console.WriteLine($"Добро пожаловать, {name}");
                    return name;
                }
            }
        }

        /// <summary>
        /// Метод, переносящий набираемое сообщение, если в процессе написания были получены новые сообщения от сервера.
        /// </summary>
        /// <param name="message"></param>
        private void Print(string message)
        {
            if (OperatingSystem.IsWindows())
            {
                var position = Console.GetCursorPosition();

                int left = position.Left;

                int top = position.Top;

                Console.MoveBufferArea(0, top, left, 1, 0, top + 1);

                Console.SetCursorPosition(0, top);

                Console.WriteLine(message);

                Console.SetCursorPosition(left, top + 1);
            }
            else
            {
                Console.WriteLine(message);
            }
        }


        private async Task ReceiveMessageAsync(DealerSocket netMQClient)
        {
            while (true)
            {
                try
                {
                    NetMQMessage netMQMessage = netMQClient.ReceiveMultipartMessage();

                    string json = netMQMessage.Last.ConvertToString();

                    TcpMessage? tcpMessage = TcpMessage.JsonToMessage(json);

                    tcpMessage.Status = Command.Confirmed;

                    netMQClient.SendFrame(tcpMessage.MessageToJson());

                    if (string.IsNullOrEmpty(tcpMessage.Text))
                    {
                        continue;
                    }

                    string messageString = $"{tcpMessage.SenderName}: {tcpMessage.Text}";

                    Print(messageString);
                }
                catch
                {
                    break;
                }
            }
        }

        private async Task SendMessageAsync(DealerSocket netMQClient)
        {
            var tcpMessage = new TcpMessage() { SenderName = Name, Status = Command.Registered };

            netMQClient.SendFrame(tcpMessage.MessageToJson());

            Console.WriteLine("Для отправки сообщений введите имя собеседника, через пробел сообщение и нажмите Enter");

            while (true)
            {
                string? message = Console.ReadLine();

                string consumerName = message.Split(' ')[0];

                tcpMessage = new TcpMessage() { SenderName = Name, ConsumerName = consumerName, Text = message.Replace(consumerName + " ", ""), Status = Command.Message };

                netMQClient.SendFrame(tcpMessage.MessageToJson());

                if (message == "Exit")
                {
                    return;
                }
            }
        }
    }
}
