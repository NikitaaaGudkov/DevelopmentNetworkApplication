using DTO;
using Interfaces;
using System.Net.Sockets;

namespace TcpLibrary
{
    public class MyTcpClient : IMessageSourceClient
    {
        public string? Name { get; private set; }
        public async Task Run()
        {
            Name = GetUserName();

            var tcpClient = new TcpClient();

            StreamReader? reader = null;

            StreamWriter? writer = null;

            try
            {
                await tcpClient.ConnectAsync("localhost", 55555);

                reader = new StreamReader(tcpClient.GetStream());

                writer = new StreamWriter(tcpClient.GetStream());

                if (reader is null || writer is null)
                {
                    Console.WriteLine("Не удалось подключиться к серверу. Для выхода из программы нажмите любую клавишу");

                    Console.ReadLine();

                    return;
                }

                _ = Task.Run(() => ReceiveMessageAsync(reader, writer));

                await Task.Run(() => SendMessageAsync(writer));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                writer?.Close();
                reader?.Close();
                tcpClient?.Close();
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


        private async Task ReceiveMessageAsync(StreamReader reader, StreamWriter writer)
        {
            while (true)
            {
                try
                {
                    string json = await reader.ReadLineAsync();

                    TcpMessage? tcpMessage = TcpMessage.JsonToMessage(json);

                    tcpMessage.Status = Command.Confirmed;

                    await writer.WriteLineAsync(tcpMessage.MessageToJson());

                    await writer.FlushAsync();

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

        private async Task SendMessageAsync(StreamWriter writer)
        {
            var tcpMessage = new TcpMessage() { SenderName = Name, Status = Command.Registered };

            await writer.WriteLineAsync(tcpMessage.MessageToJson());

            await writer.FlushAsync();

            Console.WriteLine("Для отправки сообщений введите имя собеседника, через пробел сообщение и нажмите Enter");

            while (true)
            {
                string? message = Console.ReadLine();

                string consumerName = message.Split(' ')[0];

                tcpMessage = new TcpMessage() { SenderName = Name, ConsumerName = consumerName, Text = message.Replace(consumerName + " ", ""), Status = Command.Message };

                await writer.WriteLineAsync(tcpMessage.MessageToJson());

                await writer.FlushAsync();

                if (message == "Exit")
                {
                    return;
                }
            }
        }
    }
}
