using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

//engine stuff
using static SpatialEngine.Globals;
using SpatialEngine.Rendering;
using System.IO;
using System.Linq;
using Shader = SpatialEngine.Rendering.Shader;

namespace SpatialEngine
{
    public struct SpatialObject
    {
        public Mesh SO_mesh;
        public Shader SO_shader;
        public uint SO_id;

        public SpatialObject(Mesh mesh, uint id)
        {
            SO_mesh = mesh;
            SO_id = id;
        }

        public SpatialObject(Mesh mesh, Shader shader, uint id)
        {
            SO_shader = shader;
            SO_mesh = mesh;
            SO_id = id;
        }

        public SpatialObject(Mesh mesh, string vertPath, string fragPath, uint id)
        {
            SO_shader = new Shader(gl, vertPath, fragPath);
            SO_mesh = mesh;
            SO_id = id;
        }

        public uint GetSizeUsage()
        {
            uint total = 0;
            total += (uint)(8 * sizeof(float) * SO_mesh.vertexes.Length);
            total += (uint)(sizeof(uint) * SO_mesh.indices.Length);
            return total;
        }
    }

    public class Scene
    {

        public List<SpatialObject> SpatialObjects;
        public List<uint> idList;

        public Scene()
        {
            SpatialObjects = new List<SpatialObject>();
            idList = new List<uint>();
        }

        public void AddSpatialObject(Mesh mesh)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObjects.Add(new SpatialObject(mesh, (uint)id));
            idList.Add((uint)id);
        }

        public void Clear()
        {
            SpatialObjects.Clear();
            idList.Clear();
        }
    }
}