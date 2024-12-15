using System;
using System.Numerics;
using SpatialEngine.Rendering;
using StbTrueTypeSharp;

namespace SpatialGame
{
    public class SimText : IDisposable
{
    public string text;
    public Vector2 position;
    public int width;
    public int height;
    public float scale;
    public float rotation;
    public int textHeight;
    public int numLines;
    public Vector3 color;

    Texture texture;
    byte[] bitmap;
    int elementIndex;

    public SimText()
    {
            
    }

    public SimText(string text, Vector2 pos, int width, int height, float scaleImage, float rotation, int textHeight, int numLines)
    {
        this.text = text;
        this.position = pos;
        this.width = width;
        this.height = height;
        this.scale = scaleImage;
        this.rotation = rotation;
        this.textHeight = textHeight;
        this.numLines = numLines;
            
        CreateText();
    }
        
    public unsafe void CreateText()
    {
        int imageWidth = width;
        int imageHeight = height;

        float tempScale = StbTrueType.stbtt_ScaleForPixelHeight(SimTextHandler.font, textHeight);

        height = textHeight * numLines;

        bitmap = new byte[width * height];

        int x = 0;

        int ascent, descent, lineGap;
        StbTrueType.stbtt_GetFontVMetrics(SimTextHandler.font, &ascent, &descent, &lineGap);

        ascent = (int)(ascent * tempScale);
        descent = (int)(descent * tempScale);
        fixed (byte* bitmapDataPtr = bitmap)
        {
            for (int i = 0; i < text.Length; i++)
            {
                /* how wide is this character */
                int ax;
                int lsb;
                StbTrueType.stbtt_GetCodepointHMetrics(SimTextHandler.font, text[i], &ax, &lsb);

                if (x + (ax * tempScale) > width)
                    continue;
                /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                int c_x1, c_y1, c_x2, c_y2;
                StbTrueType.stbtt_GetCodepointBitmapBox(SimTextHandler.font, text[i], tempScale, tempScale, &c_x1, &c_y1, &c_x2, &c_y2);

                /* compute y (different characters have different heights) */
                int y = ascent + c_y1;

                /* render character (stride and offset is important here) */
                int byteOffset = x + (int)(lsb * tempScale) + (y * width);
                StbTrueType.stbtt_MakeCodepointBitmap(SimTextHandler.font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, tempScale, tempScale, text[i]);

                /* advance x */
                x += (int)(ax * tempScale);

                /* add kerning */
                int kern;
                if(i + 1 != text.Length)
                {
                    kern = StbTrueType.stbtt_GetCodepointKernAdvance(SimTextHandler.font, text[i], text[i + 1]);
                    x += (int)(kern * tempScale);
                }
            }
                
        }

        color = Vector3.One;
        texture = new Texture();
        texture.LoadTexture(bitmap, width, height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
        elementIndex = UiRenderer.uiElements.Count;
        UiRenderer.AddElement(texture, position, rotation, scale, new Vector2(imageWidth, imageHeight), UiElementType.text);
    }
        
    public unsafe void CreateText(string text, Vector2 pos, int width, int height, float scaleImage, float rotation, int textHeight, int numLines)
    {
        this.text = text;
        this.position = pos;
        this.width = width;
        this.height = height;
        this.scale = scaleImage;
        this.rotation = rotation;
        this.textHeight = textHeight;
        this.numLines = numLines;
            
        int imageWidth = width;
        int imageHeight = height;

        float tempScale = StbTrueType.stbtt_ScaleForPixelHeight(SimTextHandler.font, textHeight);

        height = textHeight * numLines;

        bitmap = new byte[width * height];

        int x = 0;

        int ascent, descent, lineGap;
        StbTrueType.stbtt_GetFontVMetrics(SimTextHandler.font, &ascent, &descent, &lineGap);

        ascent = (int)(ascent * tempScale);
        descent = (int)(descent * tempScale);
        fixed (byte* bitmapDataPtr = bitmap)
        {
            for (int i = 0; i < text.Length; i++)
            {
                /* how wide is this character */
                int ax;
                int lsb;
                StbTrueType.stbtt_GetCodepointHMetrics(SimTextHandler.font, text[i], &ax, &lsb);

                if (x + (ax * tempScale) > width)
                    continue;
                /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                int c_x1, c_y1, c_x2, c_y2;
                StbTrueType.stbtt_GetCodepointBitmapBox(SimTextHandler.font, text[i], tempScale, tempScale, &c_x1, &c_y1, &c_x2, &c_y2);

                /* compute y (different characters have different heights) */
                int y = ascent + c_y1;

                /* render character (stride and offset is important here) */
                int byteOffset = x + (int)(lsb * tempScale) + (y * width);
                StbTrueType.stbtt_MakeCodepointBitmap(SimTextHandler.font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, tempScale, tempScale, text[i]);

                /* advance x */
                x += (int)(ax * tempScale);

                /* add kerning */
                int kern;
                if(i + 1 != text.Length)
                {
                    kern = StbTrueType.stbtt_GetCodepointKernAdvance(SimTextHandler.font, text[i], text[i + 1]);
                    x += (int)(kern * tempScale);
                }
            }
                
        }
            
        color = Vector3.One;
        texture = new Texture();
        texture.LoadTexture(bitmap, width, height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
        elementIndex = UiRenderer.uiElements.Count;
        UiRenderer.AddElement(texture, position, rotation, scale, new Vector2(imageWidth, imageHeight), UiElementType.text);
    }
        
    public unsafe void UpdateTextFull(string text, Vector2 pos, int width, int height, float scaleImage, float rotation, int textHeight, int numLines)
    {
        this.text = text;
        this.position = pos;
        this.width = width;
        this.height = height;
        this.scale = scaleImage;
        this.rotation = rotation;
        this.textHeight = textHeight;
        this.numLines = numLines;
            
        UiRenderer.uiElements[elementIndex].width = width;
        UiRenderer.uiElements[elementIndex].height = height;

        float tempScale = StbTrueType.stbtt_ScaleForPixelHeight(SimTextHandler.font, textHeight);

        height = textHeight * numLines;

        bitmap = new byte[width * height];

        int x = 0;

        int ascent, descent, lineGap;
        StbTrueType.stbtt_GetFontVMetrics(SimTextHandler.font, &ascent, &descent, &lineGap);

        ascent = (int)(ascent * tempScale);
        descent = (int)(descent * tempScale);
        fixed (byte* bitmapDataPtr = bitmap)
        {
            for (int i = 0; i < text.Length; i++)
            {
                /* how wide is this character */
                int ax;
                int lsb;
                StbTrueType.stbtt_GetCodepointHMetrics(SimTextHandler.font, text[i], &ax, &lsb);

                if (x + (ax * tempScale) > width)
                    continue;
                /* (Note that each Codepoint call has an alternative Glyph version which caches the work required to lookup the character word[i].) */

                /* get bounding box for character (may be offset to account for chars that dip above or below the line) */
                int c_x1, c_y1, c_x2, c_y2;
                StbTrueType.stbtt_GetCodepointBitmapBox(SimTextHandler.font, text[i], tempScale, tempScale, &c_x1, &c_y1, &c_x2, &c_y2);

                /* compute y (different characters have different heights) */
                int y = ascent + c_y1;

                /* render character (stride and offset is important here) */
                int byteOffset = x + (int)(lsb * tempScale) + (y * width);
                StbTrueType.stbtt_MakeCodepointBitmap(SimTextHandler.font, bitmapDataPtr + byteOffset, c_x2 - c_x1, c_y2 - c_y1, width, tempScale, tempScale, text[i]);

                /* advance x */
                x += (int)(ax * tempScale);

                /* add kerning */
                int kern;
                if(i + 1 != text.Length)
                {
                    kern = StbTrueType.stbtt_GetCodepointKernAdvance(SimTextHandler.font, text[i], text[i + 1]);
                    x += (int)(kern * tempScale);
                }
            }
                
        }

        UiRenderer.uiElements[elementIndex].color = color;
        UiRenderer.uiElements[elementIndex].scale = scaleImage;
        UiRenderer.uiElements[elementIndex].position = pos;
        UiRenderer.uiElements[elementIndex].rotation = rotation;
        UiRenderer.uiElements[elementIndex].texture.UpdateTexture(bitmap, width, height);
    }
        
    public unsafe void UpdateText(Vector2 pos, int width, int height, float scaleImage, float rotation)
    {
        this.position = pos;
        this.width = width;
        this.height = height;
        this.scale = scaleImage;
        this.rotation = rotation;
            
        UiRenderer.uiElements[elementIndex].width = width;
        UiRenderer.uiElements[elementIndex].height = height;
        UiRenderer.uiElements[elementIndex].color = color;
        UiRenderer.uiElements[elementIndex].scale = scaleImage;
        UiRenderer.uiElements[elementIndex].position = pos;
        UiRenderer.uiElements[elementIndex].rotation = rotation;
    }
        
    public void Dispose()
    {
        UiRenderer.DeleteElement(elementIndex);
    }
}
}

