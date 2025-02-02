using System.Collections.Generic;
using System.Numerics;
using SpatialEngine;
using SpatialEngine.Rendering;

namespace SpatialEngine.Rendering
{
    public static class UiRenderer
    {
        static Shader uiTextShader;
        static Shader uiImageShader;
        public static List<UiElement> uiElements;
        public static List<Button> buttons;
        //will reuse this quad for all elements
        static UiQuad quad;

        public static void Init()
        {
            uiTextShader = new Shader(Globals.gl, "UiText.vert", "UiText.frag");
            uiImageShader = new Shader(Globals.gl, "UiImage.vert", "UiImage.frag");

            quad = new UiQuad();
            quad.Bind();

            uiElements = new List<UiElement>();
            buttons = new List<Button>();
        }

        public static void AddElement(Texture texture, Vector2 pos, float rotation, float scale, Vector2 dimension, UiElementType type)
        {
            uiElements.Add(new UiElement(texture, pos, rotation, scale, dimension.X, dimension.Y, type));
        }

        public static void DeleteElement(int index)
        {
            uiElements.RemoveAt(index);
        }

        public static void Update()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Update();
            }
            
            for (int i = 0; i < uiElements.Count; i++)
            {
                uiElements[i].Update();
            }
        }
        
        public static void Draw()
        {
            for (int i = 0; i < uiElements.Count; i++)
            {
                switch(uiElements[i].type)
                {
                    default:
                        quad.Draw(in uiImageShader, in uiElements[i].matrix, in uiElements[i].texture, uiElements[i].color);
                        break;
                    case UiElementType.image:
                        quad.Draw(in uiImageShader, in uiElements[i].matrix, in uiElements[i].texture, uiElements[i].color);
                        break;
                    case UiElementType.text:
                        quad.Draw(in uiTextShader, in uiElements[i].matrix, in uiElements[i].texture, uiElements[i].color);
                        break;
                }
            }
        }
    }
}
