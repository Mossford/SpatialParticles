using ImGuiNET;
using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SpatialEngine.Rendering;

namespace SpatialGame
{
    public static class SimInput
    {
        public static bool mousePressed;
        public static int mouseSpawnRadius;
        public static int mouseButtonPress;

        static bool firstInit;
        static bool initButton;

        /// <summary>
        /// true is property changing
        /// false is particleSpawning
        /// </summary>
        public static bool selectionMode;
        static int maxPropertySelectionChange = 2;
        public static int mouseSelection;

        public static void Init()
        {
            if(!firstInit)
            {
                Mouse.mouse.MouseUp += MouseUp;
                Mouse.mouse.MouseDown += MouseDown;
                Mouse.mouse.Scroll += MouseScroll;
                firstInit = true;
            }

            mousePressed = false;
            mouseButtonPress = 0;
            mouseSpawnRadius = 10;
            ParticleResourceHandler.particleNameIndexes.TryGetValue("Sand", out short index);
            selectionMode = false;
            mouseSelection = index;

            MouseInteraction.Init();

            Debugging.LogConsole("Initialized Simulation Input");
        }

        public static void CleanUp()
        {
            MouseInteraction.CleanUp();
        }

        public static void FixedUpdate()
        {
            
        }
        
        public static void Update()
        {
            string name = ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleIndexes[mouseSelection]].name;
            MouseInteraction.DrawMouseElementSelect(Mouse.localPosition, mouseSpawnRadius, mousePressed, name, selectionMode, mouseSelection, 0);
            MouseInteraction.DrawMouseElementsCircle(Mouse.localPosition, mouseSpawnRadius, mousePressed, 0);
            MouseInteraction.SpawnParticlesCircleSpawner(Mouse.localPosition, Mouse.lastLocalPosition, mouseSpawnRadius, mousePressed, mouseButtonPress, name, selectionMode, mouseSelection);
            
            if (Input.IsKeyDown(Key.T) && !initButton)
            {
                GameManager.changeResolution = true;
                PixelColorer.resSwitcherDir = -1;
                GameManager.ReInitGame();
                initButton = true;
            }
            if (Input.IsKeyDown(Key.G) && !initButton)
            {
                GameManager.changeResolution = true;
                PixelColorer.resSwitcherDir = 1;
                GameManager.ReInitGame();
                initButton = true;
            }
            if (Input.IsKeyDown(Key.R) && !initButton)
            {
                GameManager.changeResolution = false;
                GameManager.ReInitGame();
                initButton = true;
            }

            if (Input.IsKeyUp(Key.T) && Input.IsKeyUp(Key.R) && Input.IsKeyUp(Key.G) && initButton)
            {
                initButton = false;
                GameManager.changeResolution = false;
            }

            if (Input.IsKeyDown(Key.ControlLeft))
            {
                mouseSelection %= maxPropertySelectionChange;
                if (mouseSelection < 0)
                    mouseSelection = maxPropertySelectionChange - 1;
                selectionMode = true;
            }
            if (Input.IsKeyDown(Key.ShiftLeft))
            {
                mouseSelection %= ParticleResourceHandler.particleIndexes.Length;
                if (mouseSelection < 0)
                    mouseSelection = ParticleResourceHandler.particleIndexes.Length - 1;
                selectionMode = false;
            }
            
        }
        static void MouseDown(IMouse mouse, MouseButton button)
        {
            if (Mouse.uiWantMouse)
                return;
            
            if (Globals.showImguiDebug && ImGui.GetIO().WantCaptureMouse)
                return;
            mouseButtonPress = (int)button;
            mousePressed = true;
        }

        static void MouseUp(IMouse mouse, MouseButton button)
        {
            mousePressed = false;
        }

        static void MouseScroll(IMouse mouse, ScrollWheel wheel)
        {
            if (Globals.showImguiDebug && ImGui.GetIO().WantCaptureMouse)
                return;
            
            if (Input.IsKeyDown(Key.ShiftLeft) && Input.IsKeyUp(Key.ControlLeft))
            {
                mouseSelection = (mouseSelection + (int)wheel.Y) % ParticleResourceHandler.particleIndexes.Length;
                if(mouseSelection < 0)
                    mouseSelection = ParticleResourceHandler.particleIndexes.Length - 1;
            }
            else if(Input.IsKeyUp(Key.ShiftLeft) && Input.IsKeyDown(Key.ControlLeft))
            {
                mouseSelection = (mouseSelection + (int)wheel.Y) % maxPropertySelectionChange;
                if (mouseSelection < 0)
                    mouseSelection = maxPropertySelectionChange - 1;
            }
            else
            {
                mouseSpawnRadius += (int)wheel.Y;
                if (mouseSpawnRadius < 1)
                    mouseSpawnRadius = 1;
                if (Window.size.Length() / new Vector2(PixelColorer.width, PixelColorer.height).Length() * mouseSpawnRadius > Window.size.Y / 2)
                    mouseSpawnRadius -= 1;
            }
        }
    }
}
