//Task 2

//Bu taskda biz Redis ishlayotganini bilishimiz uchun kerak bo'ladigan PING commandni ishlatib ko'rishimiz kerak, va javob tarzida PONG jo'natsak bo'ladi

//Hayotiy misolda esa

//Redis restaurant esa ishni boshladi, va biz eshik oldida hamma mijozlarga "xush kelibsiz, marahamat" deyish uchun ishchi yolladik. 
//Bu taskda esa mijoz eshik yoniga keladi va so'raydi "Sizlar ishlayapsizlarmi" xodim esa so'rov nimaligiga etibor bermasdan "Biz ishlayapmiz
// PONG nomli javob beradi"

//Boshladik

Console.WriteLine("Sizni mijoz bilan aloqangiz shu yerda ko'rinadi.");
TcpListener server = new TcpListener(IPAddress.Any, 6739);
server.Start();

var client = server.AcceptSocket();

string message = "+PONG\r\n";

//bu code bizga inson yozilgan yozuvlarni protokol tushunadigan byte larga o'girib beradi. 
byte[] data = Encoding.UTF8.GetBytes(message);

client.Send(data);