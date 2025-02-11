using System.Numerics;
using SpatialEngine;

namespace SpatialGame
{
    public static class DebugDrawer
    {
        public static Vector2 TransformPoint(Vector2 point)
        {
            return ((point / new Vector2(PixelColorer.width, PixelColorer.height)) * Window.size) - Window.size / 2;
        }
        
        public static void DrawLine(Vector2 start, Vector2 end, Vector3 color, bool flipY = true)
        {
            Vector3 a = new Vector3(TransformPoint(start), 0f);
            Vector3 b = new Vector3(TransformPoint(end), 0f);
            if (flipY)
            {
                a.Y *= -1;
                b.Y *= -1;
            }
            Debugging.DrawDebugLine(a, b, color);
        }

        public static void DrawSquare(Vector2 pos, float scale, Vector3 color, bool flipY = true)
        {
            Vector3 position = new Vector3(TransformPoint(pos), -scale);
            if (flipY)
            {
                position.Y *= -1;
            }
            
            Debugging.DrawDebugCube(position, scale, color);
        }
    }
}
