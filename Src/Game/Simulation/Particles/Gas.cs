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

        public int disp { get; set; } // viscosity
        public int level { get; set; } // bouyency

        public override void Update()
        {
            int num = ElementSimulation.random.Next(0, 3); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            //gravity stuff
            bool ground = ElementSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y - 1)) == ElementType.empty.ToByte();
            if (ground && num == 2)
            {
                velocity = new Vector2(0, velocity.Y);
                velocity -= new Vector2(0, 1.8f);
                MoveElement();
                return;
            }
            bool LUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y - 1)) == ElementType.empty.ToByte()
                && ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y)) == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1, -1);
                MoveElement();
                return;
            }
            bool RUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y - 1)) == ElementType.empty.ToByte()
                && ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y)) == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1, -1);
                MoveElement();
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
            bool Right = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y)) == ElementType.empty.ToByte();
            if (!ground && Right && num == 1)
            {
                for (int i = 0; i < disp; i++)
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
