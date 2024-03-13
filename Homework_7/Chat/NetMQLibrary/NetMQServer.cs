using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetMQ.Sockets;
using System.Net.Http;
using NetMQ;
using DTO;
using DataBase;
using Interfaces;

namespace NetMQLibrary
{
    public class NetMQServer : IMessageSource
    {
        private static readonly Lazy<NetMQServer> lazyInstance = new Lazy<NetMQServer>(() => new NetMQServer());

       ConcurrentDictionary<int, NetMQFrame> clients;

        public static NetMQServer Instance => lazyInstance.Value;

        RouterSocket server;

        public CancellationTokenSource cts { get; private set; }

        private NetMQServer()
        {
            cts = new CancellationTokenSource();
            clients = new ConcurrentDictionary<int, NetMQFrame>();
        }

        public async Task Run()
        {
            server = new RouterSocket();
            server.Bind("tcp://*:55555");
            var token = cts.Token;
            try
            {
                var threadManageServer = new Thread(ManageServer);

                threadManageServer.Start();

                await Console.Out.WriteLineAsync("Запущен\nДля прекращения работы сервера нажмите на любую клавишу");

                ProcessClient();
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

        private void ProcessClient()
        {
            while (true)
            {
                try
                {
                    var request = server.ReceiveMultipartMessage();

                    string json = request.Last.ConvertToString();

                    TcpMessage? message = TcpMessage.JsonToMessage(json);

                    switch (message.Status)
                    {
                        case Command.Registered:
                            RegisterClient(request);
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
                    Console.WriteLine("Ошибка: " + ex.Message);
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
        private void RegisterClient(NetMQMessage netMQMessage)
        {
            string json = netMQMessage.Last.ConvertToString();

            TcpMessage? message = TcpMessage.JsonToMessage(json);

            string name = message.SenderName;

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

                clients.TryAdd(user.Id, netMQMessage.First);
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

            var responseMessage = new NetMQMessage();

            responseMessage.Append(user.Value);

            responseMessage.Append(tcpMessage.MessageToJson());

            server.SendMultipartMessage(responseMessage);
        }


        private void Disconnect()
        {
            server.Dispose();
        }

        private void ManageServer()
        {
            string? str = Console.ReadLine();
            cts.Cancel();
        }
    }
}
