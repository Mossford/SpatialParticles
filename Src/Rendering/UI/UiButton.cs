using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Input;
using SpatialGame.Menus;

namespace SpatialEngine.Rendering
{
    public class UiButton : UiElement
    {
        public Vector2 position;
        public Vector2 size;
        public Vector4 highLightColor;
        public Vector4 clickColor;
        Action onClick;

        public UiText text;
        public UiImage image;

        public UiButton(Vector2 position, Vector2 size, Action onClick, string text, Vector4 color, Vector4 highLightColor, Vector4 clickColor)
        {
            this.position = position;
            this.size = size;
            this.scale = 1.0f;
            this.rotation = 0f;
            this.onClick = onClick;
            this.color = color;
            this.highLightColor = highLightColor;
            this.clickColor = clickColor;
            
            this.image = new UiImage(position, (int)size.X, (int)size.Y, color);
            this.image.SetLayer((int)UiLayer.UiImage);
            this.text = new UiText(text, position, 1.0f, 0.0f);
            this.text.SetLayer((int)UiLayer.UiText);
            
            //calculate text scaling
            float scale = size.Y / this.text.height;
            this.text.scale = scale;
            
            AddThis();
        }
        
        public UiButton(Vector2 position, Vector2 size, Action onClick, string text, Vector4 color, float highLightScale = 0.5f, float clickScale = 0.01f)
        {
            this.position = position;
            this.size = size;
            this.scale = 1.0f;
            this.rotation = 0f;
            this.onClick = onClick;
            this.color = color;
            this.highLightColor = new Vector4(color.X * highLightScale, color.Y * highLightScale, color.Z * highLightScale, color.W);
            this.clickColor = new Vector4(highLightColor.X * clickScale, highLightColor.Y * clickScale, highLightColor.Z * clickScale, color.W);
            
            this.image = new UiImage(position, (int)size.X, (int)size.Y, color);
            this.image.SetLayer((int)UiLayer.UiImage);
            this.text = new UiText(text, position, 1.0f, 0.0f);
            this.text.SetLayer((int)UiLayer.UiText);
            
            //calculate text scaling
            float scale = size.Y / this.text.height;
            this.text.scale = scale;
            
            AddThis();
        }
        
        public UiButton(Vector2 position, float height, Action onClick, string text, Vector4 color, Vector4 highLightColor, Vector4 clickColor)
        {
            this.position = position;
            this.onClick = onClick;
            this.scale = 1.0f;
            this.rotation = 0f;
            this.color = color;
            this.highLightColor = highLightColor;
            this.clickColor = clickColor;
            
            this.image = new UiImage(position, (int)size.X, (int)size.Y, color);
            this.image.SetLayer((int)UiLayer.UiImage);
            this.text = new UiText(text, position, 1.0f, 0.0f);
            this.text.SetLayer((int)UiLayer.UiText);
            
            //calculate text scaling
            float scale = height / this.text.height;
            this.text.scale = scale;

            this.size = new Vector2(this.text.width, height);
            this.image.UpdateImage(position, (int)size.X, (int)size.Y, color);
            
            AddThis();
        }
        
        public UiButton(Vector2 position, float height, Action onClick, string text, Vector4 color, float highLightScale = 0.5f, float clickScale = 0.01f)
        {
            this.position = position;
            this.onClick = onClick;
            this.scale = 1.0f;
            this.rotation = 0f;
            this.color = color;
            this.highLightColor = new Vector4(color.X * highLightScale, color.Y * highLightScale, color.Z * highLightScale, color.W);
            this.clickColor = new Vector4(highLightColor.X * clickScale, highLightColor.Y * clickScale, highLightColor.Z * clickScale, color.W);
            
            this.image = new UiImage(position, (int)size.X, (int)size.Y, color);
            this.image.SetLayer((int)UiLayer.UiImage);
            this.text = new UiText(text, position, 1.0f, 0.0f);
            this.text.SetLayer((int)UiLayer.UiText);
            
            //calculate text scaling
            float scale = height / this.text.height;
            this.text.scale = scale;

            this.size = new Vector2(this.text.width, height);
            this.image.UpdateImage(position, (int)size.X, (int)size.Y, color);
            
            AddThis();
        }
            
        public override void Update()
        {
            Vector2 mousePos = Mouse.localPosition * new Vector2(1.0f, -1.0f);
            this.text.position = position;
            this.image.position = position;
            this.text.scale = scale;
            this.image.scale = scale;
            this.text.rotation = rotation;
            this.image.rotation = rotation;

            Mouse.uiWantMouse = false;
            this.image.color = color;
            
            if (!BoundsCheck(mousePos) || hide)
                return;
            
            Mouse.uiWantMouse = true;
            this.image.color = highLightColor;
            
            if (Mouse.mouse.IsButtonPressed(MouseButton.Left))
            {
                this.image.color = clickColor;
                onClick();
            }
            
            UpdateMatrix();
        }

        public override void Draw(in UiQuad quad)
        {
            
        }

        public override void Cleanup()
        {
            text.Cleanup();
            image.Cleanup();
        }

        public override void SetHide(bool hide)
        {
            this.hide = hide;
            this.text.SetHide(hide);
            this.image.SetHide(hide);
        }
            
    #if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif
        public bool BoundsCheck(Vector2 pos)
        {
            if (pos.X < position.X - size.X || pos.X > position.X + size.X || pos.Y < position.Y - size.Y || pos.Y > position.Y + size.Y)
                return false;
            return true;
        }
    }
}