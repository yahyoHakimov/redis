Console.WriteLine("Logs from your program will appear here");


TcpListener tcpListener = new TcpListener(IPAddress.Any, 6379);

server.Start();

server.AcceptSocket();