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
        static List<int> textRefs;

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
        public unsafe static void CreateText(string text)
        {
            int width = 512;
            int height = 128;
            int lineHeight = 64; /* line height */

            /* create a bitmap for the phrase */
            byte[] bitmap = new byte[width * height];

            /* calculate font scaling */
            float scale = stbtt_ScaleForPixelHeight(font, lineHeight);

            int x = 0;

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(font, &ascent, &descent, &lineGap);

            ascent = (int)(ascent * scale);
            descent = (int)(descent * scale);
            fixed (byte* bitmapDataPtr = bitmap)
            {
                fixed (char* textPtr = text)
                {
                    for (int i = 0; i < text.Length; ++i)
                    {
                        /* how wide is this character */
                        int ax;
                        int lsb;
                        stbtt_GetCodepointHMetrics(font, textPtr[i], &ax, &lsb);
                        /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                        /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                        int c_x1, c_y1, c_x2, c_y2;
                        stbtt_GetCodepointBitmapBox(font, textPtr[i], scale, scale, &c_x1, &c_y1, &c_x2, &c_y2);

                        /* compute y (different characters have different heights) */
                        int y = ascent + c_y1;

                        /* render character (stride and offset is important here) */
                        int byteOffset = x + (int)(lsb * scale) + (y * width);
                        stbtt_MakeCodepointBitmap(font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, scale, scale, textPtr[i]);

                        /* advance x */
                        x += (int)(ax * scale);

                        /* add kerning */
                        int kern;
                            kern = stbtt_GetCodepointKernAdvance(font, textPtr[i], textPtr[i + 1]);
                        x += (int)(kern * scale);
                    }
                }
            }


            Texture texture = new Texture();
            texture.LoadTexture(bitmap, width, height);
            UiRenderer.AddElement(texture, Vector2.Zero, 180f, 1f, new Vector2(width, height));
        }
    }
}
