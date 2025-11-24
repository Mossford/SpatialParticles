using SpatialEngine.Rendering;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SDL;


namespace SpatialEngine.Rendering
{
    public class UiText : IDisposable
    {
        public string text;
        public Vector2 position;
        public int width;
        public int height;
        public float scale;
        public float rotation;
        public Vector3 color;

        Texture texture;
        byte[] bitmap;
        int elementIndex;

        public UiText()
        {
            
        }

        public UiText(string text, Vector2 pos, float scaleImage, float rotation)
        {
            this.text = text;
            this.position = pos;
            this.scale = scaleImage;
            this.rotation = rotation;
            
            CreateText();
        }
        
        public static byte[] CreateBitmapFromSurface(IntPtr ptr, int length)
        {
            byte[] array = new byte[length];

            //divide by 4 so we dont index outside of the pointer
            for (int i = 0; i < length / 4; i++)
            {
                array[i] = (byte)Marshal.PtrToStructure(ptr + i, typeof(byte));
            }

            return array;
        }
        
        public unsafe void CreateText()
        {
            SDL_Surface* surface;
            
            color = Vector3.One;
            SDL_Color tempColor = new SDL_Color();
            tempColor.r = (byte)(color.X * 255);
            tempColor.g = (byte)(color.Y * 255);
            tempColor.b = (byte)(color.Z * 255);
            tempColor.a = 0;
            
            byte[] textData = Encoding.UTF8.GetBytes(text);
            fixed (byte* textDataPtr = textData)
                surface = SDL3_ttf.TTF_RenderText_Solid(UiTextHandler.font, textDataPtr, (nuint)text.Length, tempColor);

            Console.WriteLine(surface->format);
            
            //the surface format is already index 8 but this seems to get it to work?
            SDL_Surface* surfaceConvert = SDL3.SDL_ConvertSurface(surface, SDL_PixelFormat.SDL_PIXELFORMAT_INDEX8);
            if (surfaceConvert is null)
            {
                Console.WriteLine("Could not create texture, " + SDL3.SDL_GetError());
                SDL3.SDL_DestroySurface(surface);
                return;
            }
            SDL3.SDL_DestroySurface(surface);
            surface = surfaceConvert;

            bitmap = CreateBitmapFromSurface(surface->pixels, surface->w * surface->h * 4);
            
            texture = new Texture();
            texture.LoadTexture(bitmap, surface->w, surface->h, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
            elementIndex = UiRenderer.uiElements.Count;
            //add on the length from the actual text to center it to a position
            UiRenderer.AddElement(texture, new Vector2(position.X, position.Y), rotation, scale, texture.size, UiElementType.text);
            
            SDL3.SDL_DestroySurface(surface);
        }
        
        public unsafe void CreateText(string text, Vector2 pos, float scaleImage, float rotation)
        {
            this.text = text;
            this.position = pos;
            this.scale = scaleImage;
            this.rotation = rotation;
            
            SDL_Surface* surface;
            
            color = Vector3.One;
            SDL_Color tempColor = new SDL_Color();
            tempColor.r = (byte)(color.X * 255);
            tempColor.g = (byte)(color.Y * 255);
            tempColor.b = (byte)(color.Z * 255);
            tempColor.a = 0;
            
            byte[] textData = Encoding.UTF8.GetBytes(text);
            fixed (byte* textDataPtr = textData)
                surface = SDL3_ttf.TTF_RenderText_Solid(UiTextHandler.font, textDataPtr, (nuint)text.Length, tempColor);

            width = surface->w;
            height = surface->h;
            
            //the surface format is already index 8 but this seems to get it to work?
            SDL_Surface* surfaceConvert = SDL3.SDL_ConvertSurface(surface, SDL_PixelFormat.SDL_PIXELFORMAT_INDEX8);
            if (surfaceConvert is null)
            {
                Console.WriteLine("Could not create texture, " + SDL3.SDL_GetError());
                SDL3.SDL_DestroySurface(surface);
                return;
            }
            SDL3.SDL_DestroySurface(surface);
            surface = surfaceConvert;

            bitmap = CreateBitmapFromSurface(surface->pixels, width * height * 4);
            
            texture = new Texture();
            texture.LoadTexture(bitmap, width, height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
            elementIndex = UiRenderer.uiElements.Count;
            //add on the length from the actual text to center it to a position
            UiRenderer.AddElement(texture, position, rotation, scale, texture.size, UiElementType.text);
            
            SDL3.SDL_DestroySurface(surface);
        }
        
        public unsafe void UpdateText(string text, Vector2 pos, float scaleImage, float rotation)
        {
            this.text = text;
            this.position = pos;
            this.scale = scaleImage;
            this.rotation = rotation;

            SDL_Surface* surface;
            
            color = Vector3.One;
            SDL_Color tempColor = new SDL_Color();
            tempColor.r = (byte)(color.X * 255);
            tempColor.g = (byte)(color.Y * 255);
            tempColor.b = (byte)(color.Z * 255);
            tempColor.a = 0;
            
            byte[] textData = Encoding.UTF8.GetBytes(text);
            fixed (byte* textDataPtr = textData)
                surface = SDL3_ttf.TTF_RenderText_Solid(UiTextHandler.font, textDataPtr, (nuint)text.Length, tempColor);

            width = surface->w;
            height = surface->h;
            
            //the surface format is already index 8 but this seems to get it to work?
            SDL_Surface* surfaceConvert = SDL3.SDL_ConvertSurface(surface, SDL_PixelFormat.SDL_PIXELFORMAT_INDEX8);
            if (surfaceConvert is null)
            {
                Console.WriteLine("Could not create texture, " + SDL3.SDL_GetError());
                SDL3.SDL_DestroySurface(surface);
                return;
            }
            SDL3.SDL_DestroySurface(surface);
            surface = surfaceConvert;

            bitmap = CreateBitmapFromSurface(surface->pixels, width * height * 4);
            
            SDL3.SDL_DestroySurface(surface);

            UiRenderer.uiElements[elementIndex].color = color;
            UiRenderer.uiElements[elementIndex].scale = scaleImage;
            UiRenderer.uiElements[elementIndex].position = position;
            UiRenderer.uiElements[elementIndex].width = width;
            UiRenderer.uiElements[elementIndex].height = height;
            UiRenderer.uiElements[elementIndex].rotation = rotation;
            UiRenderer.uiElements[elementIndex].texture.UpdateTexture(bitmap, width, height);
        }
        
        public void Dispose()
        {
            UiRenderer.DeleteElement(elementIndex);
        }
    }
    
    
    public static unsafe class UiTextHandler
    {
        public static TTF_Font* font;
        public static void Init()
        {
            byte[] file = Encoding.UTF8.GetBytes(SpatialEngine.Resources.FontPath + "JetBrainsMono-Regular.ttf");

            if (!SDL3_ttf.TTF_Init())
            {
                throw new Exception("Failed to start font library");
            }
            
            fixed (byte* fontDataPtr = file)
            {
                font = SDL3_ttf.TTF_OpenFont(fontDataPtr, 36);
                if(font is null)
                    throw new Exception("Failed to load font");
            }
        }
    }
}

