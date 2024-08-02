using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpatialGame
{
    public static class ElementSimulation
    {
        public static List<Element> elements = new List<Element>();
        /// <summary>
        /// 0 is no pixel at position,
        /// 1 is movable solid at position,
        /// 2 is movable liquid at position,
        /// 3 is movable gas at position,
        /// 100 is a unmovable at position
        /// </summary>
        public static int[] positionCheck;
        /// <summary>
        /// Holds an element id with position tied at index,
        /// for getting a element from index by id
        /// </summary>
        public static int[] idCheck;

        static bool mousePressed = false;
        static bool type = false;

        public static void InitPixelSim()
        {
            positionCheck = new int[PixelColorer.width * PixelColorer.height];
            idCheck = new int[PixelColorer.width * PixelColorer.height];
            for (int i = 0; i < 1920; i++)
            {
                int id = elements.Count;
                elements.Add(new WallPE());
                elements[id].id = id;
                elements[id].position = new Vector2(i, 600);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 100;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
            }
            for (int i = 0; i < 10000; i++)
            {
                int id = elements.Count;
                elements.Add(new WaterPE());
                elements[id].id = id;
                elements[id].position = new Vector2(850, 10);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 2;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
            }
            for (int i = 0; i < 1000; i++)
            {
                int id = elements.Count;
                elements.Add(new SandPE());
                elements[id].id = id;
                elements[id].position = new Vector2(850, 400);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 2;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
            }
            for (int i = 0; i < 1000; i++)
            {
                int id = elements.Count;
                elements.Add(new SandPE());
                elements[id].id = id;
                elements[id].position = new Vector2(450, 10);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 2;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
            }
            for (int i = 0; i < 1000; i++)
            {
                int id = elements.Count;
                elements.Add(new SandPE());
                elements[id].id = id;
                elements[id].position = new Vector2(1250, 10);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 2;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
            }

            DebugSimulation.Init();

            Input.input.Mice[0].MouseDown += MouseDown;
            Input.input.Mice[0].MouseUp += MouseUp;
        }

        public static void RunPixelSim()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                //reset the color to background from where they were and will be overwritten if they dont move
                if (!elements[i].BoundsCheck(0, 0, PixelColorer.width, PixelColorer.height))
                    PixelColorer.pixelColors[PixelColorer.PosToIndex(elements[i].position)] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
                elements[i].Update(ref elements, ref positionCheck, ref idCheck);
                if (!elements[i].BoundsCheck(0, 0, PixelColorer.width, PixelColorer.height))
                    PixelColorer.pixelColors[PixelColorer.PosToIndex(elements[i].position)] = new Vector4(elements[i].color / new Vector3(255f, 255f, 255f), 1.0f);
            }

            DebugSimulation.Update();
            CreateSphere();
        }

        public static void MouseDown(IMouse mouse, MouseButton button)
        {
            mousePressed = true;
            if (button == MouseButton.Left)
                type = true;
            if (button == MouseButton.Right)
                type = false;
        }

        public static void MouseUp(IMouse mouse, MouseButton button)
        {
            mousePressed = false;
        }

        public static void CreateSphere()
        {
            if (mousePressed == false)
                return;

            Vector2 position = new Vector2(PixelColorer.width, PixelColorer.height) * (Input.input.Mice[0].Position / (Vector2)Globals.window.Size);

            for (int x = (int)position.X - 10; x < position.X + 10; x++)
            {
                for (int y = (int)position.Y - 10; y < position.Y + 10; y++)
                {
                    float check = (float)Math.Sqrt(((x - position.X) * (x - position.X)) + ((y - position.Y) * (y - position.Y)));
                    if (check < 20)
                    {
                        int id = elements.Count;
                        if(type)
                            elements.Add(new WaterPE());
                        else
                            elements.Add(new SandPE());
                        elements[id].id = id;
                        elements[id].position = new Vector2(x, y);
                        if(type)
                            positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 2;
                        else
                            positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 1;
                        idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
                    }
                }
            }
            mousePressed = false;
        }
    }
}
