using System;
using System.Numerics;
using SpatialEngine;
using SpatialEngine.Rendering;

namespace SpatialGame
{
    public static class OptionMenu
    {
        public static bool hide;

        public static UiText enableHeatSim;
        public static UiButton enableHeatSimButton;
        public static UiText enablePerfTest;
        public static UiButton enablePerfTestButton;
        public static UiText enableParticleLight;
        public static UiButton enableParticleLightButton;
        public static UiText enableDarkLight;
        public static UiButton enableDarkLightButton;
        public static UiText particleLightRange;
        public static UiText enableGpuCompLight;
        public static UiButton enableGpuCompLightButton;
        public static UiText enableMultiThread;
        public static UiButton enableMultiThreadButton;
        public static UiText simulationSpeed;
        
        public static UiText controlsR;
        public static UiText controlsT;
        public static UiText controlsG;
        public static UiText controlsEsc;
        public static UiText controlsShift;
        public static UiText controlsLMouse;
        public static UiText controlsCtrl;
        public static UiText controlsMScroll;

        public static UiButton backButton;
        

        public static void Init()
        {
            enableHeatSim = new UiText("Heat Simulation", new Vector2(-950, 400), 0.75f, 0.0f);
            Vector4 color = Settings.SimulationSettings.EnableHeatSimulation ? new Vector4(50, 255, 50, 255) :  new Vector4(255, 50, 50, 255);
            enableHeatSimButton = new UiButton(new Vector2(enableHeatSim.position.X + enableHeatSim.width - 30, 400),
                new Vector2(30, 30),
                RunEnableHeatSim, "", color, 0.5f, 0.1f, UiAlignment.Left);
            enablePerfTest = new UiText("Performance Test", new Vector2(-950, 300), 0.75f, 0.0f);
            color = Settings.SimulationSettings.EnablePerfTestMode ? new Vector4(50, 255, 50, 255) :  new Vector4(255, 50, 50, 255);
            enablePerfTestButton = new UiButton(new Vector2(enablePerfTest.position.X + enablePerfTest.width - 30, 300),
                new Vector2(30, 30),
                RunEnablePerfTest, "", color, 0.5f, 0.1f, UiAlignment.Left);
            enableParticleLight = new UiText("Particle Lighting", new Vector2(-950, 200), 0.75f, 0.0f);
            color = Settings.SimulationSettings.EnableParticleLighting ? new Vector4(50, 255, 50, 255) :  new Vector4(255, 50, 50, 255);
            enableParticleLightButton = new UiButton(new Vector2(enableParticleLight.position.X + enableParticleLight.width - 30, 200),
                new Vector2(30, 30),
                RunEnableParticleLight, "", color, 0.5f, 0.1f, UiAlignment.Left);
            enableDarkLight = new UiText("Dark Particle Lighting", new Vector2(-950, 100), 0.75f, 0.0f);
            color = Settings.SimulationSettings.EnableDarkLighting ? new Vector4(50, 255, 50, 255) :  new Vector4(255, 50, 50, 255);
            enableDarkLightButton = new UiButton(new Vector2(enableDarkLight.position.X + enableDarkLight.width - 30, 100),
                new Vector2(30, 30),
                RunDarkLight, "", color, 0.5f, 0.1f, UiAlignment.Left);
            particleLightRange = new UiText("Particle Lighting Range", new Vector2(-950, 0), 0.75f, 0.0f);
            enableGpuCompLight = new UiText("Gpu Lighting", new Vector2(-950, -100), 0.75f, 0.0f);
            color = Settings.SimulationSettings.EnableGpuCompLighting ? new Vector4(50, 255, 50, 255) :  new Vector4(255, 50, 50, 255);
            enableGpuCompLightButton = new UiButton(new Vector2(enableGpuCompLight.position.X + enableGpuCompLight.width - 30,-100),
                new Vector2(30, 30),
                RunGpuCompLight, "", color, 0.5f, 0.1f, UiAlignment.Left);
            enableMultiThread = new UiText("MultiThreading", new Vector2(-950, -200), 0.75f, 0.0f);
            color = Settings.SimulationSettings.EnableMultiThreading ? new Vector4(50, 255, 50, 255) :  new Vector4(255, 50, 50, 255);
            enableMultiThreadButton = new UiButton(new Vector2(enableMultiThread.position.X + enableMultiThread.width - 30, -200),
                new Vector2(30, 30),
                RunMultiThread, "", color, 0.5f, 0.1f, UiAlignment.Left);
            simulationSpeed = new UiText("Simulation Speed", new Vector2(-950, -300), 0.75f, 0.0f);
            //textBackground = new UiImage(new Vector2(-550, 0), 375, 500, new Vector4(50, 50, 50, 100));
            
            controlsR = new UiText("R - Reset Sim", new Vector2(950, 250f), 0.7f, 0.0f, UiAlignment.Right);
            controlsT = new UiText("T - Resize Smaller", new Vector2(950, 200f), 0.7f, 0.0f, UiAlignment.Right);
            controlsG = new UiText("G - Resize Bigger", new Vector2(950, 150f), 0.7f, 0.0f, UiAlignment.Right);
            controlsEsc = new UiText("Escape - Pause", new Vector2(950, 100f), 0.7f, 0.0f, UiAlignment.Right);
            controlsShift = new UiText("LShift + MScroll - Switch Particle", new Vector2(950, 50f), 0.7f, 0.0f, UiAlignment.Right);
            controlsCtrl = new UiText("LCtrl + MScroll - Switch Mode", new Vector2(950, 0f), 0.7f, 0.0f, UiAlignment.Right);
            controlsLMouse = new UiText("LMouse - Spawn Particle", new Vector2(950, -50f), 0.7f, 0.0f, UiAlignment.Right);
            controlsMScroll = new UiText("MScroll - Spawner Radius", new Vector2(950, -100f), 0.7f, 0.0f, UiAlignment.Right);

            backButton = new UiButton(new Vector2(0, -400), 50, RunBack, "Back", new Vector4(50, 50, 50, 100));

            SetHide(true);
        }

        public static void SetHide(bool hide)
        {
            OptionMenu.hide = hide;
            enableHeatSim.SetHide(hide);
            enableHeatSimButton.SetHide(hide);
            enablePerfTest.SetHide(hide);
            enablePerfTestButton.SetHide(hide);
            enableParticleLight.SetHide(hide);
            enableParticleLightButton.SetHide(hide);
            enableDarkLight.SetHide(hide);
            enableDarkLightButton.SetHide(hide);
            particleLightRange.SetHide(hide);
            enableGpuCompLight.SetHide(hide);
            enableGpuCompLightButton.SetHide(hide);
            enableMultiThread.SetHide(hide);
            enableMultiThreadButton.SetHide(hide);
            simulationSpeed.SetHide(hide);
            controlsR.SetHide(hide);
            controlsT.SetHide(hide);
            controlsG.SetHide(hide);
            controlsEsc.SetHide(hide);
            controlsShift.SetHide(hide);
            controlsCtrl.SetHide(hide);
            controlsLMouse.SetHide(hide);
            controlsMScroll.SetHide(hide);
            //textBackground.SetHide(hide);
            backButton.SetHide(hide);
            
            //reuse background from mainmenu
            MainMenu.background.SetHide(hide);
        }

        static void RunEnableHeatSim()
        {
            if (Settings.SimulationSettings.EnableHeatSimulation)
            {
                Vector4 color = new Vector4(255, 50, 50, 255);
                enableHeatSimButton.color = color;
                enableHeatSimButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableHeatSimButton.clickColor = new Vector4(enableHeatSimButton.highLightColor.X * 0.1f,
                    enableHeatSimButton.highLightColor.Y * 0.1f,
                    enableHeatSimButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableHeatSimulation = false;
            }
            else
            {
                Vector4 color = new Vector4(50, 255, 50, 255);
                enableHeatSimButton.color = color;
                enableHeatSimButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableHeatSimButton.clickColor = new Vector4(enableHeatSimButton.highLightColor.X * 0.1f,
                    enableHeatSimButton.highLightColor.Y * 0.1f,
                    enableHeatSimButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableHeatSimulation = true;
            }
        }

        static void RunEnablePerfTest()
        {
            if (Settings.SimulationSettings.EnablePerfTestMode)
            {
                Vector4 color = new Vector4(255, 50, 50, 255);
                enablePerfTestButton.color = color;
                enablePerfTestButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enablePerfTestButton.clickColor = new Vector4(enablePerfTestButton.highLightColor.X * 0.1f,
                    enablePerfTestButton.highLightColor.Y * 0.1f,
                    enablePerfTestButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnablePerfTestMode = false;
            }
            else
            {
                Vector4 color = new Vector4(50, 255, 50, 255);
                enablePerfTestButton.color = color;
                enablePerfTestButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enablePerfTestButton.clickColor = new Vector4(enablePerfTestButton.highLightColor.X * 0.1f,
                    enablePerfTestButton.highLightColor.Y * 0.1f,
                    enablePerfTestButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnablePerfTestMode = true;
            }
        }

        static void RunEnableParticleLight()
        {
            if (Settings.SimulationSettings.EnableParticleLighting)
            {
                Vector4 color = new Vector4(255, 50, 50, 255);
                enableParticleLightButton.color = color;
                enableParticleLightButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableParticleLightButton.clickColor = new Vector4(enableParticleLightButton.highLightColor.X * 0.1f,
                    enableParticleLightButton.highLightColor.Y * 0.1f,
                    enableParticleLightButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableParticleLighting = false;
            }
            else
            {
                Vector4 color = new Vector4(50, 255, 50, 255);
                enableParticleLightButton.color = color;
                enableParticleLightButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableParticleLightButton.clickColor = new Vector4(enableParticleLightButton.highLightColor.X * 0.1f,
                    enableParticleLightButton.highLightColor.Y * 0.1f,
                    enableParticleLightButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableParticleLighting = true;
            }
        }

        static void RunDarkLight()
        {
            if (Settings.SimulationSettings.EnableDarkLighting)
            {
                Vector4 color = new Vector4(255, 50, 50, 255);
                enableDarkLightButton.color = color;
                enableDarkLightButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableDarkLightButton.clickColor = new Vector4(enableDarkLightButton.highLightColor.X * 0.1f,
                    enableDarkLightButton.highLightColor.Y * 0.1f,
                    enableDarkLightButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableDarkLighting = false;
            }
            else
            {
                Vector4 color = new Vector4(50, 255, 50, 255);
                enableDarkLightButton.color = color;
                enableDarkLightButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableDarkLightButton.clickColor = new Vector4(enableDarkLightButton.highLightColor.X * 0.1f,
                    enableDarkLightButton.highLightColor.Y * 0.1f,
                    enableDarkLightButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableDarkLighting = true;
            }
        }

        static void RunGpuCompLight()
        {
            if (Settings.SimulationSettings.EnableGpuCompLighting)
            {
                Vector4 color = new Vector4(255, 50, 50, 255);
                enableGpuCompLightButton.color = color;
                enableGpuCompLightButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableGpuCompLightButton.clickColor = new Vector4(enableGpuCompLightButton.highLightColor.X * 0.1f,
                    enableGpuCompLightButton.highLightColor.Y * 0.1f,
                    enableGpuCompLightButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableGpuCompLighting = false;
            }
            else
            {
                Vector4 color = new Vector4(50, 255, 50, 255);
                enableGpuCompLightButton.color = color;
                enableGpuCompLightButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableGpuCompLightButton.clickColor = new Vector4(enableGpuCompLightButton.highLightColor.X * 0.1f,
                    enableGpuCompLightButton.highLightColor.Y * 0.1f,
                    enableGpuCompLightButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableGpuCompLighting = true;
            }
        }

        static void RunMultiThread()
        {
            if (Settings.SimulationSettings.EnableMultiThreading)
            {
                Vector4 color = new Vector4(255, 50, 50, 255);
                enableMultiThreadButton.color = color;
                enableMultiThreadButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableMultiThreadButton.clickColor = new Vector4(enableMultiThreadButton.highLightColor.X * 0.1f,
                    enableMultiThreadButton.highLightColor.Y * 0.1f,
                    enableMultiThreadButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableMultiThreading = false;
            }
            else
            {
                Vector4 color = new Vector4(50, 255, 50, 255);
                enableMultiThreadButton.color = color;
                enableMultiThreadButton.highLightColor = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
                enableMultiThreadButton.clickColor = new Vector4(enableMultiThreadButton.highLightColor.X * 0.1f,
                    enableMultiThreadButton.highLightColor.Y * 0.1f,
                    enableMultiThreadButton.highLightColor.Z * 0.1f, color.W);
                Settings.SimulationSettings.EnableMultiThreading = true;
            }
        }

        static void RunBack()
        {
            SetHide(true);
            MainMenu.SetHide(false);
        }
    }
}