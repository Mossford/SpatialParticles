using Silk.NET.Input;
using SpatialEngine;
using SpatialGame.Menus;

namespace SpatialGame
{
    public class GameInteraction
    {
        public static void Update()
        {
            if (Input.IsKeyDown(Key.Escape) && !GameManager.paused)
            {
                MainMenu.hide = false;
                GameManager.paused = true;
            }
        }
    }
}