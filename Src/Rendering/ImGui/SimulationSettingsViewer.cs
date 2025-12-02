using ImGuiNET;

namespace SpatialEngine.Rendering.ImGUI
{
    public static class SimulationSettingsViewer
    {
        public static void Draw()
        {
            ImGui.Begin("Simulation Settings");
            ImGui.Checkbox("Enable ParticleLighting", ref Settings.SimulationSettings.EnableParticleLighting);
            ImGui.Checkbox("Enable Gpu Light PreCalculation", ref Settings.SimulationSettings.EnableGpuCompLighting);
            ImGui.Checkbox("Enable Dark Lighting", ref Settings.SimulationSettings.EnableDarkLighting);
            ImGui.Checkbox("Enable Heat Simulation", ref Settings.SimulationSettings.EnableHeatSimulation);
            ImGui.Checkbox("Enable Multithreading", ref Settings.SimulationSettings.EnableMultiThreading);
            ImGui.TextWrapped("Particle Light Range");
            ImGui.PushItemWidth(ImGui.CalcTextSize("Particle Light Range").X);
            ImGui.InputInt("##particleLight", ref Settings.SimulationSettings.particleLightRange);
            ImGui.End();
        }
    }
}