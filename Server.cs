using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using ServerData;
using System.Net;

namespace Server
{
    class Server
    {
        static Socket listenerSocket; //dung de nghe
        static List<ClientData> _clients;

        static void Main(string[] args){
            Console.WriteLine("starting server on " + Packet.GetIPAddress());

            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clients = new List<ClientData>();

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(Packet.GetIPAddress()), 4242);
            //lay ip cua may

            listenerSocket.Bind(ip); //cai ip cua may hien tai
            
            Thread listenThread = new Thread(ListenThread); //tao thread
            listenThread.Start();

        } //khoi tao server


        static void ListenThread()
        {
            for(; ; )
            {
                listenerSocket.Listen(0);
                _clients.Add(new ClientData(listenerSocket.Accept())); //them socket vao trong list
            }

        }//listener - nhan biet cac client dang connect

        public static void Data_IN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;

            byte[] buffer;
            int readbytes;

            for(; ; )
            {
                try
                {
                    buffer = new byte[clientSocket.SendBufferSize];

                    readbytes = clientSocket.Receive(buffer);

                    if (readbytes > 0)
                    {
                        //xu li data o day
                        Packet packet = new Packet(buffer);
                        DataManager(packet);
                    }
                }
                catch(SocketException ex)
                {
                    Console.WriteLine("a client disconnected! ");
                    Console.ReadLine();
                }
            }
        }//clientdata thread - nhan data tu tung client


        public static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case PacketType.Chat:
                    foreach (ClientData c in _clients)
                        c.clientSocket.Send(p.toBytes());
                    break;
            }
        }

        //quan ly data
    }

    class ClientData
    {
        public Socket clientSocket;
        public Thread clientThread; //
        public string id;

        public ClientData() {
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(Server.Data_IN);
            clientThread.Start(clientSocket);
            SendRegistrationPacket();
        }
        public ClientData(Socket clientSocket) {
            this.clientSocket = clientSocket;
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(Server.Data_IN);
            clientThread.Start(clientSocket);
            SendRegistrationPacket();

        }

        public void SendRegistrationPacket()
        {
            Packet p = new Packet(PacketType.Registration, "server");
            p.Gdata.Add(id);
            clientSocket.Send(p.toBytes());
        }

    }

}
