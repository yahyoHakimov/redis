// 6 - Task

TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

while (true) {
  TcpClient client = server.AcceptTcpClient(); // wait for client

  _tasks.Add(HandleClientAsync(client)); // handle client in background
}

public partial class Program {
  delegate Task AsyncCommandHandler(Stream stream, string msg,
                                    CancellationToken cancellation);

  static readonly byte[] PONG_RESPONSE = Encoding.UTF8.GetBytes("+PONG\r\n");
  static readonly byte[] OK_RESPONSE = Encoding.UTF8.GetBytes("+OK\r\n");
  static readonly byte[] NULL_RESPONSE = Encoding.UTF8.GetBytes("$-1\r\n");

  static readonly Dictionary<string, AsyncCommandHandler> COMMANDS =
      new(StringComparer.OrdinalIgnoreCase) { { "PING", PingCommandAsync },
                                              { "ECHO", EchoCommandAsync },
                                              { "SET", SetCommandAsync },
                                              { "GET", GetCommandAsync } };

  static readonly Dictionary<string, string> VALUES = new();

  static readonly List<Task> _tasks = new List<Task>();

  static async Task HandleClientAsync(TcpClient client) {
    using CancellationTokenSource src = new CancellationTokenSource();

    CancellationToken token = src.Token;

    using Stream stream = client.GetStream();

    StringBuilder sb = new StringBuilder();

    byte[] buffer = ArrayPool<byte>.Shared.Rent(100);
    char[] chars = ArrayPool<char>.Shared.Rent(buffer.Length);

    while (!token.IsCancellationRequested && client.Connected) {
      int bytesRead = await stream.ReadAsync(buffer, token);

      while (!token.IsCancellationRequested && bytesRead > 0) {
        int charsWritten =
            Encoding.UTF8.GetChars(buffer, 0, bytesRead, chars, 0);

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
    ArrayPool<char>.Shared.Return(chars, true);

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

  static async Task ClearKeyAfterAsync(int milliseconds, string key) {
    await Task.Delay(milliseconds);

    VALUES.Remove(key);
  }
}