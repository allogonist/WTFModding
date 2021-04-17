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
    public struct PlanetRenderSettings
    {
        public bool hasAtmosphere;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 rotationRate;
        public Vector3 color1;
        public Vector3 color2;
        public Vector3 color3;
        public Vector3 color4;
        public Vector3 phase;
        public Vector4 emissionColor;
        public Vector4 fluidColor;
        public float fluidEmitIntensity;
        public float radius;
        public int mask;
        public int diffuse;
        public int height;
        public int normal;
        public int emission;
        public int roughness;
        public int atmosphere;
        public ushort surfaceTilling;
        public Vector3 waveLenghts4;
        public float fluidLevel;
        public int planetType;
    }

    public struct AtmosParams
    {
        public float fInnerRadius;
        public float fOuterRadius;
        public float fRayleighScaleHeight;
        public float fMieScaleHeight;
    }

    

    public struct CloudField
    {
        public Vector3[] position;
        public float[] rotation;
        public Color[] color;
    }


    public struct StarField
    {
        public Vector3[] position;
        public int[] sliceID;
        public float[] scale;
        public Color[] color;
    }


    public struct StarDustField
    {
        public Vector3[] position;
        public float[] rotation;
        public Color[] color;
        public float[] scale;
    }


    public struct CloudLights
    {
        public Vector3[] position;
        public float[] attuneation;
        public float[] intensity;
        public float[] type;
    }


    public class ObjectFieldInstance : BackdropInstance
    {
        private Color colorKey;

        public uint uid;

        public List<Vector3>[] positions;
        public List<float>[] rotations;

        public bool hasClouds = false;
        public bool hasStarDust = false;
        public bool hasStars = false;
        public CloudField cloudField;
        public CloudField[] cloudFieldGroups;
        public StarField starField;
        public StarDustField starDustField;
        public CloudLights cloudLights;
        public TextureBatch cloudSheet;
        public Vector4 instLightShaftSet;

        public Vector3 cloudsLightA, cloudsLightB, cloudGeneralMult, cloudHdrHighPhaseColor, cloudDeepBackground, cloudStarColor, cloudStarDustColor;
        public float cloudHdrHighPhase, cloudAlphaMin, cloudRoughness, cloudEdgeIntensity;

        public Vector4 fogCloudsColor;

        public bool hasPlanet = false;
        public PlanetRenderSettings planet;

        public ObjectFieldInstance(Color k, int itemTypes)
        {
            colorKey = k;
            if(itemTypes > 0)
            {
                positions = new List<Vector3>[itemTypes];
                rotations = new List<float>[itemTypes];
            }
        }

        public Color getKey()
        {
            return colorKey;
        }

        public void update(GameTime time)
        {
            if (hasPlanet)
            {
                planet.rotation = planet.rotationRate * (float)time.TotalGameTime.TotalSeconds;
            }
        }
    }
}
