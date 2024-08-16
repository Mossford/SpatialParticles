using SpatialEngine;
using SpatialEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SpatialEngine.Globals;

namespace SpatialGame
{
    //Writing a 6th custom renderer for a specific case that for some reason I do not
    // have a general renderer for
    public static class SimRenderer
    {
        public static List<SimMesh> meshes;
        static Shader shader;

        public static void Init()
        {
            meshes = new List<SimMesh>();
            shader = new Shader(Globals.gl, "SimDefault.vert", "SimDefault.frag");
        }

        /// <summary>
        /// Creates or overwrites meshes
        /// </summary>
        public static void Update()
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].Bind();
            }
        }

        public static void Render()
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                gl.UseProgram(shader.shader);
                meshes[i].Draw();
            }
        }
    }
}
