// Структурируйте код клиента и сервера чата, используя знания о шаблонах.

namespace TcpChat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = ChatServer.Instance;

            // Применяем структурный паттерн "Фасад", для скрытия сложной системы запуска сервера
            await server.Run();
        }
    }
}