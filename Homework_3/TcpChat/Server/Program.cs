// Добавьте использование Cancellationtoken в код сервера, чтобы можно было правильно останавливать работу сервера.


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

        private CancellationTokenSource cts = new CancellationTokenSource();

        public async Task Run()
        {
            var token = cts.Token;
            try
            {
                var threadManageServer = new Thread(ManageServer);

                threadManageServer.Start();

                listener.Start();

                await Console.Out.WriteLineAsync("Запущен\nДля прекращения работы сервера нажмите на любую клавишу");

                while (true)
                {
                    var tcpClient = await listener.AcceptTcpClientAsync(token);

                    var user = new User(tcpClient, this);

                    users.Add(user);

                    _ = Task.Run(user.ProcessAsync);
                }
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                {
                    await Console.Out.WriteLineAsync("Работа сервера планово приостановлена");
                }
                else
                {
                    await Console.Out.WriteLineAsync("Сервер завершил работу с ошибкой: \n" + ex.Message);
                }
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

        public void ManageServer()
        {
            string? str = Console.ReadLine();
            cts.Cancel();
        }
    }
}