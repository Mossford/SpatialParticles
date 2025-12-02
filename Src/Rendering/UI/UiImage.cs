using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace SpatialEngine.Rendering
{
    public class UiImage : UiElement
    {
        public Texture texture;

        public UiImage(Vector2 position, Texture texture)
        {
            this.texture = texture;
            this.position = position;
            this.width = texture.size.X;
            this.height = texture.size.Y;
            this.scale = 1.0f;
            this.rotation = 0.0f;
            
            SetLayer((int)UiLayer.UiImage);
            AddThis();
        }

        public UiImage(Vector2 position, int width, int height, Vector4 color)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.color = color;
            this.scale = 1.0f;
            this.rotation = 0.0f;
            
            byte[] pixels = new byte[width * height * 3];
            for (int i = 0; i < width * height * 3; i++)
            {
                pixels[i] = 255;
            }
            
            texture = new Texture();
            texture.LoadTexture(pixels, width, height, InternalFormat.Rgb, GLEnum.Rgb);
            
            SetLayer((int)UiLayer.UiImage);
            AddThis();
        }
        
        public UiImage(Vector2 position, int width, int height, Vector4 color, int layer)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.color = color;
            this.scale = 1.0f;
            this.rotation = 0.0f;
            
            byte[] pixels = new byte[width * height * 3];
            for (int i = 0; i < width * height * 3; i++)
            {
                pixels[i] = 255;
            }
            
            texture = new Texture();
            texture.LoadTexture(pixels, width, height, InternalFormat.Rgb, GLEnum.Rgb);
            
            SetLayer(layer);
            AddThis();
        }

        public void UpdateImage(Vector2 position, int width, int height, Vector4 color)
        {
            this.width = width;
            this.height = height;
            this.position = position;
            this.color = color;
            this.scale = 1.0f;
            this.rotation = 0.0f;
            
            byte[] pixels = new byte[width * height * 3];
            for (int i = 0; i < width * height * 3; i++)
            {
                pixels[i] = 255;
            }
            
            texture.UpdateTexture(pixels, width, height);
        }
        
        public override void Update()
        {
            if(hide)
                return;
            
            UpdateMatrix();
        }

        public override void Draw(in UiQuad quad)
        {
            quad.Draw(UiRenderer.uiImageShader, matrix, texture, color / new Vector4(255));
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
}