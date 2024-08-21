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
        static float mouseScrollBefore;
        static int mouseSpawnRadius;
        static int mouseButtonPress;

        public static void Init()
        {
            Input.input.Mice[0].MouseUp += MouseUp;
            Input.input.Mice[0].MouseDown += MouseDown;
            Input.input.Mice[0].Scroll += MouseScroll;

            mouseSpawnRadius = 10;
        }

        public static void Update()
        {
            MouseInteraction.DrawMouseCircleSpawner(Input.input.Mice[0].Position, mouseSpawnRadius, mousePressed, mouseButtonPress);
        }
        public static void MouseDown(IMouse mouse, MouseButton button)
        {
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
            if(Globals.window.Size.Length / new Vector2(PixelColorer.width, PixelColorer.height).Length() * mouseSpawnRadius > Globals.window.Size.X)
                mouseSpawnRadius -= 1;
        }
    }
}
