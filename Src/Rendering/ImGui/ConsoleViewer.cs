using ImGuiNET;
using System.Numerics;

namespace SpatialEngine.Rendering.ImGUI
{
    public static class ConsoleViewer
    {
        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Console");
            ImGui.BeginChild("Output", new Vector2(0, ImGui.GetWindowSize().Y * 0.9f), true, ImGuiWindowFlags.NoResize);
            for (int i = 0; i < Debugging.consoleText.Count; i++)
            {
                if (i == 0)
                {
                    if (Debugging.consoleText[i].Item2)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0, 0, 1.0f));
                        ImGui.TextWrapped("> [" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextWrapped("> [" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                    }
                }
                else
                {
                    if (Debugging.consoleText[i].Item2)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0, 0, 1.0f));
                        ImGui.TextWrapped("[" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextWrapped("[" + (Debugging.consoleText.Count - i) + "] " + Debugging.consoleText[i].Item1);
                    }
                }
            }
            ImGui.EndChild();
            ImGui.End();
        }
    }
}