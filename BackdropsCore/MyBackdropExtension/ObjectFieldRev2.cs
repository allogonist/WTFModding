using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Win32.SafeHandles;

namespace BackdropsCore
{
    public class ObjectFieldRev2 : StellarObjectBackdrop
    {
        Rectangle rect, source;
        Texture2D texArt;

        private string contentName;//the backdrop image

        //private string[] assetNames;//the asteroids
        //private Texture2D[] assets;
        private TextureBatch assets;
        //private Vector2[] origins;
        private float[] assetScales;
        private float zStart, zRange;
        private int quantity;

        static Point[] previousKeys = new Point[4];

        Color transparentBlack = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        private float[] weights;

        Vector3[] omni_positionArray = new Vector3[8];
        float[] omni_intensityArray = new float[8];
        float[] omni_typeArray = new float[8];
        float[] omni_attenuationArray = new float[8];

        private Vector3 invY;

        private Rectangle viewableArea, screenRect;

        private string[] parallaxNames = null;//scrolling tiled parallax layers
        private Texture2D[] parallaxAssets = null;
        private float[] parallaxDepths;

        private bool hasPlanet = false;
        private bool randomizePlanet = true;
        private Model planetSurface;
        private Model planetAtmosphere;
        private Effect planetEffect;


        private Effect backgroundEffect;
        private EffectParameter backgroundEffect_text1;
        private EffectParameter backgroundEffect_text2;
        private EffectParameter backgroundEffect_text3;
        private EffectParameter backgroundEffect_text4;
        
        private EffectParameter backgroundEffect_omniType;
        private EffectParameter backgroundEffect_omniIntensity;
        private EffectParameter backgroundEffect_omniAttenuation;
        private EffectParameter backgroundEffect_omniPosition;
        
        private EffectParameter backgroundEffect_view;
        private EffectParameter backgroundEffect_projection;
        private EffectParameter backgroundEffect_viewport;

        private EffectParameter backgroundEffect_parameterA;
        private EffectParameter backgroundEffect_parameterB;
        private EffectParameter backgroundEffect_parameterC;
        private EffectParameter backgroundEffect_parameterD;

        private EffectParameter backgroundEffect_colorA;
        private EffectParameter backgroundEffect_colorB;
        private EffectParameter backgroundEffect_colorC;
        private EffectParameter backgroundEffect_colorD;
        private EffectParameter backgroundEffect_colorE;
        private EffectParameter backgroundEffect_colorLA;
        private EffectParameter backgroundEffect_colorLB;

        EffectParameter planetView;
        EffectParameter planetProject;

        EffectParameter planetLightInt;
        EffectParameter planetDomCol;
        EffectParameter planetLightDir;
        EffectParameter planetAmbient;
        EffectParameter planetCameraPos;

        EffectParameter planetMask;
        EffectParameter planetDiffuse;
        EffectParameter planetHeight;
        EffectParameter planetNormal;
        EffectParameter planetRough;
        EffectParameter planetEmiss;
        EffectParameter planetEmissCol;

        EffectParameter planetFluidCol;
        EffectParameter planetFluidEmiss;

        EffectParameter planetPhase;

        EffectParameter planetCol1;
        EffectParameter planetCol2;
        EffectParameter planetCol3;
        EffectParameter planetCol4;

        EffectParameter planetFluidLevel;
        EffectParameter planetSurfaceTilling;
        EffectParameter planetCenterPos;
        EffectParameter planetInnerRadius;
        EffectParameter planetOuterRadius;
        EffectParameter planetLut;
        EffectParameter planetV3Wavelength;

        EffectParameter planetWorld;

        EffectTechnique planetTechnique_atmo;
        EffectParameter planetLightDirObj;

        EffectTechnique backgroundTech_nebulaComp;
        EffectTechnique backgroundTech_normalMaps;
        EffectTechnique backgroundTech_starDust;
        EffectTechnique backgroundTech_stars;
       
       
       
       

        EffectParameter backgroundEffect_CameraPos;
        EffectParameter backgroundEffect_viewportScale;

        private EffectParameter backgroundEffect_positionOffset;
        private EffectParameter backgroundEffect_maskState;

        private EffectParameter backgroundEffect_lightCount;

        //private Vector3 planet;
        //private Vector3 planetRotation;
        //private Vector3 planetRotationRate;
        //private float planetScale;
        //private float atmoScale;
        //private Texture2D[] testPlanetTextures;
        private Texture2D[] mask;
        private Texture2D[] diffuse;
        private Texture2D[] height;
        private Texture2D[] normal;
        private Texture2D[] emission;
        private Texture2D[] roughness;
        private Texture2D[] luts;
        //private Vector3 wavelenghts;
        //private bool hasAtmosphere = false;
        private uint uidNext = 0;

        private int qualityDivider = 1;
        private bool qualityStarDust = true;

        //area for storing planet creation settings
        public PlanetRenderSettings prefabSettings;
        public string[] masks = null;
        public string[] diffuses = null;
        public string[] heights = null;
        public string[] normals = null;
        public string[] emissions = null;
        public string[] roughnesss = null;
        public AtmosParams[] lutParameters = null;
        public float maxInnerRadius = 900;
        public float minInnerRadius = 700;
        public int planetTypeGeneration = -1;
        public Vector3 atmoWaveLengths = Vector3.Zero;

        //for cloud stuff
        public TextureBatch cloudAssets = null;
        public float cloudHdrHighPhase, cloudAlphaMin, cloudRoughness, cloudEdgeIntensity;
        public Vector3 cloudsLightA, cloudsLightB, cloudGeneralMult, cloudHdrHighPhaseColor, cloudDeepBackground, cloudStarColor, cloudStarDustColor;
        public Vector4 fogCloudsColor;

        public ObjectFieldRev2(string artName, TextureBatch assetSource, float[] scales, int itemQuantity, float startDepth, float depthRange)
        {
            assets = assetSource;
            contentName = artName;
            assetScales = scales;
            quantity = itemQuantity;
            zStart = startDepth;
            zRange = depthRange;
            lightShaftRecomputedOrigin = lightShaftOrgin;
        }

        public ObjectFieldRev2(TextureBatch assetSource, float[] scales, int itemQuantity, float startDepth, float depthRange)
        {
            assets = assetSource;
            contentName = null;
            assetScales = scales;
            quantity = itemQuantity;
            zStart = startDepth;
            zRange = depthRange;
            lightShaftRecomputedOrigin = lightShaftOrgin;
        }

        public override void onFirstLoad(Color color, GraphicsDevice device, TextureFinder finder)
        {
            this.device = device;
            content = finder;
            colorKey = color;
            rect = new Rectangle();
            source = new Rectangle();
            calculateAspect();
            qbatch = new QuadBatch(device);
            invY = new Vector3(1, -1, 1);
            if (lutParameters != null)
            {
                maxInnerRadius = lutParameters[0].fInnerRadius;
                foreach (AtmosParams p in lutParameters)
                {
                    if (p.fInnerRadius > maxInnerRadius)
                    {
                        maxInnerRadius = p.fInnerRadius;
                    }
                }
            }


        }

        //called by background thread
        public override void onPrepare(BackdropRenderQuality quality)
        {
            //finder is a new instance every time but we re-use our first one
            //since we might be prepared multiple times
            //if(content == null)
            //{
            //    content = finder;
            //}
            if(quality == BackdropRenderQuality.ultra || quality == BackdropRenderQuality.high)
            {
                qualityDivider = 1;
                qualityStarDust = true;
            }
            else if (quality == BackdropRenderQuality.medium || quality == BackdropRenderQuality.low)
            {
                qualityDivider = 2;
                qualityStarDust = true;
            }
            else if (quality == BackdropRenderQuality.potato)
            {
                qualityDivider = 2;
                qualityStarDust = false;
            }

            if (contentName != null)
            {
                texArt = content.findTexture(contentName);
            }
            loadStellarObjects();

            hasPlanet = masks != null && diffuses != null && diffuses != null && heights != null && normals != null && emissions != null && lutParameters != null;

            if (hasPlanet)
            {
                loadPlanetStuff();
            }

            //assets = new Texture2D[assetNames.Length];
            if(assets != null)
            {
                assets.load();
            }
            if (cloudAssets != null)
            {
                cloudAssets.load();
            }
            //origins = new Vector2[assets.assetDifs.Length];
            //for (int i = 0; i < assets.assetDifs.Length; i++)
            //{
            //    origins[i] = new Vector2(assets.assetDifs[i].Width / 2, assets.assetDifs[i].Height / 2);
            //}
            viewableArea = new Rectangle();
            if (parallaxNames != null)
            {
                parallaxAssets = new Texture2D[parallaxNames.Length];
                for (int i = 0; i < parallaxNames.Length; i++)
                {
                    parallaxAssets[i] = content.findTexture(parallaxNames[i]);
                }
            }

            backgroundEffect = content.findEffect("backgroundEffect");

            backgroundEffect_text1 = backgroundEffect.Parameters["texture1"];
            backgroundEffect_text2 = backgroundEffect.Parameters["texture2"];
            backgroundEffect_text3 = backgroundEffect.Parameters["texture3"];
            backgroundEffect_text4 = backgroundEffect.Parameters["texture4"];

            backgroundEffect_omniType = backgroundEffect.Parameters["LightType"];
            backgroundEffect_omniIntensity = backgroundEffect.Parameters["LightIntensity"];
            backgroundEffect_omniAttenuation = backgroundEffect.Parameters["LightDecay"];
            backgroundEffect_omniPosition = backgroundEffect.Parameters["LightPosition"];

            backgroundEffect_view = backgroundEffect.Parameters["View"];
            backgroundEffect_projection = backgroundEffect.Parameters["Projection"];
            backgroundEffect_viewport = backgroundEffect.Parameters["viewport"];

            backgroundEffect_parameterA = backgroundEffect.Parameters["ParameterA"];
            backgroundEffect_parameterB = backgroundEffect.Parameters["ParameterB"];
            backgroundEffect_parameterC = backgroundEffect.Parameters["ParameterC"];
            backgroundEffect_parameterD = backgroundEffect.Parameters["ParameterD"];

            backgroundEffect_colorA = backgroundEffect.Parameters["ColorA"];
            backgroundEffect_colorB = backgroundEffect.Parameters["ColorB"];
            backgroundEffect_colorC = backgroundEffect.Parameters["ColorC"];
            backgroundEffect_colorD = backgroundEffect.Parameters["ColorD"];
            backgroundEffect_colorE = backgroundEffect.Parameters["ColorE"];
            backgroundEffect_colorLA = backgroundEffect.Parameters["ColorLA"];
            backgroundEffect_colorLB = backgroundEffect.Parameters["ColorLB"];

            backgroundEffect_lightCount = backgroundEffect.Parameters["lightCount"];

            backgroundEffect.Parameters["targetPlaneZ"].SetValue(-110000f);

            backgroundEffect_positionOffset = backgroundEffect.Parameters["positionOffset"];
            backgroundEffect_maskState = backgroundEffect.Parameters["maskState"];

            backgroundEffect_CameraPos = backgroundEffect.Parameters["CameraPos"];
            backgroundEffect_viewportScale = backgroundEffect.Parameters["ViewportScale"];

            backgroundTech_nebulaComp = backgroundEffect.Techniques["nebulaComp"];
            backgroundTech_normalMaps = backgroundEffect.Techniques["spriteBasic"];
            backgroundTech_starDust = backgroundEffect.Techniques["starsDustBasic"];
            backgroundTech_stars = backgroundEffect.Techniques["starsBasic"];

            weights = new float[4];


                            
        }

        public override void onShutdown()
        {
            texArt = null;
            //assets = null;
            if(assets != null)
            {
                assets.release();
            }
            //origins = null;
            unloadStellarObjects();
            content.Unload();
            disposePlanetStuff();
            parallaxAssets = null;
            if(cloudAssets != null)
            {
                cloudAssets.release();
            }
        }

        private void loadPlanetStuff()
        {
            planetSurface = content.findModel("planetSurface");
            planetAtmosphere = content.findModel("GeoSphere"); //GeoSphere
            planetEffect = content.findEffect("planetEffect");



            foreach (ModelMesh mesh in planetSurface.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = planetEffect;
                }
            }

            foreach (ModelMesh mesh in planetAtmosphere.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = planetEffect;
                }
            }
            mask = new Texture2D[masks.Length];
            for (int i = 0; i < masks.Length; i++)
            {
                mask[i] = content.findTexture(masks[i]);
            }
            diffuse = new Texture2D[diffuses.Length];
            for (int i = 0; i < diffuses.Length; i++)
            {
                diffuse[i] = content.findTexture(diffuses[i]);
            }
            height = new Texture2D[heights.Length];
            for (int i = 0; i < heights.Length; i++)
            {
                height[i] = content.findTexture(heights[i]);
            }
            normal = new Texture2D[normals.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normal[i] = content.findTexture(normals[i]);
            }
            emission = new Texture2D[emissions.Length];
            for (int i = 0; i < emissions.Length; i++)
            {
                emission[i] = content.findTexture(emissions[i]);
            }
            roughness = new Texture2D[roughnesss.Length];
            for (int i = 0; i < roughnesss.Length; i++)
            {
                roughness[i] = content.findTexture(roughnesss[i]);
            }
            luts = new Texture2D[lutParameters.Length];
            for (int i = 0; i < lutParameters.Length; i++)
            {
                luts[i] = OpticalDepthLUT.MakeOpticalLUT(device, lutParameters[i].fInnerRadius, lutParameters[i].fOuterRadius, lutParameters[i].fRayleighScaleHeight, lutParameters[i].fMieScaleHeight);
            }

            planetView = planetEffect.Parameters["View"];
            planetProject = planetEffect.Parameters["Projection"];

            planetLightInt = planetEffect.Parameters["DominantIntensity"];
            planetDomCol = planetEffect.Parameters["DominantColor"];
            planetLightDir = planetEffect.Parameters["LightDirection"];
            planetAmbient = planetEffect.Parameters["Ambient"];
            planetCameraPos = planetEffect.Parameters["CameraPos"];

            planetMask = planetEffect.Parameters["Mask"];
            planetDiffuse = planetEffect.Parameters["Diffuse1"];
            planetHeight = planetEffect.Parameters["Height"];
            planetNormal = planetEffect.Parameters["Normal"];
            planetRough = planetEffect.Parameters["Roughness"];
            planetEmiss = planetEffect.Parameters["Emission"];
            planetEmissCol = planetEffect.Parameters["EmissionColor"];

            planetFluidCol = planetEffect.Parameters["FluidColor"];
            planetFluidEmiss = planetEffect.Parameters["FluidEmission"];

            planetPhase = planetEffect.Parameters["Phase"];

            planetCol1 = planetEffect.Parameters["Color1"];
            planetCol2 = planetEffect.Parameters["Color2"];
            planetCol3 = planetEffect.Parameters["Color3"];
            planetCol4 = planetEffect.Parameters["Color4"];

            planetFluidLevel = planetEffect.Parameters["FluidLevel"];
            planetSurfaceTilling = planetEffect.Parameters["SurfaceTilling"];
            planetCenterPos = planetEffect.Parameters["CenterPos"];
            planetInnerRadius = planetEffect.Parameters["fInnerRadius"];
            planetOuterRadius = planetEffect.Parameters["fOuterRadius"];
            planetLut = planetEffect.Parameters["LUT"];
            planetV3Wavelength = planetEffect.Parameters["v3Wavelength"];


            planetWorld = planetEffect.Parameters["World"];


            planetLightDirObj = planetEffect.Parameters["LightDirectionObj"];

            planetTechnique_atmo = planetEffect.Techniques["AtmosphereTechnique"];
        }

        private void disposePlanetStuff()
        {
            mask = null;
            diffuse = null;
            height = null;
            normal = null;
            emission = null;
            roughness = null;

            if(luts != null)
            { 
                for (int i = 0; i < luts.Length; i++)
                {
                    if(luts[i] != null)
                    { 
                        luts[i].Dispose();
                        luts[i] = null;
                    }
                }
            }
            planetEffect = null;
            planetSurface = null;
            planetAtmosphere = null;
        }

        public void setPlanetParams(PlanetRenderSettings planet)
        {
            randomizePlanet = false;
            prefabSettings = planet;
        }

        //public void addPlanet(Vector3 position, float scale, float scaleA, Vector3 startRotation, Vector3 rotationRate, Vector3 wavelengts4)
        //{
        //    hasPlanet = true;
        //    hasAtmosphere = true;
        //    planet = position;
        //    planet.Y *= -1;
        //    planetScale = scale;
        //    atmoScale = scaleA;
        //    planetRotation = startRotation;
        //    planetRotationRate = rotationRate;
        //    wavelenghts = wavelengts4;
        //}

        public void setClouds(TextureBatch cloudBatch, float hdrHigh, float alphaMin, float roughness, float edgeIntensity, Vector3 lightA, Vector3 lightB, Vector3 generalMult, Vector3 hdrHphase, Vector3 deepBackground, Vector3 starColor, Vector3 starDustColor)
        {
            cloudAssets = cloudBatch;
            cloudHdrHighPhase = hdrHigh;
            cloudAlphaMin = alphaMin;
            cloudRoughness = roughness;
            cloudEdgeIntensity = edgeIntensity;
            cloudsLightA = lightA;
            cloudsLightB = lightB;
            cloudGeneralMult = generalMult;
            cloudHdrHighPhaseColor = hdrHphase;
            cloudDeepBackground = deepBackground;
            cloudStarColor = starColor;
            cloudStarDustColor = starDustColor;
            fogCloudsColor = Vector4.Zero;
        }


        public void setClouds(TextureBatch cloudBatch, BiomeSettings biomeSettings)
        {
            cloudAssets =  cloudBatch;
            cloudHdrHighPhase = biomeSettings.hdrHigh;
            cloudAlphaMin = biomeSettings.alphaMin;
            cloudRoughness = biomeSettings.roughness;
            cloudEdgeIntensity = biomeSettings.edgeIntensity;
            cloudsLightA = biomeSettings.lightA;
            cloudsLightB = biomeSettings.lightB;
            cloudGeneralMult = biomeSettings.generalMult;
            cloudHdrHighPhaseColor = biomeSettings.hdrHphase;
            cloudDeepBackground = biomeSettings.deepBackground;
            cloudStarColor = biomeSettings.starColor;
            cloudStarDustColor = biomeSettings.starDustColor;
            fogCloudsColor = biomeSettings.cloudColor;
        }



        public void setParallaxLayers(string[] layerAssetNames, float[] depths)
        {
            parallaxNames = layerAssetNames;
            parallaxDepths = depths;
        }

        //called by main game thread
        public override BackdropInstance getInstance(byte density, float noise)
        {
            Random random = new Random((int)((noise * 1000000f) - 500000f));
            ObjectFieldInstance instance;
            if (assets != null)
            {
                instance = new ObjectFieldInstance(colorKey, assets.assetDifs.Length);
                for (int i = 0; i < assets.assetDifs.Length; i++)
                {
                    instance.positions[i] = new List<Vector3>();
                    instance.rotations[i] = new List<float>();
                }
            }
            else
            {
                instance = new ObjectFieldInstance(colorKey, 0);
            }

            if(quantity > 0 && assets != null)
            {
                int type = 0;

                int gridRoids = (int)Math.Sqrt(quantity * 0.8);
                int randRoids = (int)(quantity * 0.2);

                float halfwidth = 100000;//how far out it extends
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
                            if (type >= assets.assetDifs.Length)
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
                    if (type >= assets.assetDifs.Length)
                    {
                        type = 0;
                    }
                }
            }

            if (hasPlanet)
            {
                configurePlanet(instance, noise);
            }

            if(cloudAssets != null)
            {
                if (cloudAssets.assetBumps != null)
                {
                    instance.cloudSheet = cloudAssets;
                    instance.hasClouds = true;
                    instance.cloudFieldGroups = new CloudField[cloudAssets.assetBumps.Length];
                    int averageTypeCount = 46 / cloudAssets.assetBumps.Length; //40
                    for (int i = 0; i < cloudAssets.assetBumps.Length; i++)
                    {
                        int cloudTypeCount = (int)getRandomNumber(averageTypeCount - averageTypeCount / 2, averageTypeCount + averageTypeCount / 2, random);
                        instance.cloudFieldGroups[i].position = generateField(cloudTypeCount, 100000, 100000, 52500, -177500, random); //(cloudTypeCount, 100000, 100000, 25000, -195000, random);
                        instance.cloudFieldGroups[i].rotation = generateArray(cloudTypeCount, 0.0f, 6.0f, random);
                        float[] r = generateArray(cloudTypeCount, 0.1f, 0.7f, random);
                        instance.cloudFieldGroups[i].color = new Color[cloudTypeCount];
                        for (int n = 0; n < cloudTypeCount; n++)
                        {

                            instance.cloudFieldGroups[i].color[n] = new Color(1.0f, 1.0f, 1.0f, r[n]);
                        }
                    }
                    int lightCount = 2;
                    /*
                    if (random.Next() > 0.2f)
                        lightCount++;
*/
                    //instance.cloudLights.position = generateField(lightCount, 75000, 75000, 5000, -165000, random);
                    
                    instance.cloudLights.intensity = generateArray(lightCount, 1.94f, 2.04f, random);
                    instance.cloudLights.position = generateField(lightCount, 70000, 70000, 6000, -52000, random);
                    instance.cloudLights.attuneation = generateArray(lightCount, 125000.0f, 125000.0f, random);
                    instance.cloudLights.type = new float[lightCount];
                    for (int i = 0; i < lightCount; i++)
                    {
                        instance.cloudLights.type[i] = (i % 2);
                        //instance.cloudLights.type[i] = 0;
                    }
                }

                if (cloudAssets.assetDifs != null)
                {
                    int starCount = 250;//we can set this value programatically via parameters later or you can ignore it
                    instance.hasStars = true;//this will tell your renderer to draw clouds in the future

                    instance.starField.position = generateField(starCount, 100000, 100000, 45000, -255000, random);
                    instance.starField.scale = generateArray(starCount, 75.0f, 185.0f, random);
                    instance.starField.color = new Color[starCount];
                    float[] r = generateArray(starCount, 0.1f, 1.0f, random);
                    //used for rotation
                    float[] g = generateArray(starCount, 0.0f, 1.0f, random);
                    for (int i = 0; i < starCount; i++)
                    {

                        instance.starField.color[i] = new Color(1.0f, 1.0f, g[i], r[i]);
                    }

                    instance.starField.sliceID = generateArray(starCount, 0, 3, random);


                }

                if (cloudAssets.assetEmits != null && cloudAssets.assetSpecs != null )
                {
                    int starDustCount = 22;//we can set this value programatically via parameters later or you can ignore it
                    instance.hasStarDust = true;//this will tell your renderer to draw clouds in the future

                    instance.starDustField.position = generateField(starDustCount, 100000, 100000, 18000, -180000, random);
                    instance.starDustField.rotation = generateArray(starDustCount, 0.0f, 6.0f, random);
                    instance.starDustField.color = new Color[starDustCount];
                    instance.starDustField.scale = generateArray(starDustCount, 120.0f, 240.0f, random);

                    float[] r = generateArray(starDustCount, 0.12f, 0.32f, random);
                    for (int i = 0; i < starDustCount; i++)
                    {

                        instance.starDustField.color[i] = new Color(1.0f, 1.0f, 1.0f, r[i]);
                    }
                }
            }

            instance.instLightShaftSet = lightShaftSet;
            instance.cloudHdrHighPhase = cloudHdrHighPhase;
            instance.cloudAlphaMin = cloudAlphaMin;
            instance.cloudRoughness = cloudRoughness;
            instance.cloudEdgeIntensity = cloudEdgeIntensity;
            instance.cloudsLightA = cloudsLightA;
            instance.cloudsLightB = cloudsLightB;
            instance.cloudGeneralMult = cloudGeneralMult;
            instance.cloudHdrHighPhaseColor = cloudHdrHighPhaseColor;
            instance.cloudDeepBackground = cloudDeepBackground;
            instance.cloudStarColor = cloudStarColor;
            instance.cloudStarDustColor = cloudStarDustColor;
            instance.fogCloudsColor = fogCloudsColor;

            return instance;
        }

        //helper to generate 3d position within given block
        public Vector3[] generateField(int size, float xD, float yD, float zD, float zStart, Random random)
        {
            Vector3[] aField = new Vector3[size];

            for (int i = 0; i < size; i++)
            {
                aField[i] = getRandomInBlock(new Vector3(0, 0, zStart), new Vector3(xD, yD, zD), random);
            }
            return aField;
        }

        public float[] generateArray(int size, float minimum, float maximum, Random random)
        {
            float[] output = new float[size];
            for (int i = 0; i < size; i++)
            {
                output[i] = (float)getRandomNumber((double)(minimum), (double)(maximum), random);
            }
            return output;
        }

        public int[] generateArray(int size, int minimum, int maximum, Random random)
        {
            int[] output = new int[size];
            for (int i = 0; i < size; i++)
            {
                output[i] = (int)getRandomNumber((double)(minimum), (double)(maximum), random);
            }
            return output;
        }

        public Vector3 getRandomInBlock(Vector3 origin, Vector3 field, Random random)
        {
            Vector3 point;

            point.X = (float)getRandomNumber((double)(origin.X - field.X), (double)(origin.X + field.X), random);
            point.Y = (float)getRandomNumber((double)(origin.Y - field.Y), (double)(origin.Y + field.Y), random);
            point.Z = (float)getRandomNumber((double)(origin.Z), (double)(origin.Z - field.Z), random);
            return point;
        }


        public double getRandomNumber(double minimum, double maximum, Random random)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private void configurePlanet(ObjectFieldInstance instance, float noise)
        {
            instance.hasPlanet = true;
            instance.uid = uidNext;
            uidNext++;
            if (randomizePlanet)
            {
                instance.planet = generatePlanet(noise);
            }
            else
            {
                instance.planet = prefabSettings;
            }
        }

        private PlanetRenderSettings generatePlanet(float seed)
        {
            PlanetRenderSettings planetSettings = new PlanetRenderSettings();

            int typeCount = 5;

            //Floats generated based on input seed
            //To Do: convert seed into int in some meaningful manner
            int intseed = (int)(seed * int.MaxValue);
            Random r = new Random(intseed);

            planetSettings.position = new Vector3(0, 0, -40000 * 1.5f);
            //planetSettings.rotationRate = new Vector3((float)(r.NextDouble() * 0.05) - 0.025f, (float)(r.NextDouble() * 0.05) - 0.025f, (float)(r.NextDouble() * 0.05) - 0.025f);
            planetSettings.rotationRate = new Vector3(0, (float)(r.NextDouble() * 0.05) - 0.025f, 0);
            //planetSettings.rotationRate = new Vector3(0.0f,-0.1f, 0) ;//debug
            //my section - choosing atmosphere type and maps to use
            float rad;
            planetSettings.mask = r.Next(masks.Length);
            planetSettings.diffuse = r.Next(diffuses.Length);
            planetSettings.height = r.Next(heights.Length);
            planetSettings.normal = planetSettings.height;
            planetSettings.emission = r.Next(emissions.Length);
            planetSettings.roughness = r.Next(roughness.Length);
            if (lutParameters != null)
            {
                planetSettings.hasAtmosphere = true;
                planetSettings.atmosphere = r.Next(lutParameters.Length);
                rad = lutParameters[planetSettings.atmosphere].fInnerRadius;
            }
            else
            {
                planetSettings.hasAtmosphere = false;
                //if we have no atmosphere, the size is randomly picked between the smallest and largest size
                rad = minInnerRadius + (float)((maxInnerRadius - minInnerRadius) * r.NextDouble());
            }
            planetSettings.radius = rad;

            float PhaseInput1 = (float)r.NextDouble();
            float PhaseInput2 = (float)r.NextDouble();
            float PhaseInput3 = (float)r.NextDouble();

            float ColorInput1 = (float)r.NextDouble();
            float ColorInput2 = (float)r.NextDouble();
            float ColorInput3 = (float)r.NextDouble();
            float ColorInput4 = (float)r.NextDouble();

            float TypeInput = (float)r.NextDouble();

            float FluidColorInput = (float)r.NextDouble();

            float FluidLevelInput = (float)r.NextDouble();

            float AtmoColorInput1 = (float)r.NextDouble();
            float AtmoColorInput2 = (float)r.NextDouble();
            float AtmoColorInput3 = (float)r.NextDouble();

            //Settings for generator
            Vector3 redMin = new Vector3(0.43f, 0.05f, 0.00f);
            Vector3 redMax = new Vector3(0.88f, 0.37f, 0.15f);
            Vector3 blueMin = new Vector3(0.14f, 0.20f, 0.23f);
            Vector3 blueMax = new Vector3(0.39f, 0.50f, 0.55f);
            Vector3 brownMin = new Vector3(0.27f, 0.18f, 0.06f);
            Vector3 brownMax = new Vector3(0.64f, 0.53f, 0.37f);
            Vector3 greenMin = new Vector3(0.17f, 0.26f, 0.02f);
            Vector3 greenMax = new Vector3(0.60f, 0.76f, 0.40f);
            Vector3 snowMin = new Vector3(0.87f, 0.88f, 0.89f);
            Vector3 snowMax = new Vector3(0.93f, 0.96f, 0.98f);
            Vector3 sandMin = new Vector3(0.62f, 0.59f, 0.42f);
            Vector3 sandMax = new Vector3(0.88f, 0.86f, 0.62f);
            Vector3 grayMin = new Vector3(0.17f, 0.18f, 0.18f);
            Vector3 grayMax = new Vector3(0.78f, 0.78f, 0.78f);
            Vector3 greenLightMin = new Vector3(0.51f, 0.71f, 0.32f);
            Vector3 greenLightMax = new Vector3(0.86f, 0.95f, 0.60f);
            Vector3 blackMin = new Vector3(0, 0, 0);
            Vector4 black = new Vector4(0, 0, 0, 1);
            Vector3 iceMin = new Vector3(0.26f, 0.43f, 0.61f);
            Vector3 iceMax = new Vector3(0.59f, 0.66f, 0.74f);
            Vector3 whiteMin = new Vector3(0.78f, 0.78f, 0.78f);
            Vector3 whiteMax = new Vector3(0.98f, 0.98f, 0.98f);


            planetSettings.fluidColor = new Vector4(1, 0.4f, 0.1f, 1);
            Vector3 lavaMin = new Vector3(0.74f, 0.1f, 0.00f);
            Vector3 lavaMax = new Vector3(1.0f, 0.3f, 0.02f);  //0.74 0.13 0.00

            Vector3 toxicMin = new Vector3(0.02f, 1.00f, 0.32f);
            Vector3 toxicMax = new Vector3(0.02f, 1.00f, 0.90f);

            Vector3 waterMin = new Vector3(0.18f, 0.59f, 1.00f);
            Vector3 waterMax = new Vector3(0.19f, 0.96f, 1.00f);

            Vector3 soulMin = new Vector3(0.15f, 0.3f, 1.00f);
            Vector3 soulMax = new Vector3(0.4f, 1.0f, 1.00f);

            Vector3 absorbtionAMin = new Vector3(0.06f, 0.12f, 0.21f);
            Vector3 absorbtionAMax = new Vector3(0.14f, 0.25f, 0.27f);

            Vector3 absorbtionBMin = new Vector3(0.15f, 0.90f, 0.90f); //Currently unused - replaced by absorobtionAMin
            Vector3 absorbtionBMax = new Vector3(0.15f, 0.65f, 0.75f); //(0.25f, 0.90f, 1.00f)


            Vector2 emissionIntensity = new Vector2(1.2f, 2.4f); //Min, max

            Vector2 phaseLimit1 = new Vector2(0.0f, 0.25f); //Min, max
            Vector2 phaseLimit2 = new Vector2(0.3f, 0.55f); //Min, max
            Vector2 phaseLimit3 = new Vector2(0.6f, 0.88f); //Min, max

            Vector2 magmaLevel = new Vector2(0.20f, 0.45f);  //Min, max
            Vector2 waterLevel = new Vector2(0.05f, 0.4f);  //Min, max
            Vector2 iceLevel = new Vector2(0.25f, 0.85f);  //Min, max
            Vector2 desolationLevel = new Vector2(0.0f, 0.55f);  //Min, max

            //Atmosphere values
            Vector3 atmoMagmaMin = new Vector3(0.420f, 0.600f, 0.650f);
            Vector3 atmoMagmaMax = new Vector3(0.520f, 0.70f, 0.790f);

            Vector3 atmoBarrensMin = new Vector3(0.380f, 0.540f, 0.620f);
            Vector3 atmoBarrensMax = new Vector3(0.420f, 0.680f, 0.700f);

            Vector3 atmoGaiaMin = new Vector3(0.600f, 0.520f, 0.440f);
            Vector3 atmoGaiaMax = new Vector3(0.670f, 0.600f, 0.470f);

            Vector3 atmoSnowMin = new Vector3(0.570f, 0.520f, 0.475f);
            Vector3 atmoSnowMax = new Vector3(0.660f, 0.655f, 0.650f);

            Vector3 atmoDesolationMin = new Vector3(0.700f, 0.655f, 0.570f);
            Vector3 atmoDesolationMax = new Vector3(0.600f, 0.570f, 0.520f);

            //calcuate tilling according to thickness  - starts at 3 ends at 6
            planetSettings.surfaceTilling = (ushort)(Math.Floor((rad / maxInnerRadius) * 3) + 3);
            planetSettings.surfaceTilling = 8;

            Vector3 atmoMin;
            Vector3 atmoMax;
            Vector3 tempFluid;
            float rA;
            float gA;
            float bA;

            //decide planet type - you will want to replace it with number you will generating atmosphere as you will already know planet type
            //int CurrentType = (int)Math.Floor(seed * typeCount);
            int CurrentType;
            if (planetTypeGeneration == -1)
            {
                CurrentType = r.Next(typeCount);
            }
            else
            {
                CurrentType = planetTypeGeneration;
            }
            planetSettings.planetType = CurrentType;
            switch (CurrentType)
            {
                case 0: //Magma

                    //pick lava
                    tempFluid = Vector3.Lerp(lavaMin, lavaMax, FluidColorInput);
                   
                    planetSettings.fluidColor = new Vector4(tempFluid, 1);
                    planetSettings.fluidLevel = MathHelper.Lerp(magmaLevel.X, magmaLevel.Y, FluidLevelInput);
                    planetSettings.fluidEmitIntensity = 4.5f;
                    
                    //pick transitions
                    planetSettings.phase.X = MathHelper.Lerp(phaseLimit1.X, phaseLimit1.Y, PhaseInput1) * 0.5f + planetSettings.fluidLevel; //create shores
                    planetSettings.phase.Y = MathHelper.Lerp(phaseLimit2.X, phaseLimit2.Y, PhaseInput2);
                    planetSettings.phase.Z = MathHelper.Lerp(phaseLimit3.X, phaseLimit3.Y, PhaseInput3);

                    //pick colors
                    planetSettings.color1 = Vector3.Lerp(redMin, redMax, ColorInput1);
                    planetSettings.color2 = Vector3.Lerp(brownMin, brownMax, ColorInput2);
                    planetSettings.color3 = Vector3.Lerp(grayMin, grayMax, ColorInput3);
                    planetSettings.color4 = Vector3.Lerp(grayMin, grayMax, ColorInput4);

                    //pick atmosphere color
                    atmoMin = atmoMagmaMin;
                    atmoMax = atmoMagmaMax;

                    rA = MathHelper.Lerp(atmoMin.X, atmoMax.X, AtmoColorInput1) * 1.1f;
                    gA = MathHelper.Lerp(atmoMin.Y, atmoMax.Y, AtmoColorInput2) * 1.1f;
                    bA = MathHelper.Lerp(atmoMin.Z, atmoMax.Z, AtmoColorInput3) * 1.1f;

                    planetSettings.waveLenghts4 = new Vector3((float)Math.Pow(rA, 4), (float)Math.Pow(gA, 4), (float)Math.Pow(bA, 4));
                    //TO DO:
                    //pick magma or barrens surface type texture
                    break;
                case 1: //Barrens

                    //no fluids
                    planetSettings.fluidLevel = -2.0f;
                    planetSettings.fluidColor = black;
                    planetSettings.fluidEmitIntensity = 0.0f;

                    //pick transitions
                    planetSettings.phase.X = MathHelper.Lerp(phaseLimit1.X, phaseLimit1.Y, PhaseInput1);
                    planetSettings.phase.Y = MathHelper.Lerp(phaseLimit2.X, phaseLimit2.Y, PhaseInput2);
                    planetSettings.phase.Z = MathHelper.Lerp(phaseLimit3.X, phaseLimit3.Y, PhaseInput3);

                    //pick colors
                    planetSettings.color1 = Vector3.Lerp(blackMin, redMax, ColorInput1);
                    planetSettings.color2 = Vector3.Lerp(brownMin, brownMax, ColorInput2);
                    planetSettings.color3 = Vector3.Lerp(sandMin, sandMax, ColorInput3);
                    planetSettings.color4 = Vector3.Lerp(redMin, brownMax, ColorInput4);

                    //pick atmosphere color
                    atmoMin = atmoBarrensMin;
                    atmoMax = atmoBarrensMax;

                    rA = MathHelper.Lerp(atmoMin.X, atmoMax.X, AtmoColorInput1) * 1.35f;
                    gA = MathHelper.Lerp(atmoMin.Y, atmoMax.Y, AtmoColorInput2) * 1.35f;
                    bA = MathHelper.Lerp(atmoMin.Z, atmoMax.Z, AtmoColorInput3) * 1.35f;

                    planetSettings.waveLenghts4 = new Vector3((float)Math.Pow(rA, 4), (float)Math.Pow(gA, 4), (float)Math.Pow(bA, 4));

                    //TO DO:
                    //pick magma, barrens  or Gaia surface type texture
                    break;

                case 2: //Terra

                    //no fluids
                    tempFluid = Vector3.Lerp(waterMin, waterMax, FluidColorInput);
                    planetSettings.fluidColor = new Vector4(tempFluid, 0.2f); //water is reflective
                    planetSettings.fluidLevel = MathHelper.Lerp(waterLevel.X, waterLevel.Y, FluidLevelInput);
                    planetSettings.fluidEmitIntensity = 1.0f;

                    //pick transitions
                    planetSettings.phase.X = planetSettings.fluidLevel + 0.12f + 0.15f; //create shores
                    planetSettings.phase.Y = MathHelper.Lerp(phaseLimit2.X, phaseLimit2.Y, PhaseInput2) + planetSettings.fluidLevel * 0.2f;
                    planetSettings.phase.Z = MathHelper.Lerp(phaseLimit3.X, phaseLimit3.Y, PhaseInput3);

                    //pick colors
                    planetSettings.color1 = Vector3.Lerp(sandMin, sandMax, ColorInput1); //create shores
                    planetSettings.color2 = Vector3.Lerp(greenLightMin, greenLightMax, ColorInput2);
                    planetSettings.color3 = Vector3.Lerp(greenMin, greenMax, ColorInput3);
                    planetSettings.color4 = Vector3.Lerp(brownMin, grayMax, ColorInput4);

                    //pick atmosphere color
                    atmoMin = atmoGaiaMin;
                    atmoMax = atmoGaiaMax;

                    rA = MathHelper.Lerp(atmoMin.X, atmoMax.X, AtmoColorInput1) * 1.12f;
                    gA = MathHelper.Lerp(atmoMin.Y, atmoMax.Y, AtmoColorInput2) * 1.12f;
                    bA = MathHelper.Lerp(atmoMin.Z, atmoMax.Z, AtmoColorInput3) * 1.12f;

                    planetSettings.waveLenghts4 = new Vector3((float)Math.Pow(rA, 4), (float)Math.Pow(gA, 4), (float)Math.Pow(bA, 4));

                    //TO DO:
                    //pick Gaia surface type texture
                    break;

                case 3: //Snow - Frost

                    //no fluids
                    tempFluid = Vector3.Lerp(waterMin, waterMax, FluidColorInput);
                    planetSettings.fluidColor = new Vector4(tempFluid, 0.2f); //water is reflective
                    planetSettings.fluidLevel = MathHelper.Lerp(iceLevel.X, iceLevel.Y, FluidLevelInput);
                    planetSettings.fluidEmitIntensity = 1.0f;
                    //planetSettings.fluidLevel = 0.75f;


                    //pick transitions
                    planetSettings.phase.X = MathHelper.Lerp(phaseLimit1.X, phaseLimit1.Y, PhaseInput1) * 1.22f;
                    planetSettings.phase.Y = MathHelper.Lerp(phaseLimit2.X, phaseLimit2.Y, PhaseInput2) * 1.22f;
                    planetSettings.phase.Z = MathHelper.Lerp(phaseLimit3.X, phaseLimit3.Y, PhaseInput3) * 1.22f;

                    //pick colors
                    planetSettings.color1 = Vector3.Lerp(snowMin, snowMax, ColorInput1);
                    planetSettings.color2 = Vector3.Lerp(absorbtionAMin, absorbtionAMax, ColorInput2); //used for absorbtion1   
                    planetSettings.color3 = Vector3.Lerp(absorbtionAMin, absorbtionBMax, ColorInput3); //low absorbtion
                    planetSettings.color4 = Vector3.Lerp(grayMax, grayMax, ColorInput4);
                    /*
                    if(AtmoColorInput2 > 0.5)  //hijacked free parameter
                    {
                        planetSettings.color3 = Vector3.Lerp(absorbtionAMin, absorbtionAMax, ColorInput3); //low absorbtion
                    }
                    else
                    {
                        planetSettings.color3 = Vector3.Lerp(absorbtionBMin, absorbtionBMax, ColorInput3); //high absorbtion
                    }
                    */
                    //pick atmosphere color
                    atmoMin = atmoSnowMin;
                    atmoMax = atmoSnowMax;

                    Vector3 atmo = Vector3.Lerp(atmoMin, atmoMax, AtmoColorInput1);
                    atmo *= 1.1f;
                    
                    planetSettings.waveLenghts4 = new Vector3((float)Math.Pow(atmo.X, 4), (float)Math.Pow(atmo.Y, 4), (float)Math.Pow(atmo.Z, 4));
                    //planetSettings.waveLenghts4 = new Vector3(1, 1, 1);
                    break;

                case 4: //Desolation

                    //no fluids
                    tempFluid = Vector3.Lerp(soulMin, soulMax, FluidColorInput);
                    planetSettings.fluidColor = new Vector4(tempFluid, 0.2f); //water is reflective
                    planetSettings.fluidLevel = MathHelper.Lerp(desolationLevel.X, desolationLevel.Y, FluidLevelInput);
                    planetSettings.fluidLevel = 0.5f;
                    planetSettings.fluidEmitIntensity = 6.0f;
                    

                    //pick transitions
                    planetSettings.phase.X = planetSettings.fluidLevel + 0.12f + 0.15f; //create shores
                    planetSettings.phase.Y = MathHelper.Lerp(phaseLimit2.X, phaseLimit2.Y, PhaseInput2) + planetSettings.fluidLevel * 0.2f;
                    planetSettings.phase.Z = MathHelper.Lerp(phaseLimit3.X, phaseLimit3.Y, PhaseInput3);

                    //pick colors
                    planetSettings.color1 = Vector3.Lerp(grayMin, grayMax, ColorInput1); //create shores
                    planetSettings.color2 = Vector3.Lerp(grayMin, grayMax, ColorInput2);
                    planetSettings.color3 = Vector3.Lerp(grayMin, whiteMax, ColorInput3);
                    planetSettings.color4 = Vector3.Lerp(whiteMin, grayMax, ColorInput4);

                    //pick atmosphere color
                    atmoMin = atmoDesolationMin;
                    atmoMax = atmoDesolationMax;

                    rA = MathHelper.Lerp(atmoMin.X, atmoMax.X, AtmoColorInput1) * 1.12f;
                    gA = MathHelper.Lerp(atmoMin.Y, atmoMax.Y, AtmoColorInput2) * 1.12f;
                    bA = MathHelper.Lerp(atmoMin.Z, atmoMax.Z, AtmoColorInput3) * 1.12f;

                    planetSettings.waveLenghts4 = new Vector3((float)Math.Pow(rA, 4), (float)Math.Pow(gA, 4), (float)Math.Pow(bA, 4));

                    //TO DO:
                    //pick Gaia surface type texture
                    break;
                //Snow
                //Toxic
            }

            //if we hard set wavelenghts then we use them instead
            if (atmoWaveLengths != Vector3.Zero)
                planetSettings.waveLenghts4 = atmoWaveLengths;
            //TO DO:
            //pick set of Heigh, Normal, Roughness Map

            return planetSettings;
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
            float sourceRatio = (float)texArt.Width / (float)texArt.Height;

            rect.Width = device.Viewport.Width;
            rect.Height = device.Viewport.Height;
            float ratio = (float)rect.Width / (float)rect.Height;

            if (ratio == sourceRatio)
            {
                source.Width = texArt.Width;
                source.Height = texArt.Height;

                //if ration is same we dont touch origin
                lightShaftRecomputedOrigin = lightShaftOrgin;
            }
            else if (ratio > sourceRatio)//wider, so we crop top and bot
            {
                source.Width = texArt.Width;
                //don't let it be bigger than the image
                source.Height = Math.Min(texArt.Height, (int)(source.Width / ratio));
                //calculate Y position of LS origin
                lightShaftRecomputedOrigin.Y = (lightShaftOrgin.Y * (float)texArt.Height - ((float)texArt.Height - (float)source.Height) / 2) / (float)source.Height;
                //X position of LS is same
                lightShaftRecomputedOrigin.X = lightShaftOrgin.X;
            }
            else
            {
                source.Height = texArt.Height;
                //don't let it be bigger than the image
                source.Width = Math.Min(texArt.Width, (int)(source.Height * ratio));
                //calculate X position of LS origin
                lightShaftRecomputedOrigin.X = (lightShaftOrgin.X * (float)texArt.Width - ((float)texArt.Width - (float)source.Width) / 2) / (float)source.Width;
                //Y position of LS is same
                lightShaftRecomputedOrigin.Y = lightShaftOrgin.Y;
            }
            source.X = (texArt.Width - source.Width) / 2;
            source.Y = (texArt.Height - source.Height) / 2;

            //calculate new light shaft position
            //lightShaftRecomputedOrigin.X = texArt.Width * lightShaftOrgin.X;
            //lightShaftRecomputedOrigin.Y = texArt.Height * lightShaftOrgin.Y;
            //lightShaftRecomputedOrigin.X -= source.X;
            //lightShaftRecomputedOrigin.Y -= source.Y;
            //i got confused and said fuck it at this point :(
            //lightShaftRecomputedOrigin = lightShaftOrgin;

            //account for rounding errors by adding a pixel of buffer
            if (source.X > 0)
            {
                source.X -= 1;
            }
            if (source.Y > 0)
            {
                source.Y -= 1;
            }
            if (source.Height < texArt.Height)
            {
                source.Height += 2;
            }
            if (source.Width < texArt.Width)
            {
                source.Width += 2;
            }
        }

        private int SortV3(Point[] keys, ref BackdropInstance[] backdrops, ref Vector3[] positions)
        {
            int zeroIndex = 0;

            BackdropInstance[] sortedBackdrops = new BackdropInstance[4];
            Vector3[] sortedpositions = new Vector3[4];

            int[] indices = { 0, 1, 2, 3 };
            int indTemp;

            //lexicographical sorting with simplification
            Point keyTemp;
            for (int n = 0; n < keys.Length - 1; n++)
            {
                //we sort by y
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    if (keys[i].Y > keys[i + 1].Y)
                    {
                        keyTemp = keys[i + 1];
                        keys[i + 1] = keys[i];
                        keys[i] = keyTemp;

                        indTemp = indices[i + 1];
                        indices[i + 1] = indices[i];
                        indices[i] = indTemp;
                    }
                }
            }

            //Now we need to sort segments
            for (int n = 0; n < keys.Length - 1; n++)
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    if (keys[i].Y == keys[i + 1].Y)
                    {
                        if (keys[i].X > keys[i + 1].X)
                        {
                            keyTemp = keys[i + 1];
                            keys[i + 1] = keys[i];
                            keys[i] = keyTemp;

                            indTemp = indices[i + 1];
                            indices[i + 1] = indices[i];
                            indices[i] = indTemp;

                        }
                    }
                }


            for(int i = 0; i < indices.Length; i++)
            {
                sortedBackdrops[i] = backdrops[indices[i]];
                sortedpositions[i] = positions[indices[i]];

                if (indices[i] == 0)
                    zeroIndex = i;
            }

            backdrops = sortedBackdrops;
            positions = sortedpositions;

            return zeroIndex;
        }


        private int SortV2(Point[] keys, ref BackdropInstance[] backdrops, ref Vector3[] positions)
        {
            int zeroIndex = 0;

            BackdropInstance[] sortedBackdrops = new BackdropInstance[4];
            Vector3[] sortedpositions = new Vector3[4];

            int[] indices = { 0, 1, 2, 3 };
            int temp;

            //lexicographical sorting with simplification
            Point keyTemp;
            for (int n = 0; n < keys.Length - 1; n++)
            {
                //we sort by y
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    if (keys[i].Y > keys[i + 1].Y)
                    {
                        keyTemp = keys[i + 1];
                        keys[i + 1] = keys[i];
                        keys[i] = keyTemp;


                        if (i == zeroIndex)
                        {
                            zeroIndex = i + 1;
                        }
                    }
                }
            }

            //Now we need to sort segments
            for (int n = 0; n < keys.Length - 1; n++)
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    if (keys[i].Y == keys[i + 1].Y)
                    {
                        if (keys[i].X > keys[i + 1].X)
                        {
                            keyTemp = keys[i + 1];
                            keys[i + 1] = keys[i];
                            keys[i] = keyTemp;

                            if (i == zeroIndex)
                            {
                                zeroIndex = i + 1;
                            }
                        }
                    }
                }
        




            return zeroIndex;
            
        }


        //Will break for non square data
        private int Sort(Point[] keys, ref BackdropInstance[] backdrops, ref Vector3[] positions)
        {
            int zeroIndex = 0;

            BackdropInstance[] sortedBackdrops = new BackdropInstance[4];
            Vector3[] sortedpositions = new Vector3[4];

            //Find left top corner
            int candidate = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                 if (keys[i].X <= keys[candidate].X && keys[i].Y <= keys[candidate].Y)
                 {
                    candidate = i;
                 }
            }

            sortedBackdrops[0] = backdrops[candidate];
            sortedpositions[0] = positions[candidate];
            if (candidate == 0)
                zeroIndex = 0;

            //Find right top corner
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].X >= keys[candidate].X && keys[i].Y <= keys[candidate].Y)
                {
                    candidate = i;
                }
            }

            sortedBackdrops[1] = backdrops[candidate];
            sortedpositions[1] = positions[candidate];
            if (candidate == 0)
                zeroIndex = 1;

            //Find left bot corner
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].X <= keys[candidate].X && keys[i].Y >= keys[candidate].Y)
                {
                    candidate = i;
                }
            }

            sortedBackdrops[2] = backdrops[candidate];
            sortedpositions[2] = positions[candidate];
            if (candidate == 0)
                zeroIndex = 2;

            //Find right bot corner
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].X >= keys[candidate].X && keys[i].Y >= keys[candidate].Y)
                {
                    candidate = i;
                }
            }

            sortedBackdrops[3] = backdrops[candidate];
            sortedpositions[3] = positions[candidate];
            if (candidate == 0)
                zeroIndex = 3;

            positions = sortedpositions;
            backdrops = sortedBackdrops;
            return zeroIndex;
        }

        public override Vector4 drawLayer0(RenderTarget2D backdropTarget, RenderTarget2D rtNebulaNormal, RenderTarget2D rtNebulaDepth, RenderTarget2D rtNebulaMeta, RenderTarget2D rtNebulaStars, int index, BackdropInstance[] instance, SpriteBatch batch, bool isCurrent, float[] value, Vector3[] cameraPos, Point[] gridPosition, Color lightColor, Color ambLightColor, float lightIntensity, Vector3 lightDirection, ref Vector4 colVecClouds)
        {
            Vector4 currentLSset = Vector4.Zero;
            if (cloudAssets == null && texArt != null)
            { 

                Color drawColor = Color.White;
                drawColor.A = (byte)(255f * value[index]);

                batch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                batch.Draw(texArt, rect, source, drawColor);

                batch.End();
            }



            if (isCurrent)
            {
                /*
                if(
                    previousKeys[0].X != gridPosition[0].X ||
                    previousKeys[0].Y != gridPosition[0].Y ||

                    previousKeys[1].X != gridPosition[1].X ||
                    previousKeys[1].Y != gridPosition[1].Y ||

                    previousKeys[2].X != gridPosition[2].X ||
                    previousKeys[2].Y != gridPosition[2].Y ||

                    previousKeys[3].X != gridPosition[3].X ||
                    previousKeys[3].Y != gridPosition[3].Y
                    )*/
                { 

                    for(int i = 0; i < previousKeys.Length; i++)
                    {
                        previousKeys[i] = gridPosition[i];
                    }
                    index = 0;
                    index = SortV3(gridPosition, ref instance, ref cameraPos);
                }

                //we should render clouds here
                device.SetRenderTargets(rtNebulaNormal, rtNebulaDepth, rtNebulaMeta, rtNebulaStars);
                device.Clear(transparentBlack);
                device.BlendState = BlendState.AlphaBlend;
                device.DepthStencilState = DepthStencilState.None;
                device.SamplerStates[0] = SamplerState.LinearWrap;
                device.SamplerStates[1] = SamplerState.LinearWrap;
                device.SamplerStates[2] = SamplerState.LinearWrap;

                Vector3 sectorPositionOffset = cameraPos[index]; //Saves position of current
                Vector3 cameraPosCurrent = cameraPos[index];
                Vector3 cameraTarget = cameraPos[index];
                cameraTarget.Z = 0;
                Matrix view = Matrix.CreateLookAt(cameraPos[index], cameraTarget, Vector3.Up);
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 480000);


                backgroundEffect_CameraPos.SetValue(cameraPos[index]);
                backgroundEffect_viewportScale.SetValue(new Vector2(0.5f / device.Viewport.AspectRatio, -0.5f));
                backgroundEffect_viewport.SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
                backgroundEffect_view.SetValue(view);
                backgroundEffect_projection.SetValue(projection);

                Vector3 colVecGeneral = Vector3.Zero, colVecHdrColor = Vector3.Zero, colVecBackground = Vector3.Zero, colVecStars = Vector3.Zero, colVecStarsDust = Vector3.Zero, colVecLA = Vector3.Zero, colVecLB = Vector3.Zero;
                float colHdrHigh = 0, colAlphaMin = 0, colEdgeInt = 0, colRough = 0;
                weights = findWeights(cameraPosCurrent, cameraPos);

                device.BlendState = BlendState.AlphaBlend;
                backgroundEffect.CurrentTechnique = backgroundTech_stars;
                for (int i = 0; i < 4; i++)
                {
                    sectorPositionOffset = cameraPosCurrent - cameraPos[i];
                    if (instance[i] != null && instance[i].GetType() == typeof(ObjectFieldInstance))
                    {
                        //this is the data from a valid sector which might have clouds
                        ObjectFieldInstance neighborField = instance[i] as ObjectFieldInstance;
                        
                        if (neighborField.hasStars)
                        {
                            backgroundEffect_positionOffset.SetValue(-sectorPositionOffset);
                            qbatch.Begin(backgroundEffect);

                            for (int n = 0; n < neighborField.starField.position.Length / qualityDivider; n++)
                            {
                                qbatch.DrawPoint(neighborField.cloudSheet.assetDifs[0], neighborField.starField.position[n] + sectorPositionOffset, neighborField.starField.sliceID[n], neighborField.starField.color[n], neighborField.starField.scale[n] * 128.0f);
                            }
                            qbatch.End();

                        }
                    }
                }

                backgroundEffect.CurrentTechnique = backgroundTech_starDust;
  
                    for (int i = 0; i < 4; i++)
                    {
                        sectorPositionOffset = cameraPosCurrent - cameraPos[i];
                        if (instance[i] != null && instance[i].GetType() == typeof(ObjectFieldInstance))
                        {
                            //this is the data from a valid sector which might have clouds
                            ObjectFieldInstance neighborField = instance[i] as ObjectFieldInstance;

                            if (neighborField.hasStarDust)
                            {

                                backgroundEffect_positionOffset.SetValue(-sectorPositionOffset);
                                backgroundEffect_text1.SetValue(neighborField.cloudSheet.assetEmits[0]); //tiled texture
                                if (qualityStarDust)
                                {
                                   qbatch.Begin(backgroundEffect);
                                   for (int n = 0; n < neighborField.starDustField.position.Length / qualityDivider; n++)
                                   {
                                       //tempColor.A = (byte)((float)tempColor.A * value[i]);
                                       //qbatch.Draw(neighborField.cloudSheet.assetSpecs[0], neighborField.starDustField.position[n] + sectorPositionOffset, tempColor, 0.0f, neighborField.starDustField.scale[n]);
                                       qbatch.DrawPoint(neighborField.cloudSheet.assetSpecs[0], neighborField.starDustField.position[n] + sectorPositionOffset, neighborField.starDustField.color[n], neighborField.starDustField.rotation[n], neighborField.starDustField.scale[n] * 512);
                                   }
                                   qbatch.End();
                                }
                            

                            }
                        }
                    }



                colVecClouds = Vector4.Zero;
                int lightCount = 0;
                backgroundEffect.CurrentTechnique = backgroundTech_normalMaps;
                for (int i = 0; i < 4; i++)
                {
                    
                    sectorPositionOffset = cameraPosCurrent - cameraPos[i];
                    if (instance[i] != null && instance[i].GetType() == typeof(ObjectFieldInstance))
                    {
                        //this is the data from a valid sector which might have clouds
                        ObjectFieldInstance neighborField = instance[i] as ObjectFieldInstance;
                        if (neighborField.hasClouds)
                        {
                            //accumulate omni lights
                            for (int h = 0; h < neighborField.cloudLights.position.Length; h++)
                            {
                                omni_positionArray[h + lightCount] = neighborField.cloudLights.position[h] + new Vector3(sectorPositionOffset.X, sectorPositionOffset.Y, 0);
                                float intensityTemp = progressiveFade(omni_positionArray[h + lightCount], cameraPosCurrent, -sectorPositionOffset / 200000);
                                omni_intensityArray[h + lightCount] = neighborField.cloudLights.intensity[h] * intensityTemp; //
                                omni_typeArray[h + lightCount] = neighborField.cloudLights.type[h];
                                omni_attenuationArray[h + lightCount] = neighborField.cloudLights.attuneation[h];
                            }
                            lightCount += neighborField.cloudLights.position.Length;


                            backgroundEffect_positionOffset.SetValue(-sectorPositionOffset / 200000);
                            backgroundEffect_maskState.SetValue(0.0f);
                            qbatch.Begin(backgroundEffect);

                            for (int j = 0; j < neighborField.cloudFieldGroups.Length; j++)
                            {
                                for (int n = 0; n < neighborField.cloudFieldGroups[j].position.Length / qualityDivider; n++)
                                {                               
                                    qbatch.DrawPoint(neighborField.cloudSheet.assetBumps[j], neighborField.cloudFieldGroups[j].position[n] + sectorPositionOffset, neighborField.cloudFieldGroups[j].color[n], neighborField.cloudFieldGroups[j].rotation[n], 100f * 800f);
                                }
                            }
                            qbatch.End();

                            //Average parameters and colors
                            colHdrHigh += (weights[i] *    neighborField.cloudHdrHighPhase);
                            colAlphaMin += (weights[i] *    neighborField.cloudAlphaMin);
                            colEdgeInt += (weights[i] *     neighborField.cloudEdgeIntensity);
                            colRough += (weights[i] *       neighborField.cloudRoughness);

                            colVecLA += (weights[i] * neighborField.cloudsLightA);
                            colVecLB += (weights[i] * neighborField.cloudsLightB);
                            colVecGeneral += (weights[i] *  neighborField.cloudGeneralMult);
                            colVecBackground += (weights[i] *  neighborField.cloudDeepBackground);
                            colVecHdrColor += (weights[i] *  neighborField.cloudHdrHighPhaseColor);
                            colVecStars += (weights[i] *  neighborField.cloudStarColor);
                            colVecStarsDust += (weights[i] *  neighborField.cloudStarDustColor);

                            currentLSset += (weights[i] *  (neighborField.instLightShaftSet)); //new Vector4(10,10,10,10) -
                    
                            colVecClouds += (weights[i] * (neighborField.fogCloudsColor));

                        }
                    }
                }




                //Then process all 4 rendertargets in screen space
                device.SetRenderTarget(backdropTarget); //rtNebulaNormal opened as occlusion RT, can be used for if extract light shaft function is modified
                device.BlendState = BlendState.NonPremultiplied;
                //device.Clear(new Color(0.0f, 0.0f, 0.0f, 0.0f));

                
                backgroundEffect_lightCount.SetValue(lightCount);
                //parameters that are result of Biome values blending
                backgroundEffect_parameterA.SetValue(colHdrHigh);
                backgroundEffect_parameterB.SetValue(colAlphaMin);
                backgroundEffect_parameterC.SetValue(colRough);
                backgroundEffect_parameterD.SetValue(colEdgeInt);

                backgroundEffect_colorA.SetValue(ToolBox.XYZtoRGB(colVecGeneral));
                backgroundEffect_colorB.SetValue(ToolBox.XYZtoRGB(colVecHdrColor));
                backgroundEffect_colorC.SetValue(ToolBox.XYZtoRGB(colVecBackground));
                backgroundEffect_colorD.SetValue(ToolBox.XYZtoRGB(colVecStars));
                backgroundEffect_colorE.SetValue(ToolBox.XYZtoRGB(colVecStarsDust));
                backgroundEffect_colorLA.SetValue(ToolBox.XYZtoRGB(colVecLA));
                backgroundEffect_colorLB.SetValue(ToolBox.XYZtoRGB(colVecLB));

                backgroundEffect_omniType.SetValue(omni_typeArray);
                backgroundEffect_omniIntensity.SetValue(omni_intensityArray);
                backgroundEffect_omniAttenuation.SetValue(omni_attenuationArray);
                backgroundEffect_omniPosition.SetValue(omni_positionArray);



                backgroundEffect.CurrentTechnique = backgroundTech_nebulaComp;
                backgroundEffect_text1.SetValue(rtNebulaDepth);
                backgroundEffect_text2.SetValue(rtNebulaNormal);
                backgroundEffect_text3.SetValue(rtNebulaStars);

                batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null, backgroundEffect);
                batch.Draw(rtNebulaMeta, Vector2.Zero, Color.White);
                batch.End();
            }
            device.SetRenderTarget(rtNebulaDepth);
            device.Clear(transparentBlack);

            device.SetRenderTargets(backdropTarget, rtNebulaDepth);

            return currentLSset;
        }



        private float progressiveFade(Vector3 position, Vector3 cameraXY, Vector3 sectorOffset)
        {
            //distance
            Vector2 opacity = new Vector2(position.X - cameraXY.X, position.Y - cameraXY.Y);   //new Vector2((position.X - cameraXY.X), (position.Y - cameraXY.Y));
            opacity.X *= sectorOffset.X;
            opacity.Y *= sectorOffset.Y;
            opacity = new Vector2(Math.Abs(opacity.X), Math.Abs(opacity.Y));

            //float blendDistanceStart
            Vector2 lengthMult = new Vector2(100000 - Math.Abs(cameraXY.X), 100000 - Math.Abs(cameraXY.Y)) / 100000.0f;  //return 0 at the edge, 1 in middle
            lengthMult =  new Vector2(MathHelper.Lerp(2, 1, lengthMult.X), MathHelper.Lerp(2, 1, lengthMult.Y)); 

            opacity = new Vector2(Math.Max(0, opacity.X - 80000 * lengthMult.X), Math.Max(0, opacity.Y - 80000 * lengthMult.Y));
            opacity /= 45000.0f; //4000

            float output = MathHelper.Clamp((1 - (opacity.X + opacity.Y)), 0, 1);
            return output;
        }




        private float[] findWeights(Vector3 currentPosition, Vector3[] sectorOffsets)
        {
            float[] weights = new float[4];
            float x, y, xD, yD;

            Vector3 sectorDirection;

            //calculate direction
            for (int i = 0; i < 4; i++)
            {
                sectorDirection = (currentPosition - sectorOffsets[i]) / 200000f;
                //find horizontal weight
                x = Math.Abs((200000 * sectorDirection.X - currentPosition.X) / 200000.0f);
                //find vertical weight
                y = Math.Abs((200000 * sectorDirection.Y - currentPosition.Y) / 200000.0f);

                //remap gradient
                x = MathHelper.Clamp((x - 0.4f) * 5f, 0, 1);
                y = MathHelper.Clamp((y - 0.4f) * 5f, 0, 1);
                xD = (1 - x);
                yD = (1 - y);
                weights[i] = (xD * yD);
                
            }
            return weights;
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



            if (field.hasPlanet)
            {

                Vector3 pCameraPos = cameraPos[index];
                pCameraPos.Y *= -1;
                Vector3 pCameraTarget = pCameraPos;
                pCameraTarget.Z = 0;
                /*
                pCameraPos = new Vector3(0, 10000, -40000);
                pCameraTarget = new Vector3(0, 10000, -30000);
                */
                Matrix viewp = Matrix.CreateLookAt(pCameraPos, pCameraTarget, Vector3.Up);

                /*
                EffectParameter planetEffect_View; 
                planetEffect_View = planetEffect.Parameters["View"];
                planetEffect_View.SetValue(viewp)
                */


                planetView.SetValue(viewp);
                planetProject.SetValue(project);

                planetLightInt.SetValue(lightIntensity);
                planetDomCol.SetValue(lightColor.ToVector4());
                planetLightDir.SetValue(lightDirection * invY);
                planetAmbient.SetValue(ambLightColor.ToVector3());
                planetCameraPos.SetValue(pCameraPos);

                planetMask.SetValue(mask[field.planet.mask]);
                planetDiffuse.SetValue(diffuse[field.planet.diffuse]);
                planetHeight.SetValue(height[field.planet.height]);
                planetNormal.SetValue(normal[field.planet.normal]);
                planetRough.SetValue(roughness[field.planet.roughness]);
                planetEmiss.SetValue(emission[field.planet.emission]); //this will be black pixel or whatever for most of the planets
                planetEmissCol.SetValue(field.planet.emissionColor); //Vec4 - 4th channel for intensity

                planetFluidCol.SetValue(field.planet.fluidColor); //Vec4 -  4th channel used to determine fluid roughness
                planetFluidEmiss.SetValue(field.planet.fluidEmitIntensity); //float

                planetPhase.SetValue(field.planet.phase); //Vec3 heights for material transitions

                planetCol1.SetValue(field.planet.color1); //All four colors are Vec3
                planetCol2.SetValue(field.planet.color2);
                planetCol3.SetValue(field.planet.color3);
                planetCol4.SetValue(field.planet.color4);

                planetFluidLevel.SetValue(field.planet.fluidLevel);
                //planetEffect.Parameters["FluidLevel"].SetValue(0.95f);

                planetSurfaceTilling.SetValue(field.planet.surfaceTilling); //ushort or even Byte

                planetCenterPos.SetValue(field.planet.position);

                planetInnerRadius.SetValue(field.planet.radius);

                //planetEffect.Parameters["Roughness"].SetValue(testPlanetTextures[4]);
                Matrix world;
                if (field.planet.hasAtmosphere)
                {
                    planetOuterRadius.SetValue(lutParameters[field.planet.atmosphere].fOuterRadius);
                    planetLut.SetValue(luts[field.planet.atmosphere]);
                    planetV3Wavelength.SetValue(field.planet.waveLenghts4);

                    device.BlendState = BlendState.Additive;
                    world = Matrix.CreateScale(lutParameters[field.planet.atmosphere].fOuterRadius) * Matrix.CreateTranslation(field.planet.position);
                    planetWorld.SetValue(world);



                    planetEffect.CurrentTechnique = planetTechnique_atmo;
                    foreach (ModelMesh mesh in planetAtmosphere.Meshes)
                    {
                        mesh.Draw();
                    }
                }
                device.BlendState = BlendState.Opaque;
                world = Matrix.CreateScale(field.planet.radius) * Matrix.CreateRotationX(field.planet.rotation.X) * Matrix.CreateRotationY(-field.planet.rotation.Y) * Matrix.CreateRotationZ(field.planet.rotation.Z) * Matrix.CreateTranslation(field.planet.position);  //
                planetWorld.SetValue(world);
                Matrix rotationMatrix = Matrix.CreateRotationX(field.planet.rotation.X) * Matrix.CreateRotationY(field.planet.rotation.Y) * Matrix.CreateRotationZ(field.planet.rotation.Z);
                Vector3 lightDirectionObj = Vector3.Transform(lightDirection, rotationMatrix); //(lightDirection * invY), rotationMatrix
                lightDirectionObj = lightDirection;
                planetLightDirObj.SetValue(lightDirectionObj);


                switch (field.planet.planetType)
                {
                    case 0:
                        planetEffect.CurrentTechnique = planetEffect.Techniques["MagmaTechnique"];
                        break;

                    case 1:
                        planetEffect.CurrentTechnique = planetEffect.Techniques["TerraTechnique"];
                        break;

                    case 2:
                        planetEffect.CurrentTechnique = planetEffect.Techniques["TerraTechnique"];
                        break;

                    case 3:
                        planetEffect.CurrentTechnique = planetEffect.Techniques["SnowTechnique"];
                        break;

                    case 4:
                        planetEffect.CurrentTechnique = planetEffect.Techniques["DesolationTechnique"];
                        break;


                }


                foreach (ModelMesh mesh in planetSurface.Meshes)
                {
                    mesh.Draw();
                }


                //if (!isCurrent)
                //{
                //    string debug = "Camera information:\nPosition:\n" + cameraPos.ToString() + "\nView:\n" + viewp.ToString() + "\nWorld:\n" + world.ToString();
                //    batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                //    batch.DrawString(
                //    batch.End();
                //}
            }




            Color objColor = new Color((byte)(ambLightColor.R / 3), (byte)(ambLightColor.G / 3), (byte)(ambLightColor.B / 3), (byte)255);

            drawStellarObjects(batch, isCurrent, value[index], cameraPos[index]);
            spriteBasic.CurrentTechnique = spriteBasic.Techniques["spriteBasic"];
            device.BlendState = BlendState.NonPremultiplied;
            if (assets != null)
            {
                for (int i = 0; i < assets.assetDifs.Length; i++)
                {
                    qbatch.Begin(spriteBasic);
                    int c = field.positions[i].Count;
                    if (assetScales != null)
                    {
                        for (int p = 0; p < c; p++)
                        {
                            //batch.draw(asset[i], field.positions[i][p], field.rotations[i][p])
                            qbatch.Draw(assets.assetDifs[i], field.positions[i][p], objColor, field.rotations[i][p], assetScales[i]);
                        }
                    }
                    else
                    {
                        for (int p = 0; p < c; p++)
                        {
                            
                            qbatch.Draw(assets.assetDifs[i], field.positions[i][p], objColor, field.rotations[i][p], 1);
                        }
                    }
                    qbatch.End();
                }
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
