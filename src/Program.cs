//Task 3

//Xo'sh bu task avvalgisidan biroz farq qiladi. Avval mijoz kelib faqat ishlayapti derdi va ketar edi. Endi esa mijoz keladi va bir necha savollarni 
//beradi, va bir xil javob oladi yani Pong Pong. Avvalgi holatda biz yozgan code bir necha javob bera olmasdi. Endi esa javob bera oladi

Console.WriteLine("Mijoz va restaurant muloqoti pastda ko'rindadi: ");

TcpListener server = new TcpListener(IPAddress.Any, 6739);
server.Start();

var client = server.AcceptSocket();

while (true)
{
    var suhbatQutisi = new byte[1024];
    var qabulQilinganHabar = client.ReceiveAsync(suhbatQutisi, SocketFlags.None);
    var response = Encoding.UTF8.GetString(suhbatQutisi, 0, qabulQilinganHabar);

    var suffix = "\r\n";

    if (response.IndexOf(suffix) >= 0)
    {
        Console.WriteLine($"Mijoz habari qabul qilindi: \"{response.Replace(suffix, "")}");

        var pong = "+PONG" + suffix;

        var sendingBytes = Encoding.UTF8.GetString(pong);

        await client.SendAsync(sendingBytes, SocketFlag.None);

        Console.WriteLine($"Habar mijozga jo'natildi: \"{pong.Replace(suffix, "")}\"");
    }
}