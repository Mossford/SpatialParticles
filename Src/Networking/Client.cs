using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Riptide;
using Riptide.Transports;
using Riptide.Utils;

using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Globals;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using SpatialEngine.Rendering;
using System.Collections.Generic;
using SpatialEngine.SpatialMath;
using SpatialEngine.Networking.Packets;
using SpatialGame;

namespace SpatialEngine.Networking
{
    public class SpatialClient
    {
        public static Client client;

        public ushort connectPort;
        public string connectIp;

        bool disconnected;
        bool stopping;

        bool waitPing = false;

        public float currentPing { get; protected set; }
        public int pingCount { get; protected set; } = 0;
        float lastPing = 0f;
        

        public SpatialClient()
        {
            Message.InstancesPerPeer = 100;
        }

        public void Start(string ip, ushort port)
        {
            connectIp = ip;
            connectPort = port;
            client = new Client();
            client.Connected += Connected;
            client.Disconnected += Disconnected;
            client.MessageReceived += handleMessage;
            Connect(connectIp, connectPort);
        }

        public void Connect(string ip, ushort port)
        {
            client.Connect($"{ip}:{port}", 5, 0, null, false);
            connectIp = ip;
            connectPort = port;

            ConnectPacket connectPacket = new ConnectPacket();
            SendRelib(connectPacket);
            PlayerJoinPacket playerJoinPacket = new PlayerJoinPacket(0, player.position);
            SendRelib(playerJoinPacket);
            disconnected = false;
        }

        public void Disconnect() 
        {
            client.Disconnect();
            disconnected = true;
            NetworkManager.running = false;
        }

        static float accu = 0f;
        public void Update(float deltaTime)
        {
            if (!stopping && !disconnected)
            {
                /*for (int i = 0; i < scene.SpatialObjects.Count; i++)
                {
                    SpatialObjectPacket packet = new SpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation);
                    SendUnrelib(packet);
                }*/
                PlayerPacket packet = new PlayerPacket(Mouse.localPosition, Mouse.lastLocalPosition, SimInput.mouseSpawnRadius, SimInput.mousePressed, SimInput.mouseSelection, SimInput.mouseButtonPress, SimInput.selectionMode);
                SendUnrelib(packet);
                client.Update();


                //get ping every 1 seconds and if nothing can be done disconnect from server as time out
                accu += deltaTime;
                while (accu >= 700f)
                {
                    accu -= 700f;
                    GetPingAsync();
                }
            }
        }

        void Connected(object sender, EventArgs e)
        {
               
        }

        void Disconnected(object sender, EventArgs e)
        {
            connectIp = "";
            connectPort = 0;
        }

        public void SendUnrelib(Packet packet)
        {
            if(client.IsConnected || !stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                client.Send(msgUnrelib);
            }
        }

        //calling this a lot causes null error on the message create
        public void SendRelib(Packet packet)
        {
            if (client.IsConnected || !stopping)
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                client.Send(msgRelib);
            }
        }

        public void Close()
        {
            client.Disconnect();
            stopping = true;
            client = null;
        }

        public void handleMessage(object sender, MessageReceivedEventArgs e)
        {
            if (!stopping)
                HandlePacketClient(e.Message.GetBytes());
        }

        //gets the ping of the client and removes the delay caused by waiting 16ms for an update so a true ping can
        //be returned. With checking for if the ping is less than 0 which returns the ping before it
        public float GetPing()
        {
            if(currentPing - 0.0166f > 0f)
            {
                return currentPing - 0.0166f;
            }
            else
            {
                return lastPing - 0.0166f;
            }
            
        }

        async Task GetPingAsync()
        {
            await Task.Run(() => 
            {
                float timeStart = Globals.GetTime();
                PingPacket packet = new PingPacket();
                SendRelib(packet);
                waitPing = true;
                float accum = 0f;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while(waitPing)
                {
                    //stop checking for ping after 15 seconds
                    if(stopwatch.ElapsedMilliseconds / 1000 >= 1)
                    {
                        Console.WriteLine("Timed out: Could not ping server");
                        Disconnect();
                        break;
                    }
                }
                stopwatch.Stop();
                float timeEnd = Globals.GetTime();
                if (currentPing - 0.0166f > 0f)
                    lastPing = currentPing;
                currentPing = timeEnd - timeStart;
                pingCount++;
            });
        }

        //Handles packets that come from the server

        void HandlePacketClient(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            //data sent is not a proper packet
            if (data.Length < 2)
                return;

            //packet type
            ushort type = reader.ReadUInt16();

            switch (type)
            {
                case (ushort)PacketType.Pong:
                    {
                        waitPing = false;
                        break;
                    }
                case (ushort)PacketType.ConnectReturn:
                    {
                        ConnectReturnPacket packet = new ConnectReturnPacket();
                        packet.ByteToPacket(data);
                        Console.WriteLine("Server version is: " + packet.engVersion + " Client version is: " + EngVer);
                        break;
                    }
                case (ushort)PacketType.SceneSyncStart:
                    {
                        SceneSyncClear packet = new SceneSyncClear();
                        SendRelib(packet);
                        
                        //clear current sim
                        GameManager.ReInitGame();
                        break;
                    }
                case (ushort)PacketType.Player:
                    {
                        PlayerPacket packet = new PlayerPacket();
                        packet.ByteToPacket(data);
                        
                        //since the code auto adds on multiple spawner circles we can just call the function
                        string name = ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleIndexes[packet.selection]].name;
                        MouseInteraction.DrawMouseElementSelect(packet.position, packet.radius, packet.pressing, name, packet.selectionMode, packet.selection, packet.id);
                        MouseInteraction.DrawMouseElementsCircle(packet.position, packet.radius, packet.pressing, packet.id);
                        MouseInteraction.SpawnParticlesCircleSpawner(packet.position, packet.lastPosition, packet.radius, packet.pressing, packet.mouseButtonPress, name, packet.selectionMode, packet.selection);
                        break;
                    }
                case (ushort)PacketType.PlayerJoin:
                    {
                        PlayerJoinPacket packet = new PlayerJoinPacket();
                        
                        break;
                    }
                case (ushort)PacketType.PlayerLeave:
                    {
                        PlayerLeavePacket packet = new PlayerLeavePacket();
                        packet.ByteToPacket(data);
                        break;
                    }
            }
        }

        public Connection GetConnection() => client.Connection;
        public bool IsConnected() => client.IsConnected;
    }
}