using Silk.NET.Input;
using Silk.NET.Vulkan;
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
        public static Element[] elements;
        public static Queue<int> freeElementSpots;
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
        /// Random that elements will use
        /// </summary>
        public static Random random;

        public static void InitPixelSim()
        {

            elements = new Element[PixelColorer.width * PixelColorer.height];
            freeElementSpots = new Queue<int>();
            positionCheck = new byte[PixelColorer.width * PixelColorer.height];
            idCheck = new int[PixelColorer.width * PixelColorer.height];
            idsToDelete = new List<int>();
            random = new Random();

            for (int i = 0; i < elements.Length; i++)
            {
                freeElementSpots.Enqueue(i);
            }

            for (int i = 0; i < idCheck.Length; i++)
            {
                idCheck[i] = -1;
            }

            for (int x = 0; x < PixelColorer.width; x++)
            {
                AddElement(new Vector2(x, 0), new WallPE());
                AddElement(new Vector2(x, PixelColorer.height - 1), new WallPE());
            }

            for (int y = 0; y < PixelColorer.height; y++)
            {
                AddElement(new Vector2(0, y), new WallPE());
                AddElement(new Vector2(PixelColorer.width - 1, y), new WallPE());
            }

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] is null || !elements[i].BoundsCheck(elements[i].position))
                    continue;

                elements[i].CheckDoubleOnPosition();
            }

            DeleteElementsOnQueue();

            //DebugSimulation.Init();
        }

        public static void RunPixelSim()
        {
            //DebugSimulation.Update();
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] is null || !elements[i].BoundsCheck(elements[i].position))
                    continue;

                PixelColorer.SetColorAtPos(elements[i].position, 102, 178, 204);
                elements[i].Update();
                if (elements[i].BoundsCheck(elements[i].position))
                    PixelColorer.SetColorAtPos(elements[i].position, (byte)elements[i].color.X, (byte)elements[i].color.Y, (byte)elements[i].color.Z);
            }

            /*for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] is null || !elements[i].BoundsCheck(elements[i].position))
                    continue;

                elements[i].CheckDoubleOnPosition();
            }*/

            DeleteElementsOnQueue();


        }

        static void DeleteElementsOnQueue()
        {
            if (idsToDelete.Count == 0)
                return;

            for (int i = 0; i < idsToDelete.Count; i++)
            {
                int id = idsToDelete[i];
                if (elements[id] is null)
                    continue;
                if (id >= 0 && id < elements.Length && elements[id].toBeDeleted)
                {
                    elements[id].Delete();
                }
            }

            idsToDelete.Clear();
        }

        public static void AddElement(Vector2 pos, Element element)
        {
            //we have reached where we dont have any more spots so we skip
            if (freeElementSpots.Count == 0)
                return;
            int id = freeElementSpots.Dequeue();
            elements[id] = element;
            elements[id].position = pos;
            elements[id].id = id;
            elements[id].timeSpawned = Globals.GetTime();
            SafePositionCheckSet(element.GetElementType().ToByte(), pos);
            SafeIdCheckSet(id, pos);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSet(byte type, Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if(index == -1)
                return false;
            positionCheck[index] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int id, Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return false;
            idCheck[index] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSetNoBc(byte type, Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return false;
            positionCheck[index] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSetNoBc(int id, Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return false;
            idCheck[index] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }
    }
}
