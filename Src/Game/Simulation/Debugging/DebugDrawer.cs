using System.Numerics;
using SpatialEngine;

namespace SpatialGame
{
    public static class DebugDrawer
    {
        static Vector2 TransformPoint(Vector2 point)
        {
            return ((point / new Vector2(PixelColorer.width, PixelColorer.height)) * (Vector2)Globals.window.Size) - (Vector2)Globals.window.Size / 2;
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
    }
}
