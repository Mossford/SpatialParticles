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
using SpatialEngine.Rendering.ImGUI;
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using SpatialGame;
using Spatialparticles.Rendering.ImGUI;

namespace SpatialEngine.Rendering
{
    public static class MainImGui
    {
        static bool ShowConsoleViewerMenu, 
            ShowNetworkViewerMenu, 
            ShowSimMenu, 
            ShowSimSettingsMenu, 
            ShowParticleCreationMenu,
            ShowUiViewer;
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
            ImGui.Checkbox("Simulation pause", ref GameManager.paused);
            
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
                    ImGui.MenuItem("Ui Viewer", null, ref ShowUiViewer);
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
                ConsoleViewer.Draw();
            }

            if (ShowNetworkViewerMenu)
            {
                NetworkViewer.Draw();
            }

            if (ShowSimMenu)
            {
                SimulationViewer.Draw();
            }

            if (ShowSimSettingsMenu)
            {
                SimulationSettingsViewer.Draw();
            }

            if (ShowParticleCreationMenu)
            {
                ParticleCreationViewer.Draw();
            }

            if (ShowUiViewer)
            {
                UiViewer.Draw();
            }
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
