using System;
using System.Numerics;
using SpatialEngine;
using SpatialEngine.Rendering;

namespace SpatialEngine.Rendering
{
    public abstract class UiElement : IDisposable
    {
        //size for the quad
        public float width;
        public float height;
        //transform
        public Vector2 position;
        public float rotation;
        public float scale;
        public Vector4 color;
        public Matrix4x4 matrix;
        public int index;
        public int layer;
        public bool hide;
            
        static float conv = MathF.PI / 180f;
        public void UpdateMatrix()
        {
            matrix = Matrix4x4.Identity;
            matrix *= Matrix4x4.CreateScale(width * scale * Window.scaleFromBase.X, height * scale * Window.scaleFromBase.Y, 1f);
            matrix *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotation * MathF.PI / 180f);
            matrix *= Matrix4x4.CreateTranslation(new(position.X * Window.scaleFromBase.X, position.Y * Window.scaleFromBase.Y, 0f));
            matrix *= Matrix4x4.CreateOrthographic(Window.size.X, Window.size.Y, -1, 1);
        }

        public void AddThis()
        {
            this.index = UiRenderer.uiElements.Count;
            UiRenderer.AddElement(this);
        }

        public void SetLayer(int layer)
        {
            //if changed during running the uielement list wont be sorted anymore
            this.layer = layer;
        }

        public int CompareLayer(UiElement first, UiElement second)
        {
            return first.layer.CompareTo(second);
        }

        public abstract void Update();
        public abstract void Draw(in UiQuad quad);
        public abstract void Cleanup();
        public abstract void SetHide(bool hide);

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

    }
}