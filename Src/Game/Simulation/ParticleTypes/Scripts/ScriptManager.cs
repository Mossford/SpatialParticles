using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using KeraLua;
using SpatialEngine;
using Lua = NLua.Lua;
using LuaFunction = NLua.LuaFunction;

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

            lua.LoadCLRPackage();
            
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
                    //not sure if this fully exposes System.Numerics but I can only get Vector2 to be accessed in testing so
                    RegisterLuaType(typeof(Vector2));
                    RegisterLuaType(typeof(Particle));
                    RegisterLuaType(typeof(ParticleState));
                    
                    lua.RegisterFunction("AddParticle", typeof(ParticleSimulation).GetMethod("AddParticle"));
                    lua.RegisterFunction("GetPropertiesFromName", typeof(ParticleSimulation).GetMethod("GetPropertiesFromName"));
                    lua.RegisterFunction("SafePositionCheckSet", typeof(LuaPass.LuaPassFunctions).GetMethod("SafePositionCheckSet"));
                    lua.RegisterFunction("SafeIdCheckSet", typeof(LuaPass.LuaPassFunctions).GetMethod("SafeIdCheckSet"));
                    lua.RegisterFunction("SafePositionCheckGet", typeof(LuaPass.LuaPassFunctions).GetMethod("SafePositionCheckGet"));
                    lua.RegisterFunction("SafeIdCheckGet", typeof(LuaPass.LuaPassFunctions).GetMethod("SafeIdCheckGet"));
                    lua.RegisterFunction("SafeChunkIdCheckGet", typeof(LuaPass.LuaPassFunctions).GetMethod("SafeChunkIdCheckGet"));
                    lua.RegisterFunction("ModifyParticle", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticle"));
                    lua.RegisterFunction("ModifyParticleState", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticleState"));
                    lua.RegisterFunction("ModifyParticlePosition", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticlePosition"));
                    lua.RegisterFunction("ModifyParticleVelocity", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticleVelocity"));
                    lua.RegisterFunction("ModifyParticleMoveType", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticleMoveType"));
                    lua.RegisterFunction("ModifyParticleBehaveType", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticleBehaveType"));
                    lua.RegisterFunction("ModifyParticleTemperature", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticleTemperature"));
                    lua.RegisterFunction("ModifyParticleColor", typeof(LuaPass.LuaPassFunctions).GetMethod("ModifyParticleColor"));
                    
                }
                catch (Exception e)
                {
                    Debugging.LogErrorConsole("Tried to load functions " + file + " " + e);
                }
            }
            
        }

        public static void Update()
        {
            lua.State.GarbageCollector(LuaGC.Collect, 0);
        }

        /// <summary>
        /// Will run the script at the index -31, as the first 31 defined behaviors are reserved
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
            }
            catch (Exception e)
            {
                Debugging.LogErrorConsole("Tried to run script " + scripts[index].Item2 + " " + e);
            }
        }
        
        static void RegisterLuaType(Type type)
        {
            lua.DoString($"luanet.load_assembly('{type.Assembly}')");
            lua.DoString($"{type.Name} = luanet.import_type('{type.FullName}')");
        }

    }
}