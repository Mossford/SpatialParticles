using Silk.NET.Input;
using SpatialEngine;

namespace SpatialGame
{
    public class GameInteraction
    {
        public static void Update()
        {
            if (Input.IsKeyDown(Key.Escape) && !GameManager.paused)
            {
                MainMenu.SetHide(false);
                GameManager.paused = true;
            }
        }
    }
}