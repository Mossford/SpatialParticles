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
        /// the type of the element at its position
        /// 0 is no pixel at position,
        /// 1 is movable solid at position,
        /// 2 is movable liquid at position,
        /// 3 is movable gas at position,
        /// 100 is a unmovable at position
        /// </summary>
        public static byte[] positionCheck;
        /// <summary>
        /// the ids of the elements at their position
        /// </summary>
        public static int[] idCheck;

        static bool mousePressed = false;
        static bool type = false;

        public static void InitPixelSim()
        {
            positionCheck = new byte[PixelColorer.width * PixelColorer.height];
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

            //DebugSimulation.Init();

            Input.input.Mice[0].MouseDown += MouseDown;
            Input.input.Mice[0].MouseUp += MouseUp;
        }

        public static void RunPixelSim()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                //reset the color to background from where they were and will be overwritten if they dont move
                if (!elements[i].BoundsCheck(0, 0, PixelColorer.width, PixelColorer.height))
                    PixelColorer.SetColorAtPos(elements[i].position, 51, 51, 51);
                elements[i].Update(ref elements, ref positionCheck, ref idCheck);
                if (!elements[i].BoundsCheck(0, 0, PixelColorer.width, PixelColorer.height))
                    PixelColorer.SetColorAtPos(elements[i].position, (byte)elements[i].color.X, (byte)elements[i].color.Y, (byte)elements[i].color.Z);
            }

            CreateSphere();


            //DebugSimulation.Update();
        }

        public static void DeleteElement(Vector2 position)
        {
            //get the id at the position
            int id = idCheck[PixelColorer.PosToIndex(position)];
            //check if a element exsists at the position
            if (id == 0)
                return;

            //set its position to nothing
            positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToByte();
            //set its id at its position to nothing
            idCheck[PixelColorer.PosToIndex(position)] = 0;
            //delete it from the array
            elements.RemoveAt(id);
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
                        DeleteElement(new Vector2(x, y));
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
