using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Input;

namespace SpatialEngine.Rendering
{

    public enum UiElementType : byte
    {
        image = 0,
        text = 1,
    }

    public class Button
    {
        public Vector2 position;
        public Vector2 size;
        Action onClick;

        public Button(Vector2 position, Vector2 size, Action onClick)
        {
            this.position = position;
            this.size = size;
            this.onClick = onClick;
            Mouse.mouse.MouseDown += RunOnClick;
        }
        
        public void Update()
        {
            Mouse.uiWantMouse = false;
            if (!BoundsCheck(Mouse.mouse.Position))
                return;

            Mouse.uiWantMouse = true;
        }

        void RunOnClick(IMouse mouse, MouseButton button)
        {
            if (Mouse.uiWantMouse && button == MouseButton.Left)
            {
                onClick.Invoke();
            }
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

    public class UiQuad : IDisposable
    {
        struct UIVertex
        {
            public Vector2 pos;
            public Vector2 uv;

            public UIVertex(Vector2 pos, Vector2 uv)
            {
                this.pos = pos;
                this.uv = uv;
            }
        }

        uint id;
        uint vbo;
        uint ebo;
        public unsafe void Bind()
        {
            //create quad
            UIVertex[] vert =
            {
                new(new(-1, -1), new(0, 1)),
                new(new(1, -1), new(1, 1)),
                new(new(-1, 1), new(0, 0)),
                new(new(1, 1), new(1, 0))
            };
            int[] ind = { 0, 1, 2, 1, 3, 2 };

            id = Globals.gl.GenVertexArray();
            Globals.gl.BindVertexArray(id);
            vbo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (UIVertex* buf = vert)
                Globals.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vert.Length * sizeof(UIVertex)), buf, BufferUsageARB.StreamDraw);
            fixed (int* buf = ind)
                Globals.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(ind.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            Globals.gl.EnableVertexAttribArray(0);
            Globals.gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(UIVertex), (void*)0);
            Globals.gl.EnableVertexAttribArray(1);
            Globals.gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(UIVertex), (void*)(2 * sizeof(float)));
            Globals.gl.BindVertexArray(0);

        }

        //custom shader usage
        public unsafe void Draw()
        {
            Globals.gl.BindVertexArray(id);
            Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            Globals.gl.BindVertexArray(0);
            Globals.drawCallCount++;
        }

        //ui renderer usage
        public unsafe void Draw(in Shader shader, in Matrix4x4 mat, in Texture texture, in Vector3 color)
        {
            Globals.gl.BindVertexArray(id);
            texture.Bind();
            Globals.gl.UseProgram(shader.shader);
            shader.setMat4("model", mat);
            shader.setVec3("uiColor", color);
            Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            Globals.gl.BindVertexArray(0);
            Globals.drawCallCount++;
        }

        public void Dispose()
        {
            Globals.gl.DeleteBuffer(vbo);
            Globals.gl.DeleteBuffer(ebo);
            Globals.gl.DeleteVertexArray(id);
        }
    }

    public static class UiRenderer
    {
        static Shader uiTextShader;
        static Shader uiImageShader;
        public static List<UiElement> uiElements;
        public static List<Button> buttons;
        //will reuse this quad for all elements
        static UiQuad quad;

        public static void Init()
        {
            uiTextShader = new Shader(Globals.gl, "UiText.vert", "UiText.frag");
            uiImageShader = new Shader(Globals.gl, "UiImage.vert", "UiImage.frag");

            quad = new UiQuad();
            quad.Bind();

            uiElements = new List<UiElement>();
            buttons = new List<Button>();
        }

        public static void AddElement(Texture texture, Vector2 pos, float rotation, float scale, Vector2 dimension, UiElementType type)
        {
            uiElements.Add(new UiElement(texture, pos, rotation, scale, dimension.X, dimension.Y, type));
        }

        public static void DeleteElement(int index)
        {
            uiElements.RemoveAt(index);
        }

        public static void Update()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Update();
            }
            
            for (int i = 0; i < uiElements.Count; i++)
            {
                uiElements[i].Update();
            }
        }
        
        public static void Draw()
        {
            for (int i = 0; i < uiElements.Count; i++)
            {
                switch(uiElements[i].type)
                {
                    default:
                        quad.Draw(in uiImageShader, in uiElements[i].matrix, in uiElements[i].texture, uiElements[i].color);
                        break;
                    case UiElementType.image:
                        quad.Draw(in uiImageShader, in uiElements[i].matrix, in uiElements[i].texture, uiElements[i].color);
                        break;
                    case UiElementType.text:
                        quad.Draw(in uiTextShader, in uiElements[i].matrix, in uiElements[i].texture, uiElements[i].color);
                        break;
                }
            }
        }
    }
}
