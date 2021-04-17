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
    public class ObjectFieldBackdrop : StellarObjectBackdrop
    {
        Rectangle rect, source;
        Texture2D texArt;

        private string contentName;

        private string[] assetNames;
        private Texture2D[] assets;
        private Vector2[] origins;
        private float[] assetScales;
        private float zStart, zRange;
        private int quantity;

        private Rectangle viewableArea, screenRect;
        private string[] parallaxNames = null;
        private Texture2D[] parallaxAssets = null;
        private float[] parallaxDepths;

        public ObjectFieldBackdrop(string artName, string[] names, float[] scales, int itemQuantity, float startDepth, float depthRange)
        {
            contentName = artName;
            assetNames = names;
            assetScales = scales;
            quantity = itemQuantity;
            zStart = startDepth;
            zRange = depthRange;
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
            assets = new Texture2D[assetNames.Length];
            origins = new Vector2[assetNames.Length];
            for (int i = 0; i < assetNames.Length; i++)
            {
                assets[i] = content.findTexture(assetNames[i]);
                origins[i] = new Vector2(assets[i].Width / 2, assets[i].Height / 2);
            }
            viewableArea = new Rectangle();
            if (parallaxNames != null)
            {
                parallaxAssets = new Texture2D[parallaxNames.Length];
                for (int i = 0; i < parallaxNames.Length; i++)
                {
                    parallaxAssets[i] = content.findTexture(parallaxNames[i]);
                }
            }
        }

        public override void onShutdown()
        {
            texArt = null;
            assets = null;
            origins = null;
            unloadStellarObjects();
            content.Unload();
            parallaxAssets = null;
        }

        public void setParallaxLayers(string[] layerAssetNames, float[] depths)
        {
            parallaxNames = layerAssetNames;
            parallaxDepths = depths;
        }

        public override BackdropInstance getInstance(byte density, float noise)
        {
            Random random = new Random((int)((noise * 1000000f) - 500000f));
            ObjectFieldInstance instance = new ObjectFieldInstance(colorKey, assets.Length);

            for (int i = 0; i < assets.Length; i++)
            {
                instance.positions[i] = new List<Vector3>();
                instance.rotations[i] = new List<float>();
            }

            int type = 0;

            int gridRoids = (int)Math.Sqrt(quantity * 0.8);
            int randRoids = (int)(quantity * 0.2);

            float halfwidth = 120000;//how far out it extends
            float totalWide = 2 * halfwidth;
            float gridStep = totalWide / gridRoids;

            for (int y = 0; y < gridRoids; y++)
            {
                for (int x = 0; x < gridRoids; x++)
                {
                    if (random.NextDouble() > 0.08)
                    {
                        Vector3 position = new Vector3();
                        position.X = (gridStep * x) - halfwidth + (float)(random.NextDouble() * gridStep);
                        position.Y = (gridStep * y) - halfwidth + (float)(random.NextDouble() * gridStep);
                        position.Z = zStart - (float)(random.NextDouble() * zRange);
                        float rot = (float)(random.NextDouble() * MathHelper.TwoPi);
                        instance.positions[type].Add(position);
                        instance.rotations[type].Add(rot);

                        type++;
                        if (type >= assets.Length)
                        {
                            type = 0;
                        }
                    }
                }
            }
            for (int i = 0; i < randRoids; i++)
            {
                Vector3 position = new Vector3((float)((random.NextDouble() * totalWide) - halfwidth), (float)((random.NextDouble() * totalWide) - halfwidth), zStart - (float)(random.NextDouble() * zRange));
                float rot = (float)(random.NextDouble() * MathHelper.TwoPi);
                instance.positions[type].Add(position);
                instance.rotations[type].Add(rot);

                type++;
                if (type >= assets.Length)
                {
                    type = 0;
                }
            }

            return instance;
        }

        public override void resize()
        {
            calculateAspect();
            screenRect = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
            if (texArt != null)
            {
                letterBox();
            }
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

        public override Vector4 drawLayer0(RenderTarget2D backdropTarget, RenderTarget2D rtNebulaNormal, RenderTarget2D rtNebulaDepth, RenderTarget2D rtNebulaMeta, RenderTarget2D rtNebulaStars, int index, BackdropInstance[] instance, SpriteBatch batch, bool isCurrent, float[] value, Vector3[] cameraPos, Point[] points, Color lightColor, Color ambLightColor, float lightIntensity, Vector3 lightDirection, ref Vector4 fogCloudColor)
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
            ObjectFieldInstance field = instance[index] as ObjectFieldInstance;

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

            //batch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            //batch.Draw(texArt, rect, source, drawColor);

            //batch.End();

            Color objColor = new Color((byte)(ambLightColor.R / 3), (byte)(ambLightColor.G / 3), (byte)(ambLightColor.B / 3), (byte)255);

            drawStellarObjects(batch, isCurrent, value[index], cameraPos[index]);

            device.BlendState = BlendState.NonPremultiplied;
            for (int i = 0; i < assets.Length; i++)
            {
                qbatch.Begin(spriteBasic);
                int c = field.positions[i].Count;
                for (int p = 0; p < c; p++)
                {
                    //batch.draw(asset[i], field.positions[i][p], field.rotations[i][p])
                    qbatch.Draw(assets[i], field.positions[i][p], objColor, field.rotations[i][p], assetScales[i]);
                }
                qbatch.End();
            }

            if (parallaxAssets != null)
            {
                batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                for (int i = 0; i < parallaxAssets.Length; i++)
                {
                    viewableArea.Height = (int)(2 * (cameraPos[index].Z + parallaxDepths[i]) * Math.Tan(MathHelper.PiOver2 * 0.5f));
                    viewableArea.Width = (int)(viewableArea.Height * aspectRatio);
                    viewableArea.X = (int)(cameraPos[index].X - (viewableArea.Width / 2));
                    viewableArea.Y = (int)(cameraPos[index].Y - (viewableArea.Height / 2));
                    batch.Draw(parallaxAssets[i], screenRect, viewableArea, drawColor);
                }
                batch.End();
            }
        }
    }
}
