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
    public enum ElementType : ushort
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
        public static ushort ToUshort(this ElementType elementType)
        {
            return (ushort)elementType;
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

        /// <summary>
        /// Position check must be updated when pixel pos changed
        /// </summary>
        public abstract void Update(ref List<Element> elements, ref ushort[] positionCheck, ref int[] idCheck);
        public abstract ElementType GetElementType();
        public bool BoundsCheck(int minX, int minY, int maxX, int maxY)
        {
            if (position.X <= minX || position.X >= maxX || position.Y <= minY || position.Y >= maxY)
                return true;
            return false;
        }

        public void SwapElement(Vector2 newPos, ElementType type)
        {
            ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = type.ToUshort();
            //get the id of the element below this current element
            int swapid = ElementSimulation.idCheck[PixelColorer.PosToIndex(newPos)];
            //set the element below the current element to the same position
            ElementSimulation.elements[swapid].position = position;
            //set the id at the current position to the id from the element below
            ElementSimulation.idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = swapid;

            //set the type to the new position to our current element
            ElementSimulation.positionCheck[PixelColorer.PosToIndex(newPos)] = GetElementType().ToUshort();
            //set the id of our element to the new position
            ElementSimulation.idCheck[PixelColorer.PosToIndex(newPos)] = id;
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
                    ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                    ElementSimulation.idCheck[PixelColorer.PosToIndex(position)] = 0;
                    position = pos;
                    ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = type.ToUshort();
                    ElementSimulation.idCheck[PixelColorer.PosToIndex(position)] = id;
                }
                else
                    return;
            }
        }

        public void MoveElementOne(Vector2 dir, ElementType type)
        {
            ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = type.ToUshort();
            ElementSimulation.idCheck[PixelColorer.PosToIndex(position)] = 0;
            position += dir;
            ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = GetElementType().ToUshort();
            ElementSimulation.idCheck[PixelColorer.PosToIndex(position)] = id;
        }

        public void Delete()
        {
            //set its position to nothing
            ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
            //set its id at its position to nothing
            ElementSimulation.idCheck[PixelColorer.PosToIndex(position)] = 0;
            //delete it from the array
            ElementSimulation.elements.RemoveAt(id);
        }
    }


    //movement defines

    public abstract class Unmoveable : Element
    {
        public override void Update(ref List<Element> elements, ref ushort[] positionCheck, ref int[] idCheck)
        {
            positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 100;
            idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
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
        public override void Update(ref List<Element> elements, ref ushort[] positionCheck, ref int[] idCheck)
        {

            int num = new Random().Next(0, 2); // choose random size to pick to favor instead of always left
            bool displaceLiq = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y + 1))] == ElementType.liquid.ToUshort();
            //swapping down with a liquid
            if (displaceLiq)
            {
                SwapElement(new Vector2(position.X, position.Y + 1), ElementType.liquid);
                return;
            }
            bool displaceLiqLU = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] == ElementType.liquid.ToUshort();
            if (displaceLiqLU && num == 0)
            {
                SwapElement(new Vector2(position.X - 1, position.Y + 1), ElementType.liquid);
                return;
            }
            bool displaceLiqRU = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] == ElementType.liquid.ToUshort();
            if (displaceLiqRU && num == 1)
            {
                SwapElement(new Vector2(position.X + 1, position.Y + 1), ElementType.liquid);
                return;
            }

            velocity = new Vector2(0, 0.01f);

            //if there is air under the solid

            bool grounded = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y + 1))] == ElementType.empty.ToUshort();
            if (grounded)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(0, 1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.solid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                //MoveElement(1, Globals.GetDeltaTime());
                return;
            }
            bool LUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] == ElementType.empty.ToUshort();
            if (LUnder && num == 0)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(-1, 1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.solid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
            bool RUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] == ElementType.empty.ToUshort();
            if (RUnder && num == 1)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(1, 1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.solid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
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

        public override void Update(ref List<Element> elements, ref ushort[] positionCheck, ref int[] idCheck)
        {
            int num = new Random().Next(0, 2); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            //gravity stuff
            bool ground = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y + 1))] == ElementType.empty.ToUshort();
            if (ground)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(0, 1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.liquid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                //MoveElement(2, Globals.GetDeltaTime());
                return;
            }
            bool LUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] == ElementType.empty.ToUshort()
                && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y))] == ElementType.empty.ToUshort();
            if (!ground && LUnder && num == 0)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(-1, 1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.liquid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
            bool RUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] == ElementType.empty.ToUshort()
                && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y))] == ElementType.empty.ToUshort();
            if (!ground && RUnder && num == 1)
            {
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(1, 1);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.liquid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
            bool Left = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y))] == ElementType.empty.ToUshort();
            if (!ground && Left && num == 0)
            {
                int count = 0;
                for (int i = 0; i < disp; i++)
                {
                    if (positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - (i + 1), position.Y))] == ElementType.empty.ToUshort()
                        && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - (i + 1), position.Y + 1))] != ElementType.empty.ToUshort())
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(-count, 0);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.liquid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
            bool Right = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y))] == ElementType.empty.ToUshort();
            if (!ground && Right && num == 1)
            {
                int count = 0;
                for (int i = 0; i < disp; i++)
                {
                    if (positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + (i + 1), position.Y))] == ElementType.empty.ToUshort()
                        && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + (i + 1), position.Y + 1))] != ElementType.empty.ToUshort())
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.empty.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = 0;
                position += new Vector2(count, 0);
                positionCheck[PixelColorer.PosToIndex(position)] = ElementType.liquid.ToUshort();
                idCheck[PixelColorer.PosToIndex(position)] = id;
                return;
            }
        }

        public override ElementType GetElementType()
        {
            return ElementType.liquid;
        }
    }

    public abstract class Gas : Element
    {

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
            color = new Vector3(0, 0, 0);
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
}
