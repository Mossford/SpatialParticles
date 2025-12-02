using System.Numerics;
using SpatialEngine.Rendering;

namespace SpatialGame.Menus
{
    public static class OptionMenu
    {
        public static bool hide;

        public static UiText enableHeatSim;
        public static UiText enablePerfTest;
        public static UiText enableParticleLight;
        public static UiText enableDarkLight;
        public static UiText particleLightRange;
        public static UiText enableGpuCompLight;
        public static UiText enableMultiThread;
        public static UiText simulationSpeed;

        public static UiImage textBackground;

        public static void Init()
        {
            hide = true;

            enableHeatSim = new UiText("Heat Simulation", new Vector2(-550, 400), 0.75f, 0.0f);
            enablePerfTest = new UiText("Performance Test", new Vector2(-550, 300), 0.75f, 0.0f);
            enableParticleLight = new UiText("Particle Lighting", new Vector2(-550, 200), 0.75f, 0.0f);
            enableDarkLight = new UiText("Dark Particle Lighting", new Vector2(-550, 100), 0.75f, 0.0f);
            particleLightRange = new UiText("Particle Lighting Range", new Vector2(-550, 0), 0.75f, 0.0f);
            enableGpuCompLight = new UiText("Gpu Lighting", new Vector2(-550, -100), 0.75f, 0.0f);
            enableMultiThread = new UiText("MultiThreading", new Vector2(-550, -200), 0.75f, 0.0f);
            simulationSpeed = new UiText("Simulation Speed", new Vector2(-550, -300), 0.75f, 0.0f);
            //textBackground = new UiImage(new Vector2(-550, 0), 375, 500, new Vector4(50, 50, 50, 100));
            
            enableHeatSim.SetHide(hide);
            enablePerfTest.SetHide(hide);
            enableParticleLight.SetHide(hide);
            enableDarkLight.SetHide(hide);
            particleLightRange.SetHide(hide);
            enableGpuCompLight.SetHide(hide);
            enableMultiThread.SetHide(hide);
            simulationSpeed.SetHide(hide);
        }

        public static void Update()
        {
            enableHeatSim.SetHide(hide);
            enablePerfTest.SetHide(hide);
            enableParticleLight.SetHide(hide);
            enableDarkLight.SetHide(hide);
            particleLightRange.SetHide(hide);
            enableGpuCompLight.SetHide(hide);
            enableMultiThread.SetHide(hide);
            simulationSpeed.SetHide(hide);
            //textBackground.SetHide(hide);
        }
    }
}