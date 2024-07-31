using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
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
        public abstract void Update(ref List<Element> elements, ref int[] positionCheck, ref int[] idCheck);
        public abstract int GetElementType();
        public bool BoundsCheck(int minX, int minY, int maxX, int maxY)
        {
            if (position.X <= minX || position.X >= maxX || position.Y <= minY || position.Y >= maxY)
                return true;
            return false;
        }

        public void SwapElement(Vector2 newPos, int type)
        {
            ElementSimulation.positionCheck[PixelColorer.PosToIndex(position)] = type;
            //get the id of the element below this current element
            int swapid = ElementSimulation.idCheck[PixelColorer.PosToIndex(newPos)];
            //set the element below the current element to the same position
            ElementSimulation.elements[swapid].position = position;
            //set the id at the current position to the id from the element below
            ElementSimulation.idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = swapid;

            //set the type to the new position to our current element
            ElementSimulation.positionCheck[PixelColorer.PosToIndex(newPos)] = GetElementType();
            //set the id of our element to the new position
            ElementSimulation.idCheck[PixelColorer.PosToIndex(newPos)] = id;
            //set the new position of the current element
            ElementSimulation.elements[id].position = newPos;
        }

        public void MoveElement(int type, float deltaTime)
        {
            //find new position
            Vector2 posMove = position + (velocity / deltaTime);
            //do line algorithim

            Vector2 dir = Vector2.Normalize(posMove - position);

            for (int x = (int)position.X; x < posMove.X; x++)
            {
                int id = ElementSimulation.idCheck[PixelColorer.PosToIndex(position + dir)];
                if (id == 0)
                {
                    ElementSimulation.positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                    ElementSimulation.idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                    position += dir;
                    ElementSimulation.positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = type;
                    ElementSimulation.idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
                }
                else
                    return;
            }

        }
    }


    //movement defines

    public abstract class Unmoveable : Element
    {
        public override void Update(ref List<Element> elements, ref int[] positionCheck, ref int[] idCheck)
        {
            positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 100;
            idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
        }

        public override int GetElementType()
        {
            return 100;
        }
    }

    /// <summary>
    /// Swaps places with gas and liquid movable
    /// </summary>
    public abstract class Solid : Element
    {
        public override void Update(ref List<Element> elements, ref int[] positionCheck, ref int[] idCheck)
        {

            int num = new Random().Next(0, 2); // choose random size to pick to favor instead of always left
            bool displaceLiq = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X
                , position.Y + 1))] == 2;
            //swapping down with a liquid
            if (displaceLiq)
            {
                SwapElement(new Vector2(position.X, position.Y + 1), 2);
                return;
            }
            bool displaceLiqLU = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] == 2;
            if (displaceLiqLU && num == 0)
            {
                SwapElement(new Vector2(position.X - 1, position.Y + 1), 2);
                return;
            }
            bool displaceLiqRU = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] == 2;
            if (displaceLiqRU && num == 1)
            {
                SwapElement(new Vector2(position.X + 1, position.Y + 1), 2);
                return;
            }

            //if there is air under the solid

            /*bool grounded = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y + 1))] == 0;
            if (grounded)
            {
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;

                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y + 1))] = 1;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y + 1))] = id;
                elements[id].position = new Vector2(position.X, position.Y + 1);
                return;
            }
            bool LUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] == 0;
            if (LUnder && num == 0)
            {
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;

                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] = 1;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] = id;
                elements[id].position = new Vector2(position.X - 1, position.Y + 1);
                return;
            }
            bool RUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] == 0;
            if (RUnder && num == 1)
            {
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;

                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] = 1;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] = id;
                elements[id].position = new Vector2(position.X + 1, position.Y + 1);
                return;
            }*/
            velocity = new Vector2(0, 1);

            MoveElement(1, 1.0f / 60.0f);
        }

        public override int GetElementType()
        {
            return 1;
        }
    }

    /// <summary>
    /// Swaps places with solid and gas
    /// </summary>
    public abstract class Liquid : Element
    {

        public int disp { get; set; } // viscosity
        public int level { get; set; } // bouyency

        public override void Update(ref List<Element> elements, ref int[] positionCheck, ref int[] idCheck)
        {
            int num = new Random().Next(0, 2); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            //gravity stuff
            bool ground = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y + 1))] == 0;
            if (ground)
            {
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                position += new Vector2(0, 1f);
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 2;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
                return;
            }
            bool LUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y + 1))] == 0 && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y))] == 0;
            if (!ground && LUnder && num == 0)
            {
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                position += new Vector2(-1, 1);
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 2;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
                return;
            }
            bool RUnder = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y + 1))] == 0 && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y))] == 0;
            if (!ground && RUnder && num == 1)
            {
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                position += new Vector2(1, 1);
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 2;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
                return;
            }
            bool Left = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - 1, position.Y))] == 0;
            if (!ground && Left && num == 0)
            {
                int count = 0;
                for (int i = 0; i < disp; i++)
                {
                    if (positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - (i + 1), position.Y))] == 0 && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X - (i + 1), position.Y + 1))] != 0)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                position += new Vector2(-count, 0);
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 2;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
                return;
            }
            bool Right = positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + 1, position.Y))] == 0;
            if (!ground && Right && num == 1)
            {
                int count = 0;
                for (int i = 0; i < disp; i++)
                {
                    if (positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + (i + 1), position.Y))] == 0 && positionCheck[PixelColorer.PosToIndex(new Vector2(position.X + (i + 1), position.Y + 1))] != 0)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 0;
                position += new Vector2(count, 0);
                positionCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = 2;
                idCheck[PixelColorer.PosToIndex(new Vector2(position.X, position.Y))] = id;
                return;
            }
        }

        public override int GetElementType()
        {
            return 2;
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
