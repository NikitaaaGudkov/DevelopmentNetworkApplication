// Попробуйте переработать приложение, добавив подтверждение об отправке сообщений как в сервер, так и в клиент.

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
    }
}
