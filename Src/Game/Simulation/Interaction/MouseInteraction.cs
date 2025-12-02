using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Numerics;
using SpatialEngine.Rendering;

namespace SpatialGame
{
    public static class MouseInteraction
    {
        class Spawner
        {
            public int idCircleMesh;
            public int idElementSquareMesh;
            public int idElementSqaureInnerMesh;
            public UiText elementText;
            public string nameBefore;
            public int selectionBefore;
            public bool modeBefore;

            public Spawner()
            {
                idCircleMesh = -1;
                idElementSquareMesh = -1;
                idElementSqaureInnerMesh = -1;
                elementText = new UiText();
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
            positionMouse = positionMouse + (Window.size / 2);
            oldPositionMouse = oldPositionMouse + (Window.size / 2);
            
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
                        ChunkIndex idToCheck = ParticleSimulation.SafeChunkIdCheckGet(pos);
                        
                        if(idToCheck.chunkIndex == -1)
                            continue;
                        
                        ref ParticleChunk chunk = ref ParticleChunkManager.GetChunkReference(idToCheck.chunkIndex);

                        if(button == 0 && !mode)
                        {
                            if (idToCheck.particleIndex == -1)
                            {
                                ParticleSimulation.AddParticle(pos, name);
                            }
                            else if (chunk.particles[idToCheck.particleIndex].propertyIndex != ParticleResourceHandler.particleNameIndexes[name])
                            {
                                //replaced from queue delete may cause issues
                                chunk.particles[idToCheck.particleIndex].Delete();
                                ParticleSimulation.AddParticle(pos, name);
                            }
                        }
                        else if (button == 0 && mode)
                        {
                            if(idToCheck.particleIndex != -1 && selection == 0)
                            {
                                chunk.particles[idToCheck.particleIndex].state.temperature += Globals.fixedDeltaTime;
                            }
                            if (idToCheck.particleIndex != -1 && selection == 1)
                            {
                                chunk.particles[idToCheck.particleIndex].state.temperature -=  Globals.fixedDeltaTime;
                            }
                        }
                        else if(button == 1)
                        {
                            if (idToCheck.particleIndex != -1)
                            {
                                chunk.particles[idToCheck.particleIndex].Delete();
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
                            
                            ChunkIndex idToCheck = ParticleSimulation.SafeChunkIdCheckGet(pos);
                        
                            if(idToCheck.chunkIndex == -1)
                                continue;

                            ref ParticleChunk chunk = ref ParticleChunkManager.GetChunkReference(idToCheck.chunkIndex);

                            if(button == 0 && !mode)
                            {
                                if (idToCheck.particleIndex == -1)
                                {
                                    ParticleSimulation.AddParticle(pos, name);
                                }
                                else if (chunk.particles[idToCheck.particleIndex].propertyIndex != ParticleResourceHandler.particleNameIndexes[name])
                                {
                                    //replaced from queue delete may cause issues
                                    chunk.particles[idToCheck.particleIndex].Delete();
                                    ParticleSimulation.AddParticle(pos, name);
                                }
                            }
                            else if (button == 0 && mode)
                            {
                                if(idToCheck.particleIndex != -1 && selection == 0)
                                {
                                    chunk.particles[idToCheck.particleIndex].state.temperature += Globals.fixedDeltaTime;
                                }
                                if (idToCheck.particleIndex != -1 && selection == 1)
                                {
                                    chunk.particles[idToCheck.particleIndex].state.temperature -= Globals.fixedDeltaTime;
                                }
                            }
                            else if(button == 1)
                            {
                                if (idToCheck.particleIndex != -1)
                                {
                                    chunk.particles[idToCheck.particleIndex].Delete();
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
            Vector2 textPos = Vector2.Zero;
            Vector2 BoxPos = positionMouse + new Vector2(1 * boxDist, -1 * boxDist);

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
                
                spawners[id].elementText.CreateText("text", textPos, 1f, 0f);
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
                if (spawners[id].nameBefore != name || spawners[id].modeBefore != mode)
                {
                    Vector3 color = (Vector3)Particle.GetParticleColor(name);
                    spawners[id].elementText.UpdateText(name, textPos,1.0f, 0f);
                    spawners[id].elementText.color = new Vector4(color, 255);
                    SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].color = color;
                    spawners[id].nameBefore = name;
                    spawners[id].modeBefore = mode;
                }
            }
            //function mode
            else
            {
                //heating
                if (selection == 0)
                {
                    if (selection != spawners[id].selectionBefore || spawners[id].modeBefore != mode)
                    {
                        spawners[id].elementText.UpdateText("Heat", textPos,1.0f, 0f);
                        spawners[id].elementText.color = new Vector4(240, 70, 0, 255);
                        SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].color = new Vector3(240, 70, 0);
                        spawners[id].selectionBefore = selection;
                        spawners[id].modeBefore = mode;
                    }
                }
                //cooling
                if (selection == 1)
                {
                    if (selection != spawners[id].selectionBefore || spawners[id].modeBefore != mode)
                    {
                        spawners[id].elementText.UpdateText("Cool", textPos, 1.0f, 0f);
                        spawners[id].elementText.color = new Vector4(0, 70, 240, 255);
                        SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].color = new Vector3(0, 70, 240);
                        spawners[id].selectionBefore = selection;
                        spawners[id].modeBefore = mode;
                    }
                }
            }
            
            textPos = positionMouse + new Vector2(2 * scaleX + (spawners[id].elementText.width * Window.scaleFromBase.X), -1 * boxDist);
            if (textPos.X + spawners[id].elementText.width > Window.size.X / 2f)
            {
                textPos = positionMouse + new Vector2(-2 * scaleX + (spawners[id].elementText.width * Window.scaleFromBase.X), -1 * boxDist);
            }
            textPos.Y *= -1;
            
            spawners[id].elementText.UpdateText(spawners[id].elementText.text, textPos / Window.scaleFromBase, 1.0f, 0f);
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].position = BoxPos;
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].position.Y *= -1;
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].scaleX = scaleXInner;
            SimRenderer.meshes[spawners[id].idElementSqaureInnerMesh].scaleY = scaleYInner;
            
        }


    }
}
