using System;
using System.Numerics;
using SpatialEngine;
using SpatialEngine.Rendering;

namespace SpatialGame.Menus
{
    public static class MainMenu
    {
        public static UiText title;
        public static UiImage titleBackground;
        public static UiButton playButton;
        public static UiButton optionsButton;
        public static UiButton saveButton;
        public static UiButton loadButton;
        public static UiImage background;
        public static UiText controls;
        public static UiText controlsR;
        public static UiText controlsT;
        public static UiText controlsG;
        public static UiText controlsEsc;
        public static UiText controlsShift;
        public static UiText controlsLMouse;
        public static UiText controlsCtrl;
        public static UiText controlsMScroll;
        public static bool hide;
        
        public static void Init()
        {
            title = new UiText("Spatial Particles", new Vector2(0, 400f), 1.0f, 0.0f);
            titleBackground = new UiImage(new Vector2(0, 400f), (int)title.width, (int)title.height, new Vector4(50, 50, 50, 100));
            playButton = new UiButton(new Vector2(0, 150f), 50, RunPlay, "Play", new Vector4(50, 50, 50, 100));
            optionsButton = new UiButton(new Vector2(0, 0f), 50, RunOptions, "Options", new Vector4(50, 50, 50, 100));
            saveButton = new UiButton(new Vector2(0, -150f), 50, RunSave, "Save", new Vector4(50, 50, 50, 100));
            loadButton = new UiButton(new Vector2(0, -300f), 50, RunLoad, "Load", new Vector4(50, 50, 50, 100));
            background = new UiImage(Vector2.Zero, (int)(Window.size.X / 2), (int)(Window.size.Y / 2), new Vector4(50, 50, 50, 0));
        }

        public static void SetHide(bool hide)
        {
            MainMenu.hide = hide;
            title.SetHide(hide);
            titleBackground.SetHide(hide);
            playButton.SetHide(hide);
            optionsButton.SetHide(hide);
            saveButton.SetHide(hide);
            loadButton.SetHide(hide);
            background.SetHide(hide);
        }

        public static void RunPlay()
        {
            GameManager.paused = false;
            SetHide(true);
        }
        
        public static void RunOptions()
        {
            OptionMenu.SetHide(false);
            SetHide(true);
        }

        public static void RunSave()
        {
            ParticleSaving.Save();
        }
        public static void RunLoad()
        {
            ParticleSaving.Load();
            GameManager.paused = false;
            SetHide(true);
        }
    }
}