using SpatialEngine;
using SpatialEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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
            shader = new Shader(gl, "SimDefault.vert", "SimDefault.frag");
        }

        /// <summary>
        /// Creates or overwrites meshes
        /// </summary>
        public static void UpdateMeshes()
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].Bind();
            }
        }

        public static void Update()
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].Update();
            }
        }

        public static void Render()
        {
            gl.UseProgram(shader.shader);
            for (int i = 0; i < meshes.Count; i++)
            {
                if (!meshes[i].show)
                    continue;
                shader.setMat4("model", meshes[i].model);
                //maybe correct size for projection?
                shader.setMat4("proj", Matrix4x4.CreateOrthographic(Globals.window.Size.X, Globals.window.Size.Y, -1, 1));
                shader.setVec3("color", meshes[i].color);
                meshes[i].Draw();
            }
        }

        public static void CleanUp()
        {
            shader.Dispose();
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].Dispose();
            }
        }
    }
}
