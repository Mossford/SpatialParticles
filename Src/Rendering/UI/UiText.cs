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
    public class UiText : UiElement
    {
        public string text;
        Texture texture;
        byte[] bitmap;

        public UiText()
        {
            
        }

        public UiText(string text, Vector2 pos, float scaleImage, float rotation)
        {
            this.text = text;
            this.position = pos;
            this.scale = scaleImage;
            this.rotation = rotation;
            this.width = 100;
            this.height = 100;
            this.color = new Vector4(255, 255, 255, 255);
            
            SetLayer((int)UiLayer.UiText);
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
            
            SDL_Color tempColor = new SDL_Color();
            tempColor.r = 255;
            tempColor.g = 255;
            tempColor.b = 255;
            tempColor.a = 0;
            
            //crashes when string is empty
            if (text.Length == 0)
            {
                text = " ";
            }
            
            byte[] textData = Encoding.UTF8.GetBytes(text);
            fixed (byte* textDataPtr = textData)
                surface = SDL3_ttf.TTF_RenderText_Solid(UiTextHandler.font, textDataPtr, (nuint)text.Length, tempColor);
            
            this.width = surface->w;
            this.height = surface->h;
            
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

            bitmap = CreateBitmapFromSurface(surface->pixels, (int)width * (int)height * 4);
            
            texture = new Texture();
            texture.LoadTexture(bitmap, (int)width, (int)height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
            AddThis();
            
            SDL3.SDL_DestroySurface(surface);
        }
        
        public unsafe void CreateText(string text, Vector2 pos, float scaleImage, float rotation)
        {
            this.text = text;
            this.position = pos;
            this.scale = scaleImage;
            this.rotation = rotation;
            
            SDL_Surface* surface;
            
            SDL_Color tempColor = new SDL_Color();
            tempColor.r = 255;
            tempColor.g = 255;
            tempColor.b = 255;
            tempColor.a = 0;
            
            //crashes when string is empty
            if (text.Length == 0)
            {
                text = " ";
            }
            
            byte[] textData = Encoding.UTF8.GetBytes(text);
            fixed (byte* textDataPtr = textData)
                surface = SDL3_ttf.TTF_RenderText_Solid(UiTextHandler.font, textDataPtr, (nuint)text.Length, tempColor);

            this.width = surface->w;
            this.height = surface->h;
            
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

            bitmap = CreateBitmapFromSurface(surface->pixels, (int)width * (int)height * 4);
            
            texture = new Texture();
            texture.LoadTexture(bitmap, (int)width, (int)height, Silk.NET.OpenGL.InternalFormat.Red, Silk.NET.OpenGL.GLEnum.Red);
            AddThis();
            
            SDL3.SDL_DestroySurface(surface);
        }
        
        public unsafe void UpdateText(string text, Vector2 pos, float scaleImage, float rotation)
        {
            this.text = text;
            this.position = pos;
            this.scale = scaleImage;
            this.rotation = rotation;

            SDL_Surface* surface;
            
            SDL_Color tempColor = new SDL_Color();
            tempColor.r = 255;
            tempColor.g = 255;
            tempColor.b = 255;
            tempColor.a = 0;

            //crashes when string is empty
            if (text.Length == 0)
            {
                text = " ";
            }
            
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

            bitmap = CreateBitmapFromSurface(surface->pixels, (int)width * (int)height * 4);
            
            SDL3.SDL_DestroySurface(surface);
            
            texture.UpdateTexture(bitmap, (int)width, (int)height);
        }

        public override void Update()
        {
            if(hide)
                return;
            
            UpdateMatrix();
        }

        public override void Draw(in UiQuad quad)
        {
            quad.Draw(UiRenderer.uiTextShader, matrix, texture, color / new Vector4(255));
        }

        public override void Cleanup()
        {
            texture.Dispose();
        }

        public override void SetHide(bool hide)
        {
            this.hide = hide;
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

