// Task 5

//Shu paytgacha bizni Redis restaurant imiz mijozlarni qabul qilayotgan edi. Lekin 1 - 4 taskga cha, bizni ishchilarimiz faqat har qanday mijoz
//soroviga PONG deb javob berayotgan edi. Hozirgi restoranimiz turli mijozlarga bir paytda ham javob bera olayapti. 

//Bu qisimda, biz endi biroz yangilik kiritamiz. Yani biz mijozlar qo'liga menu beramiz va buyurtma qila oladi, 
//va hizmatkorlar esa bu buyurtmani qaytaradi. 

using System.Threading.Tasks;

Console.WriteLine("Mijoz va ofitsant muloqoti shu yerda: ");

TcpListener server = new TcpListener(IPAddress.Any, 6734);
server.Start();

while (true)
{
    var client = server.AcceptSocket();

    Task.Run(() => EchoCommand(client));
}

static async Task EchoCommand(TcpClient client)
{
    Dictionary<string, string> storage = [];

    while (client.Connected)
    {
        byte[] buffer = new byte[1024];

        int bytes = await client.Client.ReceiveAsync(buffer);

        var requestedData = Encoding.UTF8.GetString(buffer).Split("\n\r");

        string responseMessage = "";

        if (requestedData.Length > 2)
        {
            string request = requestedData[2].ToLower();

            switch (request)
            {
                case "ping":
                    responseMessage = "PONG";
                case "echo":
                    responseMessage = $"${requestedData[4].Length}\r\n{requestedData[4]}\r\n";
            }
        }

        await client.Client.SendAsync(Encoding.UTF8.GetBytes(responseMessage));
    }
}