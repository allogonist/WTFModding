using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using WaywardExtensions;
using System.IO;
using System;

namespace PrefabEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch batch;
        Effect spriteBasic;

        private KeyboardState oldState;
        private MouseState oldMouse;
        private Matrix view, project;
        private Vector3 cameraPos, cameraTarget;
        private Vector2 mousePos, mouseUIPos;
        private float aspectRatio;
        private Plane shipPlane;
        private float brushBarExpansion = 0;
        private int brushBarScroll = 0;
        private int maxScroll;
        private Texture2D white;
        private Matrix m = Matrix.CreateRotationZ((float)Math.PI / 2);
        private SpriteFont font;
        //private Vector2[] brushIconSpots;
        private Item[] brushes;
        private string[] brushNames;
        private Rectangle clickPos;
        private List<Item> allItems = null;
        private Dictionary<string, List<Item>> items;
        private List<Item> pointsOfInterest;
        private List<Item> generationFlags;
        private Item dragging = null;
        private Item rotating = null;
        private float startRot;
        private float initialRot;
        private Vector2 dragOffset;
        private float constantScale;
        private Color selectGlow = new Color(0, 128, 0, 128);
        private Color selectBoxGlow = new Color(0, 128, 0, 64);
        private Vector2 cameraDragStart;
        private Vector2 mouseDragStart;
        private Rectangle worldClickRect;
        //private bool slidingOut = false;
        private float scrollWide = 0;
        static public EffectTechnique spriteBasicPixelNO;

        private Random rand = new Random();
        private Rectangle horizRule, vertRule;
        private Vector2 horizOrigin, vertOrigin;

        private string debug = "";
        private string help = "F1 - Save    ENTER - Create Point of Interest";
        private string pointName = "";
        private bool pointVisible = false;
        private Item offerPoint = null;
        private string flagName = "";
        private bool flagVisible = false;
        private Item offerFlag = null;

        private int screenWidth;
        private List<Item> randomBrushes = new List<Item>();
        private float brushBarExpansion2 = 0;
        private int brushBarScroll2 = 0;
        private int maxScroll2;

        private List<Item> selected;
        private bool selectBoxVisble;
        private Rectangle selectBox;
        private Vector2 selectPreviewDrag;
        //private int every3 = 0;

        private int placeMany = 0;
        private Item paintnext = null;
        //private bool manyFlag = false;

        const int undoCount = 10;
        private float radius = 0;
        private Prefab[] undoList;

        const int sideGap = 20;
        const float sideAnimSpeed = 600;
        //const string fabFileOutPath = "\\neutralStation1.pfb";
        //const string fabFileOutPath = "\\abndnRoidStn.pfb";
        //const string fabFileOutPath = "\\abndnRoidStn1.pfb";
        //const string fabFileOutPath = "\\abndnRoidStn2.pfb";
        //const string fabFileOutPath = "\\abndnRoidStn3.pfb";
        //const string fabFileOutPath = "\\pirateRoidStn0.pfb";
        //const string fabFileOutPath = "\\randomJunk.pfb";
        //const string fabFileOutPath = "\\orangeDynamic5.pfb";
        //const string fabFileOutPath = "\\reddynamic4.pfb";
        //const string fabFileOutPath = "\\pcove.pfb";
        public string fabFileOutPath = "\\jerkville2.pfb";
        const string brushDirectory = "/brushes/";
        const float maxScale = 120000f;
        const float minScale = 250f;
        const float scrollFactor = 0.00065f;
        const float panFactor = 0.6f;
        const int sideWide = 50;
        const float brushScaleDivisor = 8;

        const float preciseDistBetweenRandomItems = 200;
        const float distBetweenRandomItems = 900;
        const float distplaceFromMouse = 300;

        private bool flipper = false;
        
        public System.Windows.Forms.Form winForm;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1200;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);


            winForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);

            cameraPos = new Vector3(0.0f, 0.0f, 5000);
            cameraTarget = new Vector3(0.0f, 0.0f, 0);
            mousePos = new Vector2();
            shipPlane = new Plane(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0));

            allItems = new List<Item>();
            items = new Dictionary<string, List<Item>>();
            pointsOfInterest = new List<Item>();
            generationFlags = new List<Item>();
            selected = new List<Item>();

            undoList = new Prefab[undoCount];
            for (int i = 0; i < undoCount; i++)
            {
                undoList[i] = null;
            }

            horizRule = new Rectangle(0, 0, 1000, 1);
            vertRule = new Rectangle(0, 0, 1, 1000);
            horizOrigin = new Vector2(500, 0.5f);
            vertOrigin = new Vector2(0.5f, 500);

            winForm.KeyPress += Window_TextInput;

            CONTENT_HELPER.Device = GraphicsDevice;
            aspectRatio = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            screenWidth = GraphicsDevice.Viewport.Width;
            base.Initialize();
        }

        private void exportToFile()
        {
            System.Windows.Forms.SaveFileDialog openFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            openFileDialog1.Filter = "Prefab files|*.pfb";
            openFileDialog1.Title = "Select a prefab";
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fabFileOutPath = openFileDialog1.FileName;
                winForm.Text = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                //spawnFlotilla(openFileDialog1.FileName);
            }
            string path = fabFileOutPath;
            Prefab p = new Prefab();
            p.exclusionRadius = radius;
            p.itemDifs = new string[items.Keys.Count];
            p.items = new List<TerrainItemTeplate>[items.Keys.Count];
            int c = 0;
            foreach (string k in items.Keys)
            {
                List<Item> batch = items[k];
                p.itemDifs[c] = k;
                List<TerrainItemTeplate> nacho = new List<TerrainItemTeplate>();
                foreach (Item i in batch)
                {
                    nacho.Add(i.item);
                }
                p.items[c] = nacho;
                c++;
            }
            p.poeNames = new string[pointsOfInterest.Count];
            p.pointsOfInterest = new Vector2[pointsOfInterest.Count];
            c = 0;
            foreach (Item poe in pointsOfInterest)
            {
                p.poeNames[c] = poe.name;
                p.pointsOfInterest[c] = poe.position;
                c++;
            }
            p.genFlagNames = new string[generationFlags.Count];
            p.generationFlags = new Vector2[generationFlags.Count];
            c = 0;
            foreach (Item poe in generationFlags)
            {
                p.genFlagNames[c] = poe.name;
                p.generationFlags[c] = poe.position;
                c++;
            }
            p.saveData(path);
        }

        private void ImportFromFile()
        {
            //string path = Directory.GetCurrentDirectory() + fabFileOutPath;
            string path = fabFileOutPath;
            if (File.Exists(path))
            {
                Prefab p = new Prefab(path);
                p.load();
                int c = 0;
                foreach (string dif in p.itemDifs)
                {
                    if (items.ContainsKey(dif))
                    {
                        int difdex = 0;
                        while(difdex < brushNames.Length)
                        {
                            if(brushNames[difdex] == dif)
                            {
                                break;
                            }
                            difdex++;
                        }
                        foreach (TerrainItemTeplate t in p.items[c])
                        {
                            Item i = new Item(dif, brushes[difdex].art, 1);
                            i.position = t.position;
                            i.rotation = t.rotation;
                            items[dif].Add(i);
                            allItems.Add(i);
                        }
                    }
                    c++;
                }
                if (p.pointsOfInterest != null && p.poeNames != null)
                {
                    c = 0;
                    foreach (string interest in p.poeNames)
                    {
                        Item i = new Item(interest, true);
                        i.position = p.pointsOfInterest[c];
                        allItems.Add(i);
                        pointsOfInterest.Add(i);
                        c++;
                    }
                }
                if (p.generationFlags != null && p.genFlagNames != null)
                {
                    c = 0;
                    foreach (string interest in p.genFlagNames)
                    {
                        Item i = new Item(interest, false);
                        i.position = p.generationFlags[c];
                        allItems.Add(i);
                        generationFlags.Add(i);
                        c++;
                    }
                }
            }
        }

        private void saveUndoStep()
        {
            Prefab p = new Prefab();
            p.exclusionRadius = radius;
            p.itemDifs = new string[items.Keys.Count];
            p.items = new List<TerrainItemTeplate>[items.Keys.Count];
            int c = 0;
            foreach (string k in items.Keys)
            {
                List<Item> batch = items[k];
                p.itemDifs[c] = k;
                List<TerrainItemTeplate> nacho = new List<TerrainItemTeplate>();
                foreach (Item i in batch)
                {
                    nacho.Add(i.item);
                }
                p.items[c] = nacho;
                c++;
            }
            p.poeNames = new string[pointsOfInterest.Count];
            p.pointsOfInterest = new Vector2[pointsOfInterest.Count];
            c = 0;
            foreach (Item poe in pointsOfInterest)
            {
                p.poeNames[c] = poe.name;
                p.pointsOfInterest[c] = poe.position;
                c++;
            }
            p.genFlagNames = new string[generationFlags.Count];
            p.generationFlags = new Vector2[generationFlags.Count];
            c = 0;
            foreach (Item poe in generationFlags)
            {
                p.genFlagNames[c] = poe.name;
                p.generationFlags[c] = poe.position;
                c++;
            }
            for (int i = undoCount - 1; i > 0; i--)
            {
                undoList[i] = undoList[i - 1];
            }
            undoList[0] = p;

            calculateRadius();
        }

        private void undo()
        {
            if (undoList[1] != null)
            {
                selectBoxVisble = false;
                dragging = null;
                allItems.Clear();
                foreach (string k in items.Keys)
                {
                    items[k].Clear();
                }
                generationFlags.Clear();
                pointsOfInterest.Clear();
                selected = new List<Item>();


                Prefab p = undoList[1];
                int c = 0;
                foreach (string dif in p.itemDifs)
                {
                    if (items.ContainsKey(dif))
                    {
                        int difdex = 0;
                        while (difdex < brushNames.Length)
                        {
                            if (brushNames[difdex] == dif)
                            {
                                break;
                            }
                            difdex++;
                        }
                        foreach (TerrainItemTeplate t in p.items[c])
                        {
                            Item i = new Item(dif, brushes[difdex].art, 1);
                            i.position = t.position;
                            i.rotation = t.rotation;
                            items[dif].Add(i);
                            allItems.Add(i);
                        }
                    }
                    c++;
                }
                if (p.pointsOfInterest != null && p.poeNames != null)
                {
                    c = 0;
                    foreach (string interest in p.poeNames)
                    {
                        Item i = new Item(interest, true);
                        i.position = p.pointsOfInterest[c];
                        allItems.Add(i);
                        pointsOfInterest.Add(i);
                        c++;
                    }
                }
                if (p.generationFlags != null && p.genFlagNames != null)
                {
                    c = 0;
                    foreach (string interest in p.genFlagNames)
                    {
                        Item i = new Item(interest, false);
                        i.position = p.generationFlags[c];
                        allItems.Add(i);
                        generationFlags.Add(i);
                        c++;
                    }
                }
                calculateRadius();
                for (int i = 0; i < undoCount - 1; i++)
                {
                    undoList[i] = undoList[i + 1];
                }
                undoList[undoCount - 1] = null;
            }
        }

        private void calculateRadius()
        {
            radius = 0;
            foreach (Item i in allItems)
            {
                if (!i.isPointOfInterest && !i.isGenerationFlag)
                {
                    float rad = i.position.Length() + Math.Max(i.bBox.Width, i.bBox.Height);
                    if (rad > radius)
                    {
                        radius = rad;
                    }
                }
                else
                {
                    float rad = i.position.Length() + 2000;
                    if (rad > radius)
                    {
                        radius = rad;
                    }
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            batch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            spriteBasic = Content.Load<Effect>("spriteBasic");
            spriteBasicPixelNO = spriteBasic.Techniques["spriteBasicNO"];
            font = Content.Load<SpriteFont>("ForgottenSmall");
            Item.interestFont = font;
            //Item.interestArt = Content.Load<Texture2D>("POE");
            Item.interestArt = CONTENT_HELPER.readTexFromPng("POE");
            //Item.flagArt = Content.Load<Texture2D>("GenFlag");
            Item.flagArt = CONTENT_HELPER.readTexFromPng("GenFlag");
            //Item.redCircleArt = Content.Load<Texture2D>("circle_red");
            Item.redCircleArt = CONTENT_HELPER.readTexFromPng("circle_red");
            Item.interestOrigin = new Vector2(Item.interestArt.Width / 2, Item.interestArt.Height / 2);
            Item.redOrigin = new Vector2(1000, 1000);

            white = new Texture2D(GraphicsDevice, 1, 1);
            Color[] w = new Color[1];
            w[0] = Color.White;
            white.SetData(w);

            if (allItems == null)
            {
                allItems = new List<Item>();
                items = new Dictionary<string, List<Item>>();
            }

            loadBrushes();

            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "Prefab files|*.pfb";
            openFileDialog1.Title = "Select a prefab";
            openFileDialog1.Multiselect = false;
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fabFileOutPath = openFileDialog1.FileName;
                winForm.Text = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                //spawnFlotilla(openFileDialog1.FileName);
            }
            
            //string path = Directory.GetCurrentDirectory() + fabFileOutPath;
            ImportFromFile();
            calculateRadius();
            //int purged = purgeDups();
            //debug = purged.ToString() + " Duplicate items purged.";
            saveUndoStep();
        }

        private void rebuildRandomScrollBox()
        {
            maxScroll2 = 0;
            for (int i = 0; i < randomBrushes.Count; i++)
            {
                randomBrushes[i].resizePreview(scrollWide);
                maxScroll2 += (int)(randomBrushes[i].art.Height * randomBrushes[i].scale);
                maxScroll2 += sideGap;
            }
            positionBrushIcons2(brushBarExpansion2, brushBarScroll2);
        }

        private void loadBrushes()
        {
            string brushLoc = Directory.GetCurrentDirectory() + brushDirectory;
            if(!Directory.Exists(brushLoc))
            {
                Directory.CreateDirectory(brushLoc);
            }
            //Content.RootDirectory = brushLoc;
            string[] files = Directory.GetFiles(brushLoc);
            brushes = new Item[files.Length];
            //brushIconSpots = new Vector2[files.Length];
            maxScroll = 0;
            for (int i = 0; i < files.Length; i++)
            {
                //files[i] = Path.GetFileNameWithoutExtension(files[i]);
                brushes[i] = new Item(files[i], CONTENT_HELPER.readTexFromPng(files[i]), 1 / brushScaleDivisor);
                if (brushes[i].art.Width / brushScaleDivisor > scrollWide)
                {
                    scrollWide = brushes[i].art.Width / brushScaleDivisor;
                }
                if (!items.ContainsKey(files[i]))
                {
                    items[files[i]] = new List<Item>();
                }
            }
            for (int i = 0; i < brushes.Length; i++)
            {
                brushes[i].resizePreview(scrollWide);
                maxScroll += (int)(brushes[i].art.Height * brushes[i].scale);
                maxScroll += sideGap;
            }
            brushNames = files;

            positionBrushIcons(brushBarExpansion, brushBarScroll);
        }

        private int purgeDups()
        {
            int purged = 0;
            List<Item> garbage = new List<Item>();
            bool foundDups = true;
            while (foundDups)
            {
                foundDups = false;
                foreach (Item i in allItems)
                {
                    foreach (Item d in allItems)
                    {
                        if (i != d && (i.position - d.position).Length() < 1 && i.rotation == d.rotation && i.isPointOfInterest == d.isPointOfInterest && i.name == d.name)
                        {
                            garbage.Add(d);
                            foundDups = true;
                        }
                    }
                    if (foundDups)
                    {
                        break;
                    }
                }
                foreach (Item g in garbage)
                {
                    allItems.Remove(g);
                    if (items.ContainsKey(g.name))
                    {
                        items[g.name].Remove(g);
                    }
                    purged++;
                }
                garbage.Clear();
            }
            return purged;
        }

        void Window_TextInput(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            char character = e.KeyChar;
            if (character == (char)0x001b)//escape
            {
                pointName = "";
                pointVisible = false;
                offerPoint = null;
                debug = "";
                flagName = "";
                flagVisible = false;
                offerFlag = null;
                return;
            }
            else if (character == (char)13)//enter
            {
                if (pointVisible && pointName != "")
                {
                    offerPoint = new Item(pointName, true);
                    offerPoint.position = new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, 50);
                    pointVisible = false;
                    //manyFlag = false;
                }
                else if(flagVisible && flagName != "")
                {
                    offerFlag = new Item(flagName, false);
                    offerFlag.position = new Vector2(GraphicsDevice.Viewport.Width / 2 + 100, 50);
                    flagVisible = false;
                    //manyFlag = true;
                }
                //{
                //    pointVisible = true;
                //    debug = "Point of interest:";
                //}
            }
            else
            {
                if (pointVisible)
                {
                    if (character != 0x0009 && (char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) || char.IsPunctuation(character)))
                    {
                        pointName += character;
                    }
                    else if (character == 8)
                    {
                        if (pointName.Length != 0)
                        {
                            pointName = pointName.Substring(0, (pointName.Length - 1));
                        }
                    }
                }
                if (flagVisible)
                {
                    if (character != 0x0009 && (char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) || char.IsPunctuation(character)))
                    {
                        flagName += character;
                    }
                    else if (character == 8)
                    {
                        if (flagName.Length != 0)
                        {
                            flagName = flagName.Substring(0, (flagName.Length - 1));
                        }
                    }
                }
            }
        }


        void positionBrushIcons(float expandPercent, int scrollSpot)
        {
            Vector2 spot = new Vector2(0, scrollSpot);
            foreach (Item item in brushes)
            {
                spot.Y += item.art.Height * (item.scale / 2);// 25% size divided in half
                spot.Y += sideGap;
                spot.X = -item.art.Width * (item.scale / 2);
                spot.X += item.art.Width * expandPercent * item.scale;
                item.position = spot;
                spot.Y += item.art.Height * (item.scale / 2);
            }
        }
        void positionBrushIcons2(float expandPercent, int scrollSpot)
        {
            Vector2 spot = new Vector2(screenWidth, scrollSpot);
            foreach (Item item in randomBrushes)
            {
                spot.Y += item.art.Height * (item.scale / 2);// 25% size divided in half
                spot.Y += sideGap;
                spot.X = screenWidth + item.art.Width * (item.scale / 2);
                spot.X -= item.art.Width * expandPercent * item.scale;
                item.position = spot;
                spot.Y += item.art.Height * (item.scale / 2);
            }
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            //any code to be executed when the screen gets resized
            //int height = this.GraphicsDevice.Viewport.Height;
            //int width = this.GraphicsDevice.Viewport.Width;
            //int height1 = this.GraphicsDevice.DisplayMode.Height;
            //int width1 = this.GraphicsDevice.DisplayMode.Width;
            if (graphics.PreferredBackBufferWidth != Window.ClientBounds.Width || graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
            {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();


                aspectRatio = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
                screenWidth = GraphicsDevice.Viewport.Width;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState newState = Keyboard.GetState();
            MouseState newMouse = Mouse.GetState();
            mouseUIPos = new Vector2(newMouse.X, newMouse.Y);
            clickPos = new Rectangle(newMouse.X, newMouse.Y, 1, 1);

            if(!IsActive)
            {
                return;
            }

            help = "[F1]-Save  [P]-Create Point of Interest  [G]-Create generation flag  [Tab]-Draw recent entry\nCurrent Radius = " + radius.ToString();
            help += "\nCamera = " + cameraPos.ToString();
            if(placeMany == 1)
            {
                help += "\nDrawing randomized items! DRAWING THEM!!";
            }
            if (placeMany == 2)
            {
                help += "\nDrawing PRECISE randomized items! DRAWING THEM!!";
            }

            if ((newMouse.X > oldMouse.X && newMouse.X > (scrollWide * brushBarExpansion)) || newMouse.X > scrollWide)
            {
                if (brushBarExpansion > 0)
                {
                    brushBarExpansion -= 4f * elapsed;
                }
                else
                {
                    brushBarExpansion = 0;
                }
            }
            else
            {
                if (newMouse.X <= sideWide)
                {
                    brushBarExpansion = 1;
                }
                else
                {
                    float percentClose = (float)(sideWide - (newMouse.X - sideWide)) / (float)sideWide;

                    //brushBarExpansion += percentClose * elapsed;
                    if (percentClose > brushBarExpansion)
                    {
                        brushBarExpansion = percentClose;
                    }
                }
            }

            //right side box
            if ((newMouse.X < oldMouse.X && newMouse.X < screenWidth - (scrollWide * brushBarExpansion)) || newMouse.X < screenWidth - scrollWide)
            {
                if (brushBarExpansion2 > 0)
                {
                    brushBarExpansion2 -= 4f * elapsed;
                }
                else
                {
                    brushBarExpansion2 = 0;
                }
            }
            else
            {
                if (newMouse.X >= screenWidth - sideWide)
                {
                    brushBarExpansion2 = 1;
                }
                else
                {
                    float distFromSide = screenWidth - newMouse.X;
                    float percentClose = (float)(sideWide - (distFromSide - sideWide)) / (float)sideWide;
                    
                    if (percentClose > brushBarExpansion2)
                    {
                        brushBarExpansion2 = percentClose;
                    }
                }
            }

            if (newMouse.X <= (scrollWide * brushBarExpansion))
            {
                if (newMouse.ScrollWheelValue != oldMouse.ScrollWheelValue)
                {
                    brushBarScroll += (newMouse.ScrollWheelValue - oldMouse.ScrollWheelValue) / 4;
                    brushBarScroll = MathHelper.Clamp(brushBarScroll, graphics.GraphicsDevice.Viewport.Height - maxScroll, 0);
                }
            }
            else if(newMouse.X > screenWidth - (scrollWide * brushBarExpansion2))
            {
                if (newMouse.ScrollWheelValue != oldMouse.ScrollWheelValue)
                {
                    brushBarScroll2 += (newMouse.ScrollWheelValue - oldMouse.ScrollWheelValue) / 4;
                    brushBarScroll2 = MathHelper.Clamp(brushBarScroll2, graphics.GraphicsDevice.Viewport.Height - maxScroll2, 0);
                }
            }
            else
            {
                if (newMouse.ScrollWheelValue != oldMouse.ScrollWheelValue)
                {
                    cameraPos.Z -= (newMouse.ScrollWheelValue - oldMouse.ScrollWheelValue) * scrollFactor * cameraPos.Z;
                    cameraPos.Z = MathHelper.Clamp(cameraPos.Z, minScale, maxScale);
                }
            }

            if (!pointVisible && !flagVisible)
            {
                if (newState.IsKeyDown(Keys.Up) || newState.IsKeyDown(Keys.W))
                {
                    cameraPos.Y -= elapsed * panFactor * cameraPos.Z;
                }

                if (newState.IsKeyDown(Keys.Down) || newState.IsKeyDown(Keys.S))
                {
                    cameraPos.Y += elapsed * panFactor * cameraPos.Z;
                }

                if (newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.A))
                {
                    cameraPos.X -= elapsed * panFactor * cameraPos.Z;
                }

                if (newState.IsKeyDown(Keys.Right) || newState.IsKeyDown(Keys.D))
                {
                    cameraPos.X += elapsed * panFactor * cameraPos.Z;
                }
            }
            else
            {
                if (newState.IsKeyDown(Keys.Up))
                {
                    cameraPos.Y -= elapsed * panFactor * cameraPos.Z;
                }

                if (newState.IsKeyDown(Keys.Down))
                {
                    cameraPos.Y += elapsed * panFactor * cameraPos.Z;
                }

                if (newState.IsKeyDown(Keys.Left))
                {
                    cameraPos.X -= elapsed * panFactor * cameraPos.Z;
                }

                if (newState.IsKeyDown(Keys.Right))
                {
                    cameraPos.X += elapsed * panFactor * cameraPos.Z;
                }
            }

            if (newState.IsKeyUp(Keys.P) && oldState.IsKeyDown(Keys.P) && !flagVisible)
            {
                if (!pointVisible)
                {
                    pointVisible = true;
                    flagVisible = false;
                    debug = "Point of interest:";
                }
                //else if (pointName != "")
                //{
                //    offerPoint = new Item(pointName, true);
                //    offerPoint.position = new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, 50);
                //    pointVisible = false;
                //}
            }

            if (newState.IsKeyUp(Keys.G) && oldState.IsKeyDown(Keys.G) && !pointVisible)
            {
                if (!flagVisible)
                {
                    pointVisible = false;
                    flagVisible = true;
                    debug = "Generation Flag:";
                }
                //else if (flagName != "")
                //{
                //    offerFlag = new Item(flagName, false);
                //    offerFlag.position = new Vector2(GraphicsDevice.Viewport.Width / 2 + 100, 50);
                //    flagVisible = false;
                //}
            }



            if (!newState.IsKeyDown(Keys.F1) && oldState.IsKeyDown(Keys.F1))
            {
                exportToFile();
                debug = "Saved to disk at:" + DateTime.Now.ToString();
            }

            if (!newState.IsKeyDown(Keys.Tab) && oldState.IsKeyDown(Keys.Tab))
            {
                placeMany++;
                if(placeMany > 2)
                {
                    placeMany = 0;
                }
            }

            if (!newState.IsKeyDown(Keys.Escape) && oldState.IsKeyDown(Keys.Escape))
            {
                placeMany = 0;
                selected.Clear();
                selectBoxVisble = false;
            }

            if (!newState.IsKeyDown(Keys.Z) && oldState.IsKeyDown(Keys.Z) && (newState.IsKeyDown(Keys.LeftControl) || newState.IsKeyDown(Keys.RightControl)))
            {
                undo();
            }

            if (!newState.IsKeyDown(Keys.A) && oldState.IsKeyDown(Keys.A) && (newState.IsKeyDown(Keys.LeftControl) || newState.IsKeyDown(Keys.RightControl)))
            {
                selected.Clear();
                foreach (Item i in allItems)
                {
                    selected.Add(i);
                }
            }

            if (!newState.IsKeyDown(Keys.Delete) && oldState.IsKeyDown(Keys.Delete))
            {
                foreach (Item i in selected)
                {
                    allItems.Remove(i);
                    pointsOfInterest.Remove(i);
                    generationFlags.Remove(i);
                    if (items.ContainsKey(i.name))
                    {
                        items[i.name].Remove(i);
                    }
                }
                selected.Clear();
                saveUndoStep();
            }

            cameraTarget = cameraPos;
            cameraTarget.Z = 0;

            view = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.Up);
            project = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1000000);

            float newMouseZ = -(newMouse.Y - GraphicsDevice.Viewport.Height);
            Vector3 mouse3posNear = GraphicsDevice.Viewport.Unproject(new Vector3(newMouse.X, newMouseZ, 0), project, view, Matrix.Identity);
            Vector3 mouse3posFar = GraphicsDevice.Viewport.Unproject(new Vector3(newMouse.X, newMouseZ, 1), project, view, Matrix.Identity);
            Ray clickRay = new Ray(mouse3posNear, Vector3.Normalize(mouse3posFar - mouse3posNear));
            float? distance = clickRay.Intersects(shipPlane);
            Vector3 mouse3pos = distance.HasValue ? clickRay.Position + clickRay.Direction * distance.Value : Vector3.Zero;
            mousePos = new Vector2(mouse3pos.X, mouse3pos.Y);

            worldClickRect = new Rectangle((int)mousePos.X, (int)mousePos.Y, 1, 1);


            if (newMouse.MiddleButton == ButtonState.Pressed && oldMouse.MiddleButton == ButtonState.Released)
            {
                cameraDragStart = new Vector2(cameraPos.X, cameraPos.Y);
                mouseDragStart = mouseUIPos;
            }
            else if (newMouse.MiddleButton == ButtonState.Pressed)
            {
                Vector2 dragDelta = (mouseDragStart - mouseUIPos) * cameraPos.Z * 0.001f;
                cameraPos.X = cameraDragStart.X + dragDelta.X;
                cameraPos.Y = cameraDragStart.Y + dragDelta.Y;
            }

            positionBrushIcons(brushBarExpansion, brushBarScroll);
            positionBrushIcons2(brushBarExpansion2, brushBarScroll2);

            //if we just drew some things
            if(placeMany != 0 && oldMouse.LeftButton == ButtonState.Pressed && newMouse.LeftButton == ButtonState.Released && randomBrushes.Count > 0)
            {
                saveUndoStep();
            }

            if (newMouse.LeftButton == ButtonState.Pressed && placeMany == 1 && dragging == null)//we clicked
            {
                flipper = !flipper;
                if (randomBrushes.Count > 0 && flipper)
                {
                    Item paint = randomBrushes[rand.Next(randomBrushes.Count)].clone();
                    Vector2 rnd = new Vector2((float)(rand.NextDouble() * 200) - 100, (float)(rand.NextDouble() * 200) - 100);
                    paint.position = mousePos + rnd;
                    int placing = 0;
                    float rangeHalf = distplaceFromMouse;
                    float rangeFull = distplaceFromMouse * 2;
                    while (placing < 1000)
                    {
                        placing += 1001;
                        foreach (Item item in allItems)
                        {
                            float r = 0;
                            if (paint.isGenerationFlag || paint.isPointOfInterest)
                            {
                                r += distBetweenRandomItems / 2;
                            }
                            else
                            {
                                r += Math.Max(paint.art.Width, paint.art.Height) / 2;
                                //r -= 30;
                            }
                            if (item.isGenerationFlag || item.isPointOfInterest)
                            {
                                r += distBetweenRandomItems / 2;
                            }
                            else
                            {
                                r += Math.Max(item.art.Width, item.art.Height) / 2;
                                //r -= 30;
                            }
                            if (Vector2.Distance(item.position, paint.position) < r)
                            {
                                rnd = new Vector2((float)(rand.NextDouble() * rangeFull) - rangeHalf, (float)(rand.NextDouble() * rangeFull) - rangeHalf);
                                paint.position = mousePos + rnd;
                                placing -= 1000;
                                rangeHalf += 10;
                                rangeFull = rangeHalf * 2;
                                break;
                            }
                        }
                    }
                    paint.rotation = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    if (paint.isGenerationFlag)
                    {
                        generationFlags.Add(paint);
                    }
                    else if (paint.isPointOfInterest)
                    {
                        pointsOfInterest.Add(paint);
                    }
                    else
                    {
                        items[paint.name].Add(paint);
                    }
                    allItems.Add(paint);
                }

                //every3++;
                //if (every3 > 5)
                //{
                //    every3 = 0;
                //}
            }
            else if (newMouse.LeftButton == ButtonState.Pressed && placeMany == 2 && dragging == null)//we clicked
            {
                if(paintnext == null && randomBrushes.Count > 0)
                {
                    paintnext = randomBrushes[rand.Next(randomBrushes.Count)].clone();
                }
                if (paintnext != null)
                {
                    Vector2 rnd = new Vector2((float)(rand.NextDouble() * 100) - 50, (float)(rand.NextDouble() * 100) - 50);
                    paintnext.position = mousePos + rnd;
                    bool notBlocked = true;
                    foreach (Item item in allItems)
                    {
                        float r = 0;
                        if (paintnext.isGenerationFlag || paintnext.isPointOfInterest)
                        {
                            r += preciseDistBetweenRandomItems / 2;
                        }
                        else
                        {
                            r += Math.Max(paintnext.art.Width, paintnext.art.Height) / 2;
                            //r -= 30;
                        }
                        if (item.isGenerationFlag || item.isPointOfInterest)
                        {
                            r += preciseDistBetweenRandomItems / 2;
                        }
                        else
                        {
                            r += Math.Max(item.art.Width, item.art.Height) / 2;
                            //r -= 30;
                        }
                        if (Vector2.Distance(item.position, paintnext.position) < r)
                        {
                            notBlocked = false;
                            break;
                        }
                    }
                    if(notBlocked)
                    {
                        paintnext.rotation = (float)(rand.NextDouble() * MathHelper.TwoPi);
                        if (paintnext.isGenerationFlag)
                        {
                            generationFlags.Add(paintnext);
                        }
                        else if (paintnext.isPointOfInterest)
                        {
                            pointsOfInterest.Add(paintnext);
                        }
                        else
                        {
                            items[paintnext.name].Add(paintnext);
                        }
                        allItems.Add(paintnext);
                        paintnext = randomBrushes[rand.Next(randomBrushes.Count)].clone();
                    }
                }
            }
            else if (newMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released && dragging == null)//we clicked
            {
                if(placeMany == 1)
                {
                }
                else
                {
                    if (offerPoint != null && clickPos.Intersects(offerPoint.bBox))
                    {
                        dragging = offerPoint.clone();
                        dragOffset = offerPoint.position - mouseUIPos;
                        //offerPoint = null;
                    }
                    if (offerFlag != null && clickPos.Intersects(offerFlag.bBox))
                    {
                        dragging = offerFlag.clone();
                        dragOffset = offerFlag.position - mouseUIPos;
                        //offerFlag = null;
                    }
                    if (dragging == null)
                    {
                        foreach (Item i in brushes)
                        {
                            if (clickPos.Intersects(i.bBox))
                            {
                                if (newState.IsKeyDown(Keys.LeftShift))
                                {
                                    randomBrushes.Add(i.clone());
                                    rebuildRandomScrollBox();
                                    brushBarExpansion2 = 1;
                                }
                                else
                                {
                                    dragging = i.clone();
                                    dragOffset = i.position - mouseUIPos;
                                }
                                break;
                            }
                        }
                    }
                    if (dragging == null)
                    {
                        foreach (Item i in randomBrushes)
                        {
                            if (clickPos.Intersects(i.bBox))
                            {
                                //dragging = i.clone();
                                //dragOffset = i.position - mouseUIPos;
                                randomBrushes.Remove(i);
                                break;
                            }
                        }
                    }
                    if (dragging == null)
                    {
                        foreach (Item i in allItems)
                        {
                            if (worldClickRect.Intersects(i.bBox))
                            {
                                if (newState.IsKeyDown(Keys.LeftControl) || newState.IsKeyDown(Keys.RightControl))
                                {
                                    selected.Clear();
                                    dragging = i.clone();
                                    dragOffset = i.position - mousePos;
                                    break;
                                }
                                else
                                {
                                    if (selected.Count > 0)
                                    {
                                        if (selected.Contains(i))
                                        {
                                            selectPreviewDrag = mousePos;
                                            dragging = i;
                                        }
                                        else
                                        {
                                            selected.Clear();
                                        }
                                    }
                                    else
                                    {
                                        dragging = i;
                                        dragOffset = i.position - mousePos;
                                        allItems.Remove(i);
                                        pointsOfInterest.Remove(i);
                                        generationFlags.Remove(i);
                                        if (items.ContainsKey(i.name))
                                        {
                                            items[i.name].Remove(i);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    if (dragging == null)
                    {
                        selectPreviewDrag = mousePos;
                        selectBoxVisble = true;
                        selectBox = new Rectangle();
                    }
                }
            }
            else if (dragging != null)
            {
                if (selected.Count == 0)
                {
                    if (newMouse.X <= (scrollWide * brushBarExpansion))
                    {
                        if (newMouse.LeftButton == ButtonState.Pressed)//we are dragging
                        {
                            dragging.position = mouseUIPos + dragOffset;
                        }
                        else//we just released
                        {
                            dragging = null;
                            saveUndoStep();
                        }
                    }
                    else if(newMouse.X >= screenWidth - (scrollWide * brushBarExpansion2))
                    {
                        if (newMouse.LeftButton == ButtonState.Pressed)//we are dragging
                        {
                            dragging.position = mouseUIPos + dragOffset;
                        }
                        else//we just released
                        {
                            randomBrushes.Add(dragging.clone());//we clone it so that references don't get screwed up when we do an undo
                            rebuildRandomScrollBox();
                            dragging = null;
                            saveUndoStep();
                        }
                    }
                    else
                    {
                        if (newMouse.LeftButton == ButtonState.Pressed)//we are dragging
                        {
                            dragging.position = mousePos + dragOffset;
                        }
                        else//we just released
                        {
                            bool isDuplicate = false;
                            foreach (Item i in allItems)
                            {
                                if (i.position == dragging.position && i.rotation == dragging.rotation && i.isPointOfInterest == dragging.isPointOfInterest && i.name == dragging.name)
                                {
                                    isDuplicate = true;
                                }
                            }
                            if (!isDuplicate)
                            {
                                if (dragging.isPointOfInterest)
                                {
                                    pointsOfInterest.Add(dragging);
                                    allItems.Add(dragging);
                                }
                                else if (dragging.isGenerationFlag)
                                {
                                    generationFlags.Add(dragging);
                                    allItems.Add(dragging);
                                }
                                else
                                {
                                    dragging.position = mousePos + dragOffset;
                                    allItems.Add(dragging);
                                    items[dragging.name].Add(dragging);
                                }
                                calculateRadius();
                                saveUndoStep();
                            }
                            dragging = null;
                        }
                    }
                }
                else
                {
                    if (newMouse.X <= (scrollWide * brushBarExpansion))
                    {
                        //do nothing if the mouse is pressed
                        if (newMouse.LeftButton != ButtonState.Pressed)//we released the mouse and trashed all selected
                        {
                            dragging = null;
                            foreach (Item i in selected)
                            {
                                allItems.Remove(i);
                                pointsOfInterest.Remove(i);
                                generationFlags.Remove(i);
                                if (items.ContainsKey(i.name))
                                {
                                    items[i.name].Remove(i);
                                }
                            }
                            selected.Clear();
                            saveUndoStep();
                        }
                    }
                    else
                    {
                        //do nothing if the mouse is pressed
                        if (newMouse.LeftButton != ButtonState.Pressed)//we placed all dragged items
                        {
                            if (mousePos != selectPreviewDrag)
                            {
                                Vector2 selectDragOff = mousePos - selectPreviewDrag;
                                foreach (Item i in selected)
                                {
                                    i.position = i.position + selectDragOff;
                                }
                                saveUndoStep();
                            }
                            dragging = null;
                        }
                    }
                }
            }
            else if(selectBoxVisble)
            {
                if (newMouse.LeftButton == ButtonState.Pressed)//we are dragging
                {
                    selectBox.X = Math.Min((int)mousePos.X, (int)selectPreviewDrag.X);
                    selectBox.Y = Math.Min((int)mousePos.Y, (int)selectPreviewDrag.Y);
                    selectBox.Width = Math.Abs((int)mousePos.X - (int)selectPreviewDrag.X);
                    selectBox.Height = Math.Abs((int)mousePos.Y - (int)selectPreviewDrag.Y);
                }
                else//we just released
                {
                    selectBoxVisble = false;
                    if(newState.IsKeyDown(Keys.LeftShift) || newState.IsKeyDown(Keys.RightShift))
                    {
                        foreach (Item i in allItems)
                        {
                            if (i.bBox.Intersects(selectBox) && !selected.Contains(i))
                            {
                                selected.Add(i);
                            }
                        }
                    }
                    else if (newState.IsKeyDown(Keys.LeftAlt) || newState.IsKeyDown(Keys.RightAlt))
                    {
                        for(int i = 0; i < selected.Count; i++)
                        {
                            if(selected[i].bBox.Intersects(selectBox))
                            {
                                selected.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        selected.Clear();
                        foreach (Item i in allItems)
                        {
                            if (i.bBox.Intersects(selectBox))
                            {
                                selected.Add(i);
                            }
                        }
                    }
                }
            }

            if (newMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released && rotating == null)//we clicked
            {
                foreach (Item i in allItems)
                {
                    if (worldClickRect.Intersects(i.bBox))
                    {
                        rotating = i;
                        initialRot = rotating.rotation;
                        Vector2 rotVec = mousePos - rotating.position;
                        startRot = VectorToAngle(rotVec);
                        break;
                    }
                }
            }
            else if (rotating != null)
            {
                Vector2 rotVec = mousePos - rotating.position;
                rotating.rotation = initialRot + VectorToAngle(rotVec) - startRot;
                if (newMouse.RightButton != ButtonState.Pressed)
                {
                    rotating = null;
                    saveUndoStep();
                }
            }

            constantScale = cameraPos.Z / 400;
            if (constantScale < 0.5f)
            {
                constantScale = 0.5f;
            }

            oldState = newState;
            oldMouse = newMouse;
            base.Update(gameTime);
        }

        private float VectorToAngle(Vector2 vector)
        {
            if (vector == Vector2.Zero)
            {
                return 0;
            }
            Vector2 v = Vector2.Transform(vector, m);

            return (float)Math.Atan2(v.Y, v.X);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBasic.Parameters["View"].SetValue(view);
            spriteBasic.Parameters["Projection"].SetValue(project);

            spriteBasic.CurrentTechnique = spriteBasicPixelNO;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, spriteBasic);

            batch.Draw(white, Vector2.Zero, horizRule, Color.White, 0, horizOrigin, constantScale, SpriteEffects.None, 0);
            batch.Draw(white, Vector2.Zero, vertRule, Color.White, 0, vertOrigin, constantScale, SpriteEffects.None, 0);

            foreach (Item item in selected)
            {
                batch.Draw(white, item.bBox, selectGlow);
            }

            int zoomRating = (int)Math.Log(cameraPos.Z / 5000);
            if(cameraPos.Z < 5000)
            {
                zoomRating = 0;
            }

            foreach (Item item in allItems)
            {
                if (item.bBox.Intersects(worldClickRect))
                {
                    batch.Draw(white, item.bBox, selectGlow);
                }
                item.draw(batch, zoomRating);
                if(item.art == null && !item.isGenerationFlag && !item.isPointOfInterest)
                {
                    allItems.Remove(item);
                    if(items.ContainsKey(item.name))
                    {
                        items[item.name].Remove(item);
                    }
                    break;
                }
            }

            if (selectBoxVisble)
            {
                batch.Draw(white, selectBox, selectBoxGlow);
            }

            batch.End();

            // TODO: Add your drawing code here
            batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            if (brushBarExpansion > 0)
            {
                foreach (Item item in brushes)
                {
                    item.draw(batch, 0);
                }
            }

            if (brushBarExpansion2 > 0)
            {
                foreach (Item item in randomBrushes)
                {
                    item.draw(batch, 0);
                }
            }

            batch.DrawString(font, debug, new Vector2(100, 0), Color.Black);
            batch.DrawString(font, help, new Vector2(400, 0), Color.Black);

            if (pointVisible)
            {
                batch.DrawString(font, pointName, new Vector2(200, 0), Color.Red);
            }

            if (flagVisible)
            {
                batch.DrawString(font, flagName, new Vector2(200, 0), Color.Red);
            }

            if (offerPoint != null)
            {
                offerPoint.draw(batch, 0);
            }

            if (offerFlag != null)
            {
                offerFlag.draw(batch, 0);
            }

            batch.End();

            if (dragging != null)
            {
                if (selected.Count > 0)
                {
                    batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, spriteBasic);
                    Vector2 selectDragOff = mousePos - selectPreviewDrag;
                    foreach (Item i in selected)
                    {
                        Vector2 spot = i.position + selectDragOff;
                        i.drawPreview(batch, spot);
                    }
                    batch.End();
                }
                if (mouseUIPos.X <= sideWide)
                {
                    batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                    dragging.draw(batch, Color.Red, 0.25f);
                }
                else
                {
                    batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, spriteBasic);
                    dragging.draw(batch, 0);
                }
                batch.End();
            }

            base.Draw(gameTime);
        }
    }
}
