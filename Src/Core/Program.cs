using System;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using SpatialEngine.Networking;

//Custom Engine things
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Rendering.MainImGui;
using static SpatialEngine.Resources;
using static SpatialEngine.Globals;
using SpatialEngine.Rendering;
//silk net has its own shader for some reason
using Shader = SpatialEngine.Rendering.Shader;
using Texture = SpatialEngine.Rendering.Texture;
using static SpatialEngine.Debugging;
using static SpatialEngine.Input;
using SpatialGame;
using Button = SpatialEngine.Rendering.Button;

namespace SpatialEngine
{

    public static class Globals
    {
        public static GL gl;
        public static GraphicsAPI glApi = GraphicsAPI.Default;
        public static IWindow window;
        public const int SCR_WIDTH = 1920;
        public const int SCR_HEIGHT = 1080;
        public static int MAX_SCR_WIDTH;
        public static int MAX_SCR_HEIGHT;
        public static string EngVer = "PAR:0.5 | ENG:0.6.8 Stable";
        public static string OpenGlVersion = "";
        public static string Gpu = "";

        public static Scene scene;

        public static bool showImguiDebug = false;
        //going to be true because my gpu squeals if vsync is off
        public static bool vsync = true;
        public static uint vertCount;
        public static uint indCount;

        public static Player player;

        public static uint drawCallCount = 0;
        public static float totalTime = 0.0f;
        public static float deltaTime = 0.0f;
        public const float fixedDeltaTime = 16.667f;

        /// <summary>
        /// In Seconds
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTime() => totalTime;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDeltaTime() => deltaTime;
    }

    public class Game
    {
        static ImGuiController controller;

        public static void Main(string[] args)
        {
            //init resources like resources paths
            InitResources();

            if (args.Length == 0)
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
                window = Window.Create(options);
                window.Load += OnLoad;
                window.Update += OnUpdate;
                window.Render += OnRender;
                window.Run();
            }
            else if (args[0] == "server")
            {
                //handles all the initilization of scene and physics and netowrking for server
                //and starts running the update loop
                HeadlessServer.Init();
            }
            
            NetworkManager.Cleanup();
        }

        static unsafe void OnLoad() 
        {
            gl = window.CreateOpenGL();
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
            controller = new ImGuiController(gl, window, input);
            ImGui.SetWindowSize(new Vector2(850, 500));

            //Tests.RunTestMain();

            //init game
            GameManager.InitGame();
            MainImGui.Init();
            NetworkManager.Init();
            player = new Player(Vector2.Zero);

            //get the display size
            window.WindowState = WindowState.Fullscreen;
            MAX_SCR_WIDTH = window.GetFullSize().X;
            MAX_SCR_HEIGHT = window.GetFullSize().Y;
            window.WindowState = WindowState.Normal;

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
            gl.Viewport(0,0, (uint)window.Size.X, (uint)window.Size.Y);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gl.DepthFunc(GLEnum.Lequal);
            gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            
            GameManager.RenderGame();
            //renders other more generic ui stuff
            UiRenderer.Draw();
            SetNeededDebug(Matrix4x4.CreateOrthographic(window.Size.X, window.Size.Y, -1, 1f), Matrix4x4.Identity);
            Debugging.DrawDebugItems();

            if (showImguiDebug)
            {
                controller.Render();
            }
        }

        

        static unsafe void DebugProc(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint msg, nint userParam)
        {
            string _source;
            string _type;
            string _severity;

            switch (source) 
            {
                case GLEnum.DebugSourceApi:
                _source = "API";
                break;

                case GLEnum.DebugSourceWindowSystem:
                _source = "WINDOW SYSTEM";
                break;

                case GLEnum.DebugSourceShaderCompiler:
                _source = "SHADER COMPILER";
                break;

                case GLEnum.DebugSourceThirdParty:
                _source = "THIRD PARTY";
                break;

                case GLEnum.DebugSourceApplication:
                _source = "APPLICATION";
                break;

                case GLEnum.DebugSourceOther:
                _source = "UNKNOWN";
                break;

                default:
                _source = "UNKNOWN";
                break;
            }

            switch (type) {
                case GLEnum.DebugTypeError:
                _type = "ERROR";
                break;

                case GLEnum.DebugTypeDeprecatedBehavior:
                _type = "DEPRECATED BEHAVIOR";
                break;

                case GLEnum.DebugTypeUndefinedBehavior:
                _type = "UDEFINED BEHAVIOR";
                break;

                case GLEnum.DebugTypePortability:
                _type = "PORTABILITY";
                break;

                case GLEnum.DebugTypePerformance:
                _type = "PERFORMANCE";
                break;

                case GLEnum.DebugTypeOther:
                _type = "OTHER";
                break;

                case GLEnum.DebugTypeMarker:
                _type = "MARKER";
                break;

                default:
                _type = "UNKNOWN";
                break;
            }

            switch (severity) {
                case GLEnum.DebugSeverityHigh:
                _severity = "HIGH";
                break;

                case GLEnum.DebugSeverityMedium:
                _severity = "MEDIUM";
                break;

                case GLEnum.DebugSeverityLow:
                _severity = "LOW";
                break;

                case GLEnum.DebugSeverityNotification:
                _severity = "NOTIFICATION";
                break;

                default:
                _severity = "UNKNOWN";
                break;
            }

            Console.WriteLine(string.Format("{0}: {1} of {2} severity, raised from {3}: {4}", id, _type, _severity, _source, msg));
        }
    }
}
