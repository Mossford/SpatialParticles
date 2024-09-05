using ImGuiNET;
using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class SimInput
    {
        static bool mousePressed;
        static int mouseSpawnRadius;
        static int mouseButtonPress;

        static bool firstInit;
        static bool initButton;

        static int selectedElement;

        public static void Init()
        {
            if(!firstInit)
            {
                Input.input.Mice[0].MouseUp += MouseUp;
                Input.input.Mice[0].MouseDown += MouseDown;
                Input.input.Mice[0].Scroll += MouseScroll;
                firstInit = true;
            }

            mousePressed = false;
            mouseButtonPress = 0;
            mouseSpawnRadius = 10;
            ParticleResourceHandler.particleNameIndexes.TryGetValue("Sand", out int index);
            selectedElement = index;

            MouseInteraction.Init();
        }

        public static void Update()
        {
            string name = ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleIndexes[selectedElement]].name;
            MouseInteraction.DrawMouseCircleSpawner(Input.input.Mice[0].Position, mouseSpawnRadius, mousePressed, mouseButtonPress, name);
            MouseInteraction.DrawMouseElementSelect(Input.input.Mice[0].Position, mouseSpawnRadius, mousePressed, name);

            if (Input.IsKeyDown(Key.T) && !initButton)
            {
                GameManager.changeResolution = true;
                GameManager.ReInitGame();
                initButton = true;
            }
            else if (Input.IsKeyDown(Key.R) && !initButton)
            {
                GameManager.changeResolution = false;
                GameManager.ReInitGame();
                initButton = true;
            }

            if (Input.IsKeyUp(Key.T) && Input.IsKeyUp(Key.R) && initButton)
            {
                initButton = false;
                GameManager.changeResolution = false;
            }
        }
        public static void MouseDown(IMouse mouse, MouseButton button)
        {
            if (Globals.showImguiDebug && ImGui.GetIO().WantCaptureMouse)
                return;
            mouseButtonPress = (int)button;
            mousePressed = true;
        }

        public static void MouseUp(IMouse mouse, MouseButton button)
        {
            mousePressed = false;
        }

        public static void MouseScroll(IMouse mouse, ScrollWheel wheel)
        {
            if (Input.IsKeyDown(Key.ShiftLeft))
            {
                selectedElement = (selectedElement + (int)wheel.Y) % ParticleResourceHandler.particleIndexes.Length;
                if(selectedElement < 0)
                    selectedElement = ParticleResourceHandler.particleIndexes.Length - 1;
            }
            else
            {
                mouseSpawnRadius += (int)wheel.Y;
                if (mouseSpawnRadius < 1)
                    mouseSpawnRadius = 1;
                if (Globals.window.Size.Length / new Vector2(PixelColorer.width, PixelColorer.height).Length() * mouseSpawnRadius > Globals.window.Size.Y / 2)
                    mouseSpawnRadius -= 1;
            }
        }
    }
}
