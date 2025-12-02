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
        static bool ShowConsoleViewerMenu, ShowNetworkViewerMenu, ShowSimMenu, ShowSimSettingsMenu, ShowParticleCreationMenu;
        static ImFontPtr font;
        static float fpsCount;
        static float fpsTotal;
        static float fpsMax;
        static float msCount;
        static float msTotal;
        static float fpsTime;

        public static void Init()
        {
            font = ImGui.GetFont();
            font.Scale = 1.35f;
            SetImGuiStyle();
        }

        public static void ImGuiMenu(float deltaTime)
        {
            ImGuiWindowFlags window_flags = 0;
            window_flags |= ImGuiWindowFlags.NoTitleBar;
            window_flags |= ImGuiWindowFlags.MenuBar;

            ImGui.Begin("SpatialParticles", window_flags);

            //needs to be io.framerate because the actal deltatime is polled too fast and the 
            //result is hard to read
            ImGui.TextWrapped("Version " + EngVer);
            ImGui.TextWrapped("OpenGl " + OpenGlVersion);
            ImGui.TextWrapped("Gpu: " + Gpu);
            ImGui.TextWrapped($"{1.0f / ImGui.GetIO().Framerate * 1000.0f:N3} ms/frame ({ImGui.GetIO().Framerate:N1} FPS)");
            ImGui.TextWrapped($"{msTotal / fpsCount:N3} ms Avg ({fpsTotal / fpsCount:N1} FPS Avg)");
            ImGui.TextWrapped($"{1.0f / fpsMax * 1000.0f:N3} ms/frame ({fpsMax:N1} FPS Max)");
            ImGui.TextWrapped($"DrawCall per frame: ({MathF.Round(drawCallCount):N1})");
            ImGui.TextWrapped($"Particles per ms: ({(PixelColorer.width * PixelColorer.height / ( 1.0f / ImGui.GetIO().Framerate * 1000.0f)):N1}p/ms)");

            if (fpsMax < ImGui.GetIO().Framerate)
            {
                fpsMax = ImGui.GetIO().Framerate;
            }
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

            ImGui.TextWrapped($"Time Open {totalTime / 60.0f:N1} minutes");
            float mem = SpatialGame.DebugSimulation.GetCurrentMemoryOfSim();
            if (mem < 1f)
            {
                ImGui.TextWrapped($"Simulation has {mem * 1000f:N2}KB of particles pooled");
            }
            else
            {
                ImGui.TextWrapped($"Simulation has {mem:N2}MB of particles pooled");
            }
            mem = SpatialGame.DebugSimulation.GetCurrentMemoryOfSimGPU();
            if (mem < 1f)
            {
                ImGui.TextWrapped($"Simulation has {mem * 1000f:N2}KB of buffers");
            }
            else
            {
                ImGui.TextWrapped($"Simulation has {mem:N2}MB of buffers");
            }
            ImGui.TextWrapped($"Simulation has {SpatialGame.ParticleSimulation.totalParticleCount} of particles Spawned");
            ImGui.TextWrapped($"Simulation has {SpatialGame.ParticleChunkManager.chunkThreads.Length} of Chunk Threads");
            ImGui.TextWrapped($"Current resolution {SpatialGame.PixelColorer.width}, {SpatialGame.PixelColorer.height}");
            ImGui.TextWrapped($"Selected Particle {ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleIndexes[SpatialGame.SimInput.mouseSelection]].name}");
            ImGui.Checkbox("Simulation pause", ref ParticleSimulation.paused);
            
            if (ImGui.Checkbox("Vsync", ref vsync))
            {
                snWindow.VSync = vsync;
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Menus"))
                {
                    ImGui.MenuItem("Console", null, ref ShowConsoleViewerMenu);
                    ImGui.MenuItem("Networking", null, ref ShowNetworkViewerMenu);
                    ImGui.MenuItem("Simulation", null, ref ShowSimMenu);
                    ImGui.MenuItem("Simulation Settings", null, ref ShowSimSettingsMenu);
                    ImGui.MenuItem("Particle Creator", null, ref ShowParticleCreationMenu);
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (Debugging.showImmediateConsole)
            {
                Debugging.showImmediateConsole = false;
                ShowConsoleViewerMenu = true;
            }

            if (ShowConsoleViewerMenu)
            {
                ConsoleViewer();
            }

            if (ShowNetworkViewerMenu)
            {
                NetworkViewer();
            }

            if (ShowSimMenu)
            {
                SimViewer();
            }

            if (ShowSimSettingsMenu)
            {
                SimSettingsViewer();
            }

            if (ShowParticleCreationMenu)
            {
                ParticleCreationViewer();
            }
        }

        static Particle emptyParticle = new Particle();
        static void SimViewer()
        {
            ImGui.Begin("Simulation");
            ImGui.TextWrapped("Particle Info at Mouse Pos:");
            //Vector2 position = ((Mouse.localPosition / Window.size * new Vector2(PixelColorer.width, PixelColorer.height)) + (new Vector2(PixelColorer.width, PixelColorer.height) / 2));
            Vector2 position = Mouse.position * new Vector2(PixelColorer.width, PixelColorer.height) / Window.size;
            if(!PixelColorer.BoundCheck(position))
                return;
            
            ImGui.TextWrapped($"Pos {position:N1}");
            ImGui.TextWrapped($"Chunk Pos {(position / ParticleChunkManager.chunkSize):N1}");
            ImGui.TextWrapped($"Particle Index {PixelColorer.PosToIndex(position):N0}");
            ImGui.TextWrapped($"Chunk Index {ParticleChunkManager.SafeGetChunkIndexMap(position):N0}");
            ImGui.TextWrapped($"Chunk Particle Index {ParticleChunkManager.SafeGetIndexInChunksMap(position)}");
            ImGui.TextWrapped($"IdCheck {ParticleSimulation.SafeIdCheckGet(position):N0}");
            ref ParticleChunk testChunk = ref ParticleChunkManager.GetChunkReference(position);
            ImGui.TextWrapped($"Chunk Particle Count {testChunk.particleCount:N0}");
            ImGui.TextWrapped($"In chunk {testChunk.ChunkBounds(position)}");
            Vector2 floorPosition = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
            Vector4Byte color = PixelColorer.GetColorAtPos(floorPosition);
            ImGui.TextWrapped($"Color {color}");
            Vector2 textSize = ImGui.CalcTextSize($"Color {color}");
            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(cursorPos.X + textSize.X + 5, cursorPos.Y - 2), 
                new Vector2(cursorPos.X + textSize.X + 25, cursorPos.Y - textSize.Y - 3), 
                ImGui.ColorConvertFloat4ToU32((Vector4)color / 255f));
            Vector4Byte light = PixelColorer.particleLights[PixelColorer.PosToIndex(floorPosition)].color * PixelColorer.particleLights[PixelColorer.PosToIndex(floorPosition)].intensity;
            ImGui.TextWrapped($"Light {light}");
            textSize = ImGui.CalcTextSize($"Light {light}");
            cursorPos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(cursorPos.X + textSize.X + 5, cursorPos.Y - 2), 
                new Vector2(cursorPos.X + textSize.X + 25, cursorPos.Y - textSize.Y - 3), 
                ImGui.ColorConvertFloat4ToU32((Vector4)light / 255f));
            string particleName;
            if (ImGui.CollapsingHeader("Particle State"))
            {
                ChunkIndex idToCheck = ParticleSimulation.SafeChunkIdCheckGet(position);
                if (idToCheck.particleIndex != -1)
                {
                    ref ParticleChunk chunk = ref ParticleChunkManager.GetChunkReference(idToCheck.chunkIndex);
                    ImGui.TextWrapped($"{chunk.particles[idToCheck.particleIndex]}");
                }
                else
                {
                    ImGui.TextWrapped($"{emptyParticle}");
                }
            }
            if (ImGui.CollapsingHeader("Particle Properties"))
            {
                ChunkIndex idToCheck = ParticleSimulation.SafeChunkIdCheckGet(position);
                if (idToCheck.particleIndex != -1)
                {
                    ref ParticleChunk chunk = ref ParticleChunkManager.GetChunkReference(idToCheck.chunkIndex);
                    ImGui.TextWrapped($"{chunk.particles[idToCheck.particleIndex].GetParticleProperties()}");
                }
                else
                {
                    ImGui.TextWrapped($"{emptyParticle.GetParticleProperties()}");
                }
            }
            ImGui.End();
        }

        static void SimSettingsViewer()
        {
            ImGui.Begin("Simulation Settings");
            ImGui.Checkbox("Enable ParticleLighting", ref Settings.SimulationSettings.EnableParticleLighting);
            ImGui.Checkbox("Enable Gpu Light PreCalculation", ref Settings.SimulationSettings.EnableGpuCompLighting);
            ImGui.Checkbox("Enable Dark Lighting", ref Settings.SimulationSettings.EnableDarkLighting);
            ImGui.Checkbox("Enable Heat Simulation", ref Settings.SimulationSettings.EnableHeatSimulation);
            ImGui.Checkbox("Enable Multithreading", ref Settings.SimulationSettings.EnableMultiThreading);
            ImGui.TextWrapped("Particle Light Range");
            ImGui.PushItemWidth(ImGui.CalcTextSize("Particle Light Range").X);
            ImGui.InputInt("##particleLight", ref Settings.SimulationSettings.particleLightRange);
            ImGui.End();
        }

        static string pcName = "Particle";
        static int pcCurrentMoveSelect;
        static int pcCurrentBehaveSelect;
        static Vector4 pcColor = new Vector4(0f,0f,0f,1f);
        static int pcViscosity;
        static float pcXBounce;
        static float pcYBounce;
        static bool pcCanMove;
        static bool pcEnableHeatSim;
        static float pcTemperature;
        static float pcAutoIgnite;
        static float pcHeatTransferRate;
        static bool pcCanStateChange;
        static float[] pcStateChangeTemps = new float[2];
        static int[] pcStateChangeViscosity = new int[2];
        static Vector4[] pcStateChangeColors = new Vector4[3];
        static bool[] pcCanColorChange = new bool[3];
        static float pcRange;
        static float pcPower;
        static float pcFlashPoint;
        static float pcHeatOutput;
        static void ParticleCreationViewer()
        {
            ImGui.Begin("Particle Creator");

            //Properties
            
            ImGui.TextWrapped("Particle Name");
            ImGui.InputText("##name", ref pcName, 100);
            
            ImGui.TextWrapped("Particle Movement");
            string[] movementTypesNames = Enum.GetNames(typeof(ParticleMovementType));
            ImGui.Combo("##moveType", ref pcCurrentMoveSelect, movementTypesNames, movementTypesNames.Length);
            
            ImGui.TextWrapped("Particle Behavior");
            string[] behaviorTypesNames = Enum.GetNames(typeof(ParticleBehaviorType));
            ImGui.Combo("##behaveType", ref pcCurrentBehaveSelect, behaviorTypesNames, behaviorTypesNames.Length);

            ImGui.TextWrapped("Particle Color");
            ImGui.ColorEdit4("##Color", ref pcColor, ImGuiColorEditFlags.Float);

            if ((ParticleMovementType)pcCurrentMoveSelect == ParticleMovementType.liquid || (ParticleMovementType)pcCurrentMoveSelect == ParticleMovementType.gas)
            {
                ImGui.TextWrapped("Particle Viscosity");
                ImGui.DragInt("##Viscosity", ref pcViscosity);
            }
            
            ImGui.TextWrapped("Particle XBounce");
            ImGui.DragFloat("##XBounce", ref pcXBounce, 0.001f, 0f, 1f);
            
            ImGui.TextWrapped("Particle YBounce");
            ImGui.DragFloat("##YBounce", ref pcYBounce, 0.001f, 0f, 1f);
            
            ImGui.TextWrapped("Particle CanMove");
            ImGui.Checkbox("##CanMove", ref pcCanMove);
            
            //Heat properties
            ImGui.NewLine();
            ImGui.NewLine();
            
            ImGui.TextWrapped("Enable Heat Simulation");
            ImGui.Checkbox("##pcEnableHeatSim", ref pcEnableHeatSim);

            ImGui.TextWrapped("Temperature");
            ImGui.DragFloat("##pcTemperature", ref pcTemperature);

            ImGui.TextWrapped("Heat Transfer Rate");
            ImGui.DragFloat("##pcHeatTransferRate", ref pcHeatTransferRate);

            ImGui.TextWrapped("Can State Change");
            ImGui.Checkbox("##pcCanStateChange", ref pcCanStateChange);

            ImGui.TextWrapped("State Change Temperatures");
            for (int i = 0; i < pcStateChangeTemps.Length; i++)
            {
                ImGui.DragFloat($"##pcStateChangeTemp{i}", ref pcStateChangeTemps[i]);
            }

            ImGui.TextWrapped("State Change Viscosity");
            for (int i = 0; i < pcStateChangeViscosity.Length; i++)
            {
                ImGui.DragInt($"##pcStateChangeViscosity{i}", ref pcStateChangeViscosity[i]);
            }

            ImGui.TextWrapped("State Change Colors");
            for (int i = 0; i < pcStateChangeColors.Length; i++)
            {
                ImGui.ColorEdit4($"##pcStateChangeColor{i}", ref pcStateChangeColors[i], ImGuiColorEditFlags.Float);
            }

            ImGui.TextWrapped("Can Color Change");
            for (int i = 0; i < pcCanColorChange.Length; i++)
            {
                ImGui.Checkbox($"##pcCanColorChange{i}", ref pcCanColorChange[i]);
            }
            
            //Explosive properties
            
            ImGui.NewLine();
            ImGui.NewLine();
            
            ImGui.TextWrapped("Range");
            ImGui.DragFloat("##pcRange", ref pcRange);

            ImGui.TextWrapped("Power");
            ImGui.DragFloat("##pcPower", ref pcPower);

            ImGui.TextWrapped("Flash Point");
            ImGui.DragFloat("##pcFlashPoint", ref pcFlashPoint);

            ImGui.TextWrapped("Heat Output");
            ImGui.DragFloat("##pcHeatOutput", ref pcHeatOutput);

            if (ImGui.Button("Add Particle"))
            {
                ParticleProperties properties = new ParticleProperties();
                properties.name = pcName;
                properties.moveType = (ParticleMovementType)pcCurrentMoveSelect;
                properties.behaveType = (byte)pcCurrentBehaveSelect;
                properties.color = (Vector4Byte)(pcColor * 255f);
                properties.viscosity = (ushort)pcViscosity;
                properties.xBounce = pcXBounce;
                properties.yBounce = pcYBounce;
                properties.canMove = pcCanMove;
                ParticleHeatingProperties heatingProperties = new ParticleHeatingProperties();
                heatingProperties.enableHeatSim = pcEnableHeatSim;
                heatingProperties.temperature = pcTemperature;
                heatingProperties.heatTransferRate = pcHeatTransferRate;
                heatingProperties.canStateChange = pcCanStateChange;
                heatingProperties.stateChangeTemps = pcStateChangeTemps;
                heatingProperties.stateChangeViscosity = new ushort[2];
                for (int i = 0; i < heatingProperties.stateChangeViscosity.Length; i++)
                {
                    heatingProperties.stateChangeViscosity[i] = (ushort)pcStateChangeViscosity[i];
                }
                heatingProperties.stateChangeColors = new Vector4Byte[3];
                for (int i = 0; i < heatingProperties.stateChangeColors.Length; i++)
                {
                    heatingProperties.stateChangeColors[i] = (Vector4Byte)(pcStateChangeColors[i] * 255f);
                }

                heatingProperties.canColorChange = pcCanColorChange;
                properties.heatingProperties = heatingProperties;
                ParticleExplosiveProperties explosiveProperties = new ParticleExplosiveProperties();
                explosiveProperties.range = pcRange;
                explosiveProperties.power = pcPower;
                explosiveProperties.flashPoint = pcFlashPoint;
                explosiveProperties.heatOutput = pcHeatOutput;
                properties.explosiveProperties = explosiveProperties;
                
                ParticleResourceHandler.loadedParticles.Add(properties);

                short index = (short)ParticleResourceHandler.particleIndexes.Length;
                short[] particleIndexes = new short[index + 1];
                for (int i = 0; i < index; i++)
                {
                    particleIndexes[i] = ParticleResourceHandler.particleIndexes[i];
                }
                
                particleIndexes[index] = index;
                ParticleResourceHandler.particleIndexes = particleIndexes;
                Console.WriteLine(properties);
                
                if(ParticleResourceHandler.particleNameIndexes.TryAdd(properties.name, index))
                {
                    Debugging.LogConsole("Added particle " + properties.name);
                }
            }
            

            ImGui.End();
        }

        static void ConsoleViewer()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Console");
            ImGui.BeginChild("Output", new Vector2(0, ImGui.GetWindowSize().Y * 0.9f), true, ImGuiWindowFlags.NoResize);
            for (int i = 0; i < Debugging.consoleText.Count; i++)
            {
                if (i == 0)
                {
                    if (Debugging.consoleText[i].Item2)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0, 0, 1.0f));
                        ImGui.TextWrapped("> [" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextWrapped("> [" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                    }
                }
                else
                {
                    if (Debugging.consoleText[i].Item2)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0, 0, 1.0f));
                        ImGui.TextWrapped("[" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextWrapped("[" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                    }
                }
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
        
        static void SetImGuiStyle()
        {
            RangeAccessor<Vector4> colors = ImGui.GetStyle().Colors;
            colors[(int)ImGuiCol.Text]                   = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled]           = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg]               = new Vector4(0.06f, 0.06f, 0.06f, 0.94f);
            colors[(int)ImGuiCol.ChildBg]                = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg]                = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
            colors[(int)ImGuiCol.Border]                 = new Vector4(0.43f, 0.19f, 0.17f, 0.50f);
            colors[(int)ImGuiCol.BorderShadow]           = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.FrameBg]                = new Vector4(0.73f, 0.25f, 0.28f, 0.40f);
            colors[(int)ImGuiCol.FrameBgHovered]         = new Vector4(0.95f, 0.38f, 0.38f, 0.55f);
            colors[(int)ImGuiCol.FrameBgActive]          = new Vector4(0.56f, 0.41f, 0.38f, 0.58f);
            colors[(int)ImGuiCol.TitleBg]                = new Vector4(0.31f, 0.09f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive]          = new Vector4(0.51f, 0.29f, 0.11f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed]       = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
            colors[(int)ImGuiCol.MenuBarBg]              = new Vector4(0.31f, 0.09f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg]            = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
            colors[(int)ImGuiCol.ScrollbarGrab]          = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered]   = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive]    = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
            colors[(int)ImGuiCol.CheckMark]              = new Vector4(1.00f, 0.49f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab]             = new Vector4(1.00f, 0.24f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive]       = new Vector4(1.00f, 0.49f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.Button]                 = new Vector4(1.00f, 0.42f, 0.00f, 0.40f);
            colors[(int)ImGuiCol.ButtonHovered]          = new Vector4(0.81f, 0.43f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive]           = new Vector4(0.87f, 0.66f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.Header]                 = new Vector4(1.00f, 0.49f, 0.00f, 0.31f);
            colors[(int)ImGuiCol.HeaderHovered]          = new Vector4(1.00f, 0.59f, 0.00f, 0.80f);
            colors[(int)ImGuiCol.HeaderActive]           = new Vector4(1.00f, 0.59f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.Separator]              = new Vector4(0.60f, 0.27f, 0.00f, 0.50f);
            colors[(int)ImGuiCol.SeparatorHovered]       = new Vector4(0.60f, 0.39f, 0.00f, 0.78f);
            colors[(int)ImGuiCol.SeparatorActive]        = new Vector4(0.60f, 0.39f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip]             = new Vector4(0.47f, 0.14f, 0.11f, 0.20f);
            colors[(int)ImGuiCol.ResizeGripHovered]      = new Vector4(0.57f, 0.20f, 0.19f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive]       = new Vector4(0.57f, 0.20f, 0.20f, 0.95f);
            colors[(int)ImGuiCol.Tab]                    = new Vector4(0.51f, 0.13f, 0.11f, 0.86f);
            colors[(int)ImGuiCol.TabHovered]             = new Vector4(0.55f, 0.13f, 0.11f, 0.80f);
            colors[(int)ImGuiCol.TabActive]              = new Vector4(0.55f, 0.13f, 0.11f, 1.00f);
            colors[(int)ImGuiCol.TabUnfocused]           = new Vector4(0.45f, 0.10f, 0.07f, 0.97f);
            colors[(int)ImGuiCol.TabUnfocusedActive]     = new Vector4(0.45f, 0.11f, 0.09f, 1.00f);
            colors[(int)ImGuiCol.PlotLines]              = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered]       = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram]          = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered]   = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TableHeaderBg]          = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.TableBorderStrong]      = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.TableBorderLight]       = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.TableRowBg]             = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.TableRowBgAlt]          = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int)ImGuiCol.TextSelectedBg]         = new Vector4(0.51f, 0.15f, 0.11f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget]         = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int)ImGuiCol.NavHighlight]           = new Vector4(0.49f, 0.14f, 0.11f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight]  = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg]      = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg]       = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);

            ImGui.GetStyle().FrameRounding = 1;
        }

        
        
    }
}
