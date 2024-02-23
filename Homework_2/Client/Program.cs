using System.Net.Sockets;

namespace TcpChat
{
    internal class Program
    {
        static string name = string.Empty;
        static async Task Main(string[] args)
        {
            var tcpClient = new TcpClient();

            while (true)
            {
                Console.Write("Введите своё имя: ");

                name = Console.ReadLine()!;

                if (name is null || name.Length == 0)
                {
                    Console.WriteLine("Вы не ввели ваше имя!");
                }
                else
                {
                    Console.WriteLine($"Добро пожаловать, {name}");
                    break;
                }
            }

            StreamReader? reader = null;

            StreamWriter? writer = null;

            try
            {
                tcpClient.Connect("localhost", 55555);

                reader = new StreamReader(tcpClient.GetStream());

                writer = new StreamWriter(tcpClient.GetStream());

                if (reader is null || writer is null)
                {
                    Console.WriteLine("Не удалось подключиться к серверу. Для выхода из программы нажмите любую клавишу");

                    Console.ReadLine();

                    return;
                }

                _ = Task.Run(() => ReceiveMessageAsync(reader));

                await SendMessageAsync(writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                writer?.Close();

                reader?.Close();
            }

            async Task SendMessageAsync(StreamWriter writer)
            {
                await writer.WriteLineAsync(name);

                await writer.FlushAsync();

                Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter");

                while (true)
                {
                    string? message = Console.ReadLine();

                    await writer.WriteLineAsync(message);

                    await writer.FlushAsync();

                    if(message == "Exit")
                    {
                        return;
                    }
                }
            }

            async Task ReceiveMessageAsync(StreamReader reader)
            {
                while (true)
                {
                    try
                    {
                        string? message = await reader.ReadLineAsync();

                        if (string.IsNullOrEmpty(message))
                        {
                            continue;
                        }

                        Print(message);
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            // Метод, переносящий набираемое сообщение, если в процессе написания были получены новые сообщения от сервера.
            void Print(string message)
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
        }
    }
}
