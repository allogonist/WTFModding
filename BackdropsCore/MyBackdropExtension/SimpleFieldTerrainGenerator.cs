using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;

namespace BackdropsCore
{
    public class SimpleFieldTerrainGenerator : TerrainGenerator
    {
        int itemTypeCount;
        string sourceBatch;
        int density;

        public SimpleFieldTerrainGenerator(TextureBatch allItems, string allItemsName, int maxDensity)
        {
            itemTypeCount = allItems.diffNames.Length;
            density = maxDensity;
            sourceBatch = allItemsName;
        }

        public SectorTerrainList generate(byte generationFlags, float noiseSample, int gridx, int gridy)
        {
            SectorTerrainList t = new SectorTerrainList(itemTypeCount);
            
            //declare categories
            for (int i = 0; i < itemTypeCount; i++)
            {
                //parameters explained in order
                //we have exactly 1 category for each texture in the batch
                //they are all from the same texture batch
                //the index of the item in the batch is the same as the index of our category
                t.setCategory(i, sourceBatch, i);
            }

            //calculate total quantity of items as being proportional to density times alpha of the drawn pixel
            int count = (int)((generationFlags / 255f) * density);

            //generate items
            float seed = noiseSample * 100000;

            int gridRoids = (int)Math.Sqrt(count * 0.8);
            int randRoids = (int)(count * 0.2);

            float halfwidth = 90000;//how far out it extends
            float totalWide = 2 * halfwidth;
            float gridStep = totalWide / gridRoids;

            Random rand = new Random((int)(seed));

            for (int y = 0; y < gridRoids; y++)
            {
                for (int x = 0; x < gridRoids; x++)
                {
                    Vector2 position = new Vector2();
                    position.X = (gridStep * x) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    position.Y = (gridStep * y) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    int category = rand.Next(itemTypeCount);
                    TerrainItemTeplate item = new TerrainItemTeplate();
                    item.position = position;
                    item.rotation = rot;
                    t.addItem(category, item);
                }
            }
            for (int i = 0; i < randRoids; i++)
            {
                Vector2 position = new Vector2((float)((rand.NextDouble() * totalWide) - halfwidth), (float)((rand.NextDouble() * totalWide) - halfwidth));
                float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                int category = rand.Next(itemTypeCount);
                TerrainItemTeplate item = new TerrainItemTeplate();
                item.position = position;
                item.rotation = rot;
                t.addItem(category, item);
            }

            return t;
        }
    }
}
