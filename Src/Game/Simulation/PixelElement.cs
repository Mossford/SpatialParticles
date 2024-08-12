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

    public static class ElementTypeConversion
    {
        //the this keyword allows it cool as hell
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ToByte(this ElementType elementType)
        {
            return (byte)elementType;
        }
    }
    

    public abstract class Element
    {
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public Vector3 color { get; set; }
        public bool canMove { get; set; }
        public int id { get; set; }
        public Vector2 oldpos { get; set; }
        public bool toBeDeleted {get; set;}
        public int deleteIndex {get; set;}

        /// <summary>
        /// Position check must be updated when pixel pos changed
        /// </summary>
        public abstract void Update();
        public abstract ElementType GetElementType();
        public bool BoundsCheck(Vector2 position)
        {
            if (position.X < 0 || position.X >= PixelColorer.width || position.Y < 0 || position.Y >= PixelColorer.height)
                return false;
            return true;
        }

        public void SwapElement(Vector2 newPos, ElementType type)
        {
            ElementSimulation.SafePositionCheckSet(type.ToByte(), position);
            //get the id of the element below this current element
            int swapid = ElementSimulation.SafeIdCheckGet(newPos);

            if (swapid == -1)
                return;

            //check position of swap element beacuse it has a possibility to be out of bounds somehow
            if (!BoundsCheck(ElementSimulation.elements[swapid].position))
                return;

            //set the element below the current element to the same position
            ElementSimulation.elements[swapid].position = position;
            //set the id at the current position to the id from the element below
            ElementSimulation.SafeIdCheckSet(swapid, position);

            //set the type to the new position to our current element
            ElementSimulation.SafePositionCheckSet(GetElementType().ToByte(), position);
            //set the id of our element to the new position
            ElementSimulation.SafeIdCheckSet(id, newPos);
            //set the new position of the current element
            ElementSimulation.elements[id].position = newPos;
        }

        public void MoveElement(ElementType type, float deltaTime)
        {
            //find new position
            Vector2 posMove = position + (velocity * deltaTime);
            //do line algorithim

            Vector2 dir = Vector2.Normalize(posMove - position);

            int matrixX1 = (int)position.X;
            int matrixY1 = (int)position.Y;
            int matrixX2 = (int)posMove.X;
            int matrixY2 = (int)posMove.Y;

            int xDir = matrixX1 - matrixX2;
            int yDir = matrixY1 + matrixY2;
            bool xDirIsLarger = Math.Abs(xDir) > Math.Abs(yDir);

            int xModifier = xDir < 0 ? 1 : -1;
            int yModifier = yDir < 0 ? -1 : 1;

            int longerSideLength = Math.Max(Math.Abs(xDir), Math.Abs(yDir));
            int shorterSideLength = Math.Min(Math.Abs(xDir), Math.Abs(yDir));
            float slope = (shorterSideLength == 0 || longerSideLength == 0) ? 0 : ((float)(shorterSideLength) / (longerSideLength));

            int shorterSideIncrease;
            for (int i = 1; i <= longerSideLength; i++)
            {
                shorterSideIncrease = (int)Math.Round(i * slope);
                int yIncrease, xIncrease;
                if (xDirIsLarger)
                {
                    xIncrease = i;
                    yIncrease = shorterSideIncrease;
                }
                else
                {
                    yIncrease = i;
                    xIncrease = shorterSideIncrease;
                }
                int currentY = matrixY1 + (yIncrease * yModifier);
                int currentX = matrixX1 + (xIncrease * xModifier);

                Vector2 pos = new Vector2(currentX, currentY);

                int idAfter = ElementSimulation.idCheck[PixelColorer.PosToIndex(pos)];
                if (idAfter == 0)
                {
                    ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToByte();
                    ElementSimulation.idCheck[PixelColorer.PosToIndex(position)] = 0;
                    position = pos;
                    ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = type.ToByte();
                    ElementSimulation.idCheck[PixelColorer.PosToIndex(position)] = id;
                }
                else
                    return;
            }
        }

        public void MoveElementOne(Vector2 dir, ElementType type)
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
            ElementSimulation.SafePositionCheckSet(type.ToByte(), position);
            ElementSimulation.SafeIdCheckSet(id, position);
        }

        public void QueueDelete()
        {
            ElementSimulation.idsToDelete.Add(id);
            toBeDeleted = true;
            deleteIndex = ElementSimulation.idsToDelete.Count - 1;
            //ElementSimulation.SafePositionCheckSet(ElementType.empty.ToByte(), position);
            //ElementSimulation.SafeIdCheckSet(-1, position);
        }

        public void Delete()
        {
            //set its position to nothing
            ElementSimulation.SafePositionCheckSet(ElementType.empty.ToByte(), position);
            //set its id at its position to nothing
            ElementSimulation.SafeIdCheckSet(-1, position);
            //set the color to empty
            PixelColorer.SetColorAtPos(position, 102, 178, 204);
            for (int i = id + 1; i < ElementSimulation.elements.Count; i++)
            {
                ElementSimulation.elements[i].id--;
                ElementSimulation.SafeIdCheckSet(ElementSimulation.elements[i].id, ElementSimulation.elements[i].position);
            }

            //subtract from ids so that they dont go out of bounds
            for (int i = deleteIndex; i < ElementSimulation.idsToDelete.Count; i++)
            {
                ElementSimulation.idsToDelete[i]--;
            }

            //delete it from the array
            ElementSimulation.elements.RemoveAt(id);
        }
    }


    //movement defines

    public abstract class Unmoveable : Element
    {
        public override void Update()
        {
            ElementSimulation.SafePositionCheckSet(ElementType.wall.ToByte(), position);
            ElementSimulation.SafeIdCheckSet(id, position);
        }

        public override ElementType GetElementType()
        {
            return ElementType.wall;
        }
    }

    /// <summary>
    /// Swaps places with gas and liquid movable
    /// </summary>
    public abstract class Solid : Element
    {
        public override void Update()
        {

            int num = new Random().Next(0, 2); // choose random size to pick to favor instead of always left
            bool displaceLiq = ElementSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y + 1)) == ElementType.liquid.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                SwapElement(new Vector2(position.X, position.Y + 1), ElementType.liquid);
                return;
            }
            bool displaceLiqLU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1)) == ElementType.liquid.ToByte();
            if (displaceLiqLU && num == 0)
            {
                SwapElement(new Vector2(position.X - 1, position.Y + 1), ElementType.liquid);
                return;
            }
            bool displaceLiqRU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1)) == ElementType.liquid.ToByte();
            if (displaceLiqRU && num == 1)
            {
                SwapElement(new Vector2(position.X + 1, position.Y + 1), ElementType.liquid);
                return;
            }

            //if there is air under the solid

            bool grounded = ElementSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y + 1)) == ElementType.empty.ToByte();
            if (grounded)
            {
                MoveElementOne(new Vector2(0, 1), GetElementType());

                return;
            }
            bool LUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1)) == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                MoveElementOne(new Vector2(-1, 1), GetElementType());
                return;
            }
            bool RUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1)) == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                MoveElementOne(new Vector2(1, 1), GetElementType());
                return;
            }
        }

        public override ElementType GetElementType()
        {
            return ElementType.solid;
        }
    }

    /// <summary>
    /// Swaps places with solid and gas
    /// </summary>
    public abstract class Liquid : Element
    {

        public int disp { get; set; } // viscosity
        public int level { get; set; } // bouyency

        public override void Update()
        {
            int num = new Random().Next(0, 2); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            //gravity stuff
            bool ground = ElementSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y + 1)) == ElementType.empty.ToByte();
            if (ground)
            {
                MoveElementOne(new Vector2(0, 1), GetElementType());
                return;
            }
            bool LUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1)) == ElementType.empty.ToByte()
                && ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y)) == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                MoveElementOne(new Vector2(-1, 1), GetElementType());
                return;
            }
            bool RUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1)) == ElementType.empty.ToByte()
                && ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y)) == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                MoveElementOne(new Vector2(1, 1), GetElementType());
                return;
            }
            bool Left = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y)) == ElementType.empty.ToByte();
            if (!ground && Left && num == 0)
            {
                for (int i = 0; i < disp; i++)
                {
                    if (!BoundsCheck(new Vector2(position.X - (i + 1), position.Y)))
                        return;

                    if (ElementSimulation.SafePositionCheckGet(new Vector2(position.X - (i + 1), position.Y)) == ElementType.empty.ToByte())
                    {
                        MoveElementOne(new Vector2(-1, 0), GetElementType());
                    }
                    else
                    {
                        break;
                    }
                }

                return;
            }
            bool Right = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y)) == ElementType.empty.ToByte();
            if (!ground && Right && num == 1)
            {
                for (int i = 0; i < disp; i++)
                {
                    if (!BoundsCheck(new Vector2(position.X + (i + 1), position.Y)))
                        return;

                    if (ElementSimulation.SafePositionCheckGet(new Vector2(position.X + (i + 1), position.Y)) == ElementType.empty.ToByte())
                    {
                        MoveElementOne(new Vector2(1, 0), GetElementType());
                    }
                    else
                    {
                        break;
                    }
                }

                return;
            }
        }

        public override ElementType GetElementType()
        {
            return ElementType.liquid;
        }
    }

    /*public abstract class Gas : Element
    {


        public int disp { get; set; } // viscosity
        public int level { get; set; } // bouyency

        public override void Update()
        {
            int num = new Random().Next(0, 3); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            //gravity stuff
            bool ground = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y - 1))] == ElementType.empty.ToByte();
            if (ground && num == 2)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(0, -1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.gas.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                //MoveElement(2, Globals.GetDeltaTime());
                return;
            }
            bool LUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y - 1))] == ElementType.empty.ToByte()
                && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y))] == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(-1, -1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.gas.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
            bool RUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y - 1))] == ElementType.empty.ToByte()
                && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y))] == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(1, -1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.gas.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
            bool Left = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y))] == ElementType.empty.ToByte();
            if (!ground && Left && num == 0)
            {
                int count = 0;
                for (int i = 0; i < disp; i++)
                {
                    if (positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - (i + 1), position.Y))] == ElementType.empty.ToByte())
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(-count, 0);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.gas.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
            bool Right = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y))] == ElementType.empty.ToByte();
            if (!ground && Right && num == 1)
            {
                int count = 0;
                for (int i = 0; i < disp; i++)
                {
                    if (positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + (i + 1), position.Y))] == ElementType.empty.ToByte())
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(count, 0);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.gas.ToByte();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
        }

        public override ElementType GetElementType()
        {
            return ElementType.gas;
        }
    }*/

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
    }

    public class StonePE : Solid
    {
        public StonePE()
        {
            color = new Vector3(180, 180, 180);
            canMove = true;
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
    }

    /// <summary>
    /// Reacts to gravity and solids
    /// </summary>
    public class WaterPE : Liquid
    {

        public WaterPE()
        {
            disp = 10;
            canMove = true;
            color = new Vector3(40, 0, 255);
        }

    }

    /// <summary>
    /// Reacts to gravity and solids
    /// </summary>
    /*public class CarbonDioxidePE : Gas
    {

        public CarbonDioxidePE()
        {
            disp = 10;
            canMove = true;
            color = new Vector3(60, 60, 60);
        }

    }*/
}
