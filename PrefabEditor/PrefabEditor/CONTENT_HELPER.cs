using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace PrefabEditor
{
    static public class CONTENT_HELPER
    {
        static public GraphicsDevice Device;

        static public Texture2D readTexFromPng(string optArt)
        {
            string path = optArt;
            if (!File.Exists(path))
            {
                path = Directory.GetCurrentDirectory() + @"\Content\" + optArt + ".png";
            }
            Texture2D texture = null;
            //try
            //{
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        System.Windows.Media.Imaging.PngBitmapDecoder decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(fs, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.Default);
                        System.Windows.Media.Imaging.BitmapSource bitmapsource = decoder.Frames[0];

                        int w = (int)bitmapsource.PixelWidth;
                        int h = (int)bitmapsource.PixelHeight;
                        int l = w * h;
                        int stride = w * (bitmapsource.Format.BitsPerPixel / 8); //bpp should be 32 because we only do 32 bit pngs
                        byte[] pixels = new byte[h * stride];
                        bitmapsource.CopyPixels(pixels, stride, 0);

                        int byteCount = bitmapsource.Format.BitsPerPixel / 8;

                        Color[] data = new Color[l];

                        for (int i = 0; i < l; i++)
                        {
                            byte b = pixels[i * byteCount + 0];
                            byte g = pixels[i * byteCount + 1];
                            byte r = pixels[i * byteCount + 2];
                            if (byteCount > 3)
                            {
                                byte a = pixels[i * 4 + 3];
                                data[i] = new Color(r, g, b, a);
                            }
                            else
                            {
                                byte a = 255;
                                data[i] = new Color(r, g, b, a);
                            }
                        }

                        texture = new Texture2D(Device, w, h);
                        texture.SetData(data);
                        return texture;
                    }
                }
            //}
            //catch (Exception e)
            //{
            //    if (texture != null)
            //    {
            //        try
            //        {
            //            texture.Dispose();
            //        }
            //        catch
            //        {
            //        }
            //    }
            //    return null;
            //}
            return null;
        }
    }
}
