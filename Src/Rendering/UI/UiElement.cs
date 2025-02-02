using System;
using System.Numerics;
using SpatialEngine;
using SpatialEngine.Rendering;

namespace SpatialEngine.Rendering
{
public class UiElement : IDisposable
{
    //size for the quad
    public float width;
    public float height;

    //transform
    public Vector2 position;
    public float rotation;
    public float scale;
    public Vector3 color;

    public Matrix4x4 matrix;
        
    //texture that is displayed
    public Texture texture;

    public UiElementType type;

    public UiElement(string textureLoc, Vector2 pos, float rot = 0f, float scale = 1f, float length = 100, float height = 100, UiElementType type = UiElementType.image)
    {
        texture = new Texture();
        texture.LoadTexture(textureLoc);
        this.position = pos;
        this.rotation = rot;
        this.scale = scale;
        this.width = length;
        this.height = height;
        this.type = type;
        color = Vector3.One;
    }

    public UiElement(Texture texture, Vector2 pos, float rot = 0f, float scale = 1f, float length = 100, float height = 100, UiElementType type = UiElementType.image)
    {
        this.texture = texture;
        this.position = pos;
        this.rotation = rot;
        this.scale = scale;
        this.width = length;
        this.height = height;
        this.type = type;
        color = Vector3.One;
    }
        
    static float conv = MathF.PI / 180f;
    public void Update()
    {
        matrix = Matrix4x4.Identity;
        matrix *= Matrix4x4.CreateScale(width * scale, height * scale, 1f);
        matrix *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotation * conv);
        matrix *= Matrix4x4.CreateTranslation(new(position.X, position.Y, 0f));
        matrix *= Matrix4x4.CreateOrthographic(Window.size.X, Window.size.Y, -1, 1);
    }

    public void Dispose()
    {
        texture.Dispose();
        GC.SuppressFinalize(this);
    }

}
}