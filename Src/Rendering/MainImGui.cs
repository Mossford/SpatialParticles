using ImGuiNET;
using Silk.NET.OpenGL;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Riptide;

//engine stuff
using static SpatialEngine.Globals;
using SpatialEngine.Networking;
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using SpatialGame;

namespace SpatialEngine.Rendering
{
    public static class MainImGui
    {
        static bool ShowConsoleViewerMenu, ShowNetworkViewerMenu;
        static ImFontPtr font;
        static float fpsCount;
        static float fpsTotal;
        static float msCount;
        static float msTotal;
        static float fpsTime;

        public static void Init()
        {
            font = ImGui.GetFont();
            font.Scale = 1.35f;
        }

        public static void ImGuiMenu(float deltaTime)
        {
            ImGuiWindowFlags window_flags = 0;
            window_flags |= ImGuiWindowFlags.NoTitleBar;
            window_flags |= ImGuiWindowFlags.MenuBar;

            ImGui.Begin("SpaceTesting", window_flags);

            //needs to be io.framerate because the actal deltatime is polled too fast and the 
            //result is hard to read
            ImGui.Text("Version " + EngVer);
            ImGui.Text("OpenGl " + OpenGlVersion);
            ImGui.Text("Gpu: " + Gpu);
            ImGui.Text(String.Format("{0:N3} ms/frame ({1:N1} FPS)", 1.0f / ImGui.GetIO().Framerate * 1000.0f, ImGui.GetIO().Framerate));
            ImGui.Text(String.Format("{0:N3} ms Avg ({1:N1} FPS Avg)", msTotal / fpsCount, fpsTotal / fpsCount));
            ImGui.Text(String.Format("DrawCall per frame: ({0:N1})", MathF.Round(drawCallCount)));

            fpsTotal += ImGui.GetIO().Framerate;
            msTotal += 1.0f / ImGui.GetIO().Framerate * 1000f;
            fpsCount++;
            msTotal++;
            fpsTime += deltaTime;
            if(fpsTime >= 10)
            {
                fpsTime = 0f;
                fpsTotal = 0f;
                fpsCount = 0;
                msTotal = 0;
                msCount = 0;
            }

            drawCallCount = 0;

            ImGui.Text(String.Format("Time Open {0:N1} minutes", totalTime / 60.0f));
            float mem = SpatialGame.DebugSimulation.GetCurrentMemoryOfSim();
            if (mem < 1f)
            {
                ImGui.Text(String.Format("Simulation has {0:N2}KB of particles pooled", mem * 1024f));
            }
            else
            {
                ImGui.Text(String.Format("Simulation has {0:N2}MB of particles pooled", mem));
            }
            mem = SpatialGame.DebugSimulation.GetCurrentMemoryOfSimGPU();
            if (mem < 1f)
            {
                ImGui.Text(String.Format("Simulation has {0:N2}KB of buffers", mem * 1024f));
            }
            else
            {
                ImGui.Text(String.Format("Simulation has {0:N2}MB of buffers", mem));
            }
            ImGui.Text(String.Format("Simulation has {0} of particles Spawned", SpatialGame.ParticleSimulation.particleCount));
            ImGui.Text(String.Format("Current resolution {0}, {1}", SpatialGame.PixelColorer.width, SpatialGame.PixelColorer.height));
            ImGui.Text(String.Format("Selected Particle {0}", ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleIndexes[SpatialGame.SimInput.mouseSelection]].name));

            if (ImGui.Checkbox("Vsync", ref vsync))
            {
                window.VSync = vsync;
            }
            ImGui.Checkbox("EnableParticleLighting", ref Settings.SimulationSettings.EnableParticleLighting);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Menus"))
                {
                    ImGui.MenuItem("Console Viewer", null, ref ShowConsoleViewerMenu);
                    ImGui.MenuItem("Network Viewer", null, ref ShowNetworkViewerMenu);
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ShowConsoleViewerMenu)
            {
                ConsoleViewer();
            }

            if (ShowNetworkViewerMenu)
            {
                NetworkViewer();
            }
        }

        static void ConsoleViewer()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Console Viewer");
            ImGui.BeginChild("Output", new Vector2(0, ImGui.GetWindowSize().Y * 0.9f), true, ImGuiWindowFlags.NoResize);
            for (int i = 0; i < Debugging.consoleText.Count; i++)
            {
                if(i == 0)
                    ImGui.Text("> " + (Debugging.consoleText.Count - i) + " " + Debugging.consoleText[i]);
                else
                    ImGui.Text(Debugging.consoleText.Count - i + " " + Debugging.consoleText[i]);
            }
            ImGui.EndChild();
            ImGui.End();
        }
        
        static int port = 0;
        static string ip = "";
        static byte[] byteBuf = new byte[24];
        static void NetworkViewer()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Network Viewer");
            if(!NetworkManager.running)
            {
                if(ImGui.Button("StartServer"))
                {
                    NetworkManager.InitServer();
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
                        ImGui.Text("Running Server on: ");
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
                        }
                    }
                    else
                    {
                        if (ImGui.Button("StartServer"))
                        {
                            NetworkManager.InitServer();
                        }
                    }
                }
                else
                {
                    if(NetworkManager.client.IsConnected())
                    {
                        ImGui.Text("Connected to Server: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{NetworkManager.client.connectIp}:{NetworkManager.client.connectPort}");
                        Connection clientConnect = NetworkManager.client.GetConnection();
                        ImGui.Text("Client on: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{clientConnect}");
                        ImGui.Text(string.Format($"Ping: {NetworkManager.client.GetPing() * 1000f:0.00}ms"));

                        if(ImGui.Button("Disconnect"))
                        {
                            NetworkManager.client.Disconnect();
                        }
                    }
                    else
                    {
                        ImGui.Text("Client not connected to server");
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
