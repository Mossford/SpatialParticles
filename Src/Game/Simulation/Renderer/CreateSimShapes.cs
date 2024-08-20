using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class CreateSimShapes
    {
        public static SimMesh CreateCircle(int numPoints, float thickness)
        {
            int points = numPoints * 2;
            int half = numPoints;
            Vector2[] vertexes = new Vector2[points];
            float conv = MathF.PI / 180f;

            //create vertexes on a circle
            for (int i = 0; i < 720; i += 360 / half)
            {
                int index = (int)MathF.Floor(i / (360 / half));
                if (index < half)
                {
                    vertexes[index] = new Vector2(MathF.Sin(i * conv), MathF.Cos(i * conv));
                }
                else
                {
                    vertexes[index] = new Vector2(MathF.Sin(i * conv) * thickness, MathF.Cos(i * conv) * thickness);
                }
            }

            //First triangle
            /*
                start at 0 on outer circle
                go down to second circle start + half
                go left +1 but if last then start + 1
             */
            //Second triangle
            /*
                start at last point
                go up last point - half
                go right - 1 but if last then start
             */

            uint[] indices = new uint[half * 6];
            for (uint i = 0; i < half; i++)
            {
                uint pos = i * 6;
                //first triangle
                indices[pos] = i;
                indices[pos + 1] = i + (uint)half;
                if (i == half - 1)
                    indices[pos + 2] = i + 1;
                else
                    indices[pos + 2] = indices[pos + 1] + 1;
                //second triangle
                indices[pos + 3] = indices[pos + 2];
                indices[pos + 4] = indices[pos + 3] - (uint)half;
                if (i == half - 1)
                    indices[pos + 5] = i;
                else
                    indices[pos + 5] = indices[pos + 4] - 1;
            }

            return new SimMesh(vertexes, indices);
        }
    }
}
