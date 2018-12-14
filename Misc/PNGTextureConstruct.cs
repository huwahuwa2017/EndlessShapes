using System;
using UnityEngine;

namespace EndlessShapes
{
    public class PNGTextureConstruct
    {
        public static int Width;

        public static int Height;

        public static int BitDepth;

        public static int ColorType;

        public static void TextureStats(string Base64)
        {
            TextureStatsMain(Convert.FromBase64String(Base64.Substring(20, 16)), 1);
        }

        public static void TextureStats(byte[] ReadBinary)
        {
            TextureStatsMain(ReadBinary, 16);
        }

        private static void TextureStatsMain(byte[] RB, int i)
        {
            Width = BitConverter.ToInt32(new byte[] { RB[i + 3], RB[i + 2], RB[i + 1], RB[i] }, 0);
            Height = BitConverter.ToInt32(new byte[] { RB[i + 7], RB[i + 6], RB[i + 5], RB[i + 4] }, 0);
            BitDepth = RB[i + 8];
            ColorType = RB[i + 9];
        }
        
        public static Texture2D Texture2DConstruct(string Base64)
        {
            byte[] ReadBinary = Convert.FromBase64String(Base64);
            TextureStats(ReadBinary);
            Texture2D texture = new Texture2D(Width, Height);
            ImageConversion.LoadImage(texture, ReadBinary);
            return texture;
        }

        public static Material MaterialConstruct(string ImageData)
        {
            Material NewMaterial = new Material(Shader.Find("Standard"));
            NewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            NewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            NewMaterial.EnableKeyword("_ALPHABLEND_ON");
            NewMaterial.renderQueue = 3000;
            Texture2D texture2D = Texture2DConstruct(ImageData);
            NewMaterial.mainTexture = texture2D;
            return NewMaterial;
        }

        public static Material MaterialConstruct(string ImageData, string Name)
        {
            Material NewMaterial = MaterialConstruct(ImageData);
            NewMaterial.name = Name;
            return NewMaterial;
        }
    }
}