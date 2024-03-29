﻿// Доработайте чат, заменив UDP-сокеты на NetMQ. Для этого напишите новую библиотеку, где в которой вы имплементируется IMessageSource и IMessageSourceClient
// с применением указанной библиотеки.

using Interfaces;
using NetMQLibrary;
using TcpLibrary;

public class Program
{
    public static void Main(string[] args)
    {
        IMessageSource server = MyTcpServer.Instance;
        server.Run();
    }
}