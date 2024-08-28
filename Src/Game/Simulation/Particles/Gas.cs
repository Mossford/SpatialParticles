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
    public abstract class Gas : Element
    {

        public override void Update()
        {
            int num = ElementSimulation.random.Next(0, 3); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            //gravity stuff
            int posCheckBelow = ElementSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y - 1));
            bool ground = posCheckBelow == ElementType.empty.ToByte();
            if (ground && num == 2)
            {
                velocity = new Vector2(0, velocity.Y);
                velocity -= new Vector2(0, 1.8f);
                MoveElement();
                return;
            }
            int posCheckLU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y - 1));
            int posCheckL = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y));
            bool LUnder = posCheckLU == ElementType.empty.ToByte() && posCheckL == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1, -1);
                MoveElement();
                return;
            }
            int posCheckRU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y - 1));
            int posCheckR = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y));
            bool RUnder = posCheckRU == ElementType.empty.ToByte() && posCheckR == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1, -1);
                MoveElement();
                return;
            }
            bool Left = posCheckL == ElementType.empty.ToByte();
            if (!ground && Left && num == 0)
            {
                for (int i = 0; i < properties.viscosity; i++)
                {
                    if (!BoundsCheck(new Vector2(position.X - (i + 1), position.Y)))
                        return;

                    if (ElementSimulation.SafePositionCheckGet(new Vector2(position.X - (i + 1), position.Y)) == ElementType.empty.ToByte())
                    {
                        velocity = new Vector2(-1, 0);
                        MoveElement();
                    }
                    else
                    {
                        break;
                    }
                }

                return;
            }
            bool Right = posCheckR == ElementType.empty.ToByte();
            if (!ground && Right && num == 1)
            {
                for (int i = 0; i < properties.viscosity; i++)
                {
                    if (!BoundsCheck(new Vector2(position.X + (i + 1), position.Y)))
                        return;

                    if (ElementSimulation.SafePositionCheckGet(new Vector2(position.X + (i + 1), position.Y)) == ElementType.empty.ToByte())
                    {
                        velocity = new Vector2(1, 0);
                        MoveElement();
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
            return ElementType.gas;
        }
    }
}
