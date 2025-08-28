// First Task
//Biz bu taskda Redis bilan aloqa qilishimiz uchun kerakli bo'lgan TCP yo'lni ochish va yo'l raqamini tanishitirishimiz kerak. 
//Yani siz agar Redis ishlatgan bo'lsangiz siz terminaldan foydalangansiz. 
//Shu termminalda beriladigan commandlar esa sizni redis bilan bog'lagan. 
//Bu commandlar esa Tcp esa reliable protocol yani sizni commandlarizi aytilgandek ketishiga yordam beradi.
// Keyin esa bizni commandlar qayesi yo'l orqali ketishini aytish uchun port ham biriktiramiz

//Bu kabi texnik so'zlarni tushunmaydigan do'stlarimiz uchun esa biz hayotiy misollar  ham keltiramiz. Tassavur qiling siz Redis nomli restoran ochmoqchisiz. 
//Siz bu restoranni daromadi yo'q va mijozi yo'q holatdan daromadli va mijozi ko'p bo'ladigan restoranga aylantirmoqchisiz. 
//Bizni bu 1 - Task bu Redis restoran eshigiga raqam qo'yish. Yani port 6732 raqamli uy bizni Redis resotranimiz eshik raqami bo'ladi. 
//Task dagi tester esa bizni tekshirib ko'radi yani Tester kelganda eshikni taqqilatadi va eshik ochilishi yani javob kelishi kerak. 


//Boshladik

Console.WriteLine("Hey, biz Redis restoranini ochdik va mijozlarga xizmat ko'rsatishni boshlaganmiz. Eshik raqamimiz esa 6739: ");

//Biz bu yerda Redis restoranini mijozlar topib kelishi uchun raqamladik.
TcpListener server = new TcpListener(IPAddress.Any, 6739);

//Restoranni esa sahar ochdik
server.Start();

// Va mijozlar qabul qilishni boshladi
server.AcceptSocket();
