namespace TcpChat
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new Client();

            // Применяем структурный паттерн "Фасад", для скрытия сложной системы запуска клиента
            await client.Run();
        }
    }
}