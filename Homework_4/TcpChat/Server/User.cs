using System.Net.Sockets;

namespace TcpChat
{
    internal class User
    {
        public string Id { get; }
        public StreamWriter Writer { get; }
        public StreamReader Reader { get; }

        private TcpClient client;

        private ChatServer server;

        public User(TcpClient client, ChatServer server)
        {
            Id = Guid.NewGuid().ToString();

            this.client = client;

            this.server = server;

            var stream = client.GetStream();

            Reader = new StreamReader(stream);

            Writer = new StreamWriter(stream);
        }

        public async Task ProcessAsync()
        {
            try
            {
                string? userName = await Reader.ReadLineAsync();

                string? message = $"{userName} вошёл в чат";

                await server.BroadcastMessageAsync(message, Id);

                Console.WriteLine(message);

                while (true)
                {
                    try
                    {
                        message = await Reader.ReadLineAsync();

                        if (message == null)
                        {
                            continue;
                        }
                        else if (message == "Exit")
                        {
                            message = $"{userName} покинул чат";

                            Console.WriteLine(message);

                            await server.BroadcastMessageAsync(message, Id);

                            server.RemoveConnection(Id);

                            break;
                        }
                        else
                        {
                            message = $"{userName}: {message}";

                            Console.WriteLine(message);

                            await server.BroadcastMessageAsync(message, Id);
                        }
                    }
                    catch
                    {
                        if (!server.cts.IsCancellationRequested)
                        {
                            message = $"{userName} покинул чат";

                            Console.WriteLine(message);

                            await server.BroadcastMessageAsync(message, Id);

                            server.RemoveConnection(Id);

                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
            }
        }

        public void Close()
        {
            Writer.Close();

            Reader.Close();

            client.Close();
        }
    }
}