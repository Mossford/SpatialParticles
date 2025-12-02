using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Silk.NET.Input;
using SpatialGame.Menus;

namespace SpatialEngine.Rendering
{
    public class UiButton : UiElement
    {
        public Vector4 highLightColor;
        public Vector4 clickColor;
        public Action onClick;

        public UiText text;
        public UiImage image;

        public UiButton(Vector2 position, Vector2 size, Action onClick, string text, Vector4 color, Vector4 highLightColor, Vector4 clickColor)
        {
            this.position = position;
            this.width = size.X;
            this.height = size.Y;
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
            this.width = size.X;
            this.height = size.Y;
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
            
            this.image = new UiImage(position, (int)width, (int)height, color);
            this.image.SetLayer((int)UiLayer.UiImage);
            this.text = new UiText(text, position, 1.0f, 0.0f);
            this.text.SetLayer((int)UiLayer.UiText);
            
            //calculate text scaling
            float scale = height / this.text.height;
            this.text.scale = scale;
            
            this.width = this.text.width;
            this.height = height;
            this.image.UpdateImage(position, (int)width, (int)height, color);
            
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
            
            this.image = new UiImage(position, (int)width, (int)height, color);
            this.image.SetLayer((int)UiLayer.UiImage);
            this.text = new UiText(text, position, 1.0f, 0.0f);
            this.text.SetLayer((int)UiLayer.UiText);
            
            //calculate text scaling
            float scale = height / this.text.height;
            this.text.scale = scale;

            this.width = this.text.width;
            this.height = height;
            this.image.UpdateImage(position, (int)width, (int)height, color);
            
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
            this.text.width = width;
            this.image.width = width;
            this.text.height = height;
            this.image.height = height;

            Mouse.uiWantMouse = false;
            this.image.color = color;
            
            if (!BoundsCheck(mousePos) || hide || ImGui.GetIO().WantCaptureMouse)
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
            if (pos.X < position.X - width || pos.X > position.X + width || pos.Y < position.Y - height || pos.Y > position.Y + height)
                return false;
            return true;
        }
    }
}