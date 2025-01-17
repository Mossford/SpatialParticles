using System;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using SpatialEngine.Networking;

using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MainImGui;
using SpatialEngine.Rendering;
using static SpatialEngine.Debugging;
using static SpatialEngine.Input;
using SpatialGame;

using SilkNetWindow = Silk.NET.Windowing.Window;

namespace SpatialEngine
{
    public static class Window
    {
        public const int SCR_WIDTH = 1920;
        public const int SCR_HEIGHT = 1080;
        public static int MAX_SCR_WIDTH;
        public static int MAX_SCR_HEIGHT;
        public static Vector2 size;
        public static Vector2 windowScale;
        
        public static void Init()
        {
            glApi.Version = new APIVersion(4, 3);
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(SCR_WIDTH, SCR_HEIGHT),
                Title = "SpatialParticles 0.5",
                VSync = vsync,
                PreferredDepthBufferBits = 24,
                API = glApi,
            };
            snWindow = SilkNetWindow.Create(options);
            snWindow.Load += OnLoad;
            snWindow.Update += OnUpdate;
            snWindow.Render += OnRender;
            snWindow.Run();
        }
        
        static unsafe void OnLoad() 
        {
            gl = snWindow.CreateOpenGL();
            //gl = GL.GetApi(window);
            byte* text = gl.GetString(GLEnum.Renderer);
            int textLength = 0;
            while (text[textLength] != 0)
                textLength++;
            byte[] textArray = new byte[textLength];
            Marshal.Copy((IntPtr)text, textArray, 0, textLength);
            Gpu = System.Text.Encoding.Default.GetString(textArray);
            text = gl.GetString(GLEnum.Version);
            textLength = 0;
            while (text[textLength] != 0)
                textLength++;
            textArray = new byte[textLength];
            Marshal.Copy((IntPtr)text, textArray, 0, textLength);
            OpenGlVersion = System.Text.Encoding.Default.GetString(textArray);
            gl.Enable(GLEnum.DepthTest);
            gl.Enable(GLEnum.Texture2D);
            gl.Enable(GLEnum.CullFace);
            gl.Enable(GLEnum.DebugOutput);
            gl.DebugMessageCallback(DebugProc, null);
            gl.DebugMessageControl(GLEnum.DontCare, GLEnum.DontCare, GLEnum.DebugSeverityNotification, 0, null, false);
            
            //input stuffs
            Input.Init();
            Mouse.Init();
            //imgui control stuff
            controller = new ImGuiController(gl, snWindow, input);
            ImGui.SetWindowSize(new Vector2(850, 500));

            //Tests.RunTestMain();

            //init game
            GameManager.InitGame();
            MainImGui.Init();
            NetworkManager.Init();
            player = new Player(Vector2.Zero);

            //get the display size
            snWindow.WindowState = WindowState.Fullscreen;
            MAX_SCR_WIDTH = snWindow.GetFullSize().X;
            MAX_SCR_HEIGHT = snWindow.GetFullSize().Y;
            snWindow.WindowState = WindowState.Normal;
            size = (Vector2)snWindow.FramebufferSize;
            windowScale = size / (Vector2)snWindow.Size;

            input.Keyboards[0].KeyDown += KeyDown;
            
        }
        
        static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if(!showImguiDebug && key == Key.F1)
            {
                showImguiDebug = true;
            }
            else if(showImguiDebug && key == Key.F1)
            {
                showImguiDebug = false;
            }
        }
        
        static float totalTimeUpdate = 0.0f;
        static void OnUpdate(double dt)
        {
            size = (Vector2)snWindow.FramebufferSize;
            windowScale = size / (Vector2)snWindow.Size;
            
            totalTime += (float)dt;
            deltaTime = (float)dt;

            //clear first as any keys pressed can not be dectected by the order of input silk net does
            Input.Clear();
            Input.Update();
            
            GameManager.UpdateGame((float)dt);
            Mouse.Update();
            UiRenderer.Update();

            totalTimeUpdate += (float)dt * 1000;
            while (totalTimeUpdate >= fixedDeltaTime)
            {
                totalTimeUpdate -= fixedDeltaTime;
                FixedUpdate(fixedDeltaTime);
            }
            
        }
        
        static void FixedUpdate(float dt)
        {
            if (NetworkManager.running)
            {
                if(NetworkManager.isServer)
                {
                    NetworkManager.server.Update(dt);
                    GameManager.FixedUpdateGame(dt);
                    NetworkManager.client.Update(dt);
                }
                else if(NetworkManager.isClient)
                {
                    GameManager.FixedUpdateGame(dt);
                    NetworkManager.client.Update(dt);
                }
            }
            else
            {
                GameManager.FixedUpdateGame(dt);
            }
            
        }
        
        static unsafe void OnRender(double dt)
        {
            if(showImguiDebug)
            {
                controller.Update((float)dt);
                ImGuiMenu((float)dt);
            }

            gl.ClearColor(Color.Black);
            gl.Viewport(0,0, (uint)size.X, (uint)size.Y);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gl.DepthFunc(GLEnum.Lequal);
            gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            
            GameManager.RenderGame();
            //renders other more generic ui stuff
            UiRenderer.Draw();
            SetNeededDebug(Matrix4x4.CreateOrthographic(size.X, size.Y, -1, 1f), Matrix4x4.Identity);
            Debugging.DrawDebugItems();

            if (showImguiDebug)
            {
                controller.Render();
            }
        }
    }
}