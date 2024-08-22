using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    /// <summary>
    /// Swaps places with solid and gas
    /// </summary>
    public abstract class Liquid : Element
    {
        public int disp { get; set; } // viscosity
        public int level { get; set; } // bouyency

        public override void Update()
        {
            int num = ElementSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            int posCheckBelow = ElementSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y + 1));
            bool displaceLiq = posCheckBelow == ElementType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                SwapElement(new Vector2(position.X, position.Y + 1), ElementType.gas);
                return;
            }
            int posCheckLU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1));
            bool displaceLiqLU = posCheckLU == ElementType.gas.ToByte() && posCheckBelow == ElementType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                SwapElement(new Vector2(position.X - 1, position.Y + 1), ElementType.gas);
                return;
            }
            int posCheckRU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1));
            bool displaceLiqRU = posCheckRU == ElementType.gas.ToByte() && posCheckBelow == ElementType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                SwapElement(new Vector2(position.X + 1, position.Y + 1), ElementType.gas);
                return;
            }

            //gravity stuff
            bool ground = posCheckBelow == ElementType.empty.ToByte();
            if (ground)
            {
                velocity = new Vector2(0, velocity.Y);
                velocity += new Vector2(0, 0.5f);
                MoveElement();
                return;
            }
            int posCheckL = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y));
            bool LUnder = posCheckLU == ElementType.empty.ToByte() && posCheckL == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1 - (velocity.Y * 0.6f), 1 - (velocity.Y * 0.3f));
                MoveElement();
                return;
            }
            int posCheckR = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y));
            bool RUnder = posCheckRU == ElementType.empty.ToByte() && posCheckR == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1 + (velocity.Y * 0.6f), 1 - (velocity.Y * 0.3f));
                MoveElement();
                return;
            }
            bool left = posCheckBelow != ElementType.empty.ToByte() && posCheckLU != ElementType.empty.ToByte();
            if (!ground && left && num == 0)
            {
                int moveDisp = ElementSimulation.random.Next(0, disp);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(position.X - 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ElementSimulation.SafePositionCheckGet(checkPos) == ElementType.empty.ToByte())
                        {
                            velocity = new Vector2(-1, 0);
                            MoveElement();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Vector2 checkPos = new Vector2(position.X - 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ElementSimulation.SafePositionCheckGet(checkPos) == ElementType.empty.ToByte()
                            && ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1)) != ElementType.empty.ToByte())
                        {
                            velocity = new Vector2(-1, 0);
                            MoveElement();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return;
            }
            bool right = posCheckBelow != ElementType.empty.ToByte() && posCheckRU != ElementType.empty.ToByte();
            if (!ground && right && num == 1)
            {
                int moveDisp = ElementSimulation.random.Next(0, disp);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(position.X + 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ElementSimulation.SafePositionCheckGet(checkPos) == ElementType.empty.ToByte())
                        {
                            velocity = new Vector2(1, 0);
                            MoveElement();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Vector2 checkPos = new Vector2(position.X + 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ElementSimulation.SafePositionCheckGet(checkPos) == ElementType.empty.ToByte()
                            && ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1)) != ElementType.empty.ToByte())
                        {
                            velocity = new Vector2(1, 0);
                            MoveElement();
                        }
                        else
                        {
                            break;
                        }
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
}
