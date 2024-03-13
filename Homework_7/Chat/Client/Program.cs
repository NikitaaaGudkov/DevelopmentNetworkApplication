// Доработайте чат, заменив UDP-сокеты на NetMQ. Для этого напишите новую библиотеку, где в которой вы имплементируется IMessageSource и IMessageSourceClient
// с применением указанной библиотеки.

using Interfaces;
using NetMQLibrary;
using TcpLibrary;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IMessageSourceClient client = new MyTcpClient();

            await client.Run();
        }
    }
}