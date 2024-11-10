using System;
using System.Numerics;

//engine stuff
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using SpatialEngine.Rendering;
using static SpatialEngine.Globals;
using SpatialGame;

namespace SpatialEngine
{
    public class Player
    {
        public Vector2 position;
        int id;

        public Player(Vector2 position)
        {
            this.position = position;
        }

        public void UpdatePlayer(float delta)
        {
            
        }
    }
}