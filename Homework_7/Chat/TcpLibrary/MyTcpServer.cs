using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Text.Json;
using DTO;
using DataBase;
using Interfaces;

namespace TcpLibrary
{
    public class MyTcpServer : IMessageSource
    {
        private static readonly Lazy<MyTcpServer> lazyInstance = new Lazy<MyTcpServer>(() => new MyTcpServer());

        ConcurrentDictionary<int, TcpClient> clients;

        public static MyTcpServer Instance => lazyInstance.Value;

        TcpListener listener = new TcpListener(IPAddress.Any, 55555);

        public CancellationTokenSource cts { get; private set; }

        private MyTcpServer()
        {
            cts = new CancellationTokenSource();
            clients = new ConcurrentDictionary<int, TcpClient>();
        }

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

                    _ = Task.Run(() => ProcessClient(tcpClient));
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

        private async Task ProcessClient(TcpClient client)
        {
            using var reader = new StreamReader(client.GetStream());

            while (true)
            {
                try
                {
                    string json = await reader.ReadLineAsync();

                    TcpMessage? message = TcpMessage.JsonToMessage(json);

                    switch (message.Status)
                    {
                        case Command.Registered:
                            RegisterClient(client, message.SenderName);
                            break;
                        case Command.Confirmed:
                            Confirmed(message.Id);
                            break;
                        case Command.Message:
                            RegisterAndSendMessage(message);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync("Ошибка: " + ex.Message);
                    break;
                }
            }   
        }


        /// <summary>
        /// Изменить статус сообщения с недоставленного на доставленное
        /// TODO: НЕ РАБОТАЕТ, НАДО ДОДЕЛЫВАТЬ
        /// </summary>
        /// <param name="id">Идентификатор сообщения</param>
        private void Confirmed(int? id)
        {
            using var context = new ChatContext();

            var message = context.Messages.FirstOrDefault(m => m.Id == id);

            if (message != null)
                message.IsReceived = true;

            context.SaveChanges(); 
        }


        /// <summary>
        /// Зарегистрировать нового пользователя в базе данных
        /// </summary>
        /// <param name="name">Имя пользователя</param>
        private void RegisterClient(TcpClient client, string name)
        {
            User? user = null!;
            using (var context = new ChatContext())
            {
                user = context.Users.FirstOrDefault(u => u.UserName == name);
                if (user == null)
                {
                    context.Users.Add(new User { UserName = name });

                    context.SaveChanges();
                }
            }

            using (var context = new ChatContext())
            {
                user = context.Users.First(u => u.UserName == name);

                clients.TryAdd(user.Id, client);
            }
        }


        /// <summary>
        /// Регистрирует новое сообщение в базе данных и отправляет его клиенту
        /// </summary>
        /// <param name="tcpMessage">Объект передаваемых данных</param>
        private async void RegisterAndSendMessage(TcpMessage tcpMessage)
        {
            using var context = new ChatContext();

            var sender = context.Users.First(u => u.UserName == tcpMessage.SenderName);

            var consumer = context.Users.First(u => u.UserName == tcpMessage.ConsumerName);

            var message = new Message() { Author = sender, Consumer = consumer, IsReceived = false, Content = tcpMessage.Text };

            context.Messages.Add(message);

            context.SaveChanges();

            var user = clients.First(c => c.Key == message.ConsumerId);

            var writer = new StreamWriter(user.Value.GetStream());

            await writer.WriteLineAsync(tcpMessage.MessageToJson());

            await writer.FlushAsync();
        }


        private void Disconnect()
        {
            listener.Stop();
        }

        private void ManageServer()
        {
            string? str = Console.ReadLine();
            cts.Cancel();
        }
    }
}