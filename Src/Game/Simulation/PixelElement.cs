using Silk.NET.GLFW;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public enum ElementType : byte
    {
        empty = 0,
        solid = 1, //moveable
        liquid = 2,
        gas = 3,
        wall = 100,
    }

    public enum ElementTypeSpecific : ushort
    {
        sand,
        stone,
        water,
        carbonDioxide,
        wall
    }

    public static class ElementTypeConversion
    {
        //the this keyword allows it cool as hell
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ToByte(this ElementType elementType)
        {
            return (byte)elementType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ToUshort(this ElementTypeSpecific elementType)
        {
            return (ushort)elementType;
        }
    }
    

    public abstract class Element : IDisposable
    {
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public Vector3 color { get; set; }
        public bool canMove { get; set; }
        public int id { get; set; }
        public Vector2 oldpos { get; set; }
        public bool toBeDeleted {get; set;}
        public int deleteIndex {get; set;}
        public float timeSpawned { get; set;}

        /// <summary>
        /// Position check must be updated when pixel pos changed
        /// </summary>
        public abstract void Update();
        public abstract ElementType GetElementType();
        public abstract ElementTypeSpecific GetElementTypeSpecific();
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool BoundsCheck(Vector2 position)
        {
            if (position.X < 0 || position.X >= PixelColorer.width || position.Y < 0 || position.Y >= PixelColorer.height)
                return false;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SwapElement(Vector2 newPos, ElementType type)
        {
            //get the id of the element below this current element
            int swapid = ElementSimulation.SafeIdCheckGet(newPos);

            if (swapid == -1)
                return;

            //check position of swap element beacuse it has a possibility to be out of bounds somehow
            if (!BoundsCheck(ElementSimulation.elements[swapid].position))
                return;

            ElementSimulation.SafePositionCheckSet(type.ToByte(), position);

            //------safe to access the arrays directly------

            int index = PixelColorer.PosToIndexUnsafe(position);
            ElementSimulation.positionCheck[index] = type.ToByte();
            //set the element below the current element to the same position
            ElementSimulation.elements[swapid].position = position;
            //set the id at the current position to the id from the element below
            ElementSimulation.idCheck[index] = swapid;

            index = PixelColorer.PosToIndexUnsafe(newPos);
            //set the type to the new position to our current element
            ElementSimulation.positionCheck[index] = GetElementType().ToByte();
            //set the id of our element to the new position
            ElementSimulation.idCheck[index] = id;
            //set the new position of the current element
            ElementSimulation.elements[id].position = newPos;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void MoveElement()
        {
            //find new position
            Vector2 posMove = position + velocity;
            posMove.X = MathF.Floor(posMove.X);
            posMove.Y = MathF.Floor(posMove.Y);

            Vector2 dir = posMove - position;
            int step;

            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                step = (int)Math.Abs(dir.X);
            else
                step = (int)Math.Abs(dir.Y);

            Vector2 increse = dir / step;

            for (int i = 0; i < step; i++)
            {
                Vector2 newPos = position + increse;
                newPos = new Vector2(MathF.Floor(newPos.X), MathF.Floor(newPos.Y));
                if (ElementSimulation.SafeIdCheckGet(newPos) == -1)
                {
                    if (!BoundsCheck(newPos))
                    {
                        velocity = dir;
                        QueueDelete();
                        return;
                    }
                }
                else
                    return;

                //------safe to access the arrays directly------

                int index = PixelColorer.PosToIndexUnsafe(position);
                ElementSimulation.positionCheck[index] = ElementType.empty.ToByte();
                ElementSimulation.idCheck[index] = -1;
                position = newPos;
                //has to be floor other than round because round can go up and move the element
                //into a position where it is now intersecting another element
                index = PixelColorer.PosToIndexUnsafe(position);
                ElementSimulation.positionCheck[index] = GetElementType().ToByte();
                ElementSimulation.idCheck[index] = id;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void MoveElementOne(Vector2 dir)
        {
            //push to simulation to be deleted
            if(!BoundsCheck(position + dir))
            {
                velocity = dir;
                QueueDelete();
                return;
            }

            ElementSimulation.SafePositionCheckSet(ElementType.empty.ToByte(), position);
            ElementSimulation.SafeIdCheckSet(-1, position);
            position += dir;
            velocity = dir;
            ElementSimulation.SafePositionCheckSet(GetElementType().ToByte(), position);
            ElementSimulation.SafeIdCheckSet(id, position);
        }

        /// <summary>
        /// If there are two elements on the same spot one has to be deleted
        /// the one with the higher time spawned gets deleted
        /// </summary>
        public void CheckDoubleOnPosition()
        {
            //will check if its position has an id that is not negative one
            //if it does then there is a element there and check its time spawned
            //will useally delete itself in most cases since its running on its own
            //instance and that means that the element was there before it
            int idCheck = ElementSimulation.SafeIdCheckGet(position);
            if (idCheck < 0 || idCheck > ElementSimulation.elements.Length)
                return;

            if (idCheck != -1 && idCheck != id)
            {
                if (ElementSimulation.elements[idCheck].timeSpawned <= timeSpawned)
                    QueueDelete();
            }
        }

        /// <summary>
        /// Should be used when deletion needs to happen during a particles update loop
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void QueueDelete()
        {
            if (toBeDeleted)
                return;
            ElementSimulation.idsToDelete.Add(id);
            toBeDeleted = true;
            deleteIndex = ElementSimulation.idsToDelete.Count - 1;
        }

        /// <summary>
        /// Should be used when a deletion is needed right away and outside of a particles update loop
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Delete()
        {
            //set its position to nothing
            ElementSimulation.SafePositionCheckSet(ElementType.empty.ToByte(), position);
            //set its id at its position to nothing
            ElementSimulation.SafeIdCheckSet(-1, position);
            //set the color to empty
            PixelColorer.SetColorAtPos(position, 102, 178, 204);
            ElementSimulation.freeElementSpots.Enqueue(id);
            ElementSimulation.elements[id] = null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// External use outside of a instance
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector3 GetElementColor(ElementTypeSpecific type)
        {
            switch (type)
            {
                case ElementTypeSpecific.sand:
                    {
                        return new Vector3(255, 255, 180);
                    }
                case ElementTypeSpecific.stone:
                    {
                        return new Vector3(180, 180, 180);
                    }
                case ElementTypeSpecific.water:
                    {
                        return new Vector3(40, 0, 255);
                    }
                case ElementTypeSpecific.carbonDioxide:
                    {
                        return new Vector3(80, 80, 80);
                    }
                case ElementTypeSpecific.wall:
                    {
                        return new Vector3(40, 40, 40);
                    }
            }

            return new Vector3(0, 0, 0);
        }
    }

    //----Elements----
    //Can only run things using stuff from element
    //----------------


    //more specific implements

    /// <summary>
    /// Reacts to gravity
    /// </summary>
    public class SandPE : Solid
    {
        public SandPE()
        {
            color = new Vector3(255, 255, 180);
            canMove = true;
        }

        public override ElementTypeSpecific GetElementTypeSpecific()
        {
            return ElementTypeSpecific.sand;
        }
    }

    public class StonePE : Solid
    {
        public StonePE()
        {
            color = new Vector3(180, 180, 180);
            canMove = true;
        }

        public override ElementTypeSpecific GetElementTypeSpecific()
        {
            return ElementTypeSpecific.stone;
        }
    }

    /// <summary>
    /// Unmoveable pixel
    /// </summary>
    public class WallPE : Unmoveable
    {
        public WallPE()
        {
            canMove = false;
            color = new Vector3(40, 40, 40);
        }
        public override ElementTypeSpecific GetElementTypeSpecific()
        {
            return ElementTypeSpecific.wall;
        }
    }

    /// <summary>
    /// Reacts to gravity and solids
    /// </summary>
    public class WaterPE : Liquid
    {

        public WaterPE()
        {
            disp = 50;
            canMove = true;
            color = new Vector3(40, 0, 255);
        }
        public override ElementTypeSpecific GetElementTypeSpecific()
        {
            return ElementTypeSpecific.water;
        }

    }

    /// <summary>
    /// Reacts to gravity and solids
    /// </summary>
    public class CarbonDioxidePE : Gas
    {
        public CarbonDioxidePE()
        {
            disp = 10;
            canMove = true;
            color = new Vector3(80, 80, 80);
        }
        public override ElementTypeSpecific GetElementTypeSpecific()
        {
            return ElementTypeSpecific.carbonDioxide;
        }

    }
}
