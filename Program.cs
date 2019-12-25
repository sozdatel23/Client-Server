using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;

namespace Network_communication
{
    class Program
    {
        static void Main(string[] args)
        {/*
            TheListener listener = new TheListener(2001, 2046);
            listener.SetupLogger(DateTime.Now);
            listener.Start();
           */
            
            ScanSeti scanner = new ScanSeti(2000, 2046);
            scanner.SetupLogger(DateTime.Now);
            scanner.Start();

            Console.ReadLine();
        }

        //прослушивание соединений
        class TheListener
        {
            private int portFrom;
            private int portTo;

            private Logger logger;

            public TheListener(int portFrom, int portTo)
            {
                this.portFrom = portFrom;
                this.portTo = portTo;
            }

            public void SetupLogger(DateTime date)
            {
                logger = new Logger(date.ToString("MM-dd-yyyy HH-mm-ss") + ".log");
            }

            public void SetupLogger(string filePath)
            {
                logger = new Logger(filePath);
            }

            public void Start()
            {
                TcpListener listener = null;
                try
                {
                    int port;
                    for (port = portFrom; port <= portTo; port++)
                    {
                        try
                        {
                            IPAddress ip = IPAddress.Parse("127.0.0.1");
                            logger.Log(String.Format("Попытка привязки к порту {0}", port));
                            listener = new TcpListener(ip, port);
                            listener.Start();
                        }
                        catch
                        {
                            logger.Log(String.Format("Не удалось привязаться к порту {0}", port));
                            continue;
                        }
                        logger.Log(String.Format("Прослушивание порта {0}", port));
                        break;
                    }

                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();

                        string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

                        logger.Log(String.Format("Принят клиент {0}:{1}", clientIp, clientPort));

                        Thread t = new Thread(new ParameterizedThreadStart(CommunicateClient));
                        t.Start(client);
                    }

                }
                catch (SocketException e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
                finally
                {
                    listener.Stop();
                    logger.Log(String.Format("Остановка прослушивания."));
                    Console.Read();
                }
            }

            //обращение к клиенту
            private void CommunicateClient(Object obj)
            {
                TcpClient client = (TcpClient)obj;
                NetworkStream stream = client.GetStream();

                byte[] inputBytes = new byte[256];
                int numBytes;
                while ((numBytes = stream.Read(inputBytes, 0, inputBytes.Length)) != 0)
                {
                    String inputMsg = System.Text.Encoding.ASCII.GetString(inputBytes, 0, numBytes);
                    string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    string clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
                    logger.Log(String.Format("Получено собщение от {0}:{1}: {2} ", clientIp, clientPort, inputMsg));

                    if (inputMsg == "Do you understand me?")
                    {
                        String outputMsg = "Yes, I do";
                        byte[] outputBytes = System.Text.Encoding.ASCII.GetBytes(outputMsg);
                        stream.Write(outputBytes, 0, outputBytes.Length);
                        logger.Log(String.Format("Сообщение отправлено к {0}:{1}: {2}", clientIp, clientPort, outputMsg));
                    }
                }

                client.Close();
            }
        }


        //сканирование сети
        class ScanSeti
        {
            private int portRangeMin;
            private int portRangeMax;

            private Logger logger;

            private IPAddress localIp;
            private IPAddress localMask;

            private List<Sosed> neighbours;

            public ScanSeti(int portRangeMin, int portRangeMax)
            {
                this.portRangeMin = portRangeMin;
                this.portRangeMax = portRangeMax;

                neighbours = new List<Sosed>();
            }

            public void SetupLogger(DateTime date)
            {
                logger = new Logger(date.ToString("MM-dd-yyyy HH-mm-ss") + ".log");
            }

            public void SetupLogger(string filePath)
            {
                logger = new Logger(filePath);
            }


            public List<Sosed> Start()
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("77.77.77.77", 7777);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                IPAddress localIp = endPoint.Address;

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface intface in interfaces)
                {
                    if (intface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }
                    UnicastIPAddressInformationCollection unicastInfos = intface.GetIPProperties().UnicastAddresses;
                    foreach (UnicastIPAddressInformation unicastInfo in unicastInfos)
                    {
                        if (unicastInfo.Address.ToString() == localIp.ToString())
                        {
                            Console.WriteLine(intface.Description);
                            Console.WriteLine("\tIP адрес {0}", unicastInfo.Address);
                            Console.WriteLine("\tМакска подсети {0}", unicastInfo.IPv4Mask);
                            localMask = unicastInfo.IPv4Mask;
                        }
                    }
                }
                this.localIp = localIp;

                byte[] ipBytes = localIp.GetAddressBytes();
                byte[] maskBytes = localMask.GetAddressBytes();

                byte[] startIPBytes = new byte[ipBytes.Length];
                byte[] endIPBytes = new byte[ipBytes.Length];

                for (int i = 0; i < ipBytes.Length; i++)
                {
                    startIPBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                    endIPBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
                }

                IPAddress startIP = new IPAddress(startIPBytes);
                IPAddress endIP = new IPAddress(endIPBytes);
                Console.WriteLine("Первый IP {0}", startIP);
                Console.WriteLine("Последний IP {0}", endIP);

                byte[] ipStart = startIP.GetAddressBytes();
                byte[] ipEnd = endIP.GetAddressBytes();
                uint startIp = (
                    (uint)ipStart[0] << 24 |
                    (uint)ipStart[1] << 16 |
                    (uint)ipStart[2] << 8 |
                    (uint)ipStart[3]);
                uint endIp = (
                    (uint)ipEnd[0] << 24 |
                    (uint)ipEnd[1] << 16 |
                    (uint)ipEnd[2] << 8 |
                    (uint)ipEnd[3]);

                logger.Log("Сканирование LAN...");
                List<Thread> threads = new List<Thread>();
                for (uint uintIp = startIp; uintIp < endIp; uintIp++)
                {
                    string strIp = String.Format("{0}.{1}.{2}.{3}",
                        (uintIp & 0xFF000000) >> 24,
                        (uintIp & 0x00FF0000) >> 16,
                        (uintIp & 0x0000FF00) >> 8,
                        (uintIp & 0x000000FF)
                    );

                    IPAddress ip = IPAddress.Parse(strIp);
                    if (ip.ToString() == localIp.ToString())
                    {
                        ip = IPAddress.Parse("127.0.0.1");
                    }

                    Thread t = new Thread(ProverkaSosed);
                    t.Start(ip);
                    threads.Add(t);
                }
                foreach (Thread t in threads)
                {
                    t.Join();
                }

                logger.Log("Сканирование завершено");

                return neighbours;
            }

            //проверка на соседа. ответ на сообщение
            private void ProverkaSosed(object obj)
            {
                IPAddress ip = (IPAddress)obj;
                for (int port = portRangeMin; port <= portRangeMax; port++)
                {
                    SendSMS SMSSender = new SendSMS(ip.ToString(), port);
                    SMSSender.SetupLogger(logger.GetPath());
                    bool result = SMSSender.Connect();
                    if (result)
                    {
                        string response = SMSSender.Send("Do you understand me?");
                        if (response == "Yes, I do")
                        {
                            string hostname;
                            try
                            {
                                IPAddress target = (ip.ToString() == IPAddress.Parse("127.0.0.1").ToString() ? this.localIp : ip);
                                hostname = Dns.GetHostEntry(target).HostName;
                            }
                            catch
                            {
                                hostname = "unknown";
                            }
                            logger.Log(String.Format("{0}:{1}:{2} это наш сосед!", ip, port, hostname));
                            Sosed sosed = new Sosed(ip, port);
                            neighbours.Add(sosed);
                        }
                    }
                }
            }
        }
                       

        //соседи
        class Sosed
        {
            private IPAddress ip;
            private int port;

            public Sosed(string ip, int port)
            {
                this.ip = IPAddress.Parse(ip);
                this.port = port;
            }

            public Sosed(IPAddress ip, int port)
            {
                this.ip = ip;
                this.port = port;
            }

            public IPAddress GetIp()
            {
                return this.ip;
            }

            public int GetPort()
            {
                return this.port;
            }

        }


        //отправка сообщений
        
        class SendSMS
        {
            private TcpClient client;
            private string ip;
            private int port;

            private Logger logger;

            public SendSMS(string ip, int port)
            {
                this.ip = ip;
                this.port = port;
            }

            public void SetupLogger(DateTime date)
            {
                logger = new Logger(date.ToString("MM-dd-yyyy HH-mm-ss") + ".log");
            }

            public void SetupLogger(string filePath)
            {
                logger = new Logger(filePath);
            }

            public bool Connect()
            {
                try
                {
                    client = new TcpClient();
                    var result = client.BeginConnect(ip, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));
                    if (!success)
                    {
                        throw new Exception(String.Format("Соединение не удалось: {0}:{1}", ip, port));
                    }
                    client.EndConnect(result);

                    logger.Log(String.Format("Связь с {0} на порту {1}", ip, port));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public string Send(string message, bool needResponse = true)
            {
                try
                {
                    byte[] SMSBytes = System.Text.Encoding.ASCII.GetBytes(message);
                    NetworkStream stream = client.GetStream();
                    stream.Write(SMSBytes, 0, SMSBytes.Length);
                    logger.Log(String.Format("Сообщение отправлено к {0}:{1}: {2}", ip, port, message));
                    if (needResponse)
                    {
                        byte[] outputBuf = new byte[256];
                        stream.ReadTimeout = 10000;
                        int numBytes = stream.Read(outputBuf, 0, outputBuf.Length);
                        string responseMsg = System.Text.Encoding.ASCII.GetString(outputBuf, 0, numBytes);
                        logger.Log(String.Format("{0}:{1} ответил: {2}", ip, port, responseMsg));
                        stream.Close();
                        client.Close();
                        return responseMsg;
                    }
                    stream.Close();
                    client.Close();
                    return String.Empty;
                }
                catch
                {
                    Console.WriteLine("Нарушена связь с {0}:{1}", ip, port);
                    return String.Empty;
                }
            }

        }
        
    }
}
