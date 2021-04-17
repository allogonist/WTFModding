using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;
using System.Net.Configuration;

namespace BackdropsCore
{
    public class MyLoader : ExtensionLoader
    {
        ///This function is the entry-point for extensions
        /// 
        /// Because the purpose of this is to define the code that will control world generation, every item must be assigned to a color. When generating a new sector on the world map, the game will first check if that sector has been assigned a color (via .bdm file) and if it has, it will use the assigned color as a key to look up the sector generation code provided by extensions.
        /// 
        /// Every sector is assigned a default color by algorithmic world-generation, which is how the infinite universe is created. You can override the color of any sector in the infinite universe by creating a 32 bit png image, setting the color of pixels in that image, and saving the png image with a file name in this format "X_Y.bdm" where X is the world-space X coordinate of the sector defined by the top left pixel of your image and Y is the Y coordinate of that pixel. So 0_-1.bdm would be an image with the top left pixel representing the contents of world-coordinate x=0, y=-1 and each additional pixel in the image mapped to respective areas by their offset in the image.
        /// 
        /// During world generation, the game opens all .bdm files in the data folder. .bdm files are png images with their extension changed so you can easily change it back to png to see or change their contents.
        /// 
        /// To define the world-gen algorithm for a sector of your choice, create an extension and a .bdm file.
        ///  
        /// Below is the code that defines the built-in hand made sectors used for Zero Falls. Obviously if you compile it as-is everything inside will conflict with the existing BackdropCore.dll so if you want to try modifying existing backdrop extensions instead of making your own you should back up your copy of BackdropCore.dll and simply replace it with the compiled output of this project
        /// 
        /// Note: alpha values are ignored in all cases and duplicate colors with different alpha values will be discarded
        public void load(Dictionary<Color, BackdropExt> backdrops, Dictionary<string, TextureBatch> sectorTextures, Dictionary<Color, TerrainGenerator> sectorGenerators, Dictionary<Color, LightSettings> lightSettings, Dictionary<Color, LightShaftSettings> lightShaftSettings, Dictionary<Color, string[]> audioSettings, List<Color> preloadRequired, Dictionary<Color, IconBatch> mapIcons, Dictionary<Color, string> iconTechniques, List<string> mapDataIncludes, Dictionary<string, POEIcon> interestIcons, Dictionary<Color, BackdropInfo> backdropInfo)
        {
            //ToDo: comment this line and replace it with your own sector definitions
            loadExample(backdrops, sectorTextures, sectorGenerators, lightSettings, lightShaftSettings, audioSettings, preloadRequired, mapIcons, iconTechniques, mapDataIncludes, interestIcons, backdropInfo);
        }

        public void loadExample(Dictionary<Color, BackdropExt> backdrops, Dictionary<string, TextureBatch> sectorTextures, Dictionary<Color, TerrainGenerator> sectorGenerators, Dictionary<Color, LightSettings> lightSettings, Dictionary<Color, LightShaftSettings> lightShaftSettings, Dictionary<Color, string[]> audioSettings, List<Color> preloadRequired, Dictionary<Color, IconBatch> mapIcons, Dictionary<Color, string> iconTechniques, List<string> mapDataIncludes, Dictionary<string, POEIcon> interestIcons, Dictionary<Color, BackdropInfo> backdropInfo)
        {
            float planetSizeMult = 1.7f;


            /*
            BiomeSettings homeNebula = new BiomeSettings();
            homeNebula.hdrHigh = 2.011494f;
            homeNebula.alphaMin = 0.2241379f;
            homeNebula.roughness = 1.264368f;
            homeNebula.edgeIntensity = 1.425287f;
            homeNebula.lightA = new Vector3(59.08585f, 53.29189f, 19.15382f);
            homeNebula.lightB = new Vector3(44.21946f, 29.92901f, 16.67004f);
            homeNebula.generalMult = new Vector3(61.15158f, 58.78672f, 15.63869f);
            homeNebula.hdrHphase = new Vector3(41.46765f, 37.29175f, 5.553036f);
            homeNebula.deepBackground = new Vector3(0.01040825f, 0.007652197f, 0.001943454f);
            homeNebula.starColor = new Vector3(22.97016f, 23.21552f, 4.557853f);
            homeNebula.starDustColor = new Vector3(1.882478f, 1.308487f, 0.292517f);
            */
            /* //more aggresive variant
            BiomeSettings homeNebula = new BiomeSettings();
            homeNebula.hdrHigh = 5.0f;
            homeNebula.alphaMin = 0.7f;
            homeNebula.roughness = 2.091954f;
            homeNebula.edgeIntensity = 1.770115f;
            homeNebula.lightA = new Vector3(29.6396f, 19.44412f, 9.910571f);
            homeNebula.lightB = new Vector3(43.04922f, 46.98108f, 12.16744f);
            homeNebula.generalMult = new Vector3(40.37687f, 38.92302f, 19.64284f);
            homeNebula.hdrHphase = new Vector3(43.64464f, 38.03254f, 8.659586f);
            homeNebula.deepBackground = new Vector3(0.002226306f, 0.002402059f, 0.001282225f);
            homeNebula.starColor = new Vector3(15.63093f, 11.28099f, 2.141991f);
            homeNebula.starDustColor = new Vector3(3.638219f, 3.085216f, 1.21597f);
            */
            //new Vector3(0.002557754f, 0.002175016f, 0.000748136f);


            //Deposit presets
            DepositPreset domumCenterA = new DepositPreset();
            domumCenterA.minDeposits = 1;
            domumCenterA.maxDeposits = 1;
            domumCenterA.AddDepositeChance(DepositeType.iron_starter, 0.8f);
            domumCenterA.AddDepositeChance(DepositeType.iron_small, 0.2f);

            DepositPreset domumCenterB = new DepositPreset();
            domumCenterB.minDeposits = 0;
            domumCenterB.maxDeposits = 1;
            domumCenterB.AddDepositeChance(DepositeType.iron_starter, 0.2f);
            domumCenterB.AddDepositeChance(DepositeType.iron_small, 0.8f);

            domumCenterB.minMiningOperations = 0;
            domumCenterB.maxMiningOperations = 1;
            domumCenterB.AddOperationChance(DepositeType.mo_iron_free_small, 1.0f);

            DepositPreset domumEdgeA = new DepositPreset();
            domumEdgeA.minDeposits = 0;
            domumEdgeA.maxDeposits = 2;

            domumEdgeA.AddDepositeChance(DepositeType.gold_small, 0.6f);
            domumEdgeA.AddDepositeChance(DepositeType.iron_small, 0.4f);

            domumEdgeA.minMiningOperations = 0;
            domumEdgeA.maxMiningOperations = 1;
            domumEdgeA.AddOperationChance(DepositeType.mo_gold_free_small, 1.0f);



            DepositPreset wolfEdge = new DepositPreset();
            wolfEdge.minDeposits = 1;
            wolfEdge.maxDeposits = 1;

            wolfEdge.AddDepositeChance(DepositeType.iron_small, 0.6f);
            wolfEdge.AddDepositeChance(DepositeType.gold_small, 0.4f);
            wolfEdge.AddDepositeChance(DepositeType.iron_medium, 0.3f);
            wolfEdge.AddDepositeChance(DepositeType.gold_medium, 0.2f);

            DepositPreset wolfInner = new DepositPreset();
            wolfInner.minDeposits = 0;
            wolfInner.maxDeposits = 2;

            wolfInner.AddDepositeChance(DepositeType.iron_small, 0.3f);
            wolfInner.AddDepositeChance(DepositeType.gold_small, 0.1f);
            wolfInner.AddDepositeChance(DepositeType.iron_medium, 0.5f);
            wolfInner.AddDepositeChance(DepositeType.gold_medium, 0.2f);

            wolfInner.minMiningOperations = 0;
            wolfInner.maxMiningOperations = 1;
            wolfInner.AddOperationChance(DepositeType.mo_iron_pirates_small, 1.0f);
            wolfInner.AddOperationChance(DepositeType.mo_gold_pirates_small, 1.0f);
            wolfInner.AddOperationChance(DepositeType.mo_gold_free_small, 0.35f);


            DepositPreset wolfCenter = new DepositPreset();
            wolfCenter.minDeposits = 1;
            wolfCenter.maxDeposits = 2;

            wolfCenter.AddDepositeChance(DepositeType.iron_medium, 0.6f);
            wolfCenter.AddDepositeChance(DepositeType.gold_medium, 0.4f);
            wolfCenter.AddDepositeChance(DepositeType.ithacit_small, 0.25f);

            wolfCenter.minMiningOperations = 0;
            wolfCenter.maxMiningOperations = 1;
            wolfCenter.AddOperationChance(DepositeType.mo_iron_pirates_small, 0.6f);
            wolfCenter.AddOperationChance(DepositeType.mo_ithacit_pirates_small, 0.4f);

            DepositPreset wolfPlanet = new DepositPreset();
            wolfPlanet.minDeposits = 2;
            wolfPlanet.maxDeposits = 2;
            wolfPlanet.AddDepositeChance(DepositeType.gold_medium, 0.5f);
            wolfPlanet.AddDepositeChance(DepositeType.iron_medium, 0.5f);
            wolfPlanet.AddDepositeChance(DepositeType.ithacit_small, 0.2f);
            wolfPlanet.minMiningOperations = 2;
            wolfPlanet.maxMiningOperations = 2;
            wolfPlanet.AddOperationChance(DepositeType.mo_ithacit_pirates_small, 1.0f);



            DepositPreset greyEdge = new DepositPreset();
            greyEdge.minDeposits = 0;
            greyEdge.maxDeposits = 3;
            greyEdge.AddDepositeChance(DepositeType.iron_medium, 0.2f);
            greyEdge.AddDepositeChance(DepositeType.iron_large, 0.1f);
            greyEdge.AddDepositeChance(DepositeType.titanium_small, 0.6f);
            greyEdge.AddDepositeChance(DepositeType.rhodium_small, 0.6f);
            greyEdge.AddDepositeChance(DepositeType.rhodium_medium, 0.4f);
            greyEdge.AddDepositeChance(DepositeType.titanium_medium, 0.4f);

            greyEdge.minMiningOperations = 0;
            greyEdge.maxMiningOperations = 1;
            greyEdge.AddOperationChance(DepositeType.mo_iron_goliath_medium, 1.0f);
            greyEdge.AddOperationChance(DepositeType.mo_rhodium_goliath_medium, 1.0f);
            greyEdge.AddOperationChance(DepositeType.mo_titanium_goliath_medium, 1.0f);


            DepositPreset greyCenter = new DepositPreset();
            greyCenter.minDeposits = 2;
            greyCenter.maxDeposits = 3;
            greyCenter.AddDepositeChance(DepositeType.iron_large, 0.1f);
            greyCenter.AddDepositeChance(DepositeType.titanium_small, 0.4f);
            greyCenter.AddDepositeChance(DepositeType.rhodium_small, 0.4f);
            greyCenter.AddDepositeChance(DepositeType.rhodium_medium, 0.6f);
            greyCenter.AddDepositeChance(DepositeType.titanium_medium, 0.6f);
            greyCenter.AddDepositeChance(DepositeType.titanium_large, 0.2f);

            greyCenter.minMiningOperations = 0;
            greyCenter.maxMiningOperations = 1;
            greyCenter.AddOperationChance(DepositeType.mo_rhodium_goliath_large, 1.0f);
            greyCenter.AddOperationChance(DepositeType.mo_titanium_goliath_large, 1.0f);
            greyCenter.AddOperationChance(DepositeType.mo_titanium_goliath_medium, 0.5f);

            DepositPreset greyPlanet= new DepositPreset();
            greyPlanet.minDeposits = 2;
            greyPlanet.maxDeposits = 3;
            greyPlanet.AddDepositeChance(DepositeType.titanium_small, 0.4f);
            greyPlanet.AddDepositeChance(DepositeType.rhodium_medium, 0.6f);
            greyPlanet.AddDepositeChance(DepositeType.titanium_large, 0.2f);
            greyPlanet.AddDepositeChance(DepositeType.mitraxit_small, 0.8f);

            greyPlanet.minMiningOperations = 1;
            greyPlanet.maxMiningOperations = 1;
            greyPlanet.AddOperationChance(DepositeType.mo_rhodium_goliath_large, 1.0f);
            greyPlanet.AddOperationChance(DepositeType.mo_mitraxit_goliath_medium, 1.0f);

            DepositPreset greenCenter = new DepositPreset();
            greenCenter.minDeposits = 1;
            greenCenter.maxDeposits = 3;
            greenCenter.AddDepositeChance(DepositeType.rhodium_medium, 0.4f);
            greenCenter.AddDepositeChance(DepositeType.titanium_medium, 0.4f);
            greenCenter.AddDepositeChance(DepositeType.rhodium_large, 0.3f);
            greenCenter.AddDepositeChance(DepositeType.titanium_large, 0.3f);
            greenCenter.AddDepositeChance(DepositeType.mitraxit_small, 0.4f);
            greenCenter.AddDepositeChance(DepositeType.mitraxit_medium, 0.2f);

            greenCenter.minMiningOperations = 0;
            greenCenter.maxMiningOperations = 1;
            greenCenter.AddOperationChance(DepositeType.mo_mitraxit_goliath_medium, 0.4f);
            greenCenter.AddOperationChance(DepositeType.mo_titanium_goliath_large, 0.6f);

            DepositPreset greenPlanet = new DepositPreset();
            greenPlanet.minDeposits = 1;
            greenPlanet.maxDeposits = 2;
            greenPlanet.AddDepositeChance(DepositeType.mitraxit_small, 0.2f);
            greenPlanet.AddDepositeChance(DepositeType.mitraxit_medium, 0.8f);

            greenPlanet.minMiningOperations = 1;
            greenPlanet.maxMiningOperations = 1;
            greenPlanet.AddOperationChance(DepositeType.mo_mitraxit_goliath_medium, 1.0f);

            BiomeSettings homeNebula = new BiomeSettings();
            homeNebula.hdrHigh = 2.011494f;
            homeNebula.alphaMin = 0.8678161f;
            homeNebula.roughness = 1.402299f;
            homeNebula.edgeIntensity = 1.011494f;
            homeNebula.lightA = new Vector3(58.74509f, 52.24622f, 20.16306f);
            homeNebula.lightB = new Vector3(29.36468f, 30.89536f, 33.63745f);
            homeNebula.generalMult = new Vector3(41.09938f, 40.67402f, 18.94029f);
            homeNebula.hdrHphase = new Vector3(70.92018f, 89.55965f, 17.48814f);
            homeNebula.deepBackground = new Vector3(0.001166146f, 0.001004777f, 0.0002607246f);
            homeNebula.starColor = new Vector3(4.604671f, 3.75269f, 1.783027f);
            homeNebula.starDustColor = new Vector3(1.983726f, 1.861052f, 0.8826815f);
            homeNebula.cloudColor = new Vector4(1.10f, 0.95f, 0.70f, 1.75f);

            BiomeSettings homeNebula2 = new BiomeSettings();
            homeNebula2.hdrHigh = 2.011494f;
            homeNebula2.alphaMin = 1f;
            homeNebula2.roughness = 1.264368f;
            homeNebula2.edgeIntensity = 1.241379f;
            homeNebula2.lightA = new Vector3(25.33003f, 20.40416f, 8.19084f);
            homeNebula2.lightB = new Vector3(46.64599f, 31.62712f, 5.094473f);
            homeNebula2.generalMult = new Vector3(68.24397f, 72.97149f, 18.00282f);
            homeNebula2.hdrHphase = new Vector3(47.20952f, 40.66806f, 5.363525f);
            homeNebula2.deepBackground = new Vector3(0.002557754f, 0.002175016f, 0.000748136f);
            //homeNebula2.deepBackground = new Vector3(0.01040825f, 0.007652197f, 0.001943454f);
            homeNebula2.starColor = new Vector3(22.97016f, 23.21552f, 4.557853f);
            homeNebula2.starDustColor = new Vector3(2.203597f, 1.950726f, 0.3995564f);
            homeNebula2.cloudColor = new Vector4(1.10f, 0.95f, 0.70f, 1.75f);

            BiomeSettings homeNebula3 = new BiomeSettings();
            homeNebula3.hdrHigh = 4.54023f;
            homeNebula3.alphaMin = 0.7f;
            homeNebula3.roughness = 1.241379f;
            homeNebula3.edgeIntensity = 2.827586f;
            homeNebula3.lightA = new Vector3(53.90777f, 46.46038f, 6.560145f);
            homeNebula3.lightB = new Vector3(20.65627f, 29.83819f, 30.19149f);
            homeNebula3.generalMult = new Vector3(33.66351f, 29.02074f, 25.46112f);
            homeNebula3.hdrHphase = new Vector3(33.03614f, 33.82566f, 4.789019f);
            homeNebula3.deepBackground = new Vector3(0.0007783365f, 0.0007842809f, 0.002640984f);
            homeNebula3.starColor = new Vector3(14.85765f, 16.38918f, 22.45903f);
            homeNebula3.starDustColor = new Vector3(2.767419f, 2.730127f, 1.661245f);
            homeNebula3.cloudColor = new Vector4(1.00f, 0.70f, 0.47f, 2.00f);

            BiomeSettings grey1 = new BiomeSettings();
            grey1.hdrHigh = 2.873563f;
            grey1.alphaMin = 0.6436782f;
            grey1.roughness = 1.816092f;
            grey1.edgeIntensity = 1.241379f;
            grey1.lightA = new Vector3(77.62281f, 84.75375f, 106.8501f);
            grey1.lightB = new Vector3(0.6123341f, 0.6856937f, 0.9121602f);
            grey1.generalMult = new Vector3(62.44046f, 71.85616f, 105.1602f);
            grey1.hdrHphase = new Vector3(33.65396f, 40.53206f, 73.29436f);
            grey1.deepBackground = new Vector3(0.0004373032f, 0.0005275071f, 0.0009653445f);
            grey1.starColor = new Vector3(77.41413f, 93.1296f, 142.3697f);
            grey1.starDustColor = new Vector3(1.989617f, 2.415941f, 4.960389f);
            grey1.cloudColor = new Vector4(0.75f, 0.96f, 1.00f, 1.25f);

            BiomeSettings grey2 = new BiomeSettings();
            grey2.hdrHigh = 3.678161f;
            grey2.alphaMin = 0.8448276f;
            grey2.roughness = 1.908046f;
            grey2.edgeIntensity = 1.172414f;
            grey2.lightA = new Vector3(9.831491f, 10.73468f, 13.53334f);
            grey2.lightB = new Vector3(26.7634f, 31.08858f, 47.13511f);
            grey2.generalMult = new Vector3(62.44046f, 71.85616f, 105.1602f);
            grey2.hdrHphase = new Vector3(40.45116f, 48.71847f, 88.09789f);
            grey2.deepBackground = new Vector3(0.003657556f, 0.004425628f, 0.008326475f);
            grey2.starColor = new Vector3(77.41413f, 93.1296f, 142.3697f);
            grey2.starDustColor = new Vector3(1.989617f, 2.415941f, 4.960389f);
            grey2.cloudColor = new Vector4(0.75f, 0.96f, 1.00f, 1.25f);

            BiomeSettings grey3 = new BiomeSettings();
            grey3.hdrHigh = 1.954023f;
            grey3.alphaMin = 0f;
            grey3.roughness = 2.758621f;
            grey3.edgeIntensity = 0.8965517f;
            grey3.lightA = new Vector3(52.1054f, 58.77599f, 75.84399f);
            grey3.lightB = new Vector3(12.9222f, 14.29742f, 18.27733f);
            grey3.generalMult = new Vector3(52.95183f, 57.61339f, 69.76345f);
            grey3.hdrHphase = new Vector3(81.22337f, 85.45726f, 93.04193f);
            grey3.deepBackground = new Vector3(0.003657556f, 0.004425628f, 0.008326475f);
            grey3.starColor = new Vector3(77.41413f, 93.1296f, 142.3697f);
            grey3.starDustColor = new Vector3(4.406983f, 5.291645f, 9.885927f);
            grey3.cloudColor = new Vector4(0.75f, 0.96f, 1.00f, 1.25f);


            //mid
            BiomeSettings BlueZone1 = new BiomeSettings();
            BlueZone1.hdrHigh = 6.034483f;
            BlueZone1.alphaMin = 0.4712644f;
            BlueZone1.roughness = 1.310345f;
            BlueZone1.edgeIntensity = 1.057471f;
            BlueZone1.lightA = new Vector3(38.65039f, 44.48375f, 83.05136f);
            BlueZone1.lightB = new Vector3(32.18829f, 35.87666f, 49.33546f);
            BlueZone1.generalMult = new Vector3(54.90977f, 58.46052f, 64.00549f);
            BlueZone1.hdrHphase = new Vector3(19.57376f, 22.37363f, 24.96634f);
            BlueZone1.deepBackground = new Vector3(0.07255945f, 0.08883923f, 0.1435065f);
            BlueZone1.starColor = new Vector3(159.2113f, 167.5105f, 182.3777f);
            BlueZone1.starDustColor = new Vector3(3.758456f, 4.028708f, 5.650819f);

            //edges
            BiomeSettings BlueZone2 = new BiomeSettings();
            BlueZone2.hdrHigh = 6.034483f;
            BlueZone2.alphaMin = 0.4712644f;
            BlueZone2.roughness = 1.310345f;
            BlueZone2.edgeIntensity = 1.057471f;
            BlueZone2.lightA = new Vector3(38.65039f, 44.48375f, 83.05136f);
            BlueZone2.lightB = new Vector3(32.18829f, 35.87666f, 49.33546f);
            BlueZone2.generalMult = new Vector3(54.90977f, 58.46052f, 64.00549f);
            BlueZone2.hdrHphase = new Vector3(19.57376f, 22.37363f, 24.96634f);
            BlueZone2.deepBackground = new Vector3(0.01744201f, 0.02135539f, 0.03449644f);
            BlueZone2.starColor = new Vector3(159.2113f, 167.5105f, 182.3777f);
            BlueZone2.starDustColor = new Vector3(3.758456f, 4.028708f, 5.650819f);

            //cores
            BiomeSettings BlueZone3 = new BiomeSettings();
            BlueZone3.hdrHigh = 6.494253f;
            BlueZone3.alphaMin = 0.4712644f;
            BlueZone3.roughness = 1.241379f;
            BlueZone3.edgeIntensity = 0.8735632f;
            BlueZone3.lightA = new Vector3(51.7419f, 59.05496f, 85.17432f);
            BlueZone3.lightB = new Vector3(28.62817f, 29.75528f, 48.34152f);
            BlueZone3.generalMult = new Vector3(43.31982f, 48.86567f, 63.05582f);
            BlueZone3.hdrHphase = new Vector3(42.24754f, 60.42833f, 70.95039f);
            BlueZone3.deepBackground = new Vector3(0.007500626f, 0.009183503f, 0.01483458f);
            BlueZone3.starColor = new Vector3(159.2113f, 167.5105f, 182.3777f);
            BlueZone3.starDustColor = new Vector3(11.07582f, 11.83739f, 26.11773f);

            //core
            BiomeSettings wolfNebula1 = new BiomeSettings();
            wolfNebula1.hdrHigh = 4.022988f;
            wolfNebula1.alphaMin = 0.7f;
            wolfNebula1.roughness = 1.954023f;
            wolfNebula1.edgeIntensity = 1.83908f;
            wolfNebula1.lightA = new Vector3(31.53212f, 16.25874f, 1.478088f);
            wolfNebula1.lightB = new Vector3(11.25311f, 16.37736f, 21.01193f);
            wolfNebula1.generalMult = new Vector3(95.0456f, 100f, 108.8754f);
            wolfNebula1.hdrHphase = new Vector3(12.44663f, 9.702162f, 7.309071f);
            wolfNebula1.deepBackground = new Vector3(0.005067934f, 0.002613152f, 0.0002375627f);
            wolfNebula1.starColor = new Vector3(2.860611f, 1.668382f, 0.5300902f);
            wolfNebula1.starDustColor = new Vector3(5.259832f, 2.746047f, 0.3160732f);
            wolfNebula1.cloudColor = new Vector4(1.00f, 0.77f, 0.77f, 1.85f);

            //mid
            BiomeSettings wolfNebula2 = new BiomeSettings();
            wolfNebula2.hdrHigh = 3.103448f;
            wolfNebula2.alphaMin = 0f;
            wolfNebula2.roughness = 2.390805f;
            wolfNebula2.edgeIntensity = 2.965517f;
            wolfNebula2.lightA = new Vector3(41.2453f, 21.2671f, 1.9334f);
            wolfNebula2.lightB = new Vector3(13.50493f, 8.15595f, 1.990642f);
            wolfNebula2.generalMult = new Vector3(11.89653f, 7.08017f, 2.49489f);
            wolfNebula2.hdrHphase = new Vector3(25.82528f, 13.55862f, 1.707091f);
            wolfNebula2.deepBackground = new Vector3(0.03519428f, 0.01814704f, 0.001649754f);
            wolfNebula2.starColor = new Vector3(18.89201f, 9.741189f, 0.8855752f);
            wolfNebula2.starDustColor = new Vector3(6.12195f, 3.681768f, 1.362334f);
            wolfNebula2.cloudColor = new Vector4(1.00f, 0.77f, 0.77f, 1.85f);

            //edge
            BiomeSettings wolfNebula3 = new BiomeSettings();
            wolfNebula3.hdrHigh = 3.103448f;
            wolfNebula3.alphaMin = 0f;
            wolfNebula3.roughness = 2.275862f;
            wolfNebula3.edgeIntensity = 2.965517f;
            wolfNebula3.lightA = new Vector3(41.2453f, 21.2671f, 1.9334f);
            wolfNebula3.lightB = new Vector3(10.07949f, 7.256463f, 1.641446f);
            wolfNebula3.generalMult = new Vector3(14.21539f, 9.240157f, 4.578308f);
            wolfNebula3.hdrHphase = new Vector3(25.82528f, 13.55862f, 1.707091f);
            wolfNebula3.deepBackground = new Vector3(0.009706136f, 0.008359965f, 0.01867621f);
            wolfNebula3.starColor = new Vector3(27.99611f, 30.15531f, 47.66721f);
            wolfNebula3.starDustColor = new Vector3(5.259832f, 2.746047f, 0.3160732f);
            wolfNebula3.cloudColor = new Vector4(1.00f, 0.77f, 0.77f, 1.85f);

            BiomeSettings green1 = new BiomeSettings();
            green1.hdrHigh = 6.551724f;
            green1.alphaMin = 0.7f;
            green1.roughness = 1.632184f;
            green1.edgeIntensity = 2.505747f;
            green1.lightA = new Vector3(18.09307f, 17.67047f, 60.35681f);
            green1.lightB = new Vector3(45.76286f, 75.74982f, 54.14309f);
            green1.generalMult = new Vector3(32.29062f, 34.82465f, 62.92002f);
            green1.hdrHphase = new Vector3(34.36191f, 39.85612f, 100.4626f);
            green1.deepBackground = new Vector3(0.004056335f, 0.004683767f, 0.009448498f);
            green1.starColor = new Vector3(32.30175f, 22.75745f, 139.7924f);
            green1.starDustColor = new Vector3(7.368022f, 8.449943f, 21.83812f);
            green1.cloudColor = new Vector4(0.51f, 1.00f, 0.92f, 1.65f);

            BiomeSettings purpleNebula1 = new BiomeSettings();
            purpleNebula1.hdrHigh = 2f;
            purpleNebula1.alphaMin = 0.7f;
            purpleNebula1.roughness = 1.494253f;
            purpleNebula1.edgeIntensity = 2.229885f;
            purpleNebula1.lightA = new Vector3(15.6846f, 7.623137f, 21.69233f);
            purpleNebula1.lightB = new Vector3(26.35664f, 11.50398f, 95.41244f);
            purpleNebula1.generalMult = new Vector3(43.00201f, 33.83385f, 91.82114f);
            purpleNebula1.hdrHphase = new Vector3(36.81719f, 32.32293f, 62.199f);
            purpleNebula1.deepBackground = new Vector3(0.001986626f, 0.001318207f, 0.00397661f);
            purpleNebula1.starColor = new Vector3(12.69745f, 5.212856f, 60.82891f);
            purpleNebula1.starDustColor = new Vector3(1.724831f, 0.7818472f, 8.503229f);

            BiomeSettings purpleNebula2 = new BiomeSettings();
            purpleNebula2.hdrHigh = 2f;
            purpleNebula2.alphaMin = 0.7f;
            purpleNebula2.roughness = 0.9655172f;
            purpleNebula2.edgeIntensity = 1.655172f;
            purpleNebula2.lightA = new Vector3(53.14526f, 26.02707f, 64.60645f);
            purpleNebula2.lightB = new Vector3(45.58543f, 75.44696f, 63.67706f);
            purpleNebula2.generalMult = new Vector3(43.00201f, 33.83385f, 91.82114f);
            purpleNebula2.hdrHphase = new Vector3(58.09919f, 42.88883f, 99.79851f);
            purpleNebula2.deepBackground = new Vector3(0.0006321772f, 0.0007575679f, 0.001230661f);
            purpleNebula2.starColor = new Vector3(37.35214f, 34.22141f, 70.89338f);
            purpleNebula2.starDustColor = new Vector3(1.950468f, 1.233122f, 8.578442f);

            BiomeSettings pandoraNebula1 = new BiomeSettings();
            pandoraNebula1.hdrHigh = 5.69f;
            pandoraNebula1.alphaMin = 0.7f;
            pandoraNebula1.roughness = 1f;
            pandoraNebula1.edgeIntensity = 2f;
            pandoraNebula1.lightA = new Vector3(34.8102f, 27.42093f, 98.0392f);
            pandoraNebula1.lightB = new Vector3(10.08916f, 12.36316f, 22.45008f);
            pandoraNebula1.generalMult = new Vector3(30.10824f, 34.58802f, 62.98922f);
            pandoraNebula1.hdrHphase = new Vector3(30.45162f, 42.79364f, 64.55468f);
            pandoraNebula1.deepBackground = new Vector3(0.0008049852f, 0.0004565845f, 0.003824596f);
            pandoraNebula1.starColor = new Vector3(46.65211f, 56.27519f, 102.9843f);
            pandoraNebula1.starDustColor = new Vector3(6.355449f, 6.380413f, 21.49203f);
            pandoraNebula1.cloudColor = new Vector4(0.38f, 0.74f, 1.20f, 1.85f);

            /* music values
                0 - none
                1 - grey belt
                2 - green storm
                3 - cold blue
                4 - deep space
                5 - warm orange
                6 - sinister red
                7 - mysterious
            */

            //music
            string[] orangeSongs = new string[] { "Track 1 Warm Orange", "Track 2 Warm Orange", "Track 3 Warm Orange", "Track 4 Warm Orange", "Track 5 Warm Orange", "Track 6 Warm Orange" };
            string[] redSongs = new string[] { "Track 1 sinister Red", "Track 2 sinister Red" };//6;
            string[] blueSongs = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;
            string[] greySongs = new string[] { "Track 1 Grey Belt", "Track 2 Grey Belt" };
            string[] blackSongs = new string[] { "Track 1 Black Space", "Track 2 Black Space" };
            string[] greenSongs = new string[] { "Track 1 Green Nebula", "Track 2 Green Nebula" };
            string[] mysterySongs = new string[] { "Track 1 Mystery", "Track 2 Mystery" };



        //Icons for things in space

        POEIcon icon = new POEIcon();
            icon.artSource = new Rectangle(2 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_trade_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(5 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_farm_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(3 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_iron_mine_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(3 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_gold_mine_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(3 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_titanium_mine_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(1 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_smelter_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(1 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_alloy_smelter_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(1 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_refinery_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(0 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_parts_factory_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(0 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_materials_factory_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(0 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_materials_lab_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(0 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_electronics_lab_1"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(0 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_goods_factory_a"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(0 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_goods_factory_b"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(0 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_goods_factory_c"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(4 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_shipyard_2"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(2 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_trade_2"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(2 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["econ_mall_2"] = icon;

            //repair station
            icon.artSource = new Rectangle(832, 0, 64, 64);
            icon.globallyVisible = false;
            //icon.globallyVisible = true;//for debugging
            icon.sensorVisible = true;
            interestIcons["station_t1_repair"] = icon;

            //gates
            icon = new POEIcon();
            icon.artSource = new Rectangle(9 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            //icon.globallyVisible = true;//for debugging
            icon.sensorVisible = true;
            interestIcons["gate_a_inactive"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(10 * 64, 0, 64, 64);
            icon.globallyVisible = true;
            interestIcons["gate_a_active"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(10 * 64, 0, 64, 64);
            icon.globallyVisible = true;
            interestIcons["gate_a_active_destonly_higgsbase"] = icon;

            //simple abandoned mining stations
            icon = new POEIcon();
            icon.artSource = new Rectangle(11 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            //icon.globallyVisible = true;//for debugging
            icon.sensorVisible = true;
            interestIcons["station_r1_a"] = icon;

            icon = new POEIcon();
            icon.artSource = new Rectangle(15 * 64, 0, 64, 64);
            icon.globallyVisible = true;
            icon.sensorVisible = true;
            interestIcons["station_r1_a_explored"] = icon;

            //everett's coherence red zone mystery point
            icon = new POEIcon();
            icon.artSource = new Rectangle(12 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["everett_r"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(12 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["everett_b"] = icon;

            //pirates cove trade stations
            icon = new POEIcon();
            icon.artSource = new Rectangle(2 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["port_pc"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(2 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["shop_a_pc"] = icon;
            icon = new POEIcon();
            icon.artSource = new Rectangle(2 * 64, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["shop_b_pc"] = icon;

            //SSC relay station for the player to destroy
            icon = new POEIcon();
            icon.artSource = new Rectangle(896, 0, 64, 64);
            icon.globallyVisible = false;
            icon.sensorVisible = true;
            interestIcons["ssc_relay"] = icon;

            //lights
            LightSettings blueLights = new LightSettings();
            blueLights.lightColor = new Color(0.2f, 0.4f, 0.9f, 1f);
            blueLights.lightIntensity = 1.2f;
            blueLights.ambLightColor = new Color(0.17f, 0.17f, 0.2f, 1f);
            blueLights.fogColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            LightSettings greenZoneLights = new LightSettings(); //oficially blue-toxic
            greenZoneLights.lightColor = new Color(73, 134, 181, 255);
            greenZoneLights.ambLightColor = new Color(78, 172, 167, 255);
            greenZoneLights.lightIntensity = 1.16f;

            LightSettings pandoraLights = new LightSettings();
            pandoraLights.lightColor = new Color(106, 175, 255, 255);
            pandoraLights.ambLightColor = new Color(89, 120, 165, 255);
            pandoraLights.lightIntensity = 1.1f;


            LightSettings blueZoneLights = new LightSettings();
            blueZoneLights.lightColor = new Color(107, 107, 115, 255);
            blueZoneLights.ambLightColor = new Color(63, 117, 191, 255);
            blueZoneLights.lightIntensity = 1.255f;

            LightSettings wolfNebulaLights = new LightSettings();
            wolfNebulaLights.lightColor = new Color(155, 108, 108, 255);
            wolfNebulaLights.ambLightColor = new Color(74, 24, 24, 255);
            wolfNebulaLights.lightIntensity = 1.40f;

            LightSettings wolfNebulaEdgeLights = new LightSettings();
            wolfNebulaEdgeLights.lightColor = new Color(165, 89, 89, 255);
            wolfNebulaEdgeLights.ambLightColor = new Color(24, 35, 74, 255);
            wolfNebulaEdgeLights.lightIntensity = 1.05f;

            LightSettings homeNebulaLights = new LightSettings();
            homeNebulaLights.lightColor = new Color(1.0f, 0.70f, 0.4f, 1.0f);
            homeNebulaLights.ambLightColor = new Color(80, 75, 72, 255);
            homeNebulaLights.lightIntensity = 1.475f;

            LightSettings purpleNebula1Lights = new LightSettings();
            purpleNebula1Lights.lightColor = new Color(63, 92, 191, 255);
            purpleNebula1Lights.ambLightColor = new Color(174, 80, 153, 255);
            purpleNebula1Lights.lightIntensity = 1.806452f;

            LightSettings purpleNebula2Lights = new LightSettings();
            purpleNebula2Lights.lightColor = new Color(83, 107, 171, 255);
            purpleNebula2Lights.ambLightColor = new Color(137, 71, 122, 255);
            purpleNebula2Lights.lightIntensity = 1.806452f;

            //old ones
            LightSettings greyLights = new LightSettings();
            greyLights.lightColor = new Color(0.57f, 0.79f, 1f, 1f);
            greyLights.lightIntensity = 1.3f;
            greyLights.ambLightColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            greyLights.fogColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            LightSettings orangeLights = new LightSettings();
            orangeLights.lightColor = new Color(0.969f, 0.773f, 0.639f);
            orangeLights.lightIntensity = 1.5f;
            orangeLights.ambLightColor = new Color(0.25f, 0.28f, 0.35f);

            LightSettings redLights = new LightSettings();
            redLights.lightColor = new Color(1.1f, 1.0f, 1.0f, 1.0f);
            redLights.lightIntensity = 1.2f;
            redLights.ambLightColor = new Color(0.2f, 0.2f, 0.24f, 1f);

            LightSettings redLightsDeep = new LightSettings();
            redLightsDeep.lightColor = new Color(1.1f, 1.0f, 1.0f, 1.0f);
            redLightsDeep.lightIntensity = 1.2f;
            redLightsDeep.ambLightColor = new Color(0.2f, 0.2f, 0.24f, 1f);
            redLightsDeep.fogColor = new Color(0.4f, 0.0f, 0.0f, 0.1f);

            LightSettings greenLights = new LightSettings(); //officialy blue
            greenLights.lightColor = new Color(0.85f, 0.85f, 1.0f, 1.0f);
            greenLights.lightIntensity = 1.28f;
            greenLights.ambLightColor = new Color(0.22f, 0.23f, 0.23f, 1f);
            greenLights.fogColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            /*
            Vector4 blueZoneLightShafts = new Vector4(10 - 0.1f, 10 - 7.0f,0,0);

            Vector4 greenZoneLightShafts = new Vector4(10 - 0.14f, 10 - 6.5f, 0, 0);
            Vector4 greyRoidsLightShafts = new Vector4(10 - 0.12f, 10 - 6.5f, 0, 0);

            Vector4 wolfNebulaEdgeLS = new Vector4(10 - 0.3f, 10 - 6.5f, 0, 0);
            Vector4 wolfNebulaCoreLS = new Vector4(10 - 0.2f, 10 - 4.5f, 0, 0);

            Vector4 homeNebulaCoreLS = new Vector4(10 - 10.0f, 10 - 10.0f, 0, 0);
            Vector4 homeNebula2CoreLS = new Vector4(10 - 10.0f, 10 - 10.0f, 0, 0);

            Vector4 purpleNebulaEdgeLS = new Vector4(10 - 0.12f, 10 - 8.9f, 0, 0);
            Vector4 purpleNebulaCoreLS = new Vector4(10 - 0.10f, 10 - 8.45f, 0, 0);
            */


            //First three for color, last oen for fog density
            Vector4 blueZoneLightShafts = new Vector4(0.79f * 0.5f, 0.93f * 0.5f, 1.00f * 0.5f, 1.2f);

            Vector4 greenZoneLightShafts = new Vector4(0.100f, 0.12f, 0.72f, 1.45f);

            Vector4 greyRoidsLightShafts = new Vector4(0.51f * 0.4f, 0.98f * 0.4f, 0.85f, 1);
            Vector4 greyZoneLightShafts = new Vector4(0.51f * 0.5f, 0.98f * 0.5f, 0.75f, 0.85f);

            Vector4 pandora1LightShafts = new Vector4(0, 126 / 255f, 244f / 255f, 0.25f);

            Vector4 wolfNebulaEdgeLS = new Vector4(0.1f, 0.29f, 0.79f, 0.65f);
            Vector4 wolfNebulaCoreLS = new Vector4(0.87f, 0.09f, 0.09f, 1.20f);

            Vector4 homeNebulaCoreLS = new Vector4(0.97f, 0.71f, 0.41f, 0.75f);
            Vector4 homeNebula2CoreLS = new Vector4(0.97f, 0.71f, 0.41f, 0.75f);

            Vector4 purpleNebulaEdgeLS = new Vector4(0.65f, 0.27f, 0.87f, 1f);
            Vector4 purpleNebulaCoreLS = new Vector4(0.65f, 0.27f, 0.87f, 1f);

            string worldData = Directory.GetCurrentDirectory() + @"\Data\280_-200.bdm";
            mapDataIncludes.Add(worldData);

            IconBatch cyanBatch = new IconBatch("256roidBT");
            IconBatch blueBatch = new IconBatch("256roidBT");
            IconBatch blueBatchLow = new IconBatch("256roidBTlow");
            IconBatch redBatch = new IconBatch("256roidRT");
            IconBatch orangeBatch = new IconBatch("256roidOG");
            IconBatch greyBatch = new IconBatch("256roidGT");
            IconBatch greenBatch = new IconBatch("256roidGE");
            IconBatch starBatch = new IconBatch("softmask");

            starBatch.addDrawFitTechnique("star");

            cyanBatch.addTiledTechnique("nebula", new Rectangle(0, 512, 1024, 1024), new Rectangle(0, 0, 512, 512));

            blueBatch.addTiledTechnique("nebula", new Rectangle(0, 512, 1024, 1024), new Rectangle(0, 0, 512, 512));
            blueBatch.addTiledTechnique("planet", new Rectangle(0, 512, 1024, 1024), new Rectangle(0, 0, 512, 512), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));
            blueBatchLow.addTiledTechnique("nebula", new Rectangle(0, 512, 1024, 1024), new Rectangle(0, 0, 512, 512));
            blueBatch.addTiledTechnique("solo_planet", new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));

            redBatch.addTiledTechnique("nebula", new Rectangle(0, 512, 1024, 1024), new Rectangle(0, 0, 512, 512));
            redBatch.addTiledTechnique("planet", new Rectangle(0, 512, 1024, 1024), new Rectangle(0, 0, 512, 512), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));
            redBatch.addTiledTechnique("solo_planet", new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));

            orangeBatch.addTiledTechnique("nebula", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512));
            orangeBatch.addTiledTechnique("planet", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));
            orangeBatch.addTiledTechnique("solo_planet", new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));

            greyBatch.addTiledTechnique("nebula", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512));
            greyBatch.addTiledTechnique("roids", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512), new Rectangle(512, 512, 512, 512));
            greyBatch.addTiledTechnique("planet", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));
            //greyBatch.addTiledTechnique("planetroids", new Rectangle(512, 512, 1024, 1024), new Rectangle(0, 0, 512, 512), new Rectangle(512, 0, 512, 512), new Rectangle(512, 0, 512, 512));

            //orangeBatch.addTiledTechnique("nebula", new Rectangle(512, 0, 1024, 1024), new Rectangle(0, 0, 512, 512), new Rectangle(512, 0, 512, 512));
            orangeBatch.addTiledTechnique("roids", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512), new Rectangle(512, 512, 512, 512));
            orangeBatch.addTiledTechnique("planetroids", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512), new Rectangle(512, 512, 512, 512), new Rectangle(512, 0, 512, 512));

            greenBatch.addTiledTechnique("nebula", new Rectangle(0, 512, 1024, 1024), new Rectangle(0, 0, 512, 512));
            greenBatch.addTiledTechnique("planet", new Rectangle(0, 1024, 1024, 1024), new Rectangle(0, 512, 512, 512), new Rectangle(0, 0, 0, 0), new Rectangle(512, 0, 512, 512));

            //string[] starIcon = new string[3];
            //starIcon[0] = null;
            //starIcon[1] = "256starY";
            //starIcon[2] = null;

            //string[] roidIconOrange = new string[3];
            //roidIconOrange[0] = "256roidOT";
            //roidIconOrange[1] = "256roidOM";
            //roidIconOrange[2] = "256roidOB";

            //string[] roidIconGrey = new string[3];
            //roidIconGrey[0] = "256roidGT";
            //roidIconGrey[1] = "256roidGM";
            //roidIconGrey[2] = "256roidGB";

            //string[] roidIconBlue = new string[3];
            //roidIconBlue[0] = "256roidBT";
            //roidIconBlue[1] = "256roidBM";
            //roidIconBlue[2] = "256roidBB";

            //string[] roidIconRed = new string[3];
            //roidIconRed[0] = "256roidRT";
            //roidIconRed[1] = "256roidRM";
            //roidIconRed[2] = "256roidRB";




            //an example sector using a custom color that can be drawn onto map data
            Color c = new Color(127, 51, 0);
            backdrops[c] = new DefaultBackdrop("The_Eye");

            //all sectors with stars generate as yellow unless you override with your own backdrop type


            /*
            star.assetName = "StarObject";
            star.rotation = 0;
            star.scale = 30;
            star.position = new Vector3(0, 0, -50000);
            b.addStellarObject(star);
            backdrops[c] = b;
            audioSettings[c] = orangeSongs;
            */
            //mapIcons[c] = starIcon;
            /*

            //all base planets generated by noise
            int blue = 0;
            for (int i = 0; i < 14; i++)
            {
                c = new Color(38, 127, blue, 255);
                blue++;
                b = new DefaultBackdrop("Stars");
                star = new StellarObject();

                //FUCK THIS

                star.assetName = "Planet" + (blue + 1).ToString();
                star.rotation = 0;
                star.scale = 400;
                star.position = new Vector3(0, 0, -100000);
                b.addStellarObject(star);
                backdrops[c] = b;
            }*/

            string[] asteroidTexNames;
            string[] nebulaTexNames;

            #region declaringTextures

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
            sectorTextures["asteroids1"] = asteroidFieldAssets;

            TextureBatch blackAsteroidFieldAssets = new TextureBatch();
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "AsteriodBlack_dif_1";
            asteroidTexNames[1] = "AsteriodBlack_dif_2";
            asteroidTexNames[2] = "AsteriodBlack_dif_3";
            asteroidTexNames[3] = "AsteriodBlack_dif_4";
            asteroidTexNames[4] = "AsteriodBlack_dif_5";
            blackAsteroidFieldAssets.diffNames = asteroidTexNames;
            blackAsteroidFieldAssets.collideNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "AsteriodBlack_norm_1";
            asteroidTexNames[1] = "AsteriodBlack_norm_2";
            asteroidTexNames[2] = "AsteriodBlack_norm_3";
            asteroidTexNames[3] = "AsteriodBlack_norm_4";
            asteroidTexNames[4] = "AsteriodBlack_norm_5";
            blackAsteroidFieldAssets.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "AsteriodBlack_spec_1";
            asteroidTexNames[1] = "AsteriodBlack_spec_2";
            asteroidTexNames[2] = "AsteriodBlack_spec_3";
            asteroidTexNames[3] = "AsteriodBlack_spec_4";
            asteroidTexNames[4] = "AsteriodBlack_spec_5";
            blackAsteroidFieldAssets.specNames = asteroidTexNames;
            digging = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                digging[i] = false;
            }
            blackAsteroidFieldAssets.assetDiggable = digging;
            sectorTextures["asteroidsBlack"] = blackAsteroidFieldAssets;

            TextureBatch blackRedAsteroidFieldAssets = new TextureBatch();
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "AsteriodBlack_dif_1";
            asteroidTexNames[1] = "AsteriodBlack_dif_2";
            asteroidTexNames[2] = "AsteriodBlack_dif_3";
            asteroidTexNames[3] = "AsteriodBlack_dif_4";
            asteroidTexNames[4] = "AsteriodBlack_dif_5";
            blackRedAsteroidFieldAssets.diffNames = asteroidTexNames;
            blackRedAsteroidFieldAssets.collideNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "AsteriodBlack_norm_1";
            asteroidTexNames[1] = "AsteriodBlack_norm_2";
            asteroidTexNames[2] = "AsteriodBlack_norm_3";
            asteroidTexNames[3] = "AsteriodBlack_norm_4";
            asteroidTexNames[4] = "AsteriodBlack_norm_5";
            blackRedAsteroidFieldAssets.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "AsteriodBlack_spec_1";
            asteroidTexNames[1] = "AsteriodBlack_spec_2";
            asteroidTexNames[2] = "AsteriodBlack_spec_3";
            asteroidTexNames[3] = "AsteriodBlack_spec_4";
            asteroidTexNames[4] = "AsteriodBlack_spec_5";
            blackRedAsteroidFieldAssets.specNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "AsteriodBlack_emis_1";
            asteroidTexNames[1] = "AsteriodBlack_emis_2";
            asteroidTexNames[2] = "AsteriodBlack_emis_3";
            asteroidTexNames[3] = "AsteriodBlack_emis_4";
            asteroidTexNames[4] = "AsteriodBlack_emis_5";
            blackRedAsteroidFieldAssets.emitNames = asteroidTexNames;
            digging = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                digging[i] = false;
            }
            blackRedAsteroidFieldAssets.assetDiggable = digging;
            sectorTextures["asteroidsBlackRed"] = blackRedAsteroidFieldAssets;



            TextureBatch iceFieldAssets = new TextureBatch();
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "IceRock_A_dif";
            asteroidTexNames[1] = "IceRock_B_dif";
            asteroidTexNames[2] = "IceRock_C_dif";
            asteroidTexNames[3] = "IceRock_D_dif";
            asteroidTexNames[4] = "IceRock_E_dif";
            iceFieldAssets.diffNames = asteroidTexNames;
            iceFieldAssets.collideNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "IceRock_A_norm";
            asteroidTexNames[1] = "IceRock_B_norm";
            asteroidTexNames[2] = "IceRock_C_norm";
            asteroidTexNames[3] = "IceRock_D_norm";
            asteroidTexNames[4] = "IceRock_E_norm";
            iceFieldAssets.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "IceRock_A_spec";
            asteroidTexNames[1] = "IceRock_B_spec";
            asteroidTexNames[2] = "IceRock_C_spec";
            asteroidTexNames[3] = "IceRock_D_spec";
            asteroidTexNames[4] = "IceRock_E_spec";
            iceFieldAssets.specNames = asteroidTexNames;
            asteroidTexNames = new string[5];
            asteroidTexNames[0] = "IceRock_A_emis";
            asteroidTexNames[1] = "IceRock_B_emis";
            asteroidTexNames[2] = "IceRock_C_emis";
            asteroidTexNames[3] = "IceRock_D_emis";
            asteroidTexNames[4] = "IceRock_E_emis";
            iceFieldAssets.emitNames = asteroidTexNames;
            digging = new bool[11];
            for (int i = 0; i < 11; i++)
            {
                digging[i] = false;
            }
            iceFieldAssets.assetDiggable = digging;
            sectorTextures["iceAsteroids1"] = iceFieldAssets;


            TextureBatch infestedBlackAsteroidsAssets = new TextureBatch();
            int countInBatch = 6;
            asteroidTexNames = new string[countInBatch];
            asteroidTexNames[0] = "infested_roidA_dif";
            asteroidTexNames[1] = "infested_roidB_dif";
            asteroidTexNames[2] = "infested_roidC_dif";
            asteroidTexNames[3] = "infested_roidD_dif";
            asteroidTexNames[4] = "infested_roidE_dif";
            asteroidTexNames[5] = "infested_roidF_dif";
            infestedBlackAsteroidsAssets.diffNames = asteroidTexNames;
            infestedBlackAsteroidsAssets.collideNames = asteroidTexNames;
            asteroidTexNames = new string[countInBatch];
            asteroidTexNames[0] = "infested_roidA_bump";
            asteroidTexNames[1] = "infested_roidB_bump";
            asteroidTexNames[2] = "infested_roidC_bump";
            asteroidTexNames[3] = "infested_roidD_bump";
            asteroidTexNames[4] = "infested_roidE_bump";
            asteroidTexNames[5] = "infested_roidF_bump";
            infestedBlackAsteroidsAssets.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[countInBatch];
            asteroidTexNames[0] = "infested_roidA_spec";
            asteroidTexNames[1] = "infested_roidB_spec";
            asteroidTexNames[2] = "infested_roidC_spec";
            asteroidTexNames[3] = "infested_roidD_spec";
            asteroidTexNames[4] = "infested_roidE_spec";
            asteroidTexNames[5] = "infested_roidF_spec";
            infestedBlackAsteroidsAssets.specNames = asteroidTexNames;
            asteroidTexNames = new string[countInBatch];
            asteroidTexNames[0] = "infested_roidA_emis";
            asteroidTexNames[1] = "infested_roidB_emis";
            asteroidTexNames[2] = "infested_roidC_emis";
            asteroidTexNames[3] = "infested_roidD_emis";
            asteroidTexNames[4] = "infested_roidE_emis";
            asteroidTexNames[5] = "infested_roidF_emis";
            infestedBlackAsteroidsAssets.emitNames = asteroidTexNames;
            digging = new bool[countInBatch];
            for (int i = 0; i < countInBatch; i++)
            {
                digging[i] = false;
            }
            infestedBlackAsteroidsAssets.assetDiggable = digging;
            sectorTextures["asteroidInfestedBlack"] = infestedBlackAsteroidsAssets;



            TextureBatch wreckage = new TextureBatch();
            asteroidTexNames = new string[23];
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
            asteroidTexNames[11] = "prefab_mediumA_dif";
            asteroidTexNames[12] = "prefab_mediumB_dif";
            asteroidTexNames[13] = "prefab_mediumC_dif";
            asteroidTexNames[14] = "prefab_mediumD_dif";
            asteroidTexNames[15] = "prefab_mediumE_dif";
            asteroidTexNames[16] = "prefab_mediumF_dif";
            asteroidTexNames[17] = "prefab_mediumG_dif";
            asteroidTexNames[18] = "prefab_smallA_dif";
            asteroidTexNames[19] = "prefab_smallB_dif";
            asteroidTexNames[20] = "prefab_smallC_dif";
            asteroidTexNames[21] = "prefab_tinyA_dif";
            asteroidTexNames[22] = "prefab_tinyB_dif";
            wreckage.diffNames = asteroidTexNames;
            wreckage.collideNames = asteroidTexNames;
            asteroidTexNames = new string[23];
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
            asteroidTexNames[11] = "prefab_mediumA_norm";
            asteroidTexNames[12] = "prefab_mediumB_norm";
            asteroidTexNames[13] = "prefab_mediumC_norm";
            asteroidTexNames[14] = "prefab_mediumD_norm";
            asteroidTexNames[15] = "prefab_mediumE_norm";
            asteroidTexNames[16] = "prefab_mediumF_norm";
            asteroidTexNames[17] = "prefab_mediumG_norm";
            asteroidTexNames[18] = "prefab_smallA_norm";
            asteroidTexNames[19] = "prefab_smallB_norm";
            asteroidTexNames[20] = "prefab_smallC_norm";
            asteroidTexNames[21] = "prefab_tinyA_norm";
            asteroidTexNames[22] = "prefab_tinyB_norm";
            wreckage.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[23];
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
            asteroidTexNames[11] = "prefab_mediumA_spec";
            asteroidTexNames[12] = "prefab_mediumB_spec";
            asteroidTexNames[13] = "prefab_mediumC_spec";
            asteroidTexNames[14] = "prefab_mediumD_spec";
            asteroidTexNames[15] = "prefab_mediumE_spec";
            asteroidTexNames[16] = "prefab_mediumF_spec";
            asteroidTexNames[17] = "prefab_mediumG_spec";
            asteroidTexNames[18] = "prefab_smallA_spec";
            asteroidTexNames[19] = "prefab_smallB_spec";
            asteroidTexNames[20] = "prefab_smallC_spec";
            asteroidTexNames[21] = "prefab_tinyA_spec";
            asteroidTexNames[22] = "prefab_tinyB_spec";
            wreckage.specNames = asteroidTexNames;
            digging = new bool[23];
            for (int i = 0; i < 23; i++)
            {
                digging[i] = false;
            }
            wreckage.assetDiggable = digging;
            sectorTextures["wreckage"] = wreckage;//mine and jan's no red versions

            TextureBatch redWreckage = new TextureBatch();
            asteroidTexNames = new string[12];
            asteroidTexNames[0] = "prefab_mediumAred_dif";
            asteroidTexNames[1] = "prefab_mediumBred_dif";
            asteroidTexNames[2] = "prefab_mediumCred_dif";
            asteroidTexNames[3] = "prefab_mediumDred_dif";
            asteroidTexNames[4] = "prefab_mediumEred_dif";
            asteroidTexNames[5] = "prefab_mediumFred_dif";
            asteroidTexNames[6] = "prefab_mediumGred_dif";
            asteroidTexNames[7] = "prefab_smallAred_dif";
            asteroidTexNames[8] = "prefab_smallBred_dif";
            asteroidTexNames[9] = "prefab_smallCred_dif";
            asteroidTexNames[10] = "prefab_tinyAred_dif";
            asteroidTexNames[11] = "prefab_tinyBred_dif";
            redWreckage.diffNames = asteroidTexNames;
            redWreckage.collideNames = asteroidTexNames;
            asteroidTexNames = new string[12];
            asteroidTexNames[0] = "prefab_mediumA_norm";
            asteroidTexNames[1] = "prefab_mediumB_norm";
            asteroidTexNames[2] = "prefab_mediumC_norm";
            asteroidTexNames[3] = "prefab_mediumD_norm";
            asteroidTexNames[4] = "prefab_mediumE_norm";
            asteroidTexNames[5] = "prefab_mediumF_norm";
            asteroidTexNames[6] = "prefab_mediumG_norm";
            asteroidTexNames[7] = "prefab_smallA_norm";
            asteroidTexNames[8] = "prefab_smallB_norm";
            asteroidTexNames[9] = "prefab_smallC_norm";
            asteroidTexNames[10] = "prefab_tinyA_norm";
            asteroidTexNames[11] = "prefab_tinyB_norm";
            redWreckage.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[12];
            asteroidTexNames[0] = "prefab_mediumA_spec";
            asteroidTexNames[1] = "prefab_mediumB_spec";
            asteroidTexNames[2] = "prefab_mediumC_spec";
            asteroidTexNames[3] = "prefab_mediumD_spec";
            asteroidTexNames[4] = "prefab_mediumE_spec";
            asteroidTexNames[5] = "prefab_mediumF_spec";
            asteroidTexNames[6] = "prefab_mediumG_spec";
            asteroidTexNames[7] = "prefab_smallA_spec";
            asteroidTexNames[8] = "prefab_smallB_spec";
            asteroidTexNames[9] = "prefab_smallC_spec";
            asteroidTexNames[10] = "prefab_tinyA_spec";
            asteroidTexNames[11] = "prefab_tinyB_spec";
            redWreckage.specNames = asteroidTexNames;
            digging = new bool[12];
            for (int i = 0; i < 12; i++)
            {
                digging[i] = false;
            }
            redWreckage.assetDiggable = digging;
            sectorTextures["redwreckage"] = redWreckage;//red ones onnly


            TextureBatch mixedWreckage = new TextureBatch();
            asteroidTexNames = new string[35];
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
            asteroidTexNames[11] = "prefab_mediumA_dif";
            asteroidTexNames[12] = "prefab_mediumB_dif";
            asteroidTexNames[13] = "prefab_mediumC_dif";
            asteroidTexNames[14] = "prefab_mediumD_dif";
            asteroidTexNames[15] = "prefab_mediumE_dif";
            asteroidTexNames[16] = "prefab_mediumF_dif";
            asteroidTexNames[17] = "prefab_mediumG_dif";
            asteroidTexNames[18] = "prefab_smallA_dif";
            asteroidTexNames[19] = "prefab_smallB_dif";
            asteroidTexNames[20] = "prefab_smallC_dif";
            asteroidTexNames[21] = "prefab_tinyA_dif";
            asteroidTexNames[22] = "prefab_tinyB_dif";
            asteroidTexNames[23] = "prefab_mediumAred_dif";
            asteroidTexNames[24] = "prefab_mediumBred_dif";
            asteroidTexNames[25] = "prefab_mediumCred_dif";
            asteroidTexNames[26] = "prefab_mediumDred_dif";
            asteroidTexNames[27] = "prefab_mediumEred_dif";
            asteroidTexNames[28] = "prefab_mediumFred_dif";
            asteroidTexNames[29] = "prefab_mediumGred_dif";
            asteroidTexNames[30] = "prefab_smallAred_dif";
            asteroidTexNames[31] = "prefab_smallBred_dif";
            asteroidTexNames[32] = "prefab_smallCred_dif";
            asteroidTexNames[33] = "prefab_tinyAred_dif";
            asteroidTexNames[34] = "prefab_tinyBred_dif";
            mixedWreckage.diffNames = asteroidTexNames;
            mixedWreckage.collideNames = asteroidTexNames;
            asteroidTexNames = new string[35];
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
            asteroidTexNames[11] = "prefab_mediumA_norm";
            asteroidTexNames[12] = "prefab_mediumB_norm";
            asteroidTexNames[13] = "prefab_mediumC_norm";
            asteroidTexNames[14] = "prefab_mediumD_norm";
            asteroidTexNames[15] = "prefab_mediumE_norm";
            asteroidTexNames[16] = "prefab_mediumF_norm";
            asteroidTexNames[17] = "prefab_mediumG_norm";
            asteroidTexNames[18] = "prefab_smallA_norm";
            asteroidTexNames[19] = "prefab_smallB_norm";
            asteroidTexNames[20] = "prefab_smallC_norm";
            asteroidTexNames[21] = "prefab_tinyA_norm";
            asteroidTexNames[22] = "prefab_tinyB_norm";
            asteroidTexNames[23] = "prefab_mediumA_norm";
            asteroidTexNames[24] = "prefab_mediumB_norm";
            asteroidTexNames[25] = "prefab_mediumC_norm";
            asteroidTexNames[26] = "prefab_mediumD_norm";
            asteroidTexNames[27] = "prefab_mediumE_norm";
            asteroidTexNames[28] = "prefab_mediumF_norm";
            asteroidTexNames[29] = "prefab_mediumG_norm";
            asteroidTexNames[30] = "prefab_smallA_norm";
            asteroidTexNames[31] = "prefab_smallB_norm";
            asteroidTexNames[32] = "prefab_smallC_norm";
            asteroidTexNames[33] = "prefab_tinyA_norm";
            asteroidTexNames[34] = "prefab_tinyB_norm";
            mixedWreckage.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[35];
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
            asteroidTexNames[11] = "prefab_mediumA_spec";
            asteroidTexNames[12] = "prefab_mediumB_spec";
            asteroidTexNames[13] = "prefab_mediumC_spec";
            asteroidTexNames[14] = "prefab_mediumD_spec";
            asteroidTexNames[15] = "prefab_mediumE_spec";
            asteroidTexNames[16] = "prefab_mediumF_spec";
            asteroidTexNames[17] = "prefab_mediumG_spec";
            asteroidTexNames[18] = "prefab_smallA_spec";
            asteroidTexNames[19] = "prefab_smallB_spec";
            asteroidTexNames[20] = "prefab_smallC_spec";
            asteroidTexNames[21] = "prefab_tinyA_spec";
            asteroidTexNames[22] = "prefab_tinyB_spec";
            asteroidTexNames[23] = "prefab_mediumA_spec";
            asteroidTexNames[24] = "prefab_mediumB_spec";
            asteroidTexNames[25] = "prefab_mediumC_spec";
            asteroidTexNames[26] = "prefab_mediumD_spec";
            asteroidTexNames[27] = "prefab_mediumE_spec";
            asteroidTexNames[28] = "prefab_mediumF_spec";
            asteroidTexNames[29] = "prefab_mediumG_spec";
            asteroidTexNames[30] = "prefab_smallA_spec";
            asteroidTexNames[31] = "prefab_smallB_spec";
            asteroidTexNames[32] = "prefab_smallC_spec";
            asteroidTexNames[33] = "prefab_tinyA_spec";
            asteroidTexNames[34] = "prefab_tinyB_spec";
            mixedWreckage.specNames = asteroidTexNames;
            digging = new bool[35];
            for (int i = 0; i < 35; i++)
            {
                digging[i] = false;
            }
            mixedWreckage.assetDiggable = digging;
            sectorTextures["mixedwreckage"] = mixedWreckage;//both red and non-red, mine and jan's

            TextureBatch bigwrecks = new TextureBatch();
            asteroidTexNames = new string[7];
            asteroidTexNames[0] = "bigwreck_0_dif";
            asteroidTexNames[1] = "prefab_largeA_dif";
            asteroidTexNames[2] = "prefab_largeB_dif";
            asteroidTexNames[3] = "prefab_largeC_dif";
            asteroidTexNames[4] = "prefab_largeAred_dif";
            asteroidTexNames[5] = "prefab_largeBred_dif";
            asteroidTexNames[6] = "prefab_largeCred_dif";
            bigwrecks.diffNames = asteroidTexNames;
            bigwrecks.collideNames = asteroidTexNames;
            asteroidTexNames = new string[7];
            asteroidTexNames[0] = "bigwreck_0_norm";
            asteroidTexNames[1] = "prefab_largeA_norm";
            asteroidTexNames[2] = "prefab_largeB_norm";
            asteroidTexNames[3] = "prefab_largeC_norm";
            asteroidTexNames[4] = "prefab_largeA_norm";
            asteroidTexNames[5] = "prefab_largeB_norm";
            asteroidTexNames[6] = "prefab_largeC_norm";
            bigwrecks.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[7];
            asteroidTexNames[0] = "bigwreck_0_spec";
            asteroidTexNames[1] = "prefab_largeA_spec";
            asteroidTexNames[2] = "prefab_largeB_spec";
            asteroidTexNames[3] = "prefab_largeC_spec";
            asteroidTexNames[4] = "prefab_largeA_spec";
            asteroidTexNames[5] = "prefab_largeB_spec";
            asteroidTexNames[6] = "prefab_largeC_spec";
            bigwrecks.specNames = asteroidTexNames;
            digging = new bool[7];
            for (int i = 0; i < 7; i++)
            {
                digging[i] = false;
            }
            bigwrecks.assetDiggable = digging;
            sectorTextures["bigwrecks"] = bigwrecks;//only the large ones

            TextureBatch gateWrecks = new TextureBatch();
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "gatewreck_0_dif";
            gateWrecks.diffNames = asteroidTexNames;
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "gatewreck_0_collide";
            gateWrecks.collideNames = asteroidTexNames;//custom collision
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "gatewreck_0_norm";
            gateWrecks.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[1];
            asteroidTexNames[0] = "gatewreck_0_spec";
            gateWrecks.specNames = asteroidTexNames;
            digging = new bool[1];
            digging[0] = false;
            gateWrecks.assetDiggable = digging;
            sectorTextures["gatewrecks"] = gateWrecks;

            TextureBatch sharpNebulaAssets = new TextureBatch();
            nebulaTexNames = new string[4];
            nebulaTexNames[0] = "nebulaNormalH";
            nebulaTexNames[1] = "nebulaNormalF";
            nebulaTexNames[2] = "nebulaNormalC";
            nebulaTexNames[3] = "nebulaNormalE";
            sharpNebulaAssets.bumpNames = nebulaTexNames;
            nebulaTexNames = new string[1];
            nebulaTexNames[0] = "sheet_stars";
            sharpNebulaAssets.diffNames = nebulaTexNames;
            nebulaTexNames = new string[1];
            nebulaTexNames[0] = "starDust01";
            sharpNebulaAssets.emitNames = nebulaTexNames;
            nebulaTexNames = new string[1];
            nebulaTexNames[0] = "starMask01";
            sharpNebulaAssets.specNames = nebulaTexNames;

            TextureBatch generalNebulaAssets = new TextureBatch();
            nebulaTexNames = new string[5];
            nebulaTexNames[0] = "nebulaNormalA";
            nebulaTexNames[1] = "nebulaNormalB";
            nebulaTexNames[2] = "nebulaNormalC";
            nebulaTexNames[3] = "nebulaNormalD";
            nebulaTexNames[4] = "nebulaNormalE";
            generalNebulaAssets.bumpNames = nebulaTexNames;
            nebulaTexNames = new string[1];
            nebulaTexNames[0] = "sheet_stars";
            generalNebulaAssets.diffNames = nebulaTexNames;
            nebulaTexNames = new string[1];
            nebulaTexNames[0] = "starDust01";
            generalNebulaAssets.emitNames = nebulaTexNames;
            nebulaTexNames = new string[1];
            nebulaTexNames[0] = "starMask01";
            generalNebulaAssets.specNames = nebulaTexNames;


            TextureBatch everettEventRedBatch = new TextureBatch();
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "inicedif";
            asteroidTexNames[1] = "outrockred";
            everettEventRedBatch.diffNames = asteroidTexNames;
            everettEventRedBatch.collideNames = asteroidTexNames;
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "iceringbump";
            asteroidTexNames[1] = "AsteriodBlack_norm_2";
            everettEventRedBatch.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "iceringspec";
            asteroidTexNames[1] = "AsteriodBlack_spec_2";
            everettEventRedBatch.specNames = asteroidTexNames;
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "iceringemit";
            asteroidTexNames[1] = "AsteriodBlack_emis_2";
            everettEventRedBatch.emitNames = asteroidTexNames;
            digging = new bool[2];
            for (int i = 0; i < 2; i++)
            {
                digging[i] = false;
            }
            everettEventRedBatch.assetDiggable = digging;
            sectorTextures["everettred"] = everettEventRedBatch;


            TextureBatch everettEventBlueBatch = new TextureBatch();
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "outicedif";
            asteroidTexNames[1] = "inrockred";
            everettEventBlueBatch.diffNames = asteroidTexNames;
            everettEventBlueBatch.collideNames = asteroidTexNames;
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "iceringbump";
            asteroidTexNames[1] = "AsteriodBlack_norm_2";
            everettEventBlueBatch.bumpNames = asteroidTexNames;
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "iceringspec";
            asteroidTexNames[1] = "AsteriodBlack_spec_2";
            everettEventBlueBatch.specNames = asteroidTexNames;
            asteroidTexNames = new string[2];
            asteroidTexNames[0] = "iceringemit";
            asteroidTexNames[1] = "AsteriodBlack_emis_2";
            everettEventBlueBatch.emitNames = asteroidTexNames;
            digging = new bool[2];
            for (int i = 0; i < 2; i++)
            {
                digging[i] = false;
            }
            everettEventBlueBatch.assetDiggable = digging;
            sectorTextures["everettblue"] = everettEventBlueBatch;



            #endregion declaringTextures



            //Prefab myDickButtTest = new Prefab(Directory.GetCurrentDirectory() + @"\Data\Debugtestfab.pfb", "asteroidsBlack", blackAsteroidFieldAssets);
            //Prefab myDickButtTest = new Prefab(Directory.GetCurrentDirectory() + @"\Data\Debugtestfab.pfb");
            Prefab asteroidStation = new Prefab(Directory.GetCurrentDirectory() + @"\Data\abndnRoidStn.pfb");
            Prefab asteroidStation1 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\abndnRoidStn1.pfb");
            Prefab asteroidStation2 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\abndnRoidStn2.pfb");
            Prefab asteroidStation3 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\abndnRoidStn3.pfb");
            Prefab neutralStation1 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\neutralStation1.pfb");
            //Prefab asteroidStation = new Prefab(Directory.GetCurrentDirectory() + @"\Data\astStn.pfb");
            Prefab wreckageTest = new Prefab(Directory.GetCurrentDirectory() + @"\Data\wrecktest.pfb");
            Prefab pirateMaze0 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\pirMzBas.pfb");

            //a simple abandoned mining station with no maze elements
            Prefab abandonMine = new Prefab(Directory.GetCurrentDirectory() + @"\Data\abndmine.pfb");

            //an abandoned repair outpost that can be used to repair ships
            Prefab repstation = new Prefab(Directory.GetCurrentDirectory() + @"\Data\repstation.pfb");

            //economy debugging
            Prefab economyBlob = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economyblob.pfb");

            //economy items
            Prefab randEconomy = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economyrandom.pfb");//done
            Prefab econfarm = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economyfarm.pfb");//done
            Prefab econiron = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economyiron.pfb");//done
            Prefab econgold = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economygold.pfb");//done
            Prefab econtitanium = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economytitanium.pfb");//done
            Prefab econmall = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economymall.pfb");//done
            Prefab econtrade1 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economytrade1.pfb");//done
            Prefab econtrade2 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\economytrade2.pfb");//done
            Prefab shipyard = new Prefab(Directory.GetCurrentDirectory() + @"\Data\sscshipyard.pfb");

            Prefab blockade = new Prefab(Directory.GetCurrentDirectory() + @"\Data\sscblockade.pfb");
            blockade.bonusRadius = 12000;

            Prefab dynfab0 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\orangeDynamic0.pfb");
            Prefab dynfab1 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\orangeDynamic1.pfb");
            Prefab dynfab2 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\orangeDynamic2.pfb");
            Prefab dynfab3 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\orangeDynamic3.pfb");
            Prefab dynfab4 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\orangeDynamic4.pfb");

            //gates
            Prefab gate0 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate0.pfb");
            Prefab gate1 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate1.pfb");
            Prefab gate2 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate2.pfb");
            Prefab gate3 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate3.pfb");
            Prefab gate4 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate4.pfb");
            Prefab gate5 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate5.pfb");
            Prefab gate6 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate6.pfb");
            Prefab gate7 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate7.pfb");
            Prefab gate8 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate8.pfb");
            Prefab gate9 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate9.pfb");
            Prefab gate10 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\gate10.pfb");
            Prefab higgsgate = new Prefab(Directory.GetCurrentDirectory() + @"\Data\deephig.pfb");
            Prefab everettr = new Prefab(Directory.GetCurrentDirectory() + @"\Data\everettr.pfb");
            Prefab everettb = new Prefab(Directory.GetCurrentDirectory() + @"\Data\everettb.pfb");
            Prefab tutorial = new Prefab(Directory.GetCurrentDirectory() + @"\Data\tutut.pfb");

            //pirate stuff
            Prefab piratecove = new Prefab(Directory.GetCurrentDirectory() + @"\Data\pcove.pfb");
            piratecove.bonusRadius = 8000;


            Prefab ctpambush = new Prefab(Directory.GetCurrentDirectory() + @"\Data\ctpambush.pfb");

            Prefab jerkcove = new Prefab(Directory.GetCurrentDirectory() + @"\Data\jerkville.pfb");

            //start sector asteroid tutorial dungeon
            Prefab startroid = new Prefab(Directory.GetCurrentDirectory() + @"\Data\startroid.pfb");

            //red zone stuff
            Prefab redDynfab0 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\reddynamic0.pfb");
            Prefab redDynfab1 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\reddynamic1.pfb");
            Prefab redDynfab2 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\reddynamic2.pfb");
            Prefab redDynfab3 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\reddynamic3.pfb");

            Prefab redsmallfab0 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\redsimple0.pfb");
            Prefab redsmallfab1 = new Prefab(Directory.GetCurrentDirectory() + @"\Data\redsimple1.pfb");


            //SSC relay
            Prefab sscRelay = new Prefab(Directory.GetCurrentDirectory() + @"\Data\sscrelay.pfb");

            //SSC data center
            Prefab sscdata = new Prefab(Directory.GetCurrentDirectory() + @"\Data\sscdata.pfb");
            Prefab sscdrones = new Prefab(Directory.GetCurrentDirectory() + @"\Data\sscdrone.pfb");
            Prefab sscmissiles = new Prefab(Directory.GetCurrentDirectory() + @"\Data\sscmissile.pfb");
            Prefab sscgate = new Prefab(Directory.GetCurrentDirectory() + @"\Data\sscextrasystemgate.pfb");

            float[] scales = new float[11];
            for (int i = 0; i < 11; i++)
            {
                scales[i] = 1;
            }

            //we map the default backdrop to the color black which is returned when the sector has no other data assigned
            audioSettings[Color.Black] = blackSongs;
            ObjectFieldRev2 nebulaAssets = new ObjectFieldRev2(null, null, 0, 0, 0);
            nebulaAssets.cloudAssets = generalNebulaAssets;
            nebulaAssets.lightShaftSet = greyZoneLightShafts;
            backdrops[Color.Black] = nebulaAssets;
            sectorTextures["nebulaAssets"] = generalNebulaAssets;
            nebulaAssets.setClouds(generalNebulaAssets, grey1);
            


            c = Color.Yellow;
            mapIcons[c] = starBatch;
            iconTechniques[c] = "star";
            DefaultBackdrop b = new DefaultBackdrop("Stars");
            StellarObject star = new StellarObject();
            star.assetName = "StarObject";
            star.rotation = 0;
            star.scale = 30;
            star.position = new Vector3(0, 0, -50000);

            audioSettings[c] = blackSongs;
            nebulaAssets = new ObjectFieldRev2(null, null, 0, 0, 0);
            nebulaAssets.cloudAssets = generalNebulaAssets;
            nebulaAssets.lightShaftSet = greyZoneLightShafts;
            nebulaAssets.addStellarObject(star);
            backdrops[c] = nebulaAssets;
            sectorTextures["nebulaAssets"] = generalNebulaAssets;
            nebulaAssets.setClouds(generalNebulaAssets, grey1);


            //asteroids found in generated space
            c = Color.Brown;
            ObjectFieldRev2 field2 = new ObjectFieldRev2("The_Eye", asteroidFieldAssets, null, 4092, -350, 10000);
            backdrops[c] = field2;
            SimpleFieldTerrainGenerator asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            audioSettings[c] = orangeSongs;


            c = new Color(26, 26, 26);
            field2 = new ObjectFieldRev2(null, null, null, 0, 0, 0);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.lightShaftSet = greyZoneLightShafts;
            audioSettings[Color.Black] = blackSongs;
            backdrops[c] = field2;


            #region Home nebula
            //LightSettings lightSet = new LightSettings();
            //lightSet.lightColor = new Color(0.969f, 0.773f, 0.639f);
            //lightSet.lightIntensity = 1.5f;
            //lightSet.ambLightColor = new Color(0.25f, 0.28f, 0.35f);

            c = new Color(64, 50, 18);
            //backdrops[c] = new DefaultBackdrop("NebulaHome");
            preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, null, null, 0, 0, 0);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula2);
            backdrops[c] = field2;
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "nebula";
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;
            FancyFieldTerrainGenerator feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.addPrefab(randEconomy, 2);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(sscRelay);
            feildGen.preventMultiSpawn(sscRelay);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 3;

            feildGen.SetDepositPreset(domumEdgeA);

            sectorGenerators[c] = feildGen;



            c = new Color(128, 100, 35);
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3000, -350, 4000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula);
            field2.lightShaftSet = homeNebulaCoreLS;
            backdrops[c] = field2;
            iconTechniques[c] = "roids";
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.actuallyRandom = true;
            //feildGen.makeVariableDensity(900, 0, 1);
            feildGen.makeClumps(6000, 2f, 0.14f);
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.addPrefab(asteroidStation);
            feildGen.addPrefab(asteroidStation2);
            feildGen.addPrefab(asteroidStation3);
            feildGen.addPrefab(dynfab2);
            //feildGen.addPrefab(dynfab0);
            //feildGen.addPrefab(dynfab1);
            //feildGen.addPrefab(dynfab3);
            //feildGen.addPrefab(dynfab4);
            feildGen.addPrefab(repstation, 5);
            feildGen.preventMultiSpawn(repstation);
            feildGen.addPrefab(sscRelay);
            feildGen.preventMultiSpawn(sscRelay);
            feildGen.addPrefab(abandonMine, 5);
            feildGen.addPrefab(randEconomy, 4);
            feildGen.addPrefab(econtrade2);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 12;

            feildGen.SetDepositPreset(domumCenterA);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;

            //gate system
            c = new Color(255, 106, 1);//high density
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "roids";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3800, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula);
            field2.lightShaftSet = homeNebulaCoreLS;
            backdrops[c] = field2;

            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            //feildGen.makeVariableDensity(2000, 0, 1);
            feildGen.makeClumps(7000, 2f, 0.16f);
            feildGen.addManualPrefab(gate1, new Vector2(0, 0));
            feildGen.addPrefab(asteroidStation);
            feildGen.addPrefab(asteroidStation2);
            feildGen.addPrefab(asteroidStation3);
            feildGen.addPrefab(dynfab2);
            //feildGen.addPrefab(dynfab0);
            //feildGen.addPrefab(dynfab1);
            //feildGen.addPrefab(dynfab3);
            //feildGen.addPrefab(dynfab4);
            feildGen.addPrefab(repstation, 5);
            feildGen.preventMultiSpawn(repstation);
            feildGen.addPrefab(sscRelay);
            feildGen.preventMultiSpawn(sscRelay);
            feildGen.addPrefab(abandonMine, 5);
            feildGen.addPrefab(randEconomy, 4);
            feildGen.minPrefabs = 3;
            feildGen.maxPrefabs = 14;

            feildGen.SetDepositPreset(domumCenterA);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;

            //gate system
            c = new Color(255, 106, 2);//med density
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3000, -350, 4000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula);
            field2.lightShaftSet = homeNebulaCoreLS;
            backdrops[c] = field2;
            iconTechniques[c] = "roids";
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.actuallyRandom = true;
            //feildGen.makeVariableDensity(900, 0, 1);
            feildGen.makeClumps(6000, 2f, 0.14f);
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.addManualPrefab(gate2, new Vector2(0, 0));
            feildGen.addPrefab(asteroidStation);
            feildGen.addPrefab(asteroidStation2);
            feildGen.addPrefab(asteroidStation3);
            feildGen.addPrefab(dynfab2);
            //feildGen.addPrefab(dynfab0);
            //feildGen.addPrefab(dynfab1);
            //feildGen.addPrefab(dynfab3);
            //feildGen.addPrefab(dynfab4);
            feildGen.addPrefab(repstation, 5);
            feildGen.preventMultiSpawn(repstation);
            feildGen.addPrefab(sscRelay);
            feildGen.preventMultiSpawn(sscRelay);
            feildGen.addPrefab(abandonMine, 5);
            feildGen.addPrefab(randEconomy, 4);
            feildGen.addPrefab(econtrade2);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 12;
            feildGen.minDeposites = 1;
            feildGen.maxDeposites = 2;

            feildGen.SetDepositPreset(domumCenterB);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;

            //gate system
            c = new Color(255, 106, 3);//low density
            preloadRequired.Add(c);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3800, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula2);
            field2.lightShaftSet = homeNebula2CoreLS;
            backdrops[c] = field2;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.addManualPrefab(gate3, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 3);
            feildGen.addPrefab(sscRelay);
            feildGen.preventMultiSpawn(sscRelay);
            //feildGen.addPrefab(econfarm);
            feildGen.addPrefab(econtrade2);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 4;

            feildGen.SetDepositPreset(domumCenterB);
            sectorGenerators[c] = feildGen;

            //system near home system with guaranteed trade hub
            c = new Color(255, 106, 14);//high density
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "roids";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3800, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula2);
            field2.lightShaftSet = homeNebula2CoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeClumps(7000, 2f, 0.16f);
            feildGen.addManualPrefab(econtrade2, new Vector2(15000, 30000));
            feildGen.addPrefab(abandonMine, 5);
            feildGen.addPrefab(repstation);
            feildGen.preventMultiSpawn(repstation);
            feildGen.minPrefabs = 2;
            feildGen.maxPrefabs = 7;
            feildGen.SetDepositPreset(domumCenterA);

            sectorGenerators[c] = feildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;

            //home start system with player station
            c = new Color(128, 200, 35);
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "planetroids";
            //field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3500, -350, 4000);

            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3500, -350, 4000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula);
            field2.lightShaftSet = homeNebulaCoreLS;

            backdrops[c] = field2;
            field2.planetTypeGeneration = 1;
           
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[1];
            field2.heights[0] = "Planet_Height2";
            //field2.heights[1] = "Planet_Height2";
            //field2.heights[2] = "Planet_Height3";
            field2.normals = new string[1];
            field2.normals[0] = "Planet_Normal2";
            //field2.normals[1] = "Planet_Normal2";
            //field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = new AtmosParams[1];
            field2.lutParameters[0] = new AtmosParams();
            field2.lutParameters[0].fInnerRadius = 14000 * planetSizeMult;
            field2.lutParameters[0].fOuterRadius = 14200 * planetSizeMult;
            field2.lutParameters[0].fRayleighScaleHeight = 0.15f;
            field2.lutParameters[0].fMieScaleHeight = 0.10f;
            field2.atmoWaveLengths = new Vector3(0.3f, 0.51f, 0.9f) * 0.15f;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            //feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeClumps(7000, 2f, 0.16f);
            feildGen.addManualPrefab(gate0, new Vector2(25000, 15000));
            feildGen.addManualPrefab(startroid, new Vector2(-50000, -35000));
            //feildGen.addManualPrefab(dynfab1, new Vector2(20000, -20000));
            feildGen.addPrefab(abandonMine);
            //feildGen.addPrefab(asteroidStation1);
            //feildGen.addPrefab(asteroidStation2);
            //feildGen.addPrefab(asteroidStation3);
            //feildGen.addPrefab(neutralStation1);
            //feildGen.addPrefab(economyBlob);
            //feildGen.addPrefab(randEconomy, 2);
            feildGen.minPrefabs = 3;
            feildGen.maxPrefabs = 6;
            feildGen.SetDepositPreset(domumCenterA);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;


            //c = new Color(255, 106, 0);
            //field2 = new ObjectFieldRev2("NebulaHome", asteroidFieldAssets, null, 3500, -150, 4000);
            //backdrops[c] = field2;
            //feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1");
            //feildGen.makeVariableDensity(900, 0, 1);
            //sectorGenerators[c] = feildGen;
            //lightSettings[c] = lightSet;


            c = new Color(255, 200, 70);
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "roids";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3800, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula2);
            field2.lightShaftSet = homeNebula2CoreLS;
            backdrops[c] = field2;
            //asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 4200);
            //sectorGenerators[c] = asteroidFeildGen;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeClumps(7000, 2f, 0.16f);
            feildGen.addPrefab(asteroidStation);
            feildGen.addPrefab(asteroidStation2);
            feildGen.addPrefab(asteroidStation3);
            feildGen.addPrefab(dynfab2);
            //feildGen.addPrefab(dynfab0);
            //feildGen.addPrefab(dynfab1);
            //feildGen.addPrefab(dynfab3);
            //feildGen.addPrefab(dynfab4);
            feildGen.addPrefab(repstation, 5);
            feildGen.preventMultiSpawn(repstation);
            feildGen.addPrefab(sscRelay);
            feildGen.preventMultiSpawn(sscRelay);
            feildGen.addPrefab(abandonMine, 5);
            feildGen.addPrefab(randEconomy, 4);
            feildGen.minPrefabs = 3;
            feildGen.maxPrefabs = 14;

            feildGen.SetDepositPreset(domumCenterB);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;


            //special sector reserved for the survival arena
            c = new Color(4, 4, 4);
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "roids";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3800, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            field2.lightShaftSet = greyZoneLightShafts;
            backdrops[c] = field2;
            //asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 4200);
            //sectorGenerators[c] = asteroidFeildGen;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeTinySector(1800, 2f, 0.24f);

            //feildGen.addPrefab(dynfab0);
            //feildGen.addPrefab(dynfab1);
            //feildGen.addPrefab(dynfab2);
            //feildGen.addPrefab(dynfab3);
            //feildGen.addPrefab(dynfab4);
            //feildGen.addPrefab(repstation, 5);
            //feildGen.preventMultiSpawn(repstation);
            //feildGen.addPrefab(abandonMine, 5);
            //feildGen.addPrefab(randEconomy, 4);
            //feildGen.minPrefabs = 3;
            //feildGen.maxPrefabs = 14;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = redSongs;


            c = new Color(5, 5, 5);
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "roids";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3800, -350, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);
            field2.lightShaftSet = greyZoneLightShafts;
            backdrops[c] = field2;
            //asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 4200);
            //sectorGenerators[c] = asteroidFeildGen;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeTinySector(1800, 2f, 0.24f);
            feildGen.SetDepositPreset(greyCenter);
            //feildGen.addPrefab(dynfab0);
            //feildGen.addPrefab(dynfab1);
            //feildGen.addPrefab(dynfab2);
            //feildGen.addPrefab(dynfab3);
            //feildGen.addPrefab(dynfab4);
            //feildGen.addPrefab(repstation, 5);
            //feildGen.preventMultiSpawn(repstation);
            //feildGen.addPrefab(abandonMine, 5);
            //feildGen.addPrefab(randEconomy, 4);
            //feildGen.minPrefabs = 3;
            //feildGen.maxPrefabs = 14;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = redSongs;
            

            PlanetRenderSettings redArenaPlanet = new PlanetRenderSettings(); //Ice
            redArenaPlanet.position = new Vector3(25000, 0, -72000);
            redArenaPlanet.rotation = Vector3.Zero;
            redArenaPlanet.rotationRate = new Vector3(0, 0.005f, 0); //Colors, generaly keep them realistic not very saturaded, brown works, there might be little trick required for vegetation planets where green will be a thing at Color2 or/And 3
            redArenaPlanet.color1 = new Vector3(0.97f, 0.98f, 0.89f);

            redArenaPlanet.color2 = new Vector3(0.15f, 0.20f, 0.20f);
            redArenaPlanet.color3 = new Vector3(0.34f, 0.45f, 0.47f);

            redArenaPlanet.color4 = new Vector3(0.34f, 0.31f, 0.375f);
            redArenaPlanet.phase = new Vector3(0.17f, 0.45f, 0.7f);
            redArenaPlanet.emissionColor = new Vector4(0, 0.0f, 0f, 0.1f);
            redArenaPlanet.fluidColor = new Vector4(0.18f, 0.59f, 1.00f, 0.1f);
            redArenaPlanet.fluidEmitIntensity = 1.0f;
            redArenaPlanet.fluidLevel = 0.75f;
            redArenaPlanet.mask = 0;
            redArenaPlanet.diffuse = 0;
            redArenaPlanet.height = 0;
            redArenaPlanet.normal = 0;
            redArenaPlanet.emission = 0;
            redArenaPlanet.atmosphere = 0;
            redArenaPlanet.surfaceTilling = 4;
            redArenaPlanet.radius = 13800 * planetSizeMult;
            redArenaPlanet.hasAtmosphere = true;
            redArenaPlanet.waveLenghts4 = new Vector3((float)Math.Pow(0.620f, 4), (float)Math.Pow(0.470f, 4), (float)Math.Pow(0.430f, 4)) * 1.35f;
            redArenaPlanet.planetType = 3;

            //Test arena red
            c = new Color(6, 6, 6);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            //preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 1000, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets,  wolfNebula2);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            field2.planetTypeGeneration = 4;
            field2.setPlanetParams(redArenaPlanet);
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[1];
            field2.heights[0] = "Planet_Height1";
            //field2.heights[1] = "Planet_Height2";
            //field2.heights[2] = "Planet_Height3";
            field2.normals = new string[1];
            field2.normals[0] = "Planet_Normal1";
            //field2.normals[1] = "Planet_Normal2";
            //field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = new AtmosParams[1];
            field2.lutParameters[0] = new AtmosParams();
            field2.lutParameters[0].fInnerRadius = 13800 * planetSizeMult;
            field2.lutParameters[0].fOuterRadius = 15400 * planetSizeMult;
            field2.lutParameters[0].fRayleighScaleHeight = 0.13f;
            field2.lutParameters[0].fMieScaleHeight = 0.11f;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeTinySector(1800, 2f, 0.24f);
            //feildGen.makeVariableDensity(1000, -0.2f, 1);
            feildGen.minDeposites = 1;
            feildGen.maxDeposites = 2;
            feildGen.AddDepositeChance(DepositeType.iron_small, 0.2f);
            feildGen.AddDepositeChance(DepositeType.iron_medium, 0.6f);
            feildGen.AddDepositeChance(DepositeType.gold_medium, 0.6f);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;

            PlanetRenderSettings greenArenaPlanet = new PlanetRenderSettings();
            greenArenaPlanet.position = new Vector3(25000, 0, -72000);
            greenArenaPlanet.rotation = Vector3.Zero;
            greenArenaPlanet.rotationRate = new Vector3(0, 0.005f, 0);
            greenArenaPlanet.color1 = new Vector3(0.1f, 0.1f, 0.1f);  //Colors, generaly keep them realistic not very saturaded, brown works, there might be little trick required for vegetation planets where green will be a thing at Color2 or/And 3
            greenArenaPlanet.color2 = new Vector3(0.245f, 0.2f, 0.224f);
            greenArenaPlanet.color3 = new Vector3(0.894f, 0.954f, 0.978f);
            greenArenaPlanet.color4 = new Vector3(0.114f, 0.141f, 0.175f);
            greenArenaPlanet.phase = new Vector3(0.17f, 0.45f, 0.7f);
            greenArenaPlanet.emissionColor = new Vector4(0, 0.0f, 0f, 1);
            greenArenaPlanet.fluidColor = new Vector4(0.0f, 0.95f, 0.85f, 1);
            greenArenaPlanet.fluidEmitIntensity = 8.0f;
            greenArenaPlanet.mask = 0;
            greenArenaPlanet.diffuse = 0;
            greenArenaPlanet.height = 0;
            greenArenaPlanet.normal = 0;
            greenArenaPlanet.emission = 0;
            greenArenaPlanet.atmosphere = 0;
            greenArenaPlanet.surfaceTilling = 4;
            greenArenaPlanet.radius = 13800 * planetSizeMult;
            greenArenaPlanet.hasAtmosphere =  true;
            greenArenaPlanet.waveLenghts4 = new Vector3((float)Math.Pow(0.850f, 4), (float)Math.Pow(0.600f, 4), (float)Math.Pow(0.470f, 4)) * 0.85f;
            greenArenaPlanet.planetType = 4;

            //Test arena green
            c = new Color(7, 7, 7, 255);
            mapIcons[c] = greenBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, infestedBlackAsteroidsAssets, null, 4, -400, 4);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, green1);
            field2.lightShaftOrgin = new Vector2(0.48125f, 0.33f);
            field2.setPlanetParams(greenArenaPlanet);
            backdrops[c] = field2;
            //preloadRequired.Add(c);
            field2.planetTypeGeneration = 4;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[1];
            field2.heights[0] = "Planet_Height3";
            //field2.heights[1] = "Planet_Height2";
            //field2.heights[2] = "Planet_Height3";
            field2.normals = new string[1];
            field2.normals[0] = "Planet_Normal3";
            //field2.normals[1] = "Planet_Normal2";
            //field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = new AtmosParams[1];
            field2.lutParameters[0] = new AtmosParams();
            field2.lutParameters[0].fInnerRadius = 13800 * planetSizeMult;
            field2.lutParameters[0].fOuterRadius = 14400 * planetSizeMult;
            field2.lutParameters[0].fRayleighScaleHeight = 0.13f;
            field2.lutParameters[0].fMieScaleHeight = 0.12f;
            field2.lightShaftSet = greenZoneLightShafts;
            feildGen = new FancyFieldTerrainGenerator(infestedBlackAsteroidsAssets, "asteroidInfestedBlack", mixedWreckage, "mixedwreckage");
            feildGen.makeVariableDensity(1400, 0, 1);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greenZoneLights;
            audioSettings[c] = greenSongs;//6;

            //Test arena pandora
            c = new Color(10, 10, 10);
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "roids";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 3800, -350, 5000);
            field2.cloudAssets = generalNebulaAssets;
            //field2.setClouds(generalNebulaAssets, hdrHigh: pandoraNebula1.hdrHigh, alphaMin: pandoraNebula1.alphaMin, roughness: pandoraNebula1.roughness, edgeIntensity: pandoraNebula1.edgeIntensity, lightA: pandoraNebula1.lightA, lightB: pandoraNebula1.lightB, generalMult: pandoraNebula1.generalMult, hdrHphase: pandoraNebula1.hdrHphase, deepBackground: pandoraNebula1.deepBackground, starColor: pandoraNebula1.starColor, starDustColor: pandoraNebula1.starDustColor);
            field2.setClouds(generalNebulaAssets, pandoraNebula1);
            field2.lightShaftSet = pandora1LightShafts;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            //feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeTinySector(1800, 2f, 0.24f);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = pandoraLights;
            audioSettings[c] = blackSongs;

            c = new Color(9, 9, 9);
            mapIcons[c] = orangeBatch;
            preloadRequired.Add(c);
            iconTechniques[c] = "roids";
            field2 = new ObjectFieldRev2(null, null, 0, 0, 0);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            field2.lightShaftSet = greyRoidsLightShafts;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.environmentCloudDensity = 4;
            //feildGen.actuallyRandom = true;
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeTinySector(10, 2f, 0.24f);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greenZoneLights;
            audioSettings[c] = blackSongs;




            #endregion

            #region defaultBackdrops

            //notice: this is temporary code to change the default backdrop used in all sectors

            //a planet
            //PlanetRenderSettings samplePlanet = new PlanetRenderSettings();
            //samplePlanet.position = new Vector3(0, 0, -60000);
            //samplePlanet.rotation = Vector3.Zero;
            //samplePlanet.rotationRate = new Vector3(0, 0.01f, 0);
            //samplePlanet.color1 = new Vector3(0.3f, 0.1f, 0.0f);  //Colors, generaly keep them realistic not very saturaded, brown works, there might be little trick required for vegetation planets where green will be a thing at Color2 or/And 3
            //samplePlanet.color2 = new Vector3(0.545f, 0.4f, 0.294f);
            //samplePlanet.color3 = new Vector3(0.894f, 0.954f, 0.978f);
            //samplePlanet.color4 = new Vector3(0.314f, 0.141f, 0.075f);
            //samplePlanet.phase = new Vector3(0.17f, 0.45f, 0.7f);
            //samplePlanet.emissionColor = new Vector4(0, 0.424f, 1, 4);
            //samplePlanet.fluidColor = new Vector4(1, 0.424f, 0, 0);
            //samplePlanet.mask = 0;
            //samplePlanet.diffuse = 0;
            //samplePlanet.height = 0;
            //samplePlanet.normal = 0;
            //samplePlanet.emission = 0;
            //samplePlanet.atmosphere = 0;
            //samplePlanet.surfaceTilling = 4;
            //samplePlanet.radius = 400;
            //samplePlanet.hasAtmosphere = true;

            AtmosParams atmosphere;
            AtmosParams[] atmos = new AtmosParams[4];

            atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 9500 * planetSizeMult;
            atmosphere.fOuterRadius = 9710 * planetSizeMult;
            atmosphere.fRayleighScaleHeight = 0.14f;
            atmosphere.fMieScaleHeight = 0.10f;
            atmos[0] = atmosphere;

            atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 11000 * planetSizeMult;
            atmosphere.fOuterRadius = 11350 * planetSizeMult;
            atmosphere.fRayleighScaleHeight = 0.18f;
            atmosphere.fMieScaleHeight = 0.10f;
            atmos[1] = atmosphere;

            atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 11000 * planetSizeMult;
            atmosphere.fOuterRadius = 12100 * planetSizeMult;
            atmosphere.fRayleighScaleHeight = 0.17f;
            atmosphere.fMieScaleHeight = 0.10f;
            atmos[2] = atmosphere;

            atmosphere = new AtmosParams();
            atmosphere.fInnerRadius = 14000 * planetSizeMult;
            atmosphere.fOuterRadius = 15100 * planetSizeMult;
            atmosphere.fRayleighScaleHeight = 0.13f;
            atmosphere.fMieScaleHeight = 0.10f;
            atmos[3] = atmosphere;


            //magma planet 0 with red asteroids
            c = new Color(38, 127, 20, 255);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, wolfNebula1);
            field2.planetTypeGeneration = 0;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;


            //magma planet 0 with orange asteroids
            c = new Color(38, 127, 30, 255);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula2);
            field2.lightShaftSet = homeNebula2CoreLS;
            field2.planetTypeGeneration = 0;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;


            //magma planet 0 with grey asteroids
            c = new Color(38, 127, 40, 255);
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            field2.planetTypeGeneration = 0;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;


            //magma planet 0 with blue asteroids
            c = new Color(38, 127, 50, 255);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, BlueZone2);
            field2.planetTypeGeneration = 0;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = blueSongs;


            //magma planet 0 with green asteroids
            c = new Color(38, 127, 60, 255);
            mapIcons[c] = greenBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2("ForestL3", asteroidFieldAssets, null, 500, -350, 10000);
            field2.planetTypeGeneration = 0;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greenLights;
            audioSettings[c] = greenSongs;


            //magma planet 0 alone
            c = new Color(38, 127, 0, 255);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "solo_planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 15, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            field2.planetTypeGeneration = 0;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            lightSettings[c] = greyLights;
            audioSettings[c] = mysterySongs;//7;


            //barrens planet 1 with red asteroids (Barrens)
            c = new Color(38, 127, 21, 255);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, wolfNebula1);
            field2.planetTypeGeneration = 1;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined1";
            field2.diffuses[1] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;


            //barrens planet 1 with orange asteroids (Barrens)
            c = new Color(38, 127, 31, 255);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula2);
            field2.lightShaftSet = homeNebula2CoreLS;
            field2.planetTypeGeneration = 1;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined1";
            field2.diffuses[1] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;


            //barrens planet 1 with grey asteroids (Barrens)
            c = new Color(38, 127, 41, 255);
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.planetTypeGeneration = 1;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined1";
            field2.diffuses[1] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            field2.lightShaftSet = greyRoidsLightShafts;

            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;


            //barrens planet 1 with blue asteroids (Barrens)
            c = new Color(38, 127, 51, 255);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula2);
            field2.lightShaftSet = homeNebula2CoreLS;
            field2.planetTypeGeneration = 1;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined1";
            field2.diffuses[1] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = blueSongs;


            //barrens planet 1 with green asteroids (Barrens)
            c = new Color(38, 127, 61, 255);
            mapIcons[c] = greenBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2("ForestL3", asteroidFieldAssets, null, 500, -350, 10000);
            field2.planetTypeGeneration = 1;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined1";
            field2.diffuses[1] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greenLights;
            audioSettings[c] = greenSongs;


            //planet 1 alone (Barrens)
            c = new Color(38, 127, 1, 255);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "solo_planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 15, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.planetTypeGeneration = 1;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined1";
            field2.diffuses[1] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            field2.lightShaftSet = greyRoidsLightShafts;
            backdrops[c] = field2;
            lightSettings[c] = greyLights;
            audioSettings[c] = mysterySongs;//7;


            //terra planet 2 with red asteroids
            c = new Color(38, 127, 22, 255);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, wolfNebula1);
            field2.planetTypeGeneration = 2;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;


            //terra planet 2 with orange asteroids
            c = new Color(38, 127, 32, 255);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula);
            field2.lightShaftSet = homeNebulaCoreLS;
            field2.planetTypeGeneration = 2;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;


            //terra planet 2 with grey asteroids
            c = new Color(38, 127, 42, 255);
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.planetTypeGeneration = 2;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;


            //terra planet 2 with blue asteroids
            c = new Color(38, 127, 52, 255);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, BlueZone2);
            field2.planetTypeGeneration = 2;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = blueSongs;


            //terra planet 2 with green asteroids
            c = new Color(38, 127, 62, 255);
            mapIcons[c] = greenBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2("ForestL3", asteroidFieldAssets, null, 500, -350, 10000);
            field2.planetTypeGeneration = 2;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greenLights;
            audioSettings[c] = greenSongs;


            //planet 2 alone Terra
            c = new Color(38, 127, 2, 255);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "solo_planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 15, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.planetTypeGeneration = 2;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            lightSettings[c] = greyLights;
            audioSettings[c] = mysterySongs;//7;


            //snow planet 3 with red asteroids
            c = new Color(38, 127, 23, 255);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, wolfNebula1);
            field2.planetTypeGeneration = 3;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined3";
            field2.diffuses[1] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//7;


            //snow planet 3 with orange asteroids
            c = new Color(38, 127, 33, 255);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula);
            field2.lightShaftSet = homeNebulaCoreLS;
            field2.planetTypeGeneration = 3;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined3";
            field2.diffuses[1] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;//7;


            //snow planet 3 with grey asteroids
            c = new Color(38, 127, 43, 255);
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.planetTypeGeneration = 3;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined3";
            field2.diffuses[1] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;


            //snow planet 3 with blue asteroids
            c = new Color(38, 127, 53, 255);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, BlueZone2);
            field2.planetTypeGeneration = 3;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined3";
            field2.diffuses[1] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = blueSongs;


            //snow planet 3 with green asteroids
            c = new Color(38, 127, 63, 255);
            mapIcons[c] = greenBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2("ForestL3", asteroidFieldAssets, null, 500, -350, 10000);
            field2.planetTypeGeneration = 3;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined3";
            field2.diffuses[1] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greenLights;
            audioSettings[c] = greenSongs;


            //planet 3 alone snow
            c = new Color(38, 127, 3, 255);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "solo_planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 15, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.planetTypeGeneration = 3;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[2];
            field2.diffuses[0] = "TerrainCombined3";
            field2.diffuses[1] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            lightSettings[c] = greyLights;
            audioSettings[c] = mysterySongs;


            //toxic planet 4 with red asteroids
            c = new Color(38, 127, 24, 255);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, wolfNebula1);
            field2.planetTypeGeneration = 4;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;


            //toxic planet 4 with orange asteroids
            c = new Color(38, 127, 34, 255);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, homeNebula);
            field2.lightShaftSet = homeNebulaCoreLS;
            field2.planetTypeGeneration = 4;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = orangeSongs;


            //toxic planet 4 with grey asteroids
            c = new Color(38, 127, 44, 255);
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.planetTypeGeneration = 4;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;


            //toxic planet 4 with blue asteroids
            c = new Color(38, 127, 54, 255);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 500, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, BlueZone2);
            field2.planetTypeGeneration = 4;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = blueSongs;


            //toxic planet 4 with green asteroids
            c = new Color(38, 127, 64, 255);
            mapIcons[c] = greenBatch;
            iconTechniques[c] = "planet";
            field2 = new ObjectFieldRev2(null, infestedBlackAsteroidsAssets, null, 4, -400, 4);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, green1);
            field2.lightShaftOrgin = new Vector2(0.48125f, 0.33f);
            backdrops[c] = field2;
            //preloadRequired.Add(c);
            field2.planetTypeGeneration = 4;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined2";
            field2.heights = new string[1];
            field2.heights[0] = "Planet_Height3";
            //field2.heights[1] = "Planet_Height2";
            //field2.heights[2] = "Planet_Height3";
            field2.normals = new string[1];
            field2.normals[0] = "Planet_Normal3";
            //field2.normals[1] = "Planet_Normal2";
            //field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = new AtmosParams[1];
            field2.lutParameters[0] = new AtmosParams();
            field2.lutParameters[0].fInnerRadius = 14000 * planetSizeMult;
            field2.lutParameters[0].fOuterRadius = 15200 * planetSizeMult;
            field2.lutParameters[0].fRayleighScaleHeight = 0.16f;
            field2.lutParameters[0].fMieScaleHeight = 0.12f;
            field2.lightShaftSet = greenZoneLightShafts;
            feildGen = new FancyFieldTerrainGenerator(infestedBlackAsteroidsAssets, "asteroidInfestedBlack", mixedWreckage, "mixedwreckage");
            feildGen.makeVariableDensity(1400, 0, 1);
            feildGen.SetDepositPreset(greenPlanet);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greenZoneLights;
            audioSettings[c] = greenSongs;//6;


            //planet 4 alone toxic green
            c = new Color(38, 127, 4, 255);
            mapIcons[c] = orangeBatch;
            iconTechniques[c] = "solo_planet";
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 15, -350, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            field2.planetTypeGeneration = 4;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[3];
            field2.heights[0] = "Planet_Height1";
            field2.heights[1] = "Planet_Height2";
            field2.heights[2] = "Planet_Height3";
            field2.normals = new string[3];
            field2.normals[0] = "Planet_Normal1";
            field2.normals[1] = "Planet_Normal2";
            field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = atmos;
            backdrops[c] = field2;
            lightSettings[c] = greyLights;
            audioSettings[c] = mysterySongs;//7;


            #endregion
            
            #region blue nebula

            c = new Color(0, 65, 65);
            mapIcons[c] = cyanBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2("NebulaBlue", iceFieldAssets, null, 2400, -300, 5000);
            backdrops[c] = field2;
            field2.lightShaftOrgin = new Vector2(0.6f, 0.33f);
            asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 2000);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = homeNebulaLights;
            audioSettings[c] = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;
            #endregion

            #region Ice nebula

            
            //everett's prefab sector b
            c = new Color(255, 106, 13);
            mapIcons[c] = blueBatchLow;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 2400, -300, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, BlueZone2);
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", wreckage, "wreckage");
            feildGen.makeEvenDistribution(2000);
            feildGen.trackTextures(everettEventBlueBatch, "everettblue");
            feildGen.addManualPrefab(everettb, new Vector2(-35800, -62100));
            sectorGenerators[c] = feildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;

            c = new Color(0, 17, 33);
            mapIcons[c] = blueBatchLow;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 2400, -300, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, BlueZone2);
            field2.lightShaftSet = blueZoneLightShafts;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", 2000);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;

            c = new Color(0, 34, 65);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 2600, -300, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: BlueZone1.hdrHigh, alphaMin: BlueZone1.alphaMin, roughness: BlueZone1.roughness, edgeIntensity: BlueZone1.edgeIntensity, lightA: BlueZone1.lightA, lightB: BlueZone1.lightB, generalMult: BlueZone1.generalMult, hdrHphase: BlueZone1.hdrHphase, deepBackground: BlueZone1.deepBackground, starColor: BlueZone1.starColor, starDustColor: BlueZone1.starDustColor);
            field2.lightShaftSet = blueZoneLightShafts;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", 3000);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;

            c = new Color(0, 68, 130);
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 3000, -300, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: BlueZone3.hdrHigh, alphaMin: BlueZone3.alphaMin, roughness: BlueZone3.roughness, edgeIntensity: BlueZone3.edgeIntensity, lightA: BlueZone3.lightA, lightB: BlueZone3.lightB, generalMult: BlueZone3.generalMult, hdrHphase: BlueZone3.hdrHphase, deepBackground: BlueZone3.deepBackground, starColor: BlueZone3.starColor, starDustColor: BlueZone3.starDustColor);
            field2.lightShaftSet = blueZoneLightShafts;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", 4000);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;

            c = new Color(0, 92, 195);
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 3000, -300, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: BlueZone3.hdrHigh, alphaMin: BlueZone3.alphaMin, roughness: BlueZone3.roughness, edgeIntensity: BlueZone3.edgeIntensity, lightA: BlueZone3.lightA, lightB: BlueZone3.lightB, generalMult: BlueZone3.generalMult, hdrHphase: BlueZone3.hdrHphase, deepBackground: BlueZone3.deepBackground, starColor: BlueZone3.starColor, starDustColor: BlueZone3.starDustColor);
            backdrops[c] = field2;
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "planet";
            field2.planetTypeGeneration = 3;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.lightShaftSet = blueZoneLightShafts;
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined3";
            field2.heights = new string[1];
            field2.heights[0] = "Planet_Height1";
            field2.normals = new string[1];
            field2.normals[0] = "Planet_Normal1";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = new AtmosParams[1];
            field2.lutParameters[0] = new AtmosParams();
            field2.lutParameters[0].fInnerRadius = 11000 * planetSizeMult;
            field2.lutParameters[0].fOuterRadius = 11300 * planetSizeMult;
            field2.lutParameters[0].fRayleighScaleHeight = 0.15f;
            field2.lutParameters[0].fMieScaleHeight = 0.10f;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", 4000);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;

            #endregion

            #region General belts

            c = new Color(255, 106, 7);//low
            preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2000, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(1500, 1, 0.3f);
            feildGen.addManualPrefab(gate7, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 9);
            feildGen.addPrefab(econtitanium);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 2;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;//1;

            c = new Color(255, 106, 8);//high
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2800, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2500, 1, 0.3f);
            feildGen.addManualPrefab(gate8, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 9);
            feildGen.addPrefab(econtitanium);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 2;
            feildGen.SetDepositPreset(greyCenter);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;//1;

            c = new Color(255, 106, 9);//medium
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2400, -400, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.lightShaftSet = greyRoidsLightShafts;
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2000, 1, 0.3f);
            feildGen.addManualPrefab(gate9, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 9);
            feildGen.addPrefab(econtitanium);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 2;
            feildGen.SetDepositPreset(greyEdge);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;//1;

            c = new Color(255, 106, 10);//medium
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2400, -400, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.lightShaftSet = greyRoidsLightShafts;
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2000, 1, 0.3f);
            feildGen.addManualPrefab(gate10, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 9);
            feildGen.addPrefab(econtitanium);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 2;
            feildGen.SetDepositPreset(greyEdge);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;//1;

            c = new Color(45, 45, 45);
            preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2000, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(1500, 1, 0.3f);
            feildGen.addPrefab(randEconomy, 9);
            feildGen.addPrefab(econtitanium);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.SetDepositPreset(greyEdge);
            //feildGen.addPrefab(econshipyard);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 2;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;//1;


            c = new Color(90, 90, 90);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2400, -400, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2000, 1, 0.3f);
            feildGen.addPrefab(randEconomy, 9);
            feildGen.addPrefab(econtitanium);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.SetDepositPreset(greyCenter);
            //feildGen.addPrefab(econshipyard);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 2;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;//1;

            c = new Color(180, 180, 180);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2800, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);

            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2500, 1, 0.3f);
            feildGen.addPrefab(randEconomy, 9);
            feildGen.addPrefab(econtitanium);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.SetDepositPreset(greyCenter);
            //feildGen.addPrefab(econshipyard);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 2;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;//1;
            #endregion

            //SSC shipyard sector
            c = new Color(128, 128, 190);
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeEvenDistribution(300);
            preloadRequired.Add(c);
            feildGen.addPrefab(shipyard);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 1;
            feildGen.SetDepositPreset(greyEdge);
            sectorGenerators[c] = feildGen;

            //SSC data center
            c = new Color(233, 16, 93);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2800, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2500, 1, 0.3f);
            feildGen.addManualPrefab(sscdata, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 3);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.SetDepositPreset(greyCenter);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 4;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;

            //SSC drone boss
            c = new Color(233, 16, 94);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2800, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2500, 1, 0.3f);
            feildGen.addManualPrefab(sscdrones, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 3);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.SetDepositPreset(greyCenter);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 4;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;

            //SSC missile boss
            c = new Color(233, 16, 95);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2800, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2500, 1, 0.3f);
            feildGen.addManualPrefab(sscmissiles, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 3);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.SetDepositPreset(greyEdge);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 4;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;

            //SSC extra system gate
            c = new Color(233, 16, 96);
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 2800, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey3);
            backdrops[c] = field2;
            mapIcons[c] = greyBatch;
            iconTechniques[c] = "roids";
            preloadRequired.Add(c);
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.makeClumps(2500, 1, 0.3f);
            feildGen.addManualPrefab(sscgate, new Vector2(0, 0));
            feildGen.addPrefab(randEconomy, 3);
            feildGen.addPrefab(econtrade2);
            feildGen.addPrefab(econmall);
            feildGen.SetDepositPreset(greyCenter);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 4;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;

            //higgs research base
            c = new Color(255, 106, 11);
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 0, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey2);
            field2.lightShaftSet = greyRoidsLightShafts;
           
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.trackTextures(gateWrecks, "gatewrecks");
            preloadRequired.Add(c);
            feildGen.addManualPrefab(higgsgate, Vector2.Zero);
            sectorGenerators[c] = feildGen;

            #region Purple nebula
            c = new Color(178, 0, 255);//Purple edge
            lightSettings[c] = purpleNebula2Lights;
            audioSettings[c] = redSongs;
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 200, -400, 2000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: purpleNebula2.hdrHigh, alphaMin: purpleNebula2.alphaMin, roughness: purpleNebula2.roughness, edgeIntensity: purpleNebula2.edgeIntensity, lightA: purpleNebula2.lightA, lightB: purpleNebula2.lightB, generalMult: purpleNebula2.generalMult, hdrHphase: purpleNebula2.hdrHphase, deepBackground: purpleNebula2.deepBackground, starColor: purpleNebula2.starColor, starDustColor: purpleNebula2.starDustColor);
            field2.lightShaftSet = purpleNebulaEdgeLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            //feildGen.addPrefab(blockade);
            //feildGen.minPrefabs = 8;
            //feildGen.maxPrefabs = 12;
            sectorGenerators[c] = feildGen;

            c = new Color(72, 0, 255);//Purple center
            lightSettings[c] = purpleNebula1Lights;
            audioSettings[c] = redSongs;
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 200, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: purpleNebula1.hdrHigh, alphaMin: purpleNebula1.alphaMin, roughness: purpleNebula1.roughness, edgeIntensity: purpleNebula1.edgeIntensity, lightA: purpleNebula1.lightA, lightB: purpleNebula1.lightB, generalMult: purpleNebula1.generalMult, hdrHphase: purpleNebula1.hdrHphase, deepBackground: purpleNebula1.deepBackground, starColor: purpleNebula1.starColor, starDustColor: purpleNebula1.starDustColor);
            field2.lightShaftSet = purpleNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            //feildGen.addPrefab(blockade);
            //feildGen.minPrefabs = 8;
            //feildGen.maxPrefabs = 12;
            sectorGenerators[c] = feildGen;
            #endregion

            #region Wolf nebula

            //ssc blockade
            c = new Color(0, 128, 175);//in deep space
            lightSettings[c] = greyLights;
            audioSettings[c] = redSongs;
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 200, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", wreckage, "wreckage");
            feildGen.addPrefab(blockade);
            feildGen.minPrefabs = 8;
            feildGen.maxPrefabs = 12;
            sectorGenerators[c] = feildGen;

            //gate system
            c = new Color(255, 106, 4);//low density
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            //preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2400, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: wolfNebula3.hdrHigh, alphaMin: wolfNebula3.alphaMin, roughness: wolfNebula3.roughness, edgeIntensity: wolfNebula3.edgeIntensity, lightA: wolfNebula3.lightA, lightB: wolfNebula3.lightB, generalMult: wolfNebula3.generalMult, hdrHphase: wolfNebula3.hdrHphase, deepBackground: wolfNebula3.deepBackground, starColor: wolfNebula3.starColor, starDustColor: wolfNebula3.starDustColor);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaEdgeLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1000, -0.2f, 1);
            feildGen.addManualPrefab(gate4, new Vector2(0, 0));
            feildGen.addPrefab(redDynfab0);
            feildGen.addPrefab(redDynfab1);
            feildGen.addPrefab(redDynfab2);
            feildGen.addPrefab(redDynfab3);
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 4;
            feildGen.SetDepositPreset(wolfEdge);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaEdgeLights;
            

            //everett's prefab sector r
            c = new Color(255, 106, 12);//low density
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2400, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: wolfNebula3.hdrHigh, alphaMin: wolfNebula3.alphaMin, roughness: wolfNebula3.roughness, edgeIntensity: wolfNebula3.edgeIntensity, lightA: wolfNebula3.lightA, lightB: wolfNebula3.lightB, generalMult: wolfNebula3.generalMult, hdrHphase: wolfNebula3.hdrHphase, deepBackground: wolfNebula3.deepBackground, starColor: wolfNebula3.starColor, starDustColor: wolfNebula3.starDustColor);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaEdgeLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(everettEventRedBatch, "everettred");
            feildGen.makeVariableDensity(1000, -0.2f, 1);
            feildGen.addManualPrefab(everettr, new Vector2(35800, 62100));
            feildGen.SetDepositPreset(wolfEdge);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaEdgeLights;

            //gate system 
            c = new Color(255, 106, 5);//low density
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            //preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2400, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: wolfNebula3.hdrHigh, alphaMin: wolfNebula3.alphaMin, roughness: wolfNebula3.roughness, edgeIntensity: wolfNebula3.edgeIntensity, lightA: wolfNebula3.lightA, lightB: wolfNebula3.lightB, generalMult: wolfNebula3.generalMult, hdrHphase: wolfNebula3.hdrHphase, deepBackground: wolfNebula3.deepBackground, starColor: wolfNebula3.starColor, starDustColor: wolfNebula3.starDustColor);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaEdgeLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1000, -0.2f, 1);
            feildGen.addManualPrefab(gate5, new Vector2(0, 0));
            feildGen.addPrefab(redDynfab0);
            feildGen.addPrefab(redDynfab1);
            feildGen.addPrefab(redDynfab2);
            feildGen.addPrefab(redDynfab3);
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 4;
            feildGen.SetDepositPreset(wolfEdge);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaEdgeLights;


            //gate system
            c = new Color(255, 106, 6);//in deep space
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);
            backdrops[c] = field2;

            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            feildGen.SetDepositPreset(greyEdge);
            //preloadRequired.Add(c);
            feildGen.addManualPrefab(gate6, new Vector2(0, 0));
            sectorGenerators[c] = feildGen;

            c = new Color(58, 4, 0);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            //preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2400, -400, 5000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: wolfNebula3.hdrHigh, alphaMin: wolfNebula3.alphaMin, roughness: wolfNebula3.roughness, edgeIntensity: wolfNebula3.edgeIntensity, lightA: wolfNebula3.lightA, lightB: wolfNebula3.lightB, generalMult: wolfNebula3.generalMult, hdrHphase: wolfNebula3.hdrHphase, deepBackground: wolfNebula3.deepBackground, starColor: wolfNebula3.starColor, starDustColor: wolfNebula3.starDustColor);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1000, -0.2f, 1);
            feildGen.addPrefab(redDynfab0);
            feildGen.addPrefab(redDynfab1);
            feildGen.addPrefab(redDynfab2);
            feildGen.addPrefab(redDynfab3);
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 0;
            feildGen.maxPrefabs = 5;
            feildGen.SetDepositPreset(wolfEdge);
            //feildGen.addPrefab(myDickButtTest);
            //feildGen.minPrefabs = 5;
            //feildGen.maxPrefabs = 20;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;

            

            //sector with cutthroat base
            c = new Color(233, 16, 91);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2600, -400, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets,  wolfNebula2);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.trackTextures(asteroidFieldAssets, "asteroids1");
            feildGen.makeVariableDensity(1200, -0.1f, 1);
            feildGen.addPrefab(redDynfab0);
            feildGen.addPrefab(redDynfab1);
            feildGen.addPrefab(redDynfab2);
            feildGen.addPrefab(redDynfab3);
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 6;
            feildGen.addManualPrefab(jerkcove, new Vector2(3000, -45000));
            feildGen.SetDepositPreset(wolfInner);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;
            preloadRequired.Add(c);

            c = new Color(117, 8, 0);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            //preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2600, -400, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets,  wolfNebula2);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1200, -0.1f, 1);
            feildGen.addPrefab(redDynfab0);
            feildGen.addPrefab(redDynfab1);
            feildGen.addPrefab(redDynfab2);
            feildGen.addPrefab(redDynfab3);
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 6;
            //feildGen.addPrefab(myDickButtTest);
            //feildGen.minPrefabs = 5;
            feildGen.SetDepositPreset(wolfInner);
            //feildGen.maxPrefabs = 20;
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;


            c = new Color(233, 16, 0);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            //preloadRequired.Add(c);
            field2 = new ObjectFieldRev2(null, blackRedAsteroidFieldAssets, null, 3000, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, wolfNebula1);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackRedAsteroidFieldAssets, "asteroidsBlackRed", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1200, 0, 1);
            feildGen.addPrefab(redDynfab0);
            feildGen.addPrefab(redDynfab1);
            feildGen.addPrefab(redDynfab2);
            feildGen.addPrefab(redDynfab3);
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 6;
            feildGen.SetDepositPreset(wolfCenter);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;



            c = new Color(117, 9, 0);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2600, -400, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets,  wolfNebula2);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1000, -0.1f, 1);
            feildGen.SetDepositPreset(wolfInner);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;


            c = new Color(233, 17, 0);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, blackRedAsteroidFieldAssets, null, 3000, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, wolfNebula1);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackRedAsteroidFieldAssets, "asteroidsBlackRed", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1200, 0, 1);
            feildGen.SetDepositPreset(wolfCenter);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;


            //where the ambush station lives
            c = new Color(233, 16, 92);
            mapIcons[c] = redBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2600, -400, 7000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets,  wolfNebula2);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            backdrops[c] = field2;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.makeVariableDensity(1200, -0.1f, 1);
            feildGen.addManualPrefab(ctpambush, new Vector2(-5000, 5000));
            feildGen.addPrefab(redDynfab0);
            feildGen.addPrefab(redDynfab1);
            feildGen.addPrefab(redDynfab2);
            feildGen.addPrefab(redDynfab3);
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 6;
            feildGen.SetDepositPreset(wolfInner);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;
            preloadRequired.Add(c);

            c = new Color(233, 16, 90);
            field2 = new ObjectFieldRev2(null, blackAsteroidFieldAssets, null, 2000, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets,  wolfNebula2);
            field2.lightShaftOrgin = new Vector2(0.75f, 0.375f);
            field2.lightShaftSet = wolfNebulaCoreLS;
            mapIcons[c] = redBatch;
            iconTechniques[c] = "planet";
            backdrops[c] = field2;
            //preloadRequired.Add(c);
            field2.planetTypeGeneration = 0;
            field2.masks = new string[1];
            field2.masks[0] = "dynplanetmask";
            field2.diffuses = new string[1];
            field2.diffuses[0] = "TerrainCombined1";
            field2.heights = new string[1];
            field2.heights[0] = "Planet_Height3";
            //field2.heights[1] = "Planet_Height2";
            //field2.heights[2] = "Planet_Height3";
            field2.normals = new string[1];
            field2.normals[0] = "Planet_Normal3";
            //field2.normals[1] = "Planet_Normal2";
            //field2.normals[2] = "Planet_Normal3";
            field2.emissions = new string[1];
            field2.emissions[0] = "dynplanetlights";
            field2.roughnesss = new string[1];
            field2.roughnesss[0] = "Lave_surf_spec";
            field2.lutParameters = new AtmosParams[1];
            field2.lutParameters[0] = new AtmosParams();
            field2.lutParameters[0].fInnerRadius = 14000 * planetSizeMult;
            field2.lutParameters[0].fOuterRadius = 14200 * planetSizeMult;
            field2.lutParameters[0].fRayleighScaleHeight = 0.15f;
            field2.lutParameters[0].fMieScaleHeight = 0.10f;
            feildGen = new FancyFieldTerrainGenerator(blackAsteroidFieldAssets, "asteroidsBlack", mixedWreckage, "mixedwreckage");
            feildGen.trackTextures(bigwrecks, "bigwrecks");
            feildGen.trackTextures(asteroidFieldAssets, "asteroids1");
            feildGen.makeVariableDensity(900, 0, 1);
            feildGen.addManualPrefab(piratecove, new Vector2(0, -10000));
            feildGen.addPrefab(redsmallfab0);
            feildGen.addPrefab(redsmallfab1);
            feildGen.minPrefabs = 1;
            feildGen.maxPrefabs = 6;
            feildGen.SetDepositPreset(wolfPlanet);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = wolfNebulaLights;
            audioSettings[c] = redSongs;//6;
            preloadRequired.Add(c);
            #endregion


            //Greenzone
            c = new Color(130, 160, 0);
            mapIcons[c] = greenBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, infestedBlackAsteroidsAssets, null, 4, -400, 4);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, green1);
            field2.lightShaftOrgin = new Vector2(0.48125f, 0.33f);
            backdrops[c] = field2;      
            field2.lightShaftSet = greenZoneLightShafts;
            feildGen = new FancyFieldTerrainGenerator(infestedBlackAsteroidsAssets, "asteroidInfestedBlack", mixedWreckage, "mixedwreckage");
            feildGen.SetDepositPreset(greenCenter);
            feildGen.makeVariableDensity(1400, 0, 1);
            sectorGenerators[c] = feildGen;
            lightSettings[c] = greenZoneLights;
            audioSettings[c] = greenSongs;//6;
            /*
            mapIcons[c] = blueBatch;
            iconTechniques[c] = "nebula";
            field2 = new ObjectFieldRev2(null, iceFieldAssets, null, 3000, -300, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, hdrHigh: BlueZone3.hdrHigh, alphaMin: BlueZone3.alphaMin, roughness: BlueZone3.roughness, edgeIntensity: BlueZone3.edgeIntensity, lightA: BlueZone3.lightA, lightB: BlueZone3.lightB, generalMult: BlueZone3.generalMult, hdrHphase: BlueZone3.hdrHphase, deepBackground: BlueZone3.deepBackground, starColor: BlueZone3.starColor, starDustColor: BlueZone3.starDustColor);
            field2.lightShaftSet = blueZoneLightShafts;
            backdrops[c] = field2;
            asteroidFeildGen = new SimpleFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", 4000);
            sectorGenerators[c] = asteroidFeildGen;
            lightSettings[c] = blueZoneLights;
            audioSettings[c] = new string[] { "Track 1 Cold Blue", "Track 2 Cold Blue" };//3;
            */


            //c = new Color(255, 200, 70);
            //field2 = new ObjectFieldRev2("NebulaHome", asteroidFieldAssets, null, 4092, -350, 10000);
            //backdrops[c] = field2;
            ////asteroidFeildGen = new SimpleFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", 3200);
            ////sectorGenerators[c] = asteroidFeildGen;
            //feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1");
            //feildGen.makeClumps(4500, 1, 0.6f);
            //sectorGenerators[c] = feildGen;

            //c = Color.LightBlue;
            //field2 = new ObjectFieldRev2("FrozenNebula", iceFieldAssets, null, 3092, -350, 10000);
            //backdrops[c] = field2;
            //asteroidFeildGen = new SimpleFieldTerrainGenerator(iceFieldAssets, "iceAsteroids1", 2600);
            //sectorGenerators[c] = asteroidFeildGen;


            //intro system
            c = new Color(8, 8, 8);//in deep space
            lightSettings[c] = greyLights;
            audioSettings[c] = greySongs;
            field2 = new ObjectFieldRev2(null, asteroidFieldAssets, null, 500, -400, 10000);
            field2.cloudAssets = generalNebulaAssets;
            field2.setClouds(generalNebulaAssets, grey1);

            backdrops[c] = field2;

            feildGen = new FancyFieldTerrainGenerator(asteroidFieldAssets, "asteroids1", wreckage, "wreckage");
            //preloadRequired.Add(c);
            feildGen.addManualPrefab(tutorial, new Vector2(0, 0));
            sectorGenerators[c] = feildGen;

            //Build info data
            //backdropInfo = new Dictionary<Color, BackdropInfo>();

            BackdropInfo info;
            foreach(KeyValuePair<Color, TerrainGenerator> sectorType in sectorGenerators)
            {
                FancyFieldTerrainGenerator temp = sectorType.Value as FancyFieldTerrainGenerator;
                if(temp != null)
                {
                    info = new BackdropInfo();
                    info.depositeWeights = temp.depositeWeights;
                    info.maxDeposites = temp.maxDeposites;
                    info.minDeposites = temp.minDeposites;
                    
                    if(mapIcons.ContainsKey(sectorType.Key))
                    {
                        IconBatch iconB = mapIcons[sectorType.Key];
                        switch(iconB.assetName)
                        {
                            case "256roidBT":
                                    info.biomeType = BiomeType.outer_nebula;
                              break;
                            case "256roidBTlow":
                                    info.biomeType = BiomeType.outer_nebula;
                                    break;
                            case "256roidRT":
                                    info.biomeType = BiomeType.wolf_nebula;
                                    break;
                            case "256roidOG":
                                    info.biomeType = BiomeType.domum_nebula;
                                    break;
                            case "256roidGT":
                                    info.biomeType = BiomeType.grey_nebula;
                                    break;
                            case "256roidGE":
                                    info.biomeType = BiomeType.infested_nebula;
                                    break;
                        }
                        
                    }
                     

                    backdropInfo.Add(sectorType.Key, info);


                    //this isnt ideal but for sake of my sanity we base it on 
                }
            }


        }
    }
}
