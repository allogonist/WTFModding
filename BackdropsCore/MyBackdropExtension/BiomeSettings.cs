using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BackdropExtension
{
    public struct BiomeSettings
    {
        public float hdrHigh;
        public float alphaMin;
        public float roughness;
        public float edgeIntensity;
        public Vector3 lightA;
        public Vector3 lightB;
        public Vector3 generalMult;
        public Vector3 hdrHphase;
        public Vector3 deepBackground;
        public Vector3 starColor;
        public Vector3 starDustColor;

        public Vector4 cloudColor;
    }
}
