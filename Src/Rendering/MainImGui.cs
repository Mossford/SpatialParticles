using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using SpatialGame;

namespace SpatialEngine.Rendering
{
    public static class MainImGui
    {
        static bool ShowConsoleViewerMenu;
        static ImFontPtr font;
        static float fpsCount;

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
            ImGui.Text(String.Format("DrawCall per frame: ({0:N1})", MathF.Round(drawCallCount)));

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
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ShowConsoleViewerMenu)
            {
                ConsoleViewer();
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
    }
}
