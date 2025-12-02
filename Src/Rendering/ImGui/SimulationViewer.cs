using System;
using System.Numerics;
using ImGuiNET;
using SpatialGame;

namespace SpatialEngine.Rendering.ImGUI
{
    public static class SimulationViewer
    {
        static Particle emptyParticle = new Particle();
        public static void Draw()
        {
            ImGui.Begin("Simulation");
            ImGui.TextWrapped("Particle Info at Mouse Pos:");
            //Vector2 position = ((Mouse.localPosition / Window.size * new Vector2(PixelColorer.width, PixelColorer.height)) + (new Vector2(PixelColorer.width, PixelColorer.height) / 2));
            Vector2 position = Mouse.position * new Vector2(PixelColorer.width, PixelColorer.height) / Window.size;
            if(!PixelColorer.BoundCheck(position))
                return;
            
            ImGui.TextWrapped($"Pos {position:N1}");
            ImGui.TextWrapped($"Chunk Pos {(position / ParticleChunkManager.chunkSize):N1}");
            ImGui.TextWrapped($"Particle Index {PixelColorer.PosToIndex(position):N0}");
            ImGui.TextWrapped($"Chunk Index {ParticleChunkManager.SafeGetChunkIndexMap(position):N0}");
            ImGui.TextWrapped($"Chunk Particle Index {ParticleChunkManager.SafeGetIndexInChunksMap(position)}");
            ImGui.TextWrapped($"IdCheck {ParticleSimulation.SafeIdCheckGet(position):N0}");
            ref ParticleChunk testChunk = ref ParticleChunkManager.GetChunkReference(position);
            ImGui.TextWrapped($"Chunk Particle Count {testChunk.particleCount:N0}");
            ImGui.TextWrapped($"In chunk {testChunk.ChunkBounds(position)}");
            Vector2 floorPosition = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
            Vector4Byte color = PixelColorer.GetColorAtPos(floorPosition);
            ImGui.TextWrapped($"Color {color}");
            Vector2 textSize = ImGui.CalcTextSize($"Color {color}");
            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(cursorPos.X + textSize.X + 5, cursorPos.Y - 2), 
                new Vector2(cursorPos.X + textSize.X + 25, cursorPos.Y - textSize.Y - 3), 
                ImGui.ColorConvertFloat4ToU32((Vector4)color / 255f));
            Vector4Byte light = PixelColorer.particleLights[PixelColorer.PosToIndex(floorPosition)].color * PixelColorer.particleLights[PixelColorer.PosToIndex(floorPosition)].intensity;
            ImGui.TextWrapped($"Light {light}");
            textSize = ImGui.CalcTextSize($"Light {light}");
            cursorPos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(cursorPos.X + textSize.X + 5, cursorPos.Y - 2), 
                new Vector2(cursorPos.X + textSize.X + 25, cursorPos.Y - textSize.Y - 3), 
                ImGui.ColorConvertFloat4ToU32((Vector4)light / 255f));
            string particleName;
            if (ImGui.CollapsingHeader("Particle State"))
            {
                ChunkIndex idToCheck = ParticleSimulation.SafeChunkIdCheckGet(position);
                if (idToCheck.particleIndex != -1)
                {
                    ref ParticleChunk chunk = ref ParticleChunkManager.GetChunkReference(idToCheck.chunkIndex);
                    ImGui.TextWrapped($"{chunk.particles[idToCheck.particleIndex]}");
                }
                else
                {
                    ImGui.TextWrapped($"{emptyParticle}");
                }
            }
            if (ImGui.CollapsingHeader("Particle Properties"))
            {
                ChunkIndex idToCheck = ParticleSimulation.SafeChunkIdCheckGet(position);
                if (idToCheck.particleIndex != -1)
                {
                    ref ParticleChunk chunk = ref ParticleChunkManager.GetChunkReference(idToCheck.chunkIndex);
                    ImGui.TextWrapped($"{chunk.particles[idToCheck.particleIndex].GetParticleProperties()}");
                }
                else
                {
                    ImGui.TextWrapped($"{emptyParticle.GetParticleProperties()}");
                }
            }
            ImGui.End();
        }
    }
}