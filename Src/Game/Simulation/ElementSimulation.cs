using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        /// <summary>
        /// Queue of elements that will be deleted
        /// </summary>
        public static List<int> idsToDelete;
        /// <summary>
        /// Holds the amount that should be subtracted from the idcheck array
        /// turns 1/2n^2 to n
        /// </summary>
        public static Dictionary<int, int> indexCountDelete;
        /// <summary>
        /// Random that elements will use
        /// </summary>
        public static Random random;

        public static void InitPixelSim()
        {
            positionCheck = new byte[PixelColorer.width * PixelColorer.height];
            idCheck = new int[PixelColorer.width * PixelColorer.height];
            idsToDelete = new List<int>();
            indexCountDelete = new Dictionary<int, int>();
            random = new Random();

            for (int i = 0; i < idCheck.Length; i++)
            {
                idCheck[i] = -1;
            }

            /*for (int x = 0; x < PixelColorer.width; x++)
            {
                int id = elements.Count;
                elements.Add(new WallPE());
                elements[id].id = id;
                elements[id].position = new Vector2(x, 0);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 100;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;

                id = elements.Count;
                elements.Add(new WallPE());
                elements[id].id = id;
                elements[id].position = new Vector2(x, PixelColorer.height - 1);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 100;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
            }*/

            for (int y = 0; y < PixelColorer.height; y++)
            {
                int id = elements.Count;
                elements.Add(new WallPE());
                elements[id].id = id;
                elements[id].position = new Vector2(0, y);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 100;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;

                id = elements.Count;
                elements.Add(new WallPE());
                elements[id].id = id;
                elements[id].position = new Vector2(PixelColorer.width - 1, y);
                positionCheck[PixelColorer.PosToIndex(elements[id].position)] = 100;
                idCheck[PixelColorer.PosToIndex(elements[id].position)] = elements[id].id;
            }

            //DebugSimulation.Init();
        }

        public static void RunPixelSim()
        {
            DeleteElementsOnQueue();

            for (int i = 0; i < elements.Count; i++)
            {
                //check if pass bounds check and delete if not
                if (elements[i].BoundsCheck(elements[i].position))
                {
                    PixelColorer.SetColorAtPos(elements[i].position, 102, 178, 204);
                    elements[i].Update();
                    //check if pass bounds check and delete if not
                    if (elements[i].BoundsCheck(elements[i].position))
                        PixelColorer.SetColorAtPos(elements[i].position, (byte)elements[i].color.X, (byte)elements[i].color.Y, (byte)elements[i].color.Z);
                }
            }

            //DebugSimulation.Update();
        }

        static void DeleteElementsOnQueue()
        {
            if (idsToDelete.Count == 0)
                return;

            for (int i = 0; i < idsToDelete.Count; i++)
            {
                int id = idsToDelete[i];
                int adder = -0;
                int[] keys = indexCountDelete.Keys.ToArray();
                for (int g = 0; g < indexCountDelete.Count; g++)
                {
                    if (id > keys[g])
                        adder++;
                    else
                        break;
                }
                id -= adder;
                if (id >= 0 && id < elements.Count && elements[id].toBeDeleted)
                    elements[id].Delete();
            }

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].id = i;
                SafeIdCheckSet(elements[i].id, elements[i].position);
            }

            idsToDelete.Clear();
            indexCountDelete.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafePositionCheckSet(byte type, Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if(index == -1)
                return;
            positionCheck[index] = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeIdCheckSet(int id, Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return;
            idCheck[index] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SafePositionCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SafeIdCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafePositionCheckSetNoBc(byte type, Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return;
            positionCheck[index] = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeIdCheckSetNoBc(int id, Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return;
            idCheck[index] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SafePositionCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SafeIdCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }
    }
}
