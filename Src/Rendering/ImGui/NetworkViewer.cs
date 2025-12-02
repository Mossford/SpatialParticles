using System;
using System.Net;
using ImGuiNET;
using System.Numerics;
using Riptide;
using SpatialEngine.Networking;

namespace SpatialEngine.Rendering.ImGUI
{
    public class NetworkViewer
    {
        static int port = 0;
        static string ip = "";
        static byte[] byteBuf = new byte[24];
        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Networking");
            if(!NetworkManager.running)
            {
                if(ImGui.Button("StartServer"))
                {
                    NetworkManager.InitServer();
                    NetworkManager.InitClient();
                }
                if (ImGui.Button("StartClient"))
                {
                    NetworkManager.InitClient();
                }
            }
            else
            {
                if(NetworkManager.isServer)
                {
                    if(NetworkManager.server.IsRunning())
                    {
                        ImGui.TextWrapped("Running Server on: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{NetworkManager.server.ip}:{NetworkManager.server.port}");
                        int currentSel = 0;
                        Connection[] connections = NetworkManager.server.GetServerConnections();
                        if (ImGui.BeginListBox("##Connections", new Vector2(ImGui.GetWindowSize().X, (connections.Length + 1) * ImGui.GetTextLineHeightWithSpacing())))
                        {
                            for (int n = 0; n < connections.Length; n++)
                            {
                                bool is_selected = (currentSel == n);
                                string name = $"Client {n + 1} at {connections[n]}";
                                if (ImGui.Selectable(name, is_selected))
                                    currentSel = n;

                                if (is_selected)
                                    ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndListBox();
                        }

                        if (ImGui.Button("Stop Server"))
                        {
                            NetworkManager.server.Stop();
                            NetworkManager.client.Close();
                        }
                    }
                    else
                    {
                        if (ImGui.Button("StartServer"))
                        {
                            NetworkManager.InitServer();
                            NetworkManager.InitClient();
                        }
                    }
                }
                if(NetworkManager.isClient)
                {
                    if(NetworkManager.client.IsConnected())
                    {
                        ImGui.TextWrapped("Connected to Server: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{NetworkManager.client.connectIp}:{NetworkManager.client.connectPort}");
                        Connection clientConnect = NetworkManager.client.GetConnection();
                        ImGui.TextWrapped("Client on: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{clientConnect}");
                        ImGui.TextWrapped(string.Format($"Ping: {NetworkManager.client.GetPing() * 1000f:0.00}ms"));

                        if(ImGui.Button("Disconnect"))
                        {
                            NetworkManager.client.Disconnect();
                        }
                    }
                    else
                    {
                        ImGui.TextWrapped("Client not connected to server");
                        ImGui.InputText("IP Address", ref ip, 24, ImGuiInputTextFlags.CharsNoBlank);
                        ImGui.InputInt("Port", ref port);
                        port = (int)MathF.Abs(port);

                        if (ImGui.Button("Connect"))
                        {
                            if (IPAddress.TryParse(ip, out IPAddress address))
                            {
                                NetworkManager.client.Connect(address.ToString(), (ushort)port);
                            }
                        }
                    }
                }    
            }
            ImGui.End();
        }
    }
}