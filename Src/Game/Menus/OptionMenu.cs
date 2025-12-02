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
        
        public static UiText controls;
        public static UiText controlsR;
        public static UiText controlsT;
        public static UiText controlsG;
        public static UiText controlsEsc;
        public static UiText controlsShift;
        public static UiText controlsLMouse;
        public static UiText controlsCtrl;
        public static UiText controlsMScroll;

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
            
            controls = new UiText("Controls:", new Vector2(650, 300f), 0.7f, 0.0f);
            controlsR = new UiText("R - Reset Sim", new Vector2(650, 250f), 0.7f, 0.0f);
            controlsT = new UiText("T - Resize Smaller", new Vector2(650, 200f), 0.7f, 0.0f);
            controlsG = new UiText("G - Resize Bigger", new Vector2(650, 150f), 0.7f, 0.0f);
            controlsEsc = new UiText("Escape - Pause", new Vector2(650, 100f), 0.7f, 0.0f);
            controlsShift = new UiText("LShift + MScroll - Switch Particle", new Vector2(650, 50f), 0.7f, 0.0f);
            controlsCtrl = new UiText("LCtrl + MScroll - Switch Mode", new Vector2(650, 0f), 0.7f, 0.0f);
            controlsLMouse = new UiText("LMouse - Spawn Particle", new Vector2(650, -50f), 0.7f, 0.0f);
            controlsMScroll = new UiText("MScroll - Spawn Radius", new Vector2(650, -100f), 0.7f, 0.0f);
            
            enableHeatSim.SetHide(hide);
            enablePerfTest.SetHide(hide);
            enableParticleLight.SetHide(hide);
            enableDarkLight.SetHide(hide);
            particleLightRange.SetHide(hide);
            enableGpuCompLight.SetHide(hide);
            enableMultiThread.SetHide(hide);
            simulationSpeed.SetHide(hide);
            controls.SetHide(hide);
            controlsR.SetHide(hide);
            controlsT.SetHide(hide);
            controlsG.SetHide(hide);
            controlsEsc.SetHide(hide);
            controlsShift.SetHide(hide);
            controlsCtrl.SetHide(hide);
            controlsLMouse.SetHide(hide);
            controlsMScroll.SetHide(hide);
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
            controls.SetHide(hide);
            controlsR.SetHide(hide);
            controlsT.SetHide(hide);
            controlsG.SetHide(hide);
            controlsEsc.SetHide(hide);
            controlsShift.SetHide(hide);
            controlsCtrl.SetHide(hide);
            controlsLMouse.SetHide(hide);
            controlsMScroll.SetHide(hide);
            //textBackground.SetHide(hide);
        }
    }
}