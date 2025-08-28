//6 - task

//Bu qismda endi biz haqiqiy redis tizimini ko'ramiz. Yani Restoran misolini ko'rsak mijoz kelib o'tirdi va ofitsantga zakaz berdi va endi uni xotiraga saqlab qo'ydi.
//Bundan avvalgi taskda siz echo yordamida unga buyruq berayotgan edi va u darhol esdan chqiayotgan edi. Endi esa bu narsa esidan chiqmedi yani zakasni girgitton 
// daftariga yozib oladi. Masalan menga bu pitsa yoqdi, desa girgitton buni yozib oladi va. Keyinchalik menga zakasni keltr deb GET desa zakasni qaytarib oladi. 

using System.Threading.Tasks;

TcpListener server = new TcpListener(IPAddress.Any, 6734);

server.Start();

while (true)
{
    var client = server.AcceptSocket();

    Task.Run(() => SetAndGet(client));
}

static async Task SetAndGet(TcpClient client)
{
    Dictionary<string, string> storage = new Dictionary<string, string>();

    while (client.Connected)
    {
        var buffer = new byte[1024];

        int bytes = client.Client.ReceivedAsync(buffer);

        var requestData = Encoding.ASCII.GetString(buffer).Split("\n\r");

        string responseString = "";

        if (requestData.Length > 2)
        {
            var request = requestData[2].ToLower();

            switch (request)
            {
                case "ping":
                    responseString = "+PONG";
                    break;
                case "echo":
                    responseString = $"${requestData[4].Length}\r\n{requestData[4]}\r\n";
                    break;
                case "set":
                    storage.Add(requestData[4], requestData[6]);
                    responseString = "+Ok\n\r";
                    break;
                case "get":
                    string data = storage(requestData[4]);
                    responseString = $"{data.Length}\n\r{data}\n\r";
                    break;

            }
        }
        await client.Client.SendAsync(Encoding.UTF8.GetBytes(responseString));
    }
}