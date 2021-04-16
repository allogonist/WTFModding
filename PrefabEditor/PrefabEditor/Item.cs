using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrefabEditor
{

    public enum SpecialFlagType : uint
    {
        none,
        red_region
    }

    public class Item
    {
        public TerrainItemTeplate item = new TerrainItemTeplate();
        public static Texture2D interestArt;
        public static Texture2D flagArt;
        public static Texture2D redCircleArt;
        public static Vector2 interestOrigin;
        public static Vector2 redOrigin;
        public static SpriteFont interestFont;
        public static Vector2 interestText = new Vector2(22, -14);
        public Texture2D art;
        public Vector2 artOrigin;
        public Rectangle bBox;
        public float scale;
        public string name;
        public bool isPointOfInterest = false;
        public bool isGenerationFlag = false;
        private SpecialFlagType renderType = SpecialFlagType.none;

        //red circle things
        private float radius
        {
            set
            {
                circleScale = value / 1000f;
            }
        }
        private float circleScale = 1;

        static public Color previewColor = new Color(255, 255, 255, 128);

        public Vector2 position
        {
            get
            {
                return item.position;
            }
            set
            {
                item.position = value;
                if (isPointOfInterest || isGenerationFlag)
                {
                    bBox.X = (int)(item.position.X - (interestOrigin.X * scale));
                    bBox.Y = (int)(item.position.Y - (interestOrigin.Y * scale));
                }
                else
                {
                    bBox.X = (int)(item.position.X - (artOrigin.X * scale));
                    bBox.Y = (int)(item.position.Y - (artOrigin.Y * scale));
                }
            }
        }

        public float rotation
        {
            get
            {
                return item.rotation;
            }
            set
            {
                item.rotation = value;
            }
        }

        public Item(string value, bool pointOfInterest)
        {
            art = flagArt;
            name = value;
            if(name.ToLower().Contains("interdict_"))
            {
                renderType = SpecialFlagType.red_region;
                float rad = 10000;
                float.TryParse(name.Split('_').Last(), out rad);
                radius = rad;
            }
            if (name.ToLower().Contains("room_"))
            {
                renderType = SpecialFlagType.red_region;
                float rad = 10000;

                float.TryParse(name.Split('_')[1], out rad);
                radius = rad;
            }
            isPointOfInterest = pointOfInterest;
            isGenerationFlag = !pointOfInterest;
            scale = 1;
            bBox = new Rectangle(0, 0, interestArt.Width, interestArt.Height);
        }

        public Item(string filename, Texture2D tex, float size)
        {
            name = filename;
            scale = size;
            art = tex;
            artOrigin = new Vector2(tex.Width / 2, tex.Height / 2);
            bBox = new Rectangle(0, 0, (int)(art.Width * scale), (int)(art.Height * scale));
        }

        public Item clone()
        {
            Item i;
            if (!isPointOfInterest && !isGenerationFlag)
            {
                i = new Item(name, art, 1);
            }
            else
            {
                i = new Item(name, isPointOfInterest);
            }
            i.position = position;
            i.rotation = rotation;
            i.isGenerationFlag = isGenerationFlag;
            i.isPointOfInterest = isPointOfInterest;
            return i;
        }

        public void resizePreview(float targetWidth)
        {
            if(art != null)
            {
                scale = targetWidth / art.Width;
                if (scale > 2)
                {
                    scale = 2;
                }
                bBox = new Rectangle(0, 0, (int)(art.Width * scale), (int)(art.Height * scale));
            }
        }

        public void draw(SpriteBatch batch, Color color, float scaleOverride)
        {
            if (isPointOfInterest)
            {
                batch.Draw(interestArt, item.position, null, color, item.rotation, interestOrigin, scaleOverride, SpriteEffects.None, 0);
                batch.DrawString(interestFont, name, item.position + interestText, Color.Red);
            }
            else if (isGenerationFlag)
            {
                batch.Draw(flagArt, item.position, null, color, item.rotation, interestOrigin, scaleOverride, SpriteEffects.None, 0);
                batch.DrawString(interestFont, name, item.position + interestText, Color.Red);
            }
            else
            {
                batch.Draw(art, item.position, null, color, item.rotation, artOrigin, scaleOverride, SpriteEffects.None, 0);
            }
        }

        public void drawPreview(SpriteBatch batch, Vector2 spot)
        {
            if (isPointOfInterest)
            {
                batch.Draw(interestArt, spot, null, previewColor, item.rotation, interestOrigin, scale, SpriteEffects.None, 0);
                //batch.DrawString(interestFont, name, spot + interestText, Color.Red);
            }
            else if (isGenerationFlag)
            {
                batch.Draw(flagArt, spot, null, previewColor, item.rotation, interestOrigin, scale, SpriteEffects.None, 0);
            }
            else
            {
                batch.Draw(art, spot, null, previewColor, item.rotation, artOrigin, scale, SpriteEffects.None, 0);
            }
        }

        public void draw(SpriteBatch batch, int extraDark)
        {
            if (extraDark > 0)
            {
                if (isPointOfInterest)
                {
                    batch.Draw(interestArt, item.position, null, Color.White, item.rotation, interestOrigin, scale * 10 * extraDark, SpriteEffects.None, 0);
                    //batch.DrawString(interestFont, name, item.position + interestText, Color.Red);
                }
                else if (isGenerationFlag)
                {
                    batch.Draw(flagArt, item.position, null, Color.White, item.rotation, interestOrigin, scale * 10 * extraDark, SpriteEffects.None, 0);
                    //batch.DrawString(interestFont, name, item.position + interestText, Color.Red);
                }
                else if (art != null)
                {
                    batch.Draw(art, item.position, null, Color.White, item.rotation, artOrigin, scale, SpriteEffects.None, 0);
                }
            }
            else
            {
                if (isPointOfInterest)
                {
                    batch.Draw(interestArt, item.position, null, Color.White, item.rotation, interestOrigin, scale, SpriteEffects.None, 0);
                    batch.DrawString(interestFont, name, item.position + interestText, Color.Red);
                }
                else if (isGenerationFlag)
                {
                    batch.Draw(flagArt, item.position, null, Color.White, item.rotation, interestOrigin, scale, SpriteEffects.None, 0);
                    batch.DrawString(interestFont, name, item.position + interestText, Color.Red);
                }
                else if (art != null)
                {
                    batch.Draw(art, item.position, null, Color.White, item.rotation, artOrigin, scale, SpriteEffects.None, 0);
                }
            }
            switch(renderType)
            {
                case SpecialFlagType.red_region:
                    {
                        batch.Draw(redCircleArt, item.position, null, Color.White, item.rotation, redOrigin, circleScale, SpriteEffects.None, 0);
                    }
                    break;
            }
        }
    }
}
