//Добавьте возможность ввести слово Exit в чате клиента, чтобы можно было завершить его работу.
//В коде сервера добавьте ожидание нажатия клавиши, чтобы также прекратить его работу.

//** Добавляем многопоточность в чат позволяя серверной части получать сообщения сразу от нескольких респондентов.
//Временно удалим из сервера возможность ввода сообщений. Сделаем так чтобы чат всегда отвечал “Сообщение получено”.
//Протестируем наш чат запустив сразу 10 клиентов. Для удобства сделаем так чтобы клиент также ничего не запускал
//но просто слал привет.

using System.Net;
using System.Net.Sockets;

namespace TcpChat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new ChatServer();

            await server.Run();
        }
    }

    public class ChatServer
    {
        List<User> users = new List<User>();

        TcpListener listener = new TcpListener(IPAddress.Any, 55555);
        public async Task Run()
        {
            try
            {
                var thread = new Thread(FinishServer);

                thread.Start();

                listener.Start();

                await Console.Out.WriteLineAsync("Запущен");

                while (true)
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();

                    var user = new User(tcpClient, this);

                    users.Add(user);

                    _ = Task.Run(user.ProcessAsync);
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        public async Task BroadcastMessageAsync(string message, string id)
        {
            foreach (var user in users)
            {
                if (user.Id != id)
                {
                    await user.Writer.WriteLineAsync(message);

                    await user.Writer.FlushAsync();
                }
            }
        }

        public void RemoveConnection(string id)
        {
            User? user = users.FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                users.Remove(user);

                user.Close();
            }
        }

        public void Disconnect()
        {
            foreach (var user in users)
            {
                user.Close();
            }

            listener.Stop();
        }

        public void FinishServer()
        {
            string? str = Console.ReadLine();
            if(str != null)
            {
                Disconnect();
            }
        }
    }
}
