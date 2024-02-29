using System.Net.Sockets;
using System.Net;

namespace TcpChat
{
    public class ChatServer
    {
        // Применяем порождающий паттерн "Синглтон", чтобы в программе можно было работать только с одним экземпляром класса ChatServer
        private static readonly Lazy<ChatServer> lazyInstance = new Lazy<ChatServer>(() => new ChatServer());

        private ChatServer()
        {
            cts = new CancellationTokenSource();
        }

        public static ChatServer Instance => lazyInstance.Value;

        List<User> users = new List<User>();

        TcpListener listener = new TcpListener(IPAddress.Any, 55555);

        public CancellationTokenSource cts { get; private set; }

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

        // Используем поведенческий паттерн "Медиатор" для того, чтобы у всех клиентов была взаимодействие только через сервер.
        // Тем самым снижается их зависимость друг от друга
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
