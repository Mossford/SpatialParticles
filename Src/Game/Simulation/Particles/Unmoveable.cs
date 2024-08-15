using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
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
}
