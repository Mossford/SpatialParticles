using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpatialGame
{
    public static class MouseInteraction
    {
        class Spawner
        {
            public int idCircleMesh;
            public int idElementSquareMesh;
            public int idElementSqaureInnerMesh;
            public SimText elementText;
            public string nameBefore;
            public int selectionBefore;

            public Spawner()
            {
                idCircleMesh = -1;
                idElementSquareMesh = -1;
                idElementSqaureInnerMesh = -1;
                elementText = new SimText();
                nameBefore = "";
                selectionBefore = -1;
            }
        }

        static List<Spawner> spawners;

        public static void Init()
        {
            spawners = new List<Spawner>();
            spawners.Add(new Spawner());
        }

        public static void CleanUp()
        {
            
        }

        public static void DrawMouseElementsCircle(Vector2 positionMouse, int radius, bool pressed, int id)
        {
            if (!pressed)
            {
                if (id >= spawners.Count || spawners[id].idCircleMesh == -1)
                {
                    if (id >= spawners.Count)
                    {
                        spawners.Add(new Spawner());
                    }
                    spawners[id].idCircleMesh = SimRenderer.meshes.Count;
                    SimRenderer.meshes.Add(CreateSimShapes.CreateCircle(60, 0.95f));
                }

                SimRenderer.meshes[spawners[id].idCircleMesh].show = true;
                SimRenderer.meshes[spawners[id].idCircleMesh].position = positionMouse;
                SimRenderer.meshes[spawners[id].idCircleMesh].position.Y *= -1;
                SimRenderer.meshes[spawners[id].idCircleMesh].scaleX = Window.size.X / PixelColorer.width * radius;
                SimRenderer.meshes[spawners[id].idCircleMesh].scaleY = Window.size.Y / PixelColorer.height * radius;
                return;
            }

            if (id >= spawners.Count || spawners[id].idCircleMesh == -1)
            {
                if (id >= spawners.Count)
                {
                    spawners.Add(new Spawner());
                }
                spawners[id].idCircleMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateCircle(60, 0.95f));
            }
            SimRenderer.meshes[spawners[id].idCircleMesh].show = true;
            SimRenderer.meshes[spawners[id].idCircleMesh].position = positionMouse;
            SimRenderer.meshes[spawners[id].idCircleMesh].position.Y *= -1;
            SimRenderer.meshes[spawners[id].idCircleMesh].scaleX = Window.size.X / PixelColorer.width * radius;
            SimRenderer.meshes[spawners[id].idCircleMesh].scaleY = Window.size.Y / PixelColorer.height * radius;
        }

        /// <summary>
        /// Shows the area elements can be spawned in
        /// </summary>
        public static void SpawnParticlesCircleSpawner(Vector2 positionMouse, Vector2 oldPositionMouse, int radius, bool pressed, int button, string name, bool mode, int selection)
        {
            if (!pressed)
            {
                return;
            }
            
            //put mouse position out of local window space
            positionMouse = (positionMouse * 2 + Window.size) / 2;
            oldPositionMouse = (oldPositionMouse * 2 + Window.size) / 2;
            
            if (positionMouse == oldPositionMouse || (positionMouse - oldPositionMouse).Length() < radius)
            {
                Vector2 newPos = new Vector2(PixelColorer.width, PixelColorer.height) * (positionMouse / Window.size);
                newPos.X = MathF.Floor(newPos.X);
                newPos.Y = MathF.Floor(newPos.Y);
                
                //create circle of particles
                for (int x = (int)newPos.X - radius; x < newPos.X + radius; x++)
                {
                    for (int y = (int)newPos.Y - radius; y < newPos.Y + radius; y++)
                    {
                        if (x < 0 || x >= PixelColorer.width || y < 0 || y >= PixelColorer.height)
                            continue;

                        float check = (float)Math.Sqrt(((x - newPos.X + 0.5f) * (x - newPos.X + 0.5f)) + ((y - newPos.Y + 0.5f) * (y - newPos.Y + 0.5f)));
                        if (check > radius)
                            continue;
                        
                        Vector2 pos = new Vector2(MathF.Round(x), MathF.Round(y));
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
                                ParticleSimulation.AddParticle(pos, name);
                            }
                        }
                        else if (button == 0 && mode)
                        {
                            if(idToCheck != -1 && selection == 0)
                            {
                                ParticleSimulation.particles[idToCheck].state.temperature += Globals.fixedDeltaTime;
                            }
                            if (idToCheck != -1 && selection == 1)
                            {
                                ParticleSimulation.particles[idToCheck].state.temperature -=  Globals.fixedDeltaTime;
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
            else
            {
                Vector2 tempPos = oldPositionMouse;
                Vector2 dir = positionMouse - oldPositionMouse;

                Vector2 increase = dir / radius;

                for (int i = 0; i < radius; i++)
                {
                    tempPos += increase;
                    
                    Vector2 newPos = new Vector2(PixelColorer.width, PixelColorer.height) * (tempPos / Window.size);
                    newPos.X = MathF.Floor(newPos.X);
                    newPos.Y = MathF.Floor(newPos.Y);
                    
                    //create circle of particles
                    for (int x = (int)newPos.X - radius; x < newPos.X + radius; x++)
                    {
                        for (int y = (int)newPos.Y - radius; y < newPos.Y + radius; y++)
                        {
                            if (x < 0 || x >= PixelColorer.width || y < 0 || y >= PixelColorer.height)
                                continue;

                            float check = (float)Math.Sqrt(((x - newPos.X + 0.5f) * (x - newPos.X + 0.5f)) + ((y - newPos.Y + 0.5f) * (y - newPos.Y + 0.5f)));
                            if (check >= radius)
                                continue;
                            
                            Vector2 pos = new Vector2(MathF.Round(x), MathF.Round(y));
                            
                            float distanceToOldPos = MathF.Sqrt((x - oldPositionMouse.X + 0.5f) * (x - oldPositionMouse.X + 0.5f) + (y - oldPositionMouse.Y + 0.5f) * (y - oldPositionMouse.Y + 0.5f));
                            if (distanceToOldPos <= radius)
                                continue;
                            
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
                                    ParticleSimulation.AddParticle(pos, name);
                                }
                            }
                            else if (button == 0 && mode)
                            {
                                if(idToCheck != -1 && selection == 0)
                                {
                                    ParticleSimulation.particles[idToCheck].state.temperature += Globals.fixedDeltaTime;
                                }
                                if (idToCheck != -1 && selection == 1)
                                {
                                    ParticleSimulation.particles[idToCheck].state.temperature -=  Globals.fixedDeltaTime;
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
        }

        public static void DrawMouseElementSelect(Vector2 positionMouse, int radius, bool pressed, string name, bool mode, int selection, int id)
        {
            float scaleX = Window.size.X / PixelColorer.width * radius;
            float scaleY = Window.size.Y / PixelColorer.height * radius;
            float scaleXInner = (Window.size.X / PixelColorer.width - (0.35f * Window.size.X / PixelColorer.width)) * radius;
            float scaleYInner = (Window.size.Y / PixelColorer.height - (0.35f * Window.size.Y / PixelColorer.height)) * radius;
            float boxDist = MathF.Sqrt((scaleX * scaleX) + (scaleY * scaleY));
            Vector2 BoxPos = positionMouse + new Vector2(1 * boxDist, -1 * boxDist);
            Vector2 textPos = positionMouse + new Vector2(2 * scaleX + 250, -1 * boxDist);
            if (textPos.X - 125 > Window.size.X / 2f)
            {
                textPos = positionMouse + new Vector2(-2 * scaleX + 250, -1 * boxDist);
            }
            textPos.Y *= -1;

            if (id >= spawners.Count || spawners[id].idElementSquareMesh == -1)
            {
                if (id >= spawners.Count)
                {
                    spawners.Add(new Spawner());
                }
                spawners[id].idElementSquareMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(0.05f));

                spawners[id].idElementSqaureInnerMesh = SimRenderer.meshes.Count;
                SimRenderer.meshes.Add(CreateSimShapes.CreateSquare(1f));
                
                spawners[id].elementText.CreateText("text", textPos, 100, 50, 1f, 0f, 32, 1);
            }

            SimRenderer.meshes[spawners[id].idElementSquareMesh].show = true;
            SimRenderer.meshes[spawners[id].idElementSquareMesh].position = BoxPos;
            SimRenderer.meshes[spawners[id].idElementSquareMesh].position.Y *= -1;
            SimRenderer.meshes[spawners[id].idElementSquareMesh].scaleX = scaleX;
            SimRenderer.meshes[spawners[id].idElementSquareMesh].scaleY = scaleY;

            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].show = true;
            //particle spawn mode
            if (!mode)
            {
                if (spawners[id].nameBefore != name)
                {
                    Vector3 color = (Vector3)Particle.GetParticleColor(name) / 255f;
                    spawners[id].elementText.UpdateTextFull(name, textPos,500, 75, 0.5f, 0f, 64, 1);
                    spawners[id].elementText.color = color;
                    SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].color = color;
                    spawners[id].nameBefore = name;
                }
            }
            //function mode
            else
            {
                //heating
                if (selection == 0)
                {
                    if (selection != spawners[id].selectionBefore)
                    {
                        spawners[id].elementText.UpdateTextFull("Heat", textPos,500, 75, 0.5f, 0f, 64, 1);
                        spawners[id].elementText.color = new Vector3(1f, 100 / 255f, 0);
                        SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].color = new Vector3(1f, 100 / 255f, 0);
                        spawners[id].selectionBefore = selection;
                    }
                }
                //cooling
                if (selection == 1)
                {
                    if (selection != spawners[id].selectionBefore)
                    {
                        spawners[id].elementText.UpdateTextFull("Cool", textPos, 500, 75, 0.5f, 0f, 64, 1);
                        spawners[id].elementText.color = new Vector3(0, 100 / 255f, 1f);
                        SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].color = new Vector3(0, 100 / 255f, 1f);
                        spawners[id].selectionBefore = selection;
                    }
                }
            }
            
            spawners[id].elementText.UpdateText(textPos, 500, 75, 0.5f, 0f);
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].position = BoxPos;
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].position.Y *= -1;
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].scaleX = scaleXInner;
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].scaleY = scaleYInner;
            
        }


    }
}
