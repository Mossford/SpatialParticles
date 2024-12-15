using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StbTrueTypeSharp.StbTrueType;

namespace SpatialGame
{
    public static class SimTextHandler
    {
        public static stbtt_fontinfo font;
        static byte[] fontData;
        public unsafe static void Init()
        {
            fontData = File.ReadAllBytes(SpatialEngine.Resources.FontPath + "JetBrainsMono-Regular.ttf");
            font = new stbtt_fontinfo();
            
            fixed (byte* fontDataPtr = fontData)
            {
                if (stbtt_InitFont(font, fontDataPtr, 0) == 0)
                {
                    throw new Exception("Error occured while loading font");
                }
            }
        }
    }
}
