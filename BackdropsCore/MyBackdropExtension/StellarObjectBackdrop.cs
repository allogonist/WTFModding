using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BackdropExtension
{
    public class StellarObjectBackdrop : BackdropExt
    {
        protected GraphicsDevice device;
        protected ContentManager content;

        protected Color colorKey;

        protected List<StellarObject> stellarObjects;
        protected Effect spriteBasic;

        public Vector4 lightShaftSet = Vector4.Zero; //(threshold, bias, placeholder, placeholder)


        protected float aspectRatio;
        protected QuadBatch qbatch;

        public Vector2 lightShaftOrgin = new Vector2(0.5f, 0.5f);
        protected Vector2 lightShaftRecomputedOrigin = new Vector2(0.5f, 0.5f);

        public virtual void onFirstLoad(Color color, GraphicsDevice device, IServiceProvider services)
        {
            this.device = device;
            content = new ContentManager(services);
            content.RootDirectory = "Content";
            colorKey = color;
            calculateAspect();
            qbatch = new QuadBatch(device);
        }

        public virtual void onPrepare(BackdropRenderQuality quality)
        {
            loadStellarObjects();

        }

        public void loadStellarObjects()
        {
            spriteBasic = content.Load<Effect>("spriteBasic");


            if (stellarObjects != null)
            {
                for (int i = 0; i < stellarObjects.Count; i++)
                {
                    try
                    {
                        stellarObjects[i].art = content.Load<Texture2D>(stellarObjects[i].assetName);
                    }
                    catch
                    {
                        stellarObjects.Remove(stellarObjects[i]);//remove the offender
                        loadStellarObjects();//start again from the beginning
                        return;//don't finish
                    }
                    stellarObjects[i].origin = new Vector2(stellarObjects[i].art.Width / 2, stellarObjects[i].art.Height / 2);
                }
            }
        }

        public void unloadStellarObjects()
        {
            if (stellarObjects != null)
            {
                for (int i = 0; i < stellarObjects.Count; i++)
                {
                    stellarObjects[i].art = null;
                }
            }
        }

        public virtual void onShutdown()
        {
            unloadStellarObjects();
            content.Unload();
        }

        public virtual BackdropInstance getInstance(byte density, float noise)
        {
            return new GenericEmptyInstance(colorKey);
        }

        public void addStellarObject(StellarObject item)
        {
            if (stellarObjects == null)
            {
                stellarObjects = new List<StellarObject>();
            }
            stellarObjects.Add(item);
        }

        public virtual void resize()
        {
            calculateAspect();
        }

        public void calculateAspect()
        {
            aspectRatio = (float)device.Viewport.Width / (float)device.Viewport.Height;
        }

        public void drawStellarObjects(SpriteBatch batch, bool isCurrent, float value, Vector3 cameraPos)
        {
            if (stellarObjects != null)
            {
                Color drawColor = Color.White;
                //if (!isCurrent)
                //{
                drawColor.A = (byte)(255f * value);
                //}

                device.BlendState = BlendState.NonPremultiplied;
                //batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                for (int i = 0; i < stellarObjects.Count; i++)
                {
                    qbatch.Begin(spriteBasic);
                    StellarObject s = stellarObjects[i];
                    //spriteBasic.Parameters["depth"].SetValue(s.position.Z);
                    //Vector3 pPos = device.Viewport.Project(stellarObjects[i].position, project, view, Matrix.Identity);
                    //Vector2 dPos = new Vector2(s.position.X, s.position.Y);
                    qbatch.Draw(s.art, s.position, drawColor, s.rotation, s.scale);
                    qbatch.End();
                }
            }
        }

        public virtual Texture2D returnLayerZero()
        {
            return null;
        }

        public virtual Vector4 drawLayer0(RenderTarget2D backdropTarget, RenderTarget2D rtNebulaNormal, RenderTarget2D rtNebulaDepth, RenderTarget2D rtNebulaMeta, RenderTarget2D rtNebulaStars, int index, BackdropInstance[] instance, SpriteBatch batch, bool isCurrent, float[] value, Vector3[] cameraPos, Point[] gridPositions, Color lightColor, Color ambLightColor, float lightIntensity, Vector3 lightDirection, ref Vector4 cloudsColor)
        {
            return lightShaftSet;
        }

        public virtual void drawLayer1(RenderTarget2D target, int index, BackdropInstance[] instance, SpriteBatch batch, bool isCurrent, float[] value, Vector3[] cameraPos, Color lightColor, Color ambLightColor, float lightIntensity, Vector3 lightDirection)
        {

            Vector3 cameraTarget = cameraPos[index];
            cameraTarget.Z = 0;
            Matrix view = Matrix.CreateLookAt(cameraPos[index], cameraTarget, Vector3.Up);
            Matrix project = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 10000000);

            spriteBasic.Parameters["View"].SetValue(view);
            spriteBasic.Parameters["Projection"].SetValue(project);

            drawStellarObjects(batch, isCurrent, value[index], cameraPos[index]);
        }
    }
}
