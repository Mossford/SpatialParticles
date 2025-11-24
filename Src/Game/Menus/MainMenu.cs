using System.Numerics;
using SpatialEngine.Rendering;

namespace SpatialGame.Menus
{
    public static class MainMenu
    {

        public static UiText title;
        
        public static void Init()
        {
            title = new UiText("Spatial Particles", new Vector2(0, 400f), 1.0f, 0.0f);
        }

        public static void Update()
        {
            
        }
    }
}