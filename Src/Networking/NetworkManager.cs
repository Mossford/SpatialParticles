using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Riptide;
using Riptide.Utils;

namespace SpatialEngine.Networking
{
    public static class NetworkManager
    {
        public static SpatialClient client;
        public static SpatialServer server;
        public static bool isServer;
        public static bool isClient;
        public static bool didInit;
        public static bool running;

        public static void Init()
        {
            RiptideLogger.Initialize(Console.WriteLine, true);
        }

        public static void InitClient()
        {
            client = new SpatialClient();
            client.Start("127.0.0.1", 58301);
            didInit = true;
            isClient = true;
            running = true;
        }

        public static void InitServer() 
        {
            server = new SpatialServer("127.0.0.1");
            server.Start();
            didInit = true;
            isServer = true;
            running = true;
        }

        public static void Cleanup()
        {
            if(didInit)
            {
                if(isServer)
                {
                    server.Close();
                    isServer = false;
                }
                else
                {
                    client.Close();
                    isServer = false;
                }
            }
        }
    }
}