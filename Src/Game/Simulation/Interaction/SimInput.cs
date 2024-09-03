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

        static string selectedElement;

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
            selectedElement = "Sand";

            MouseInteraction.Init();
        }

        public static void Update()
        {
            if(Input.IsKeyDown(Key.Number1))
            {
                selectedElement = "Sand";
            }
            if (Input.IsKeyDown(Key.Number2))
            {
                selectedElement = "Stone";
            }
            if (Input.IsKeyDown(Key.Number3))
            {
                selectedElement = "Water";
            }
            if (Input.IsKeyDown(Key.Number4))
            {
                selectedElement = "Smoke";
            }
            if (Input.IsKeyDown(Key.Number5))
            {
                selectedElement = "Wall";
            }
            if (Input.IsKeyDown(Key.Number6))
            {
                selectedElement = "Fire";
            }

            MouseInteraction.DrawMouseCircleSpawner(Input.input.Mice[0].Position, mouseSpawnRadius, mousePressed, mouseButtonPress, selectedElement);
            MouseInteraction.DrawMouseElementSelect(Input.input.Mice[0].Position, mouseSpawnRadius, mousePressed, selectedElement);

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
            mouseSpawnRadius += (int)wheel.Y;
            if(mouseSpawnRadius < 1)
                mouseSpawnRadius = 1;
            if(Globals.window.Size.Length / new Vector2(PixelColorer.width, PixelColorer.height).Length() * mouseSpawnRadius > Globals.window.Size.Y / 2)
                mouseSpawnRadius -= 1;
        }
    }
}
