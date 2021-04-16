using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WaywardExtensions;

namespace BackdropExtension
{
    public class BackdropTestApp : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;

        //backdrop you are testing
        BackdropExt backdrop;
        BackdropInstance instance;
        RenderTarget2D backdropTarget;
        TextureBatch[] textures;
        Texture2D opticalLUT;
        OpticalDepthLUT opticalDepthMaker;

        float planetRadius = 100;
        float atmosphereRadius = 120;
        float rayleigh = 0.15f;
        static Vector3 waveLenghts = new Vector3(0.470f,0.550f,0.650f) * 1.0f;
        Vector3 waveLenghts4 = (new Vector3((float)Math.Pow(waveLenghts.X, 4), (float)Math.Pow(waveLenghts.Y, 4), (float)Math.Pow(waveLenghts.Z, 4))); //470,550,650

        //parameters
        Color domLight = Color.White;
        Color ambLight = new Color(0.2f,0.2f,0.2f);
        Vector3 cameraPos = new Vector3(0, 0, 850);
        Vector3 lightDir = new Vector3(-1, -1, 0.2f);

        float lightIntensity = 1.2f;

        public BackdropTestApp()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = true;

            Content.RootDirectory = "Content";

            this.IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;

            //opticalDepthMaker = new OpticalDepthLUT(device);
            //opticalLUT = opticalDepthMaker.MakeOpticalLUT(planetRadius,atmosphereRadius,rayleigh,0.10f);

            textures = new TextureBatch[3];

            string[] asteroidTexNames;
            TextureBatch asteroidFieldAssets = new TextureBatch();
            asteroidTexNames = new string[11];
            asteroidTexNames[0] = "asteroidA";
            asteroidTexNames[1] = "asteroidB";
            asteroidTexNames[2] = "asteroidC";
            asteroidTexNames[3] = "asteroidD";
            asteroidTexNames[4] = "asteroidE";
            asteroidTexNames[5] = "asteroidF";
            asteroidTexNames[6] = "asteroidG";
            asteroidTexNames[7] = "asteroidH";
            asteroidTexNames[8] = "asteroidI";
            asteroidTexNames[9] = "asteroidJ";
            asteroidTexNames[10] = "asteroidK";
            asteroidFieldAssets.diffNames = asteroidTexNames;
            asteroidFieldAssets.collideNames = asteroidTexNames;
            asteroidTexNames = new string[11];
            asteroidTexNames[0] = "asteroidBumpA";
            asteroidTexNames[1] = "asteroidBumpB";
            asteroidTexNames[2] = "asteroidBumpC";
            asteroidTexNames[3] = "asteroidBumpD";
            asteroidTexNames[4] = "asteroidBumpE";
            asteroidTexNames[5] = "asteroidBumpF";
            asteroidTexNames[6] = "asteroidBumpG";
            asteroidTexNames[7] = "asteroidBumpH";
            asteroidTexNames[8] = "asteroidBumpI";
            asteroidTexNames[9] = "asteroidBumpJ";
            asteroidTexNames[10] = "asteroidBumpK";
            asteroidFieldAssets.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[11];
            asteroidTexNames[0] = "asteroidSpecA";
            asteroidTexNames[1] = "asteroidSpecB";
            asteroidTexNames[2] = "asteroidSpecC";
            asteroidTexNames[3] = "asteroidSpecD";
            asteroidTexNames[4] = "asteroidSpecE";
            asteroidTexNames[5] = "asteroidSpecF";
            asteroidTexNames[6] = "asteroidSpecG";
            asteroidTexNames[7] = "asteroidSpecH";
            asteroidTexNames[8] = "asteroidSpecI";
            asteroidTexNames[9] = "asteroidSpecJ";
            asteroidTexNames[10] = "asteroidSpecK";
            asteroidFieldAssets.specNames = asteroidTexNames;
            bool[] digging = new bool[11];
            for (int i = 0; i < 11; i++)
            {
                digging[i] = true;
            }
            asteroidFieldAssets.assetDiggable = digging;
            textures[0] = asteroidFieldAssets;

            TextureBatch wreckage = new TextureBatch();
            asteroidTexNames = new string[11];
            asteroidTexNames[0] = "wreck_0_dif";
            asteroidTexNames[1] = "wreck_1_dif";
            asteroidTexNames[2] = "wreck_2_dif";
            asteroidTexNames[3] = "wreck_3_dif";
            asteroidTexNames[4] = "wreck_4_dif";
            asteroidTexNames[5] = "wreck_5_dif";
            asteroidTexNames[6] = "wreck_6_dif";
            asteroidTexNames[7] = "wreck_7_dif";
            asteroidTexNames[8] = "wreck_8_dif";
            asteroidTexNames[9] = "wreck_9_dif";
            asteroidTexNames[10] = "wreck_10_dif";
            wreckage.diffNames = asteroidTexNames;
            wreckage.collideNames = asteroidTexNames;
            asteroidTexNames = new string[11];
            asteroidTexNames[0] = "wreck_0_norm";
            asteroidTexNames[1] = "wreck_1_norm";
            asteroidTexNames[2] = "wreck_2_norm";
            asteroidTexNames[3] = "wreck_3_norm";
            asteroidTexNames[4] = "wreck_4_norm";
            asteroidTexNames[5] = "wreck_5_norm";
            asteroidTexNames[6] = "wreck_6_norm";
            asteroidTexNames[7] = "wreck_7_norm";
            asteroidTexNames[8] = "wreck_8_norm";
            asteroidTexNames[9] = "wreck_9_norm";
            asteroidTexNames[10] = "wreck_10_norm";
            wreckage.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[11];
            asteroidTexNames[0] = "wreck_0_spec";
            asteroidTexNames[1] = "wreck_1_spec";
            asteroidTexNames[2] = "wreck_2_spec";
            asteroidTexNames[3] = "wreck_3_spec";
            asteroidTexNames[4] = "wreck_4_spec";
            asteroidTexNames[5] = "wreck_5_spec";
            asteroidTexNames[6] = "wreck_6_spec";
            asteroidTexNames[7] = "wreck_7_spec";
            asteroidTexNames[8] = "wreck_8_spec";
            asteroidTexNames[9] = "wreck_9_spec";
            asteroidTexNames[10] = "wreck_10_spec";
            wreckage.specNames = asteroidTexNames;
            asteroidTexNames = new string[11];
            asteroidTexNames[0] = "wreck_0_emit";
            asteroidTexNames[1] = "wreck_1_emit";
            asteroidTexNames[2] = "wreck_2_emit";
            asteroidTexNames[3] = "wreck_3_emit";
            asteroidTexNames[4] = "wreck_4_emit";
            asteroidTexNames[5] = "wreck_5_emit";
            asteroidTexNames[6] = "wreck_6_emit";
            asteroidTexNames[7] = "wreck_7_emit";
            asteroidTexNames[8] = "wreck_8_emit";
            asteroidTexNames[9] = "wreck_9_emit";
            asteroidTexNames[10] = "wreck_10_emit";
            wreckage.emitNames = asteroidTexNames;
            digging = new bool[11];
            for (int i = 0; i < 11; i++)
            {
                digging[i] = false;
            }
            wreckage.assetDiggable = digging;
            textures[1] = asteroidFieldAssets;

            TextureBatch bigwrecks = new TextureBatch();
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "bigwreck_0_dif";
            bigwrecks.diffNames = asteroidTexNames;
            bigwrecks.collideNames = asteroidTexNames;
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "bigwreck_0_norm";
            bigwrecks.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "bigwreck_0_spec";
            bigwrecks.specNames = asteroidTexNames;
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "bigwreck_0_emit";
            bigwrecks.emitNames = asteroidTexNames;
            digging = new bool[1];
            for (int i = 0; i < 1; i++)
            {
                digging[i] = false;
            }
            bigwrecks.assetDiggable = digging;
            textures[2] = bigwrecks;
            //sectorTextures["wreckage"] = wreckage;

            //Prefab myDickButtTest = new Prefab(Directory.GetCurrentDirectory() + @"\Data\Debugtestfab.pfb", "asteroidsBlack", asteroidFieldAssets);

            Prefab pirateMaze0 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\pirMzBas.pfb");
            Prefab asteroidStation = new Prefab(Directory.GetCurrentDirectory() + @"\Data\abndnRoidStn3.pfb");

            PlanetRenderSettings samplePlanet = new PlanetRenderSettings();
            samplePlanet.position = new Vector3(0, 0, 60000);
            samplePlanet.rotation = Vector3.Zero;
            samplePlanet.rotationRate = new Vector3(0, 0.01f, 0);
            samplePlanet.color1 = new Vector3(0.3f, 0.1f, 0.0f);  //Colors, generaly keep them realistic not very saturaded, brown works, there might be little trick required for vegetation planets where green will be a thing at Color2 or/And 3
            samplePlanet.color2 = new Vector3(0.545f, 0.4f, 0.294f);
            samplePlanet.color3 = new Vector3(0.894f, 0.954f, 0.978f);
            samplePlanet.color4 = new Vector3(0.314f, 0.141f, 0.075f);
            samplePlanet.phase = new Vector3(0.17f, 0.45f, 0.7f);
            samplePlanet.emissionColor = new Vector4(0, 0.424f, 1, 1);
            samplePlanet.fluidColor = new Vector4(0.914f, 0.34f, 0.075f, 0);
            samplePlanet.fluidEmitIntensity = 2.2f;
            samplePlanet.mask = 0;
            samplePlanet.diffuse = 0;
            samplePlanet.height = 0;
            samplePlanet.normal = 0;
            samplePlanet.emission = 0;
            samplePlanet.atmosphere = 0;
            samplePlanet.surfaceTilling = 4;
            samplePlanet.radius = 300;
            samplePlanet.hasAtmosphere = true;
            samplePlanet.waveLenghts4 = new Vector3((float)Math.Pow(0.470f, 4), (float)Math.Pow(0.550f, 4), (float)Math.Pow(0.650f, 4));

            

            ObjectFieldRev2 field = new ObjectFieldRev2("NebulaHome", asteroidFieldAssets, null, 500, -350, 10000);


            //field.setPlanetParams(samplePlanet);
            field.masks = new string[1];
            field.masks[0] = "dynplanetmask";
            field.diffuses = new string[1];
            field.diffuses[0] = "TerrainCombined1";
            //field.diffuses[1] = "TerrainCombined2";
            //field.diffuses[2] = "TerrainCombined3";
            field.heights = new string[3];
            field.heights[0] = "Planet_Height1";
            field.heights[1] = "Planet_Height2";
            field.heights[2] = "Planet_Height3";
            field.normals = new string[3];
            field.normals[0] = "Planet_Normal1";
            field.normals[1] = "Planet_Normal2";
            field.normals[2] = "Planet_Normal3";
            field.emissions = new string[1];
            field.emissions[0] = "dynplanetlights";
            field.roughnesss = new string[1];
            field.roughnesss[0] = "Lave_surf_spec";
            field.lutParameters = new AtmosParams[4];

            AtmosParams atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 9500 * 1.5f;
            atmosphere.fOuterRadius = 9550 * 1.5f;
            atmosphere.fRayleighScaleHeight = 0.15f;
            atmosphere.fMieScaleHeight = 0.10f;
            field.lutParameters[0] = atmosphere;

            atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 11000;
            atmosphere.fOuterRadius = 11100;
            atmosphere.fRayleighScaleHeight = 0.15f;
            atmosphere.fMieScaleHeight = 0.10f;
            field.lutParameters[1] = atmosphere;

            atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 11000;
            atmosphere.fOuterRadius = 11300;
            atmosphere.fRayleighScaleHeight = 0.15f;
            atmosphere.fMieScaleHeight = 0.10f;
            field.lutParameters[2] = atmosphere;

            atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 14000;
            atmosphere.fOuterRadius = 14600;
            atmosphere.fRayleighScaleHeight = 0.15f;
            atmosphere.fMieScaleHeight = 0.10f;
            field.lutParameters[3] = atmosphere;
            //field.lutParameters = new AtmosParams[1];
            //AtmosParams atmosphere = new AtmosParams();
            //atmosphere.fInnerRadius = 10000;
            //atmosphere.fOuterRadius = 10900;
            //atmosphere.fRayleighScaleHeight = 0.15f;
            //atmosphere.fMieScaleHeight = 0.10f;
            //field.lutParameters[0] = atmosphere;
            //atmosphere = new AtmosParams();
            //atmosphere.fInnerRadius = 12000;
            //atmosphere.fOuterRadius = 13000;
            //atmosphere.fRayleighScaleHeight = 0.19f;
            //atmosphere.fMieScaleHeight = 0.10f;
            // field.lutParameters[1] = atmosphere;

            //Prefab asteroidStation = new Prefab(Directory.GetCurrentDirectory() + @"\Data\astStn.pfb");

            backdrop = field;
            FancyFieldTerrainGenerator feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeVariableDensity(1400, -0.2f, 1);
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.addPrefab(asteroidStation);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 4;

            Random rand = new Random();

            feildGen.generate(255, (float)rand.NextDouble(), 0, 0);

            //field.addPlanet(new Vector3(0, 0, 0), planetRadius, atmosphereRadius, Vector3.Zero, new Vector3(0.0f, 0.1f, 0.0f), opticalLUT, waveLenghts4);
            lightDir = Vector3.Normalize(lightDir);
            base.Initialize();
        }

        protected override void LoadContent()
        {


            foreach (TextureBatch b in textures)
            {
                b.onFirstLoad(this.Services);
            }

            backdrop.onFirstLoad(Color.Black, device, this.Services);
            backdrop.onPrepare(BackdropRenderQuality.ultra);
        }

        protected override void BeginRun()
        {
            Random r = new Random();
            instance = backdrop.getInstance(255, (float)r.NextDouble());
            //instance = backdrop.getInstance(255, 0.6200628f);
            //instance = backdrop.getInstance(127, 0.41f);

            makeTarget();
            backdrop.resize();
        }

        private void makeTarget()
        {
            if (backdropTarget != null)
            {
                backdropTarget.Dispose();
            }
            PresentationParameters pp = device.PresentationParameters;
            SurfaceFormat format = SurfaceFormat.HalfVector4;
            backdropTarget = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false,
                                       format, DepthFormat.Depth24, pp.MultiSampleCount,
                                       RenderTargetUsage.DiscardContents);
        }

        protected override void EndRun()
        {
            base.EndRun();
        }

        protected override void UnloadContent()
        {
            backdrop.onShutdown();
        }


        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (graphics.PreferredBackBufferWidth != Window.ClientBounds.Width || graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
            {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();
            }
            makeTarget();
            backdrop.resize();
        }

        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            instance.update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //device.SetRenderTarget(backdropTarget);
            //device.Clear(Color.Black);
            //backdrop.drawLayer0(backdropTarget, instance, spriteBatch, true, 1, cameraPos, domLight, ambLight, lightIntensity, lightDir);
            //backdrop.drawLayer1(backdropTarget, instance, spriteBatch, true, 1, cameraPos, domLight, ambLight, lightIntensity, lightDir);
            //device.SetRenderTarget(null);

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, RasterizerState.CullNone);
            //spriteBatch.Draw(backdropTarget, Vector2.Zero, Color.White);
            ////spriteBatch.Draw(opticalLUT, Vector2.Zero + new Vector2(50,50), Color.Red);
            //spriteBatch.End();

            

            base.Draw(gameTime);
        }
    }
}
