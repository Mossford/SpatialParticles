using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class SimInput
    {
        static bool mousePressed;

        public static void Init()
        {
            Input.input.Mice[0].MouseUp += MouseUp;
            Input.input.Mice[0].MouseDown += MouseDown;
        }

        public static void Update()
        {
            MouseInteraction.DrawMouseCircleSpawner(Input.input.Mice[0].Position, 10, mousePressed);
        }
        public static void MouseDown(IMouse mouse, MouseButton button)
        {
            mousePressed = true;
        }

        public static void MouseUp(IMouse mouse, MouseButton button)
        {
            mousePressed = false;
        }
    }
}
