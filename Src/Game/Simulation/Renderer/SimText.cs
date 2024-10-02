using SpatialEngine.Rendering;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StbTrueTypeSharp.StbTrueType;

namespace SpatialGame
{
    public static class SimText
    {
        static stbtt_fontinfo font;
        public static List<int> textRefs;
        public static Vector3 color;

        public unsafe static void Init()
        {
            textRefs = new List<int>();
            byte[] fontData = File.ReadAllBytes(SpatialEngine.Resources.FontPath + "JetBrainsMono-Regular.ttf");
            font = new stbtt_fontinfo();
            
            fixed (byte* fontDataPtr = fontData)
            {
                if (stbtt_InitFont(font, fontDataPtr, 0) == 0)
                {
                    Console.WriteLine("failed text");
                }
            }
        }

        //slow make better
        public unsafe static void CreateText(string text, Vector2 pos, int width, int height, float scaleImage, float rotation, int textHeight, int numLines)
        {
            int imageWidth = width;
            int imageHeight = height;

            float scale = stbtt_ScaleForPixelHeight(font, textHeight);

            height = textHeight * numLines;

            byte[] bitmap = new byte[width * height];

            int x = 0;

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(font, &ascent, &descent, &lineGap);

            ascent = (int)(ascent * scale);
            descent = (int)(descent * scale);
            fixed (byte* bitmapDataPtr = bitmap)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    /* how wide is this character */
                    int ax;
                    int lsb;
                    stbtt_GetCodepointHMetrics(font, text[i], &ax, &lsb);

                    if (x + (ax * scale) > width)
                        continue;
                    /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                    /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                    int c_x1, c_y1, c_x2, c_y2;
                    stbtt_GetCodepointBitmapBox(font, text[i], scale, scale, &c_x1, &c_y1, &c_x2, &c_y2);

                    /* compute y (different characters have different heights) */
                    int y = ascent + c_y1;

                    /* render character (stride and offset is important here) */
                    int byteOffset = x + (int)(lsb * scale) + (y * width);
                    stbtt_MakeCodepointBitmap(font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, scale, scale, text[i]);

                    /* advance x */
                    x += (int)(ax * scale);

                    /* add kerning */
                    int kern;
                    if(i + 1 != text.Length)
                    {
                        kern = stbtt_GetCodepointKernAdvance(font, text[i], text[i + 1]);
                        x += (int)(kern * scale);
                    }
                }
                
            }

            color = Vector3.One;
            Texture texture = new Texture();
            texture.LoadTexture(bitmap, width, height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
            textRefs.Add(UiRenderer.uiElements.Count);
            UiRenderer.AddElement(texture, pos, rotation, scaleImage, new Vector2(imageWidth, imageHeight), UiElementType.text);
        }

        public unsafe static void UpdateText(string text, int index, Vector2 pos, int width, int height, float scaleImage, float rotation, int textHeight, int numLines)
        {
            UiRenderer.uiElements[index].width = width;
            UiRenderer.uiElements[index].height = height;

            float scale = stbtt_ScaleForPixelHeight(font, textHeight);

            height = textHeight * numLines;

            byte[] bitmap = new byte[width * height];

            int x = 0;

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(font, &ascent, &descent, &lineGap);

            ascent = (int)(ascent * scale);
            descent = (int)(descent * scale);
            fixed (byte* bitmapDataPtr = bitmap)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    /* how wide is this character */
                    int ax;
                    int lsb;
                    stbtt_GetCodepointHMetrics(font, text[i], &ax, &lsb);

                    if (x + (ax * scale) > width)
                        continue;
                    /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                    /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                    int c_x1, c_y1, c_x2, c_y2;
                    stbtt_GetCodepointBitmapBox(font, text[i], scale, scale, &c_x1, &c_y1, &c_x2, &c_y2);

                    /* compute y (different characters have different heights) */
                    int y = ascent + c_y1;

                    /* render character (stride and offset is important here) */
                    int byteOffset = x + (int)(lsb * scale) + (y * width);
                    stbtt_MakeCodepointBitmap(font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, scale, scale, text[i]);

                    /* advance x */
                    x += (int)(ax * scale);

                    /* add kerning */
                    int kern;
                    if (i + 1 != text.Length)
                    {
                        kern = stbtt_GetCodepointKernAdvance(font, text[i], text[i + 1]);
                        x += (int)(kern * scale);
                    }
                }

            }

            UiRenderer.uiElements[index].color = color;
            UiRenderer.uiElements[index].scale = scaleImage;
            UiRenderer.uiElements[index].position = pos;
            UiRenderer.uiElements[index].rotation = rotation;
            UiRenderer.uiElements[index].texture.UpdateTexture(bitmap, width, height);
        }
    }
}
