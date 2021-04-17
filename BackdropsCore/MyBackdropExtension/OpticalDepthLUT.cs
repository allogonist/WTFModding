using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace BackdropsCore
{
    class OpticalDepthLUT
    {
        const int LUT_w = 256;
        const int LUT_h = 256;


        static public Texture2D MakeOpticalLUT(GraphicsDevice graphicsDevice, float fInnerRadius, float fOuterRadius, float fRayleighScaleHeight, float fMieScaleHeight)
        {
            const float DELTA = 1e-6f;
            //const int nSize = LUT_w;
            const int nSamples = 10;
            float fScale = 1.0f / (fOuterRadius - fInnerRadius);

            float[] aRayleighDensity = new float[LUT_w * LUT_h];
            float[] aRayleighDepth = new float[LUT_w * LUT_h];
            float[] aMieDensityRatio = new float[LUT_w * LUT_h];
            float[] aMieDepth = new float[LUT_w * LUT_h];

            int nIndex = 0;
            for (int nAngle = 0; nAngle < LUT_h; nAngle++)
            {
                float fCos = 1.0f - (nAngle + nAngle) / (float)LUT_h;
                float fAngle = (float)Math.Acos(fCos);
                Vector3 vRay = new Vector3((float)Math.Sin(fAngle), (float)Math.Cos(fAngle), 0);
                for (int nHeight = 0; nHeight < LUT_w; nHeight++)
                {
                    float fHeight = DELTA + fInnerRadius + ((fOuterRadius - fInnerRadius) * nHeight) / LUT_w;
                    Vector3 vPos = new Vector3(0, fHeight, 0);

                    float B = 2.0f * Vector3.Dot(vPos, vRay);
                    float Bsq = B * B;
                    float Cpart = Vector3.Dot(vPos, vPos);
                    float C = Cpart - fInnerRadius * fInnerRadius;
                    float fDet = Bsq - 4.0f * C;
                    bool bVisible = (fDet < 0 || (0.5f * (-B - (float)Math.Sqrt(fDet)) <= 0) && (0.5f * (-B + (float)Math.Sqrt(fDet)) <= 0));
                    float fRayleighDensityRatio;
                    float fMieDensityRatio;
                    if (bVisible)
                    {
                        fRayleighDensityRatio = (float)Math.Exp(-(fHeight - fInnerRadius) * fScale / fRayleighScaleHeight);
                        fMieDensityRatio = (float)Math.Exp(-(fHeight - fInnerRadius) * fScale / fMieScaleHeight);
                    }
                    else
                    {
                        // Smooth the transition from light to shadow (it is a soft shadow after all)
                        fRayleighDensityRatio = aRayleighDensity[nIndex - LUT_w] * (0.75f + 0.17f);
                        fMieDensityRatio = aMieDensityRatio[nIndex - LUT_w] * (0.75f + 0.17f);
                    }



                    C = Cpart - fOuterRadius * fOuterRadius;
                    fDet = Bsq - 4.0f * C;
                    float fFar = 0.5f * (-B + (float)Math.Sqrt(fDet));

                    float fSampleLength = fFar / nSamples;
                    float fScaledLength = fSampleLength * fScale;
                    Vector3 vSampleRay = vRay * fSampleLength;
                    vPos += vSampleRay * 0.5f;

                    float fRayleighDepth = 0;
                    float fMieDepth = 0;
                    for (int i = 0; i < nSamples; i++)
                    {
                        fHeight = vPos.Length();
                        float fAltitude = (fHeight - fInnerRadius) * fScale;
                        fAltitude = MathHelper.Max(fAltitude, 0.0f);
                        fRayleighDepth += (float)Math.Exp(-fAltitude / fRayleighScaleHeight);
                        fMieDepth += (float)Math.Exp(-fAltitude / fMieScaleHeight);
                        vPos += vSampleRay;
                    }

                    fRayleighDepth *= fScaledLength;
                    fMieDepth *= fScaledLength;

                    aRayleighDensity[nIndex] = fRayleighDensityRatio;
                    aRayleighDepth[nIndex] = fRayleighDepth;
                    aMieDensityRatio[nIndex] = fMieDensityRatio;
                    aMieDepth[nIndex] = fMieDepth;
                    nIndex++;
                }
            }

            return convertToTex(graphicsDevice, aRayleighDensity, aRayleighDepth, aMieDensityRatio, aMieDepth);
        }

        static private Texture2D convertToTex(GraphicsDevice graphicsDevice, float[] inputArray1, float[] inputArray2, float[] inputArray3, float[] inputArray4)
        {
            Texture2D outputTex = new Texture2D(graphicsDevice, LUT_w, LUT_h, false, SurfaceFormat.Vector4);
            Vector4[] temp = new Vector4[LUT_w * LUT_h];
            for (int i = 0; i < LUT_w * LUT_h; i++)
            {
                temp[i] = new Vector4(inputArray1[i], inputArray2[i], inputArray3[i], inputArray4[i]);
            }
            outputTex.SetData<Vector4>(temp);
            return outputTex;
        }
    }
}
