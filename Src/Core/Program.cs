using SpatialEngine.Networking;

//Custom Engine things
using static SpatialEngine.Resources;

namespace SpatialEngine
{
    public class Game
    {
        public static void Main(string[] args)
        {
            //init resources like resources paths
            InitResources();

            if (args.Length == 0)
            {
                Window.Init();
            }
            else if (args[0] == "server")
            {
                //handles all the initilization of scene and physics and netowrking for server
                //and starts running the update loop
                HeadlessServer.Init();
            }
            
            //wont reach after here until window closes
            NetworkManager.Cleanup();
            Window.Cleanup();
        }

        
    }
}
