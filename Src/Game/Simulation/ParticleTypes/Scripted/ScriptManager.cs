using System;
using System.Collections.Generic;
using System.Numerics;
using NLua;
using SpatialEngine;

namespace SpatialGame
{
    public static class ScriptManager
    {
        public static Lua lua;
        public const int reservedSpots = 31;
        static List<Tuple<LuaFunction, string>> scripts;
        
        public static void Init()
        {
            scripts = new List<Tuple<LuaFunction, string>>();
            lua = new Lua();

            //lua.LoadCLRPackage();
            
            for (int i = 0; i < ParticleResourceHandler.particleScripts.Length; i++)
            {
                string file = ParticleResourceHandler.particleScripts[i];
                try
                {
                    scripts.Add(new Tuple<LuaFunction, string>(lua.LoadFile(file), file));
                }
                catch (Exception e)
                {
                    Debugging.LogErrorConsole("Tried to load script " + file + " " + e);
                }

                try
                {
                    //add functions
                    //lua.DoString($"import('{typeof(ParticleSimulation).Namespace}')");
                    //lua.DoString($"import('{typeof(Vector2).Namespace}')");
                    //lua.RegisterFunction(file, typeof(ParticleSimulation).GetMethod("SafePositionCheckSet"));
                }
                catch (Exception e)
                {
                    Debugging.LogErrorConsole("Tried to load functions " + file + " " + e);
                }
            }
            
        }

        public static void Update()
        {
            
        }

        /// <summary>
        /// Will run the script at the index -31, as the first 31 defined behaviors are reserved
        /// </summary>
        public static void RunScript(ref Particle particle)
        {
            int index;
            if (!ParticleResourceHandler.particleBehaveIndexes.TryGetValue(particle.state.behaveType, out int scriptIndex))
            {
                return;
            }
            index = scriptIndex;
            if(index >= scripts.Count)
                return;
            
            lua["particle"] = particle;
            LuaFunction script = scripts[index].Item1;
            try
            {
                script.Call();
                //lua.DoFile(ParticleResourceHandler.particleScripts[index]);
                //scripts[index] = new Tuple<LuaFunction, string>(
                //    lua.LoadFile(ParticleResourceHandler.particleScripts[index]),
                //    ParticleResourceHandler.particleScripts[index]);
            }
            catch (Exception e)
            {
                Debugging.LogErrorConsole("Tried to run script " + scripts[index].Item2 + " " + e);
            }
        }
    }
}