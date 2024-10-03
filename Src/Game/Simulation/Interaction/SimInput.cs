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
        static bool mousePressed;
        static int mouseSpawnRadius;
        static int mouseButtonPress;

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
            ParticleResourceHandler.particleNameIndexes.TryGetValue("Sand", out int index);
            selectionMode = false;
            mouseSelection = index;

            MouseInteraction.Init();

            Debugging.LogConsole("Initalized Simulation Input");
        }

        public static void FixedUpdate()
        {
            string name = ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleIndexes[mouseSelection]].name;
            MouseInteraction.SpawnParticlesCircleSpawner(Mouse.position, mouseSpawnRadius, mousePressed, mouseButtonPress, name, selectionMode, mouseSelection);
        }
        
        public static void Update()
        {
            string name = ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleIndexes[mouseSelection]].name;
            MouseInteraction.DrawMouseElementSelect(Mouse.position, mouseSpawnRadius, mousePressed, name, selectionMode, mouseSelection);
            MouseInteraction.DrawMouseElementsCircle(Mouse.position, mouseSpawnRadius, mousePressed);
            
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
                if (Globals.window.Size.Length / new Vector2(PixelColorer.width, PixelColorer.height).Length() * mouseSpawnRadius > Globals.window.Size.Y / 2)
                    mouseSpawnRadius -= 1;
            }
        }
    }
}
