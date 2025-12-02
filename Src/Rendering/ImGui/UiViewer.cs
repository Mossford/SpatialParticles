using System;
using ImGuiNET;
using System.Numerics;
using SpatialEngine;
using SpatialEngine.Rendering;

namespace Spatialparticles.Rendering.ImGUI
{
    public static class UiViewer
    {
        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Ui");

            if (ImGui.TreeNode("Ui Elements"))
            {
                for (int i = 0; i < UiRenderer.uiElements.Count; i++)
                {
                    UiElement element = UiRenderer.uiElements[i];
                    Type elementType = element.GetType();
                    
                    ImGui.Separator();

                    if (ImGui.TreeNode(i, $"Element {i} {elementType.Name}"))
                    {
                        switch (element)
                        {
                            case UiButton button:
                            {
                                if (ImGui.InputText("", ref button.text.text, 255))
                                {
                                    button.text.UpdateText(button.text.text, button.position, button.scale, button.rotation);
                                }
                                ImGui.Text($"Method: {button.onClick.Method.Name}");
                                if (ImGui.CollapsingHeader("Background Color"))
                                {
                                    button.color /= 255.0f;
                                    ImGui.ColorPicker4($"Background Color", ref button.color);
                                    button.color *= 255.0f;
                                }

                                if (ImGui.CollapsingHeader("HighLight Color"))
                                {
                                    button.highLightColor /= 255.0f;
                                    ImGui.ColorPicker4($"HighLight Color", ref button.highLightColor);
                                    button.highLightColor *= 255.0f;
                                }

                                if (ImGui.CollapsingHeader("Click Color"))
                                {
                                    button.clickColor /= 255.0f;
                                    ImGui.ColorPicker4($"Click Color", ref button.clickColor);
                                    button.clickColor *= 255.0f;
                                }

                                DrawForElement(ref element);

                                break;
                            }
                            case UiImage image:
                            {
                                if (ImGui.CollapsingHeader("Color"))
                                {
                                    image.color /= 255.0f;
                                    ImGui.ColorPicker4($"Background Color", ref image.color);
                                    image.color *= 255.0f;
                                }
                                
                                DrawForElement(ref element);
                                break;
                            }
                            case UiText text:
                            {
                                if (ImGui.InputText("", ref text.text, 255))
                                {
                                    text.UpdateText(text.text, text.position, text.scale, text.rotation);
                                }
                                if (ImGui.CollapsingHeader("Color"))
                                {
                                    text.color /= 255.0f;
                                    ImGui.ColorPicker4($"Background Color", ref text.color);
                                    text.color *= 255.0f;
                                }
                                
                                DrawForElement(ref element);
                                break;
                            }
                        }
                        
                        ImGui.TreePop();
                    }
                    
                }
                ImGui.TreePop();
            }
            
            ImGui.End();
        }

        static void DrawForElement(ref UiElement element)
        {
            ImGui.DragFloat2("Position", ref element.position);
            ImGui.DragFloat("Rotation", ref element.rotation);
            ImGui.DragFloat("Scale", ref element.scale, 0.01f);
            ImGui.DragFloat("Width", ref element.width);
            ImGui.DragFloat("Height", ref element.height);
            ImGui.InputInt("Layer", ref element.layer);
        }
    }
}