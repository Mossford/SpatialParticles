using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    /// <summary>
    /// Swaps places with gas and liquid movable
    /// </summary>
    public abstract class Solid : Element
    {
        public override void Update()
        {
            int num = ElementSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left
            int posCheckBelow = ElementSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y + 1));
            bool displaceLiq = posCheckBelow == ElementType.liquid.ToByte() || posCheckBelow == ElementType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                SwapElement(new Vector2(position.X, position.Y + 1), (ElementType)posCheckBelow);
                return;
            }
            int posCheckLU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1));
            bool displaceLiqLU = posCheckLU == ElementType.liquid.ToByte() || posCheckLU == ElementType.gas.ToByte() && posCheckBelow == ElementType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                SwapElement(new Vector2(position.X - 1, position.Y + 1), (ElementType)posCheckLU);
                return;
            }
            int posCheckRU = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1));
            bool displaceLiqRU = posCheckRU == ElementType.liquid.ToByte() || posCheckRU == ElementType.gas.ToByte() && posCheckBelow == ElementType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                SwapElement(new Vector2(position.X + 1, position.Y + 1), (ElementType)posCheckRU);
                return;
            }

            //if there is air under the solid
            bool grounded = posCheckBelow == ElementType.empty.ToByte();
            if (grounded)
            {
                velocity = new Vector2(0, velocity.Y);
                velocity += new Vector2(0, 0.5f);
                MoveElement();
                return;
            }
            bool LUnder = posCheckLU == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1 - (velocity.Y * 0.12f), 1 - (velocity.Y * 0.07f));
                MoveElement();
                return;
            }
            bool RUnder = posCheckRU == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1 + (velocity.Y * 0.12f), 1 - (velocity.Y * 0.07f));
                MoveElement();
                return;
            }
        }

        public override ElementType GetElementType()
        {
            return ElementType.solid;
        }
    }
}
