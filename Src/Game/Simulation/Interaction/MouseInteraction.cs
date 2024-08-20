using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpatialGame
{
    public static class MouseInteraction
    {
        static int idMesh = -1;

        public static void DrawMouseCircleSpawner(Vector2 positionMouse, int radius, bool pressed)
        {
            if (!pressed)
            {
                if(idMesh != -1)
                {
                    SimRenderer.meshes[idMesh].show = false;
                }
                return;
            }

            if (idMesh == -1)
            {
                idMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateCircle(20, 0.95f));
            }

            SimRenderer.meshes[idMesh].show = true;

            SimRenderer.meshes[idMesh].position = ((positionMouse * 2) - (Vector2)Globals.window.Size) / 2;
            SimRenderer.meshes[idMesh].position.Y *= -1;
            SimRenderer.meshes[idMesh].scale = Globals.window.Size.Length / new Vector2(PixelColorer.width, PixelColorer.height).Length() * radius;
            Vector2 position = new Vector2(PixelColorer.width, PixelColorer.height) * (positionMouse / (Vector2)Globals.window.Size);
            position.X = MathF.Floor(position.X);
            position.Y = MathF.Floor(position.Y);

            for (int x = (int)position.X - radius; x < position.X + radius; x++)
            {
                for (int y = (int)position.Y - radius; y < position.Y + radius; y++)
                {
                    if (x < 0 || x >= PixelColorer.width || y < 0 || y >= PixelColorer.height)
                        continue;

                    float check = (float)Math.Sqrt(((x - position.X) * (x - position.X)) + ((y - position.Y) * (y - position.Y)));
                    if (check < radius)
                    {
                        Vector2 pos = new Vector2(x, y);
                        int idToCheck = ElementSimulation.SafeIdCheckGet(pos);

                        if (idToCheck != -1)
                        {
                            //Find amount that was deleted before the current element and subtract that from the id used
                            /*int adder = 0;
                            int[] keys = indexCountDelete.Keys.ToArray();
                            for (int i = 0; i < indexCountDelete.Count; i++)
                            {
                                if (idToCheck > keys[i])
                                    adder++;
                                else
                                    break;
                            }
                            elements[idToCheck - adder].Delete();*/
                        }
                        int id = ElementSimulation.elements.Count;
                        ElementSimulation.elements.Add(new SandPE());
                        ElementSimulation.elements[id].id = id;
                        ElementSimulation.elements[id].position = pos;
                        ElementSimulation.SafePositionCheckSet(ElementType.solid.ToByte(), ElementSimulation.elements[id].position);
                        ElementSimulation.SafeIdCheckSet(id, ElementSimulation.elements[id].position);
                    }
                }
            }
        }
    }
}
