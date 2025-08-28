// Task 4

//Restoranimizni bu qismi avvalgisidan biroz farq qiladi.
//Avval bir mijoz bir nechta "zakaslar" qilgan bo'lsa, bu safar endi Restoranimiz kengaydii va ko'p mijozlar kelishni boshladi
//Bu task maqsadi avval bir mijozni bir nechta so'rovlariga javob bergan bo'lsak endi bir necha mijozlarni bir necha so'rovlariga javob berishimiz kerak

Console.WriteLine("Mijoz va ofitsant bilan bo'lgan muloqot bu yerda aks etadi: ");

TcpListener server = new TcpListener(IPAddress.Any, 6732);
server.Start();

while (true)
{
    var client = server.AcceptSocket();

    Task(() => MultipleClient(client));

}

static void MultipleClient(TcpClient client)
{
    var stream = client.GetStream();

    while (client.Connected)
    {
        var muloqotQutisi = new byte[1024];
        var mijozSoroviOqish = stream.Read(muloqotQutisi, 0, muloqotQutisi.Length);
        var mijozSorovi = Encoding.UTF8.GetString(muloqotQutisi, 0, mijozSorovi);

        var javob = "+PONG\n\r";

        var responseByte = Encoding.UTF8.GetBytes(responseByte);

        stream.Write(responseByte, 0, responseByte.Length);
    }
    client.Console();
}