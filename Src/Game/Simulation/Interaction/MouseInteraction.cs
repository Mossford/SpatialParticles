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

        public static void DrawMouseCircleSpawner(Vector2 positionMouse, int radius, bool pressed, int button)
        {
            if (!pressed)
            {
                if (idMesh == -1)
                {
                    idMesh = SimRenderer.meshes.Count;
                    SimRenderer.meshes.Add(CreateSimShapes.CreateCircle(60, 0.95f));
                }

                SimRenderer.meshes[idMesh].show = true;
                SimRenderer.meshes[idMesh].position = ((positionMouse * 2) - (Vector2)Globals.window.Size) / 2;
                SimRenderer.meshes[idMesh].position.Y *= -1;
                SimRenderer.meshes[idMesh].scale = Globals.window.Size.Length / new Vector2(PixelColorer.width, PixelColorer.height).Length() * radius;
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

                        if(button == 0)
                        {
                            if (idToCheck == -1)
                            {
                                int id = ElementSimulation.elements.Length;
                                ElementSimulation.AddElement(pos, new SandPE());
                            }
                            else if (ElementSimulation.elements[idToCheck].GetElementType() != ElementType.solid)
                            {
                                ElementSimulation.elements[idToCheck].QueueDelete();
                            }
                        }
                        else
                        {
                            if (idToCheck != -1)
                            {
                                ElementSimulation.elements[idToCheck].QueueDelete();
                            }
                        }
                    }
                }
            }
        }
    }
}
