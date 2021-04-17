using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BackdropsCore
{
    public class DefaultBackdrop : StellarObjectBackdrop
    {
        Rectangle rect, source;
        Texture2D texArt;

        private string contentName;

        public DefaultBackdrop(string artName)
        {
            contentName = artName;
        }

        public override void onFirstLoad(Color color, GraphicsDevice device, TextureFinder finder)
        {
            this.device = device;
            content = finder;
            //content = new ContentManager(services);
            //content.RootDirectory = "Content";
            colorKey = color;
            rect = new Rectangle();
            source = new Rectangle();
            calculateAspect();
            qbatch = new QuadBatch(device);
        }

        public override void onPrepare(BackdropRenderQuality quality)
        {
            texArt = content.findTexture(contentName);
            loadStellarObjects();
        }

        public override void onShutdown()
        {
            texArt = null;
            unloadStellarObjects();
            content.Unload();
        }

        public override BackdropInstance getInstance(byte density, float noise)
        {
            return new GenericEmptyInstance(colorKey);
        }

        //called when the game screen changes size
        public override void resize()
        {
            calculateAspect();
            if (texArt != null)
            {
                letterBox();
            }
        }

        public override Vector4 drawLayer0(RenderTarget2D backdropTarget, RenderTarget2D rtNebulaNormal, RenderTarget2D rtNebulaDepth, RenderTarget2D rtNebulaMeta, RenderTarget2D rtNebulaStars, int index, BackdropInstance[] instance,  SpriteBatch batch, bool isCurrent, float[] value, Vector3[] cameraPos, Point[] points, Color lightColor, Color ambLightColor, float lightIntensity, Vector3 lightDirection, ref Vector4 fogCloudColor)
        {
            Color drawColor = Color.White;
            drawColor.A = (byte)(255f * value[index]);
            
            batch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            batch.Draw(texArt, rect, source, drawColor);

            batch.End();

            return lightShaftSet;
        }

        //draws your instance. Backdrop manager handles draw order of multiple instances when transitioning between zones
        public override void drawLayer1(RenderTarget2D target, int index, BackdropInstance[] instance, SpriteBatch batch, bool isCurrent, float[] value, Vector3[] cameraPos, Color lightColor, Color ambLightColor, float lightIntensity, Vector3 lightDirection)
        {
            Color drawColor = Color.White;
            //if (!isCurrent)
            //{
                drawColor.A = (byte)(255f * value[index]);
            //}

            Vector3 cameraTarget = cameraPos[index];
            cameraTarget.Z = 0;
            Matrix view = Matrix.CreateLookAt(cameraPos[index], cameraTarget, Vector3.Up);
            Matrix project = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 10000000);

            spriteBasic.Parameters["View"].SetValue(view);
            spriteBasic.Parameters["Projection"].SetValue(project);

            drawStellarObjects(batch, isCurrent, value[index], cameraPos[index]);
        }

        private void letterBox()
        {
            float sourceRatio = texArt.Width / (float)texArt.Height;

            rect.Width = device.Viewport.Width;
            rect.Height = device.Viewport.Height;
            float ratio = rect.Width / (float)rect.Height;

            if (ratio == sourceRatio)
            {
                source.Width = texArt.Width;
                source.Height = texArt.Height;
            }
            else if (ratio > sourceRatio)//wider, so we crop top and bot
            {
                source.Width = texArt.Width;
                source.Height = (int)(source.Width / ratio);
            }
            else
            {
                source.Height = texArt.Height;
                source.Width = (int)(source.Height * ratio);
            }
            source.X = (texArt.Width - source.Width) / 2;
            source.Y = (texArt.Height - source.Height) / 2;
        }
    }
}
