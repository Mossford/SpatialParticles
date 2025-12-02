using System;
using System.Collections.Generic;
using System.Numerics;
using SpatialEngine;
using SpatialEngine.Rendering;

namespace SpatialEngine.Rendering
{
    public static class UiRenderer
    {
        public static Shader uiTextShader;
        public static Shader uiImageShader;
        public static List<UiElement> uiElements;
        //will reuse this quad for all elements
        static UiQuad quad;

        public static void Init()
        {
            uiTextShader = new Shader(Globals.gl, "UiText.vert", "UiText.frag");
            uiImageShader = new Shader(Globals.gl, "UiImage.vert", "UiImage.frag");

            quad = new UiQuad();
            quad.Bind();

            uiElements = new List<UiElement>();
            Mouse.uiWantMouse = false;
        }

        public static void AddElement(UiElement element)
        {
            element.index = 0;
            for (int i = 0; i < uiElements.Count; i++)
            {
                if (element.layer > uiElements[i].layer)
                {
                    element.index = i + 1;
                }
                if (element.layer <= uiElements[i].layer)
                {
                    element.index = i;
                    break;
                }
            }
            
            uiElements.Insert(element.index, element);
        }

        public static void DeleteElement(int index)
        {
            uiElements.RemoveAt(index);
        }

        public static void Update()
        {
            for (int i = 0; i < uiElements.Count; i++)
            {
                uiElements[i].Update();
            }
        }
        
        public static void Draw()
        {
            for (int i = 0; i < uiElements.Count; i++)
            {
                if(!uiElements[i].hide)
                    uiElements[i].Draw(quad);
            }
        }

        public static void Cleanup()
        {
            for (int i = 0; i < uiElements.Count; i++)
            {
                uiElements[i].Dispose();
            }
            uiElements.Clear();
            quad.Dispose();
            uiTextShader.Dispose();
            uiImageShader.Dispose();
        }
    }
}
