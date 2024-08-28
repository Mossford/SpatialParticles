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
        static int idCircleMesh;
        static int idElementSquareMesh;
        static int idElementSqaureInnerMesh;

        public static void Init()
        {
            idCircleMesh = -1;
            idElementSquareMesh = -1;
            idElementSqaureInnerMesh = -1;
        }

        /// <summary>
        /// Shows the area elements can be spawned in
        /// </summary>
        public static void DrawMouseCircleSpawner(Vector2 positionMouse, int radius, bool pressed, int button, ElementTypeSpecific type)
        {
            if (!pressed)
            {
                if (idCircleMesh == -1)
                {
                    idCircleMesh = SimRenderer.meshes.Count;
                    SimRenderer.meshes.Add(CreateSimShapes.CreateCircle(60, 0.95f));
                }

                SimRenderer.meshes[idCircleMesh].show = true;
                SimRenderer.meshes[idCircleMesh].position = ((positionMouse * 2) - (Vector2)Globals.window.Size) / 2;
                SimRenderer.meshes[idCircleMesh].position.Y *= -1;
                SimRenderer.meshes[idCircleMesh].scaleX = (float)Globals.window.Size.X / PixelColorer.width * radius;
                SimRenderer.meshes[idCircleMesh].scaleY = (float)Globals.window.Size.Y / PixelColorer.height * radius;
                return;
            }

            if (idCircleMesh == -1)
            {
                idCircleMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateCircle(20, 0.95f));
            }
            Debugging.LogConsole("click mouse " + Debugging.currentStringCounter);
            SimRenderer.meshes[idCircleMesh].show = true;
            SimRenderer.meshes[idCircleMesh].position = ((positionMouse * 2) - (Vector2)Globals.window.Size) / 2;
            SimRenderer.meshes[idCircleMesh].position.Y *= -1;
            SimRenderer.meshes[idCircleMesh].scaleX = (float)Globals.window.Size.X / PixelColorer.width * radius;
            SimRenderer.meshes[idCircleMesh].scaleY = (float)Globals.window.Size.Y / PixelColorer.height * radius;
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
                                switch(type)
                                {
                                    case ElementTypeSpecific.sand:
                                        {
                                            ElementSimulation.AddElement(pos, new SandPE());
                                            break;
                                        }
                                    case ElementTypeSpecific.stone:
                                        {
                                            ElementSimulation.AddElement(pos, new StonePE());
                                            break;
                                        }
                                    case ElementTypeSpecific.water:
                                        {
                                            ElementSimulation.AddElement(pos, new WaterPE());
                                            break;
                                        }
                                    case ElementTypeSpecific.carbonDioxide:
                                        {
                                            ElementSimulation.AddElement(pos, new CarbonDioxidePE());
                                            break;
                                        }
                                    case ElementTypeSpecific.wall:
                                        {
                                            ElementSimulation.AddElement(pos, new WallPE());
                                            break;
                                        }
                                }
                            }
                            else if (ElementSimulation.elements[idToCheck].GetElementTypeSpecific() != type)
                            {
                                //replaced from queue delete may cause issues
                                ElementSimulation.elements[idToCheck].Delete();
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

        public static void DrawMouseElementSelect(Vector2 positionMouse, int radius, bool pressed, ElementTypeSpecific type)
        {
            float scaleX = (float)Globals.window.Size.X / PixelColorer.width * radius;
            float scaleY = (float)Globals.window.Size.Y / PixelColorer.height * radius;
            float scaleXInner = ((float)Globals.window.Size.X / PixelColorer.width - (0.35f * Globals.window.Size.X / PixelColorer.width)) * radius;
            float scaleYInner = ((float)Globals.window.Size.Y / PixelColorer.height - (0.35f * Globals.window.Size.Y / PixelColorer.height)) * radius;
            float boxDist = MathF.Sqrt((scaleX * scaleX) + (scaleY * scaleY));
            Vector2 BoxPos = (((positionMouse * 2) - (Vector2)Globals.window.Size) / 2) + new Vector2(1 * boxDist, -1 * boxDist);

            if (!pressed)
            {
                if (idElementSquareMesh == -1)
                {
                    idElementSquareMesh = SimRenderer.meshes.Count;
                    SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(0.05f));

                    idElementSqaureInnerMesh = SimRenderer.meshes.Count;
                    SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(1f));
                }

                SimRenderer.meshes[idElementSquareMesh].show = true;
                SimRenderer.meshes[idElementSquareMesh].position = BoxPos;
                SimRenderer.meshes[idElementSquareMesh].position.Y *= -1;
                SimRenderer.meshes[idElementSquareMesh].scaleX = scaleX;
                SimRenderer.meshes[idElementSquareMesh].scaleY = scaleY;

                SimRenderer.meshes[idElementSqaureInnerMesh].show = true;
                SimRenderer.meshes[idElementSqaureInnerMesh].color = Element.GetElementColor(type) / 255f;
                SimRenderer.meshes[idElementSqaureInnerMesh].position = BoxPos;
                SimRenderer.meshes[idElementSqaureInnerMesh].position.Y *= -1;
                SimRenderer.meshes[idElementSqaureInnerMesh].scaleX = scaleXInner;
                SimRenderer.meshes[idElementSqaureInnerMesh].scaleY = scaleYInner;
                return;
            }

            if (idElementSquareMesh == -1)
            {
                idElementSquareMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(0.05f));
            }

            SimRenderer.meshes[idElementSquareMesh].show = true;
            SimRenderer.meshes[idElementSquareMesh].position = BoxPos;
            SimRenderer.meshes[idElementSquareMesh].position.Y *= -1;
            SimRenderer.meshes[idElementSquareMesh].scaleX = scaleX;
            SimRenderer.meshes[idElementSquareMesh].scaleY = scaleY;

            if (idElementSqaureInnerMesh == -1)
            {
                idElementSqaureInnerMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(1f));
            }

            SimRenderer.meshes[idElementSqaureInnerMesh].show = true;
            SimRenderer.meshes[idElementSqaureInnerMesh].color = Element.GetElementColor(type) / 255f;
            SimRenderer.meshes[idElementSqaureInnerMesh].position = BoxPos;
            SimRenderer.meshes[idElementSqaureInnerMesh].position.Y *= -1;
            SimRenderer.meshes[idElementSqaureInnerMesh].scaleX = scaleXInner;
            SimRenderer.meshes[idElementSqaureInnerMesh].scaleY = scaleYInner;

            Vector2 position = new Vector2(PixelColorer.width, PixelColorer.height) * (positionMouse / (Vector2)Globals.window.Size);
            position.X = MathF.Floor(position.X);
            position.Y = MathF.Floor(position.Y);
        }


    }
}
