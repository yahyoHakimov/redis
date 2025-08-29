// 7 - Task

//Bu taskda biz Redis restaratimizni yanaham kengayganini va murakkabroq vazifalarni ham bajarayotganini ko'rishimiz mumkin. 
//Avval ofitsant kelib mijozga faqat PONG javobi yani har qanday savolga shunday javob berardi. 
//Keyin ofitsant mijoz aytgan so'rovni o'zini aytadigan bo'ldi. 
//oxirgilarida esa mijoz aytgan zakaslarni note ga yozib oladigan bo'ldi. Keyin bu notlarni saqlaydigan bo'ldi.
//Bu taskda esa biz endi bir nechta zakaslarni ham saqlab ola olish qobilyatini ishga tushuramiz

//Just comment for test my github profile

Console.WriteLine("Mijoz va ofitsant muloqotini bu yerda ko'rishiz mumkin: ");

TcpListener server = new TcpListener(IPAddress.Any, 6734);

server.Start();

//bu bizga bie necha clientlarga xizmat ko'rsatishga yordam beradi. 
while (true)
{
  TcpClient client = server.AcceptTcpClient();

  _task.Add(HandleClientAsync(client));
}

public partial class Program() {
  delegate Task AsyncCommandHandler(Stream stream, string msg, CancellationToken cancellationToken);
  static readonly byte[] PONG_RESPONSE = Encoding.UTF8.GetBytes("+PONG\r\n");
  static readonly byte[] OK_RESPONSE = Encoding.UTF8.GetBytes("+OK\r\n");
  static readonly byte[] NULL_RESPONSE = Encoding.UTF8.GetBytes("+$-1\r\n");
  static readonly Dictionary<string, AsyncCommandHandler> COMMANDS = new(StringComparer.OrdinalIgnoreCase) {
    {"PING", PingCommandAsync},
    {"ECHO", EchoCommandAsync},
    {"SET", SetCommandAsync},
    {"GET", GetCommandAsync},
    {"RPUSH", RPushCommandAsync}
  };

  static readonly Dictionary<string, string> VALUES = new();
  static readonly List<Task> _task = new List<Task>();

  static async Task HandleClientAsync(TcpClient client) {
    using CancellationTokenSource src = new CancellationTokenSource();
    CancellationToken token = src.Token();

    using Stream stream = client.GetStream();

    StringBuilder sb = new StringBuilder();

    byte[] buffer = ArrayPool<byte>.Shared.Rent(100);
    char[] chars = ArrayPool<char>.Shared.Rent(buffer.Length);

    while (!token.IsCancellationRequested && client.Connected)
    {
      int bytesRead = await stream.ReadAsync(buffer, token);

      while (!token.IsCancellationRequested && bytesRead > 0)
      {
        int charsWritten = Encoding.UTF8.GetChars(buffer, 0, bytesRead, chars, 0);

        sb.Append(chars, 0, charsWritten);

        if (sb[^2] == '\r' && sb[^1] == '\n') {
          sb.Length -= 2;

          string msg = sb.ToString();

          string[] parts = msg.Split("\r\n", 5);

          string cmd = parts[2];

          Console.WriteLine($"Length: {parts.Length}, Command: {cmd}");

          string arg = parts.Length >= 5 ? parts[4] : string.Empty;

          await COMMANDS[cmd].Invoke(stream, arg, token);

          sb.Clear();
        }

        bytesRead = await stream.ReadAsync(buffer, token);
      }
    }

    src.Cancel();

    ArrayPool<byte>.Shared.Return(buffer, true);
    ArrayPool<byte>.Shared.Return(chars, true);

    client.Dispose();
  }

  static async Task PingCommandAsync(Stream stream, string arg,
                                     CancellationToken cancellation) {
    await stream.WriteAsync(PONG_RESPONSE, cancellation);
  }

  static async Task EchoCommandAsync(Stream stream, string arg,
                                     CancellationToken cancellation) {
    byte[] response = Encoding.UTF8.GetBytes($"${arg.Length}\r\n{arg}\r\n");

    await stream.WriteAsync(response, cancellation);
  }

  static async Task SetCommandAsync(Stream stream, string arg,
                                    CancellationToken cancellation) {
    Console.WriteLine($"SET {arg}");

    string[] parts = arg.Split("\r\n", 2);

    string key = parts[0];

    string value = parts[1];

    ReadOnlySpan<char> valueSpan = value.AsSpan();

    // *NUM indicates additional arguments other than KEY VALUE
    if (valueSpan.Count('$') > 1) {
      string[] valueParts = value.Split("\r\n");

      value = $"{valueParts[0]}\r\n{valueParts[1]}";

      int length = valueParts.Length;

      for (int i = 0; i < length; i++) {
        string argument = valueParts[i];

        if (argument.Equals("px", StringComparison.OrdinalIgnoreCase)) {
          int milliseconds = int.Parse(valueParts[i + 2]);

          Console.WriteLine($"EXPIRY: {milliseconds}ms");

          _tasks.Add(ClearKeyAfterAsync(milliseconds, key));
        }
      }
    }

    VALUES[key] = $"{value}\r\n";

    await stream.WriteAsync(OK_RESPONSE, cancellation);
  }

  static async Task GetCommandAsync(Stream stream, string arg,
                                    CancellationToken cancellation) {
    Console.WriteLine($"GET {arg}");

        byte[] response = VALUES.TryGetValue(arg, out string? value)
            ? Encoding.UTF8.GetBytes(value) 
            : NULL_RESPONSE;

        await stream.WriteAsync(response, cancellation);
  }

  static async Task RPushCommandAsync(Stream stream, string arg, CancellationToken cancellation)
  {
    //if client savatchasi bo'sh bo'lsa
    // bizga yangi list ochiladi
    //agar savatcha bo'sh bo'lmasa eskisiga 1 ta yangi malumot qo'shiladi. 

    string[] parts = arg.Split("\r\n", 2);

    string[] key = parts[0];
    string[] value = parts[1];

    List<string> values;

    if (VALUES.TryGetValue(key, out object? instance))
    {
      if (instance is List<string> list)
      {
        values = list;
        values.Add(value);
      }
      else
      {
        throw new InvalidOperationException($"Key '{key}' already exists with a different type.");
      }
    }
    else
    {
      values = new List<string> { parts[1] };

      VALUES[key] = values;
    }

    int length = values.Count;

    byte[] response = Encoding.UTF8.GetBytes($":{length}\r\n");

    await stream.WriteAsync(response, cancellation);
  }
  static async Task ClearKeyAfterAsync(int milliseconds, string key)
  {
    await Task.Delay(milliseconds);

    VALUES.Remove(key);
  }
}
