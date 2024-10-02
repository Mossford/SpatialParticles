using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
        static int idElementText;

        public static void Init()
        {
            idCircleMesh = -1;
            idElementSquareMesh = -1;
            idElementSqaureInnerMesh = -1;
            idElementText = -1;
        }

        /// <summary>
        /// Shows the area elements can be spawned in
        /// </summary>
        public static void DrawMouseCircleSpawner(Vector2 positionMouse, int radius, bool pressed, int button, string name, bool mode, int selection)
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
                        int idToCheck = ParticleSimulation.SafeIdCheckGet(pos);

                        if(button == 0 && !mode)
                        {
                            if (idToCheck == -1)
                            {
                                ParticleSimulation.AddParticle(pos, name);
                            }
                            else if (ParticleSimulation.particles[idToCheck].propertyIndex != ParticleResourceHandler.particleNameIndexes[name])
                            {
                                //replaced from queue delete may cause issues
                                ParticleSimulation.particles[idToCheck].Delete();
                            }
                        }
                        else if (button == 0 && mode)
                        {
                            if(idToCheck != -1 && selection == 0)
                            {
                                ParticleSimulation.particles[idToCheck].state.temperature += 10f;
                            }
                            if (idToCheck != -1 && selection == 1)
                            {
                                ParticleSimulation.particles[idToCheck].state.temperature -= 10f;
                            }
                        }
                        else if(button == 1)
                        {
                            if (idToCheck != -1)
                            {
                                ParticleSimulation.particles[idToCheck].Delete();
                            }
                        }
                    }
                }
            }
        }

        public static void DrawMouseElementSelect(Vector2 positionMouse, int radius, bool pressed, string name, bool mode, int selection)
        {
            float scaleX = (float)Globals.window.Size.X / PixelColorer.width * radius;
            float scaleY = (float)Globals.window.Size.Y / PixelColorer.height * radius;
            float scaleXInner = ((float)Globals.window.Size.X / PixelColorer.width - (0.35f * Globals.window.Size.X / PixelColorer.width)) * radius;
            float scaleYInner = ((float)Globals.window.Size.Y / PixelColorer.height - (0.35f * Globals.window.Size.Y / PixelColorer.height)) * radius;
            float boxDist = MathF.Sqrt((scaleX * scaleX) + (scaleY * scaleY));
            Vector2 BoxPos = (((positionMouse * 2) - (Vector2)Globals.window.Size) / 2) + new Vector2(1 * boxDist, -1 * boxDist);
            Vector2 textPos = (((positionMouse * 2) - (Vector2)Globals.window.Size) / 2) + new Vector2(2 * boxDist + 250, -1 * boxDist);
            textPos.Y *= -1;

            if (idElementSquareMesh == -1)
            {
                idElementSquareMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(0.05f));

                idElementSqaureInnerMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(1f));
                    
                idElementText = SimText.textRefs.Count;
                SimText.CreateText("text", textPos,100, 50, 1f, 0f, 32, 1);
            }

            SimRenderer.meshes[idElementSquareMesh].show = true;
            SimRenderer.meshes[idElementSquareMesh].position = BoxPos;
            SimRenderer.meshes[idElementSquareMesh].position.Y *= -1;
            SimRenderer.meshes[idElementSquareMesh].scaleX = scaleX;
            SimRenderer.meshes[idElementSquareMesh].scaleY = scaleY;

            SimRenderer.meshes[idElementSqaureInnerMesh].show = true;
            if (!mode)
                SimRenderer.meshes[idElementSqaureInnerMesh].color = (Vector3)Particle.GetParticleColor(name) / 255f;
            else
            {
                if(selection == 0)
                    SimRenderer.meshes[idElementSqaureInnerMesh].color = new Vector3(1f, 100 / 255f, 0);
                if(selection == 1)
                    SimRenderer.meshes[idElementSqaureInnerMesh].color = new Vector3(0, 100 / 255f, 1f);
            }
            SimRenderer.meshes[idElementSqaureInnerMesh].position = BoxPos;
            SimRenderer.meshes[idElementSqaureInnerMesh].position.Y *= -1;
            SimRenderer.meshes[idElementSqaureInnerMesh].scaleX = scaleXInner;
            SimRenderer.meshes[idElementSqaureInnerMesh].scaleY = scaleYInner;
                
            SimText.UpdateText(name, idElementText, textPos,500, 75, 0.5f, 0f, 64, 1);
            //SimText.color = (Vector3)Particle.GetParticleColor(name) / 255f;
        }


    }
}
