using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using SpatialEngine;

namespace SpatialGame
{
    public class PerformanceTester
    {
        public const int tests = 40;
        int currentTest;
        float currentDelta;
        int frameCount;
        string file;

        public double particleUpdateTime;
        public long particleUpdateTicks;

        public void Init()
        {
            Settings.SimulationSettings.EnablePerfTestMode = true;
            Settings.SimulationSettings.EnableHeatSimulation = true;
            Settings.SimulationSettings.EnableParticleLighting = true;
            Settings.SimulationSettings.EnableMultiThreading = false;

            file = Resources.SimPath + "Test: " + DateTime.Now.ToLongDateString() + ".csv";
            File.WriteAllText(file, "Test,Fps,ParticlePerms,ParticleperFrame\n");
            
            GameManager.paused = false;
                
            GameManager.changeResolution = true;
            PixelColorer.resSwitcherDir = 0;
            PixelColorer.resolutions[PixelColorer.resSwitcher] = new Vector2(16, 9);
            GameManager.ReInitGame();
        }
        
        public void UpdatePerformanceTester(float delta)
        {
            if(currentTest > tests)
                return;
            
            //start a new test after 20s
            currentDelta += delta;
            frameCount++;
            if (currentDelta >= 3)
            {
                RunTest();
            }
        }

        void RunTest()
        {
            GameManager.paused = false;
            currentTest++;
                
            GameManager.changeResolution = true;
            PixelColorer.resSwitcherDir = 0;
            PixelColorer.resolutions[PixelColorer.resSwitcher] = new Vector2(16, 9) * currentTest;
            GameManager.ReInitGame();

            double fps = frameCount / currentDelta;
            double particlems = (PixelColorer.width * PixelColorer.height / (particleUpdateTime / particleUpdateTicks));
            double particleframe =
                (PixelColorer.width * PixelColorer.height / (1.0f / fps * 1000.0f));

            File.AppendAllText(file, 
                $"{PixelColorer.resolutions[PixelColorer.resSwitcher].X}x{PixelColorer.resolutions[PixelColorer.resSwitcher].Y}," +
                $"{fps}," +
                $"{particlems}," +
                $"{particleframe}\n");
            
            frameCount = 0;
            currentDelta = 0f;
            particleUpdateTime = 0f;
            particleUpdateTicks = 0;
        }
    }
}