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
                velocity = new Vector2(0, velocity.Y);
                velocity += new Vector2(0, 0.5f);
                MoveElement();
                //MoveElementOne(new Vector2(0, 1));
                return;
            }
            bool LUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1)) == ElementType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1 - (velocity.Y * 0.07f), 1 - (velocity.Y * 0.07f));
                MoveElement();
                return;
            }
            bool RUnder = ElementSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1)) == ElementType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1 + (velocity.Y * 0.07f), 1 - (velocity.Y * 0.07f));
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
