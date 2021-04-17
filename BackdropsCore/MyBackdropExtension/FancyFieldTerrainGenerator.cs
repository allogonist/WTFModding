using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;

namespace BackdropsCore
{
    public enum GenerationStyle : uint
    {
        none,
        even_distribution,
        variable_density,
        clumps,
        doughnuts,
        tiny_sector
    }

    public struct DeadZone
    {
        public float range;
        public Vector2 position;
    }

    class FancyFieldTerrainGenerator : TerrainGenerator
    {
        int itemTypeCount;
        int wreckTypeCount;
        string sourceBatch;
        string wreckBatch;
        int density = 1000;
        float extraChance = 0;
        float scale = 1;
        float clumpCoverage = 0.5f;
        GenerationStyle style = GenerationStyle.none;
        List<DeadZone> deadZones;

        private List<Vector2> goodOreNodeSpots = new List<Vector2>();

        public int minPrefabs, maxPrefabs;
        public bool actuallyRandom = false;
        public bool randomPrefabs = false;

        public int minDeposites = 0;
        public int maxDeposites = 0;

        public int minMiningOperations = 0;
        public int maxMiningOperations = 0;

        public Dictionary<DepositeType, float> depositeWeights;
        public Dictionary<DepositeType, float> miningOpWeights;
        public List<DepositeType> gurantedDeposits;

        //above 4 = 100% clouds
        //real range = 0 to 4
        //reasonable range = 0 to 1
        public float environmentCloudDensity = -1;//default value of -1 means unless instructed, all session types have some clouds
        public float plasmaCloudDensity = 0;
        //public float oreDensity = 0.01f;
        public string oreType = "orange";
        //public int oreQuality = 1;

        private List<Prefab> prefabs;
        private Dictionary<Prefab, int> prefabWeights;
        private List<Prefab> prefabExclusives;

        public Dictionary<Vector3, Prefab> manualPrefabs;

        Dictionary<string, Dictionary<string, int>> textureList;

        public FancyFieldTerrainGenerator(TextureBatch allItems, string allItemsName, TextureBatch wreckItems, string wreckItemsName)
        {
            itemTypeCount = allItems.diffNames.Length;
            wreckTypeCount = wreckItems.diffNames.Length;
            sourceBatch = allItemsName;
            wreckBatch = wreckItemsName;
            textureList = new Dictionary<string, Dictionary<string, int>>();
            trackTextures(allItems, allItemsName);
            trackTextures(wreckItems, wreckItemsName);
        }

        public void SetDepositPreset(DepositPreset preset)
        {
            minDeposites = preset.minDeposits;
            maxDeposites = preset.maxDeposits;

            minMiningOperations = preset.minMiningOperations;
            maxMiningOperations = preset.maxMiningOperations;

            depositeWeights = preset.depositeWeights;

            miningOpWeights = preset.operationsWeights;
        }


        public void AddDepositeChance(DepositeType deposite, float weight)
        {
            if (depositeWeights == null)
                depositeWeights = new Dictionary<DepositeType, float>();

            if(!depositeWeights.ContainsKey(deposite))
            {
                depositeWeights.Add(deposite, weight);
            }
            else
            {
                depositeWeights[deposite] += weight;
            }
        }

        public void trackTextures(TextureBatch allItems, string allItemsName)
        {
            textureList[allItemsName] = new Dictionary<string, int>();
            for (int i = 0; i < allItems.diffNames.Length; i++)
            {
                textureList[allItemsName][allItems.diffNames[i]] = i;
            }
        }

        private string findSourceBatch(string assetName)
        {
            foreach(string s in textureList.Keys)
            {
                if(textureList[s].ContainsKey(assetName))
                {
                    return s;
                }
            }
            throw new Exception("Did not find a texture batch for the following asset: " + assetName);
            //return null;
        }

        #region zoneTypes

        /// <summary>
        /// Uses a randomized grid point displacement to ensure even distribution across the entire zone
        /// </summary>
        public void makeEvenDistribution(int maxDensity)
        {
            density = maxDensity;
            style = GenerationStyle.even_distribution;
        }

        /// <summary>
        /// Asteroids spawn everywhere, but some regions are more dense than others
        /// </summary>
        /// <param name="baselineDensity">Values -1 to 1 determine baseline spawn chance regardless of density. A value of 1 results in max density everywhere. Values below zero will create islands</param>
        /// <param name="noiseScale">Determines scale of noise artifacts, larger values sample more noise and return smaller artifacts</param>
        public void makeVariableDensity(int maxDensity, float baselineDensity, float noiseScale)
        {
            density = maxDensity;
            extraChance = baselineDensity + 0.01f;
            scale = (1 / noiseScale) * 0.00005f;
            style = GenerationStyle.variable_density;
        }

        /// <summary>
        /// Asteroids spawn in clumps of constant density separated by regions of no asteroids
        /// </summary>
        /// <param name="noiseScale">Determines scale of noise artifacts</param>
        /// <param name="coverage">percentage clump coverage. 0 = no clumps. 1 = entire zone is a single clump.</param>
        public void makeClumps(int maxDensity, float noiseScale, float coverage)
        {
            density = maxDensity;
            scale = (1 / noiseScale) * 0.0001f;
            clumpCoverage = coverage;
            style = GenerationStyle.clumps;
        }

        /// <summary>
        /// A stupid layout added because whynot. Makes clumps with holes in the middle of them.
        /// </summary>
        /// <param name="noiseScale">Determines scale of noise artifacts</param>
        /// <param name="ringWidth">values between 0 and 1 determine ring width</param>
        public void makeDoughnuts(int maxDensity, float noiseScale, float ringWidth)
        {
            density = maxDensity;
            scale = (1 / noiseScale) * 0.0001f;
            clumpCoverage = ringWidth;
            style = GenerationStyle.doughnuts;
        }

        /// <summary>
        /// Asteroids spawn in clumps of constant density separated by regions of no asteroids
        /// </summary>
        /// <param name="noiseScale">Determines scale of noise artifacts</param>
        /// <param name="coverage">percentage clump coverage. 0 = no clumps. 1 = entire zone is a single clump.</param>
        public void makeTinySector(int maxDensity, float noiseScale, float coverage)
        {
            density = maxDensity;
            scale = (1 / noiseScale) * 0.0008f;
            clumpCoverage = coverage;
            style = GenerationStyle.tiny_sector;
        }


        private void doughnutDist(SectorTerrainList t, byte generationFlags, float noiseSample, List<DeadZone> blockedZones)
        {
            //SectorTerrainList t = prepareList();

            int count = (int)((generationFlags / 255f) * density);

            List<Vector2> oreNodes = new List<Vector2>();
            List<string> oreValues = new List<string>();

            //generate items
            float seed = noiseSample * 100000;

            int gridRoids = (int)Math.Sqrt(count * 3);

            float halfwidth = 90000;//how far out it extends
            float totalWide = 2 * halfwidth;
            float gridStep = totalWide / gridRoids;

            Random rand;
            if (actuallyRandom)
            {
                rand = new Random();
            }
            else
            {
                rand = new Random((int)(seed));
            }

            double noisePoint;

            double lowend, highend;
            lowend = 0.5 - (clumpCoverage / 2);
            highend = 0.5 + (clumpCoverage / 2);

            for (int y = 0; y < gridRoids; y++)
            {
                for (int x = 0; x < gridRoids; x++)
                {
                    Vector2 position = new Vector2();
                    position.X = (gridStep * x) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    position.Y = (gridStep * y) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    if (allowed(position, blockedZones))
                    {
                        noisePoint = NOISE.GetNoise(position.X * scale, position.Y * scale, noiseSample);
                        if (noisePoint > lowend && noisePoint < highend)
                        {
                            float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                            int category = rand.Next(itemTypeCount);
                            TerrainItemTeplate item = new TerrainItemTeplate();
                            item.position = position;
                            item.rotation = rot;
                            t.addItem(category, item);

                            //if(rand.NextDouble() < oreDensity)
                            //{
                            //    oreNodes.Add(position);
                            //    //oreValues.Add("ore_" + rand.Next(5).ToString());
                            //    oreValues.Add("ore_" + oreType);
                            //}
                        }
                        else if (noisePoint > highend)
                        {
                            goodOreNodeSpots.Add(position);
                        }
                    }
                }
            }
            t.addPointsOInterest(oreNodes.ToArray(), oreValues.ToArray());
            //return t;
        }

        private void tinySectorDist(SectorTerrainList t, byte generationFlags, float noiseSample, List<DeadZone> blockedZones)
        {
            //SectorTerrainList t = prepareList();

            int count = (int)((generationFlags / 255f) * density);

            List<Vector2> oreNodes = new List<Vector2>();
            List<string> oreValues = new List<string>();

            //generate items
            float seed = noiseSample * 100000;

            int gridRoids = (int)Math.Sqrt(count * 3);

            float halfwidth = 20000;//how far out it extends
            float totalWide = 2 * halfwidth;
            float gridStep = totalWide / gridRoids;

            Random rand;
            if (actuallyRandom)
            {
                rand = new Random();
            }
            else
            {
                rand = new Random((int)(seed));
            }

            double noisePoint;

            for (int y = 0; y < gridRoids; y++)
            {
                for (int x = 0; x < gridRoids; x++)
                {
                    Vector2 position = new Vector2();
                    position.X = (gridStep * x) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    position.Y = (gridStep * y) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    if (allowed(position, blockedZones))
                    {
                        noisePoint = NOISE.GetNoise(position.X * scale, position.Y * scale, noiseSample);
                        if (noisePoint < clumpCoverage)
                        {
                            float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                            int category = rand.Next(itemTypeCount);
                            TerrainItemTeplate item = new TerrainItemTeplate();
                            item.position = position;
                            item.rotation = rot;
                            t.addItem(category, item);

                            //if (rand.NextDouble() < oreDensity)
                            //{
                            //    oreNodes.Add(position);
                            //    //oreValues.Add("ore_" + rand.Next(5).ToString());
                            //    oreValues.Add("ore_" + oreType);
                            //}
                        }
                        else if (noisePoint < clumpCoverage * 1.1)
                        {
                            goodOreNodeSpots.Add(position);
                        }
                    }
                }
            }

            t.addPointsOInterest(oreNodes.ToArray(), oreValues.ToArray());
            //return t;
        }

        private void clumpDist(SectorTerrainList t, byte generationFlags, float noiseSample, List<DeadZone> blockedZones)
        {
            //SectorTerrainList t = prepareList();

            int count = (int)((generationFlags / 255f) * density);

            //List<Vector2> oreNodes = new List<Vector2>();
            //List<string> oreValues = new List<string>();

            //generate items
            float seed = noiseSample * 100000;

            int gridRoids = (int)Math.Sqrt(count * 3);

            float halfwidth = 90000;//how far out it extends
            float totalWide = 2 * halfwidth;
            float gridStep = totalWide / gridRoids;

            Random rand;
            if (actuallyRandom)
            {
                rand = new Random();
            }
            else
            {
                rand = new Random((int)(seed));
            }

            double noisePoint;

            for (int y = 0; y < gridRoids; y++)
            {
                for (int x = 0; x < gridRoids; x++)
                {
                    Vector2 position = new Vector2();
                    position.X = (gridStep * x) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    position.Y = (gridStep * y) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    if (allowed(position, blockedZones))
                    {
                        noisePoint = NOISE.GetNoise(position.X * scale, position.Y * scale, noiseSample);
                        if (noisePoint < clumpCoverage)
                        {
                            float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                            int category = rand.Next(itemTypeCount);
                            TerrainItemTeplate item = new TerrainItemTeplate();
                            item.position = position;
                            item.rotation = rot;
                            t.addItem(category, item);

                            //if (rand.NextDouble() < oreDensity)
                            //{
                            //    oreNodes.Add(position);
                            //    //oreValues.Add("ore_" + rand.Next(5).ToString());
                            //    oreValues.Add("ore_" + oreType);
                            //}
                        }
                        else if(noisePoint < clumpCoverage * 1.1)
                        {
                            goodOreNodeSpots.Add(position);
                        }
                    }
                }
            }

            //t.addPointsOInterest(oreNodes.ToArray(), oreValues.ToArray());
            //return t;
        }

        private void varDensDist(SectorTerrainList t, byte generationFlags, float noiseSample, List<DeadZone> blockedZones)
        {
            //SectorTerrainList t = prepareList();

            int count = (int)((generationFlags / 255f) * density);

            List<Vector2> oreNodes = new List<Vector2>();
            List<string> oreValues = new List<string>();

            //generate items
            float seed = noiseSample * 100000;

            int gridRoids = (int)Math.Sqrt(count * 20);
            int randRoids = (int)(count);

            float halfwidth = 90000;//how far out it extends
            float totalWide = 2 * halfwidth;
            float gridStep = totalWide / gridRoids;

            Random rand;
            if (actuallyRandom)
            {
                rand = new Random();
            }
            else
            {
                rand = new Random((int)(seed));
            }

            double noisePoint;

            for (int y = 0; y < gridRoids; y++)
            {
                for (int x = 0; x < gridRoids; x++)
                {
                    Vector2 position = new Vector2();
                    position.X = (gridStep * x) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    position.Y = (gridStep * y) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    if (allowed(position, blockedZones))
                    {
                        noisePoint = NOISE.GetNoise(position.X * scale, position.Y * scale, noiseSample);
                        noisePoint *= noisePoint;
                        noisePoint *= noisePoint;
                        noisePoint *= noisePoint;
                        double randRoll = rand.NextDouble();
                        if (randRoll < noisePoint + extraChance)
                        {
                            float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                            int category = rand.Next(itemTypeCount);
                            TerrainItemTeplate item = new TerrainItemTeplate();
                            item.position = position;
                            item.rotation = rot;
                            t.addItem(category, item);

                            //if (rand.NextDouble() < oreDensity)
                            //{
                            //    oreNodes.Add(position);
                            //    //oreValues.Add("ore_" + rand.Next(5).ToString());
                            //    oreValues.Add("ore_" + oreType);
                            //}
                        }
                    }
                }
            }
            for (int i = 0; i < randRoids; i++)
            {
                Vector2 position = new Vector2((float)((rand.NextDouble() * totalWide) - halfwidth), (float)((rand.NextDouble() * totalWide) - halfwidth));
                if (allowed(position, blockedZones))
                {
                    noisePoint = NOISE.GetNoise(position.X * scale, position.Y * scale, noiseSample);
                    if (rand.NextDouble() < noisePoint + extraChance)
                    {
                        float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                        int category = rand.Next(itemTypeCount);
                        TerrainItemTeplate item = new TerrainItemTeplate();
                        item.position = position;
                        item.rotation = rot;
                        t.addItem(category, item);

                        //if (rand.NextDouble() < oreDensity)
                        //{
                        //    oreNodes.Add(position);
                        //    //oreValues.Add("ore_" + rand.Next(5).ToString());
                        //    oreValues.Add("ore_" + oreType);
                        //}
                    }
                }
            }
            int oreSpots = randRoids / 4;
            for (int i = 0; i < oreSpots; i++)
            {
                Vector2 position = new Vector2((float)((rand.NextDouble() * totalWide) - halfwidth), (float)((rand.NextDouble() * totalWide) - halfwidth));
                if (allowed(position, blockedZones))
                {
                    noisePoint = NOISE.GetNoise(position.X * scale, position.Y * scale, noiseSample);
                    if (rand.NextDouble() < noisePoint + extraChance)
                    {
                        goodOreNodeSpots.Add(position);
                    }
                }
            }

            t.addPointsOInterest(oreNodes.ToArray(), oreValues.ToArray());
            //return t;
        }

        private void evenDist(SectorTerrainList t, byte generationFlags, float noiseSample, List<DeadZone> blockedZones)
        {
            //SectorTerrainList t = prepareList();

            //calculate total quantity of items as being proportional to density times alpha of the drawn pixel
            int count = (int)((generationFlags / 255f) * density);

            List<Vector2> oreNodes = new List<Vector2>();
            List<string> oreValues = new List<string>();

            //generate items
            float seed = noiseSample * 100000;

            int gridRoids = (int)Math.Sqrt(count * 0.8);
            int randRoids = (int)(count * 0.2);

            float halfwidth = 90000;//how far out it extends
            float totalWide = 2 * halfwidth;
            float gridStep = totalWide / gridRoids;

            Random rand;
            if (actuallyRandom)
            {
                rand = new Random();
            }
            else
            {
                rand = new Random((int)(seed));
            }

            for (int y = 0; y < gridRoids; y++)
            {
                for (int x = 0; x < gridRoids; x++)
                {
                    Vector2 position = new Vector2();
                    position.X = (gridStep * x) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    position.Y = (gridStep * y) - halfwidth + (float)(rand.NextDouble() * gridStep);
                    if (allowed(position, blockedZones))
                    {
                        float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                        int category = rand.Next(itemTypeCount);
                        TerrainItemTeplate item = new TerrainItemTeplate();
                        item.position = position;
                        item.rotation = rot;
                        t.addItem(category, item);

                        //if (rand.NextDouble() < oreDensity)
                        //{
                        //    oreNodes.Add(position);
                        //    //oreValues.Add("ore_" + rand.Next(5).ToString());
                        //    oreValues.Add("ore_" + oreType);
                        //}
                    }
                }
            }
            for (int i = 0; i < randRoids; i++)
            {
                Vector2 position = new Vector2((float)((rand.NextDouble() * totalWide) - halfwidth), (float)((rand.NextDouble() * totalWide) - halfwidth));
                if (allowed(position, blockedZones))
                {
                    float rot = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    int category = rand.Next(itemTypeCount);
                    TerrainItemTeplate item = new TerrainItemTeplate();
                    item.position = position;
                    item.rotation = rot;
                    t.addItem(category, item);

                    //if (rand.NextDouble() < oreDensity)
                    //{
                    //    oreNodes.Add(position);
                    //    //oreValues.Add("ore_" + rand.Next(5).ToString());
                    //    oreValues.Add("ore_" + oreType);
                    //}
                }
            }
            int oreSpots = randRoids / 4;
            for (int i = 0; i < oreSpots; i++)
            {
                Vector2 position = new Vector2((float)((rand.NextDouble() * totalWide) - halfwidth), (float)((rand.NextDouble() * totalWide) - halfwidth));
                if (allowed(position, blockedZones))
                {
                    goodOreNodeSpots.Add(position);
                }
            }

            t.addPointsOInterest(oreNodes.ToArray(), oreValues.ToArray());
            //return t;
        }
        #endregion

        public void addDeadZone(Vector2 where, float radius)
        {
            if (deadZones == null)
            {
                deadZones = new List<DeadZone>();
            }
            DeadZone z = new DeadZone();
            z.position = where;
            z.range = radius;
        }

        private bool allowed(Vector2 pos, List<DeadZone> prefabs)
        {
            //if (deadZones == null)
            //{
            //    return true;
            //}
            if(deadZones != null)
            {
                foreach (DeadZone z in deadZones)
                {
                    if (Vector2.Distance(z.position, pos) < z.range)
                    {
                        return false;
                    }
                }
            }
            foreach (DeadZone z in prefabs)
            {
                if (Vector2.Distance(z.position, pos) < z.range + 500)
                {
                    return false;
                }
            }
            return true;
        }

        private float howBad(Vector2 pos, float radius, List<DeadZone> prefabs)
        {
            float badness = 0;
            if (deadZones != null)
            {
                foreach (DeadZone z in deadZones)
                {
                    float b = (z.range + radius) - Vector2.Distance(z.position, pos);
                    if (b > 0)
                    {
                        badness += b;
                    }
                }
            }
            foreach (DeadZone z in prefabs)
            {
                float b = (z.range + radius) - Vector2.Distance(z.position, pos);
                if (b > 0)
                {
                    badness += b;
                }
            }
            return badness;
        }

        private bool allowed(Vector2 pos, float radius, List<DeadZone> prefabs)
        {
            if (deadZones != null)
            {
                foreach (DeadZone z in deadZones)
                {
                    if (Vector2.Distance(z.position, pos) < z.range + radius)
                    {
                        return false;
                    }
                }
            }
            foreach (DeadZone z in prefabs)
            {
                if (Vector2.Distance(z.position, pos) < z.range + radius)
                {
                    return false;
                }
            }
            return true;
        }

        public void addManualPrefab(Prefab prefab, Vector2 location)
        {
            Vector3 loc = new Vector3(location, 0);
            if (manualPrefabs == null)
            {
                manualPrefabs = new Dictionary<Vector3, Prefab>();
            }
            manualPrefabs[loc] = prefab;
        }

        public void addManualPrefab(Prefab prefab, Vector3 location)
        {
            if(manualPrefabs == null)
            {
                manualPrefabs = new Dictionary<Vector3, Prefab>();
            }
            manualPrefabs[location] = prefab;
        }

        public void addPrefab(Prefab prefab, int frequencyMultiplier)
        {
            if (prefabs == null)
            {
                prefabs = new List<Prefab>();
                prefabWeights = new Dictionary<Prefab, int>();
            }
            prefabs.Add(prefab);
            prefabWeights[prefab] = frequencyMultiplier;
        }

        public void addPrefab(Prefab prefab)
        {
            if (prefabs == null)
            {
                prefabs = new List<Prefab>();
                prefabWeights = new Dictionary<Prefab, int>();
            }
            prefabs.Add(prefab);
            prefabWeights[prefab] = 1;
        }

        public void preventMultiSpawn(Prefab prefab)
        {
            if(prefabExclusives == null)
            {
                prefabExclusives = new List<Prefab>();
            }
            prefabExclusives.Add(prefab);
        }

        private SectorTerrainList generatePrefabs(float noiseSample, List<DeadZone> blockedZones)
        {
            //the results of random selection stored here
            Queue<Prefab> toSpawn = new Queue<Prefab>();
            Queue<Vector3> prefabSpots = new Queue<Vector3>();

            //int placementFailures = 0;
            

            if (manualPrefabs != null)
            {
                foreach (Prefab p in manualPrefabs.Values)
                {
                    p.load();
                }
                foreach (Vector3 v in manualPrefabs.Keys)
                {
                    Prefab p = manualPrefabs[v];

                    DeadZone z = new DeadZone();
                    z.position = new Vector2(v.X, v.Y);
                    z.range = p.exclusionRadius;
                    blockedZones.Add(z);
                    toSpawn.Enqueue(p);
                    prefabSpots.Enqueue(v);
                }
            }

            if ((maxPrefabs > 0 && prefabs != null) || toSpawn.Count > 0)
            {
                if (prefabs != null)
                {
                    foreach (Prefab p in prefabs)
                    {
                        p.load();
                    }
                }
                int intseed = (int)(noiseSample * int.MaxValue);

                Random r;// = new Random(intseed);
                if (randomPrefabs || actuallyRandom)
                {
                    r = new Random();
                }
                else
                {
                    r = new Random(intseed);
                }

                int fabs = r.Next(maxPrefabs);
                if (fabs < minPrefabs)
                {
                    fabs = minPrefabs;
                }

                //used for random selection
                if (maxPrefabs > 0 && fabs > 0 && prefabs != null)
                {
                    List<Prefab> candidates = new List<Prefab>();
                    for (int i = 0; i < fabs; i++)
                    {
                        if (candidates.Count == 0)
                        {
                            foreach (Prefab s in prefabs)
                            {
                                int weight = prefabWeights[s];
                                for (int w = 0; w < weight; w++)
                                {
                                    candidates.Add(s);
                                }
                            }
                        }
                        int pickSpot = r.Next(candidates.Count);
                        Prefab chosen = candidates[pickSpot];

                        //try to pick a location for it
                        Vector2 pspot = pickPrefabSpot(chosen, r.Next(9), blockedZones, r);
                        if (allowed(pspot, chosen.exclusionRadius, blockedZones))
                        {
                            DeadZone z = new DeadZone();
                            z.position = pspot;
                            z.range = chosen.exclusionRadius;
                            blockedZones.Add(z);
                            toSpawn.Enqueue(chosen);
                            prefabSpots.Enqueue(new Vector3(pspot, (float)r.NextDouble() * MathHelper.TwoPi));
                        }
                        //else
                        //{
                        //    placementFailures++;
                        //}

                        if(prefabExclusives != null && prefabExclusives.Contains(chosen))
                        {
                            while(candidates.Contains(chosen))
                            {
                                candidates.Remove(chosen);
                            }
                        }
                        else
                        {
                            candidates.RemoveAt(pickSpot);
                        }
                        //chosen.load();
                        //placePrefab(t, chosen, r.Next(9), (float)(r.NextDouble() * MathHelper.TwoPi));
                    }
                }

                //foreach (Prefab p in toSpawn)
                //{
                //    p.load();
                //}
                
                List<string> assetsNeeded = new List<string>();
                //List<string> addedBatches = new List<string>();
                //List<int> prefabIndexOffsets = new List<int>();

                foreach(string k in textureList[sourceBatch].Keys)
                {
                    assetsNeeded.Add(k);
                }

                //count textures used by prefabs with unique texture batches
                bool wrecksAdded = false;

                //first add wreck textures if used at all
                foreach (Prefab p in toSpawn)
                {
                    if(p.genFlagNames != null)
                    {
                        foreach (string s in p.genFlagNames)
                        {
                            string n = s.ToLower();
                            if (n == "randwreck" || n == "maybewreck" || n.Contains("pickwreck") || n.Contains("maybewreckgrp"))
                            {
                                wrecksAdded = true;
                                foreach (string k in textureList[wreckBatch].Keys)
                                {
                                    if (!assetsNeeded.Contains(k))
                                    {
                                        assetsNeeded.Add(k);
                                    }
                                }
                                break;
                            }
                        }
                        if (wrecksAdded)
                        {
                            break;
                        }
                    }
                }

                //then add up all unique textures used by prefabs
                foreach (Prefab p in toSpawn)
                {
                    if(p.itemDifs != null)
                    {
                        for(int i = 0; i < p.itemDifs.Length; i++)
                        {
                            if(p.items[i] != null && p.items[i].Count > 0)
                            {
                                string s = p.itemDifs[i];
                                if (!textureList[sourceBatch].ContainsKey(s) && (!wrecksAdded || !textureList[wreckBatch].ContainsKey(s)) && !assetsNeeded.Contains(s))
                                {
                                    assetsNeeded.Add(s);
                                }
                            }
                        }
                    }
                    //if(!wrecksAdded && p.genFlagNames != null)
                    //{
                    //    foreach(string s in p.genFlagNames)
                    //    {
                    //        string n = s.ToLower();
                    //        if(n == "randwreck" || n == "maybewreck" || n.Contains("pickwreck") || n.Contains("maybewreckgrp"))
                    //        {
                    //            wrecksAdded = true;
                    //            foreach (string k in textureList[wreckBatch].Keys)
                    //            {
                    //                if (!assetsNeeded.Contains(k))
                    //                {
                    //                    assetsNeeded.Add(k);
                    //                }
                    //            }
                    //            break;
                    //        }
                    //    }
                    //}
                    //if (p.itemSourceBatch != sourceBatch)
                    //{
                    //    uniqueCount += p.itemDifs.Length;
                    //}
                }

                //make a list of the correct size
                //SectorTerrainList t = new SectorTerrainList(uniqueCount);
                SectorTerrainList t = new SectorTerrainList(assetsNeeded.Count + itemTypeCount);

                //set up the list for our default terrain items
                int c = 0;
                while (c < itemTypeCount)
                {
                    t.setCategory(c, sourceBatch, c);
                    c++;
                }

                //make a new entry for every asset used by a prefab
                for(int i = 0; i < assetsNeeded.Count; i++)
                {
                    string source = findSourceBatch(assetsNeeded[i]);
                    if (source != null)
                    {
                        t.setCategory(c, source, textureList[source][assetsNeeded[i]]);
                        c++;
                    }
                }

                //if(toSpawn.Count() < minPrefabs)
                //{
                //    throw new Exception(toSpawn.Count().ToString() + "\n" + minPrefabs.ToString() + "\n" + placementFailures.ToString() + "\n" + prefabs.Count().ToString());
                //}
                //actually place the damn prefabs into the list
                int namnum = 0;
                while (toSpawn.Count > 0)
                {
                    //int fabdexoff = prefabIndexOffsets[i];
                    //float fabRotation = (float)r.NextDouble() * MathHelper.TwoPi;
                    Vector3 threeSpot = prefabSpots.Dequeue();
                    Matrix m = Matrix.CreateRotationZ(threeSpot.Z);
                    Vector2 offSpot = new Vector2(threeSpot.X, threeSpot.Y);
                    Prefab fab = toSpawn.Dequeue();
                    if (fab.items != null)
                    {
                        for (int b = 0; b < fab.items.Length; b++)
                        {
                            //look up the texture batch to use
                            string s = findSourceBatch(fab.itemDifs[b]);
                            if (s != null)
                            {
                                //look up the texture to use
                                int sourceDex = textureList[s][fab.itemDifs[b]];
                                //look up what category that combo is using in this terrain list
                                int category = t.getCategory(s, sourceDex);
                                List<TerrainItemTeplate> items = fab.items[b];
                                foreach (TerrainItemTeplate item in items)
                                {
                                    TerrainItemTeplate newItem = new TerrainItemTeplate();
                                    newItem.position = Vector2.Transform(item.position, m) + offSpot;
                                    newItem.rotation = item.rotation + threeSpot.Z;
                                    t.addItem(category, newItem);
                                }
                            }
                        }
                    }
                    if (fab.pointsOfInterest != null && fab.poeNames != null)
                    {
                        Vector2[] transPoints = new Vector2[fab.pointsOfInterest.Length];
                        for (int b = 0; b < fab.pointsOfInterest.Length; b++)
                        {
                            transPoints[b] = Vector2.Transform(fab.pointsOfInterest[b], m) + offSpot;
                            //if (fab.poeNames[b] != null && fab.poeNames[b] != "")
                            //{
                            //    t.addPointsOInterest()
                            //}
                        }
                        string[] transValues = new string[fab.poeNames.Length];
                        for(int p = 0; p < fab.poeNames.Length; p++)
                        {
                            string nam = fab.poeNames[p];
                            if(nam.Contains('#'))
                            {
                                nam = nam.Replace("#", "#" + namnum.ToString());
                            }
                            transValues[p] = nam;
                        }

                        t.addPointsOInterest(transPoints, transValues);
                    }
                    if (fab.generationFlags != null && fab.genFlagNames != null)
                    {
                        Dictionary<string, bool> groupRandoms = new Dictionary<string, bool>();
                        Dictionary<string, List<string>> groupPickOnes = new Dictionary<string, List<string>>();

                        //pregeneration pass
                        for (int b = 0; b < fab.generationFlags.Length; b++)
                        {
                            string caseNeutral = fab.genFlagNames[b].ToLower();
                            if (caseNeutral.Contains("pickroid") || caseNeutral.Contains("pickwreck"))
                            {
                                try
                                {
                                    string[] split = caseNeutral.Split('_');
                                    string category = split[1];
                                    string option = split[2];
                                    if (!groupPickOnes.ContainsKey(category))
                                    {
                                        groupPickOnes[category] = new List<string>();
                                    }
                                    if (!groupPickOnes[category].Contains(option))
                                    {
                                        groupPickOnes[category].Add(option);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        //selection
                        foreach (List<string> group in groupPickOnes.Values)
                        {
                            if (group.Count > 1)
                            {
                                string chosen = group[r.Next(group.Count)];
                                group.Clear();
                                group.Add(chosen);
                            }
                        }

                        //generation pass
                        for (int b = 0; b < fab.generationFlags.Length; b++)
                        {
                            string caseNeutral = fab.genFlagNames[b].ToLower();
                            if (caseNeutral.Contains("pickroid"))
                            {
                                try
                                {
                                    string[] split = caseNeutral.Split('_');
                                    string cat = split[1];
                                    string option = split[2];
                                    if(groupPickOnes.ContainsKey(cat))
                                    {
                                        if (groupPickOnes[cat].Contains(option))
                                        {
                                            float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                            int category = r.Next(itemTypeCount);
                                            TerrainItemTeplate item = new TerrainItemTeplate();
                                            item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                            item.rotation = rot;
                                            t.addItem(category, item);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                            if (caseNeutral.Contains("pickwreck"))
                            {
                                try
                                {
                                    string[] split = caseNeutral.Split('_');
                                    string cat = split[1];
                                    string option = split[2];
                                    if (groupPickOnes.ContainsKey(cat))
                                    {
                                        if (groupPickOnes[cat].Contains(option))
                                        {
                                            float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                            int category = t.getCategory(wreckBatch, r.Next(wreckTypeCount));
                                            TerrainItemTeplate item = new TerrainItemTeplate();
                                            item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                            item.rotation = rot;
                                            t.addItem(category, item);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                            if (caseNeutral.Contains("mayberoidgrp"))
                            {
                                try
                                {
                                    string[] split = caseNeutral.Split('_');
                                    string maybeGroup = split[1];
                                    bool doIt;
                                    if(!groupRandoms.ContainsKey(maybeGroup))
                                    {
                                        float chance = 0.3f;
                                        if(split.Length > 2)
                                        {
                                            float.TryParse(split[2], out chance);
                                        }
                                        doIt = r.NextDouble() < chance;
                                        groupRandoms[maybeGroup] = doIt;
                                    }
                                    else
                                    {
                                        doIt = groupRandoms[maybeGroup];
                                    }
                                    if(doIt)
                                    {
                                        float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                        int category = r.Next(itemTypeCount);
                                        TerrainItemTeplate item = new TerrainItemTeplate();
                                        item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                        item.rotation = rot;
                                        t.addItem(category, item);
                                    }
                                }
                                catch
                                {
                                }
                            }
                            if (caseNeutral.Contains("maybewreckgrp"))
                            {
                                try
                                {
                                    string[] split = caseNeutral.Split('_');
                                    string maybeGroup = split[1];
                                    bool doIt;
                                    if (!groupRandoms.ContainsKey(maybeGroup))
                                    {
                                        float chance = 0.3f;
                                        if (split.Length > 2)
                                        {
                                            float.TryParse(split[2], out chance);
                                        }
                                        doIt = r.NextDouble() < chance;
                                        groupRandoms[maybeGroup] = doIt;
                                    }
                                    else
                                    {
                                        doIt = groupRandoms[maybeGroup];
                                    }
                                    if (doIt)
                                    {
                                        float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                        int category = t.getCategory(wreckBatch, r.Next(wreckTypeCount));
                                        TerrainItemTeplate item = new TerrainItemTeplate();
                                        item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                        item.rotation = rot;
                                        t.addItem(category, item);
                                    }
                                }
                                catch
                                {
                                }
                            }
                            if (caseNeutral == "randroid")
                            {
                                float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                int category = r.Next(itemTypeCount);
                                TerrainItemTeplate item = new TerrainItemTeplate();
                                item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                item.rotation = rot;
                                t.addItem(category, item);
                            }
                            if (caseNeutral == "mayberoid" && r.Next(3) > 1)
                            {
                                float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                int category = r.Next(itemTypeCount);
                                TerrainItemTeplate item = new TerrainItemTeplate();
                                item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                item.rotation = rot;
                                t.addItem(category, item);
                            }
                            if (caseNeutral == "randwreck")
                            {
                                float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                int category = t.getCategory(wreckBatch, r.Next(wreckTypeCount));
                                TerrainItemTeplate item = new TerrainItemTeplate();
                                item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                item.rotation = rot;
                                t.addItem(category, item);
                            }
                            if (caseNeutral == "maybewreck" && r.Next(3) > 1)
                            {
                                float rot = (float)(r.NextDouble() * MathHelper.TwoPi);
                                int category = t.getCategory(wreckBatch, r.Next(wreckTypeCount));
                                TerrainItemTeplate item = new TerrainItemTeplate();
                                item.position = Vector2.Transform(fab.generationFlags[b], m) + offSpot;
                                item.rotation = rot;
                                t.addItem(category, item);
                            }
                        }
                    }
                    namnum++;
                }

                if (prefabs != null)
                {
                    foreach (Prefab p in prefabs)
                    {
                        p.unload();
                    }
                }
                if (manualPrefabs != null)
                {
                    foreach (Prefab p in manualPrefabs.Values)
                    {
                        p.unload();
                    }
                }

                return t;
            }
            else
            {
                return prepareList();
            }
        }

        //private void placePrefab(SectorTerrainList t, Prefab p, int placementSeed, float rotation)
        //{
        //    int tries = 0;
        //    int index = placementSeed;
        //    Vector2 spot;
        //    while (tries < 9)
        //    {
        //        if (index >= 9)
        //        {
        //            index = 0;
        //        }
        //        spot = seedSpot(index, p.exclusionRadius);
        //        if (allowed(spot, p.exclusionRadius))
        //        {

        //        }
        //        index++;
        //        tries++;
        //    }
        //}

        private Vector2 pickPrefabSpot(Prefab p, int placementSeed, List<DeadZone> blockedZones, Random rand)
        {
            int tries = 0;
            int index = placementSeed;
            Vector2 spot = Vector2.Zero;
            int attempts = 16 + (blockedZones.Count * 3);
            float overlap = 1000000;
            while (tries < attempts)
            {
                if (index >= 9)
                {
                    index = 0;
                }
                Vector2 s = seedSpot(index, p.exclusionRadius, rand);
                if (allowed(s, p.exclusionRadius, blockedZones))
                {
                    return s;
                }
                else
                {
                    //calculate how bad it was
                    float bad = howBad(s, p.exclusionRadius, blockedZones);

                    //save it if it was less bad
                    if (bad < overlap)
                    {
                        spot = s;
                        overlap = bad;
                    }
                }
                index++;
                tries++;
            }
            return spot;
        }

        private Vector2 seedSpot(int placementID, float exclusionRadius, Random rand)
        {
            Vector2 spot = new Vector2();
            switch (placementID)
            {
                case 0:
                    {
                        spot.X = -60000 + exclusionRadius;
                        spot.Y = -60000 + exclusionRadius;
                    }
                    break;
                case 1:
                    {
                        spot.X = 0;
                        spot.Y = -60000 + exclusionRadius;
                    }
                    break;
                case 2:
                    {
                        spot.X = 60000 - exclusionRadius;
                        spot.Y = -60000 + exclusionRadius;
                    }
                    break;
                case 3:
                    {
                        spot.X = -60000 + exclusionRadius;
                        spot.Y = 0;
                    }
                    break;
                case 4:
                    {
                        spot.X = 0;
                        spot.Y = 0;
                    }
                    break;
                case 5:
                    {
                        spot.X = 60000 - exclusionRadius;
                        spot.Y = 0;
                    }
                    break;
                case 6:
                    {
                        spot.X = -60000 + exclusionRadius;
                        spot.Y = 60000 - exclusionRadius;
                    }
                    break;
                case 7:
                    {
                        spot.X = 0;
                        spot.Y = 60000 - exclusionRadius;
                    }
                    break;
                case 8:
                    {
                        spot.X = 60000 - exclusionRadius;
                        spot.Y = 60000 - exclusionRadius;
                    }
                    break;
            }
            spot.X += (float)((rand.NextDouble() - 0.5) * 40000);
            spot.Y += (float)((rand.NextDouble() - 0.5) * 40000);
            return spot;
        }

        public void addClouds(float density, string type, SectorTerrainList t, float noiseSample, int gridx, int gridy)
        {
            if(density == 0)
            {
                return;
            }

            float threshold = 1.5f;//noise sample gotta be above this to register
            if(density > 0)
            {
                threshold = 2 - (density * 0.5f);//noise sample gotta be above this to register
            }
            else
            {
                float sample = NOISE.GetNoise(gridx, gridy, 42);
                sample -= 0.1f;//default sessions have light cloud coverage
                if(sample < 0.1)
                {
                    return;
                }
                threshold = 2 - (sample * 0.5f);//noise sample gotta be above this to register
            }

            float seed = noiseSample * 100000;
            Random rand;
            if (actuallyRandom)
            {
                rand = new Random();
            }
            else
            {
                rand = new Random((int)(seed));
            }

            float scale = 0.25f;
            float quarterScale = scale / 8;
            int samples = 400;

            float gapsize = 200000 / samples;
            float biggapsize = gapsize * 1.3f;
            float halfGapsize = gapsize / 2;

            List<Vector3> clouds = new List<Vector3>();

            //int Zpos1 = rand.Next();
            //int Zpos2 = rand.Next() + Zpos1 + 200;

            //place circles at bright spots in the noise
            for (int y = 0; y < samples; y++)
            {
                for (int x = 0; x < samples; x++)
                {
                    //float cloudSample = NOISE.GetNoise(x * scale, y * scale, Zpos1);
                    //cloudSample += NOISE.GetNoise(x * quarterScale, y * quarterScale, Zpos2);
                    float cloudSample = NOISE.GetNoise(x * scale, y * scale, 10);
                    cloudSample += NOISE.GetNoise(x * quarterScale, y * quarterScale, 200);

                    if (cloudSample > threshold)
                    {
                        Vector3 poe = new Vector3((x * gapsize) + (float)((rand.NextDouble() * 200) - 100) - 100000, (y * gapsize) + (float)((rand.NextDouble() * 200) - 100) - 100000, biggapsize);
                        clouds.Add(poe);
                    }
                    else if(cloudSample > threshold - 0.1 && rand.Next(3) > 1)
                    {
                        Vector3 poe = new Vector3((x * gapsize) + (float)((rand.NextDouble() * 200) - 100) - 100000, (y * gapsize) + (float)((rand.NextDouble() * 200) - 100) - 100000, gapsize);
                        clouds.Add(poe);
                    }
                    else if (cloudSample > threshold - 0.2 && rand.Next(6) > 1)
                    {
                        Vector3 poe = new Vector3((x * gapsize) + (float)((rand.NextDouble() * 200) - 100) - 100000, (y * gapsize) + (float)((rand.NextDouble() * 200) - 100) - 100000, halfGapsize);
                        clouds.Add(poe);
                    }
                }
            }

            //replace points of interest for this session because we are debugging so fuck it
            Vector2[] pointsOfInterest = new Vector2[clouds.Count];
            string[] valuesOfInterest = new string[clouds.Count];

            for (int i = 0; i < clouds.Count; i++)
            {
                pointsOfInterest[i] = new Vector2(clouds[i].X, clouds[i].Y);
                valuesOfInterest[i] = "cloud_" + type + "_" + ((int)clouds[i].Z).ToString();
            }

            t.addPointsOInterest(pointsOfInterest, valuesOfInterest);
        }

        /*
        public void addClouds(float density, string type, SectorTerrainList t, float noiseSample, int gridx, int gridy)
        {
            if (density == 0)
            {
                return;
            }

            //Get how many epicenters we are going to create based on density and bit of randomness




            t.addPointsOInterest(pointsOfInterest, valuesOfInterest);
        }
        */

        public void addDeposites(SectorTerrainList t, float noiseSample, int gridx, int gridy)
        {
            if (maxDeposites == 0 && maxMiningOperations == 0)
                return;

            float seed = noiseSample * 100000;
            Random rand;
            if (actuallyRandom)
            {
                rand = new Random();
            }
            else
            {
                rand = new Random((int)(seed));
            }
            PlaceDeposites(t, rand);
        }

        public void PlaceDeposites(SectorTerrainList t, Random rand)
        {
            if (goodOreNodeSpots.Count > 0)
            {
                RandomHelper.shuffle(ref goodOreNodeSpots, rand);
            }

            if (depositeWeights != null && depositeWeights.Count > 0)
            {
                int depositeCount = 0;
                //generate how many we need
                depositeCount = rand.Next(minDeposites, maxDeposites + 1); //Max is exclusive in c# random
                DepositeType[] deposites = new DepositeType[depositeCount];
                Vector2[] depositelocations = new Vector2[depositeCount];
                string[] depositeStrings = new string[depositeCount];

                float depositeInnerOffset = 15000;
                float depositeRandomDistRange = 25000;

                float startAngel = (float)(rand.NextDouble() * Math.PI * 2);
                float angelStep = (float)(Math.PI * 2) / (float)depositeCount;
                for (int i = 0; i < depositeCount; i++)
                {
                    //pull for every deposite slot
                    deposites[i] = (DepositeType)RandomHelper.GetWeightedRandom(depositeWeights, rand);

                    if(goodOreNodeSpots.Count > 0)
                    {
                        Vector2 spot = goodOreNodeSpots.First();
                        goodOreNodeSpots.RemoveAt(0);
                        depositelocations[i] = spot;
                    }
                    else
                    {
                        //place it around the semirandomized circle
                        float currentAngel = startAngel + i * angelStep; //some randomization might be in order for angel step
                        depositelocations[i] = ToolBox.AngleToVector(currentAngel) * ((float)rand.NextDouble() * depositeRandomDistRange + depositeInnerOffset);
                    }
                    depositeStrings[i] = "deposit_" + deposites[i].ToString();
                }

                //convert data into point of interest and add them
                t.addPointsOInterest(depositelocations, depositeStrings);
            }

            if (miningOpWeights != null && miningOpWeights.Count > 0)
            {
                int depositeCount = 0;
                //generate how many we need
                depositeCount = rand.Next(minMiningOperations, maxMiningOperations + 1); //Max is exclusive in c# random
                DepositeType[] deposites = new DepositeType[depositeCount];
                Vector2[] depositelocations = new Vector2[depositeCount];
                string[] depositeStrings = new string[depositeCount];

                float depositeInnerOffset = 32000;
                float depositeRandomDistRange = 8000;

                float startAngel = (float)(rand.NextDouble() * Math.PI * 2);
                float angelStep = (float)(Math.PI * 2) / (float)depositeCount;
                for (int i = 0; i < depositeCount; i++)
                {
                    //pull for every deposite slot
                    deposites[i] = (DepositeType)RandomHelper.GetWeightedRandom(miningOpWeights, rand);

                    if (goodOreNodeSpots.Count > 0)
                    {
                        Vector2 spot = goodOreNodeSpots.First();
                        goodOreNodeSpots.RemoveAt(0);
                        depositelocations[i] = spot;
                    }
                    else
                    {
                        //place it around the semirandomized circle
                        float currentAngel = startAngel + i * angelStep; //some randomization might be in order for angel step
                        depositelocations[i] = ToolBox.AngleToVector(currentAngel) * ((float)rand.NextDouble() * depositeRandomDistRange + depositeInnerOffset);
                    }

                    depositeStrings[i] = "deposit_" + deposites[i].ToString();
                }

                //convert data into point of interest and add them
                t.addPointsOInterest(depositelocations, depositeStrings);
            }




        }

        public SectorTerrainList generate(byte generationFlags, float noiseSample, int gridx, int gridy)
        {
            //prepareList();

            List<DeadZone> blockedZones = new List<DeadZone>();
            SectorTerrainList t = generatePrefabs(noiseSample, blockedZones);

            switch (style)
            {
                case GenerationStyle.even_distribution:
                    evenDist(t, generationFlags, noiseSample, blockedZones);
                    break;
                case GenerationStyle.variable_density:
                    varDensDist(t, generationFlags, noiseSample, blockedZones);
                    break;
                case GenerationStyle.clumps:
                    clumpDist(t, generationFlags, noiseSample, blockedZones);
                    break;
                case GenerationStyle.doughnuts:
                    doughnutDist(t, generationFlags, noiseSample, blockedZones);
                    break;
                case GenerationStyle.tiny_sector:
                    tinySectorDist(t, generationFlags, noiseSample, blockedZones);
                    break;
            }
            addClouds(environmentCloudDensity, "e", t, noiseSample, gridx, gridy);
            addClouds(plasmaCloudDensity, "p", t, noiseSample, gridx, gridy);

            addDeposites(t, noiseSample, gridx, gridy);
            //return evenDist(t, generationFlags, noiseSample, blockedZones);
            return t;
        }

        private SectorTerrainList prepareList()
        {
            SectorTerrainList t = new SectorTerrainList(itemTypeCount);

            for (int i = 0; i < itemTypeCount; i++)
            {
                t.setCategory(i, sourceBatch, i);
            }

            return t;
        }

        //private SectorTerrainList preparePrefabTerrainList(List<Prefab> prefabs)
        //{
        //    int uniqueCount = itemTypeCount;

        //    List<string> addedBatches = null;

        //    //do stuff
        //    foreach (Prefab p in prefabs)
        //    {
        //        if (p.itemSourceBatch != sourceBatch)
        //        {
        //        }
        //    }

        //    SectorTerrainList t = new SectorTerrainList(uniqueCount);

        //    for (int i = 0; i < itemTypeCount; i++)
        //    {
        //        t.setCategory(i, sourceBatch, i);
        //    }

        //    return t;
        //}
    }
}
