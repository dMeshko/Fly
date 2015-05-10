using Fly.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fly
{
    public partial class Form1 : Form
    {
        //public static
        public static readonly int MAX_WIDTH = 600;
        public static readonly int MAX_HEIGHT = 640;
        public static readonly int SQUARE_SIZE = 60;
        public static readonly int pipeMinHeight = 70;
        public static readonly int pipeDistanceY = 142;
        public static readonly int pipeDistanceX = 300;
        public static readonly int coinDistance = 200;
        public static readonly int MAX_POWERUP_SECONDS = 4;
        public static readonly int BEST_PLAYERS_DISPLAYED = 2;
        public static int numOfCoins;

        //public
        public Actor bird { get; set; }
        public static List<Tunnel> topTerrain { get; set; }
        public static List<Tunnel> bottomTerrain { get; set; }
        public List<Coin> Coins { get; set; }
        public int DownVelocity { get; set; }



        private Image homeBut;
        private Image exitBut;
        private Image homeSc;

        
        private Timer timer;
        private Timer autoPilotTimer;
        private Timer hcTimer;
        private Label label;
        private Label labelAutoPilot;
        private Label bestScore;
        private int Points;
        private int FRAMES_PER_SECOND;
        private int LOCAL_MAX;
        private bool gamePlaying;
        private bool autoPilot;
        private int seconds;
        private int highScore;
        private bool endGame;
        private bool homeScreen;
        private bool playedOnce;
        private bool scoreScreen;
        private int count;


        private SoundPlayer coinSound;
        private SoundPlayer powerupSound;
        private SoundPlayer gameOverSound;
        private SoundPlayer birdFlapSound;

        //FONT
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();

        Font myFont;
        //FONT

        public Form1()
        {
            //FONT
            byte[] fontData = Properties.Resources.Rumpelstiltskin;
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Properties.Resources.Rumpelstiltskin.Length);
            AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.Rumpelstiltskin.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            myFont = new Font(fonts.Families[0], 16.0F);
            //FONT

            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();

            DoubleBuffered = true;


            highScore = 0;
            if (File.Exists("highScore.txt"))
            {
                StreamReader fo = File.OpenText("highScore.txt");
                highScore = Convert.ToInt32(fo.ReadLine());
                fo.Close();
            }
            else
            {
                StreamWriter fi = File.CreateText("highScore.txt");
                fi.WriteLine(highScore);
                fi.Flush();
                fi.Close();
            }
            this.ControlBox = false;
            //set the heights
            this.Width = MAX_WIDTH;
            this.Height = MAX_HEIGHT;
            this.BackgroundImage = Resources.background;
            label = new Label();
            labelAutoPilot = new Label();
            label.Width = 230;
            label.BackColor = Color.Transparent;
            labelAutoPilot.BackColor = Color.Transparent;
            //increase the font!!
            label.Font = new Font(fonts.Families[0], 50); 

            labelAutoPilot.Font = new Font(fonts.Families[0], 50); 
            label.Location = new Point(MAX_WIDTH / 2 - 10, 30);
            labelAutoPilot.Location = new Point(MAX_WIDTH - SQUARE_SIZE - 30, 10);
            label.ForeColor = Color.White;
            labelAutoPilot.ForeColor = Color.Orange;
            label.Height = 100;
            labelAutoPilot.Height = 100;
            this.Controls.Add(label);
            this.Controls.Add(labelAutoPilot);

            bestScore = new Label();
            bestScore.BackColor = Color.Transparent;
            //bestScore.Font = new Font("Arial", 30);
            bestScore.Location = new Point(120, MAX_HEIGHT / 4);
            bestScore.ForeColor = Color.White;
            bestScore.Height = 70;
            bestScore.Width = MAX_WIDTH - 100;
            this.Controls.Add(bestScore);
            bestScore.Visible = false;

            coinSound = new SoundPlayer(Resources.coin_sound);
            powerupSound = new SoundPlayer(Resources.powerup_sound);
            gameOverSound = new SoundPlayer(Resources.gameover_sound);
            birdFlapSound = new SoundPlayer(Resources.flap_sound);
            gamePlaying = false;
            endGame = false;
            scoreScreen = false;
            count = 0;
        }
        
        public void newGame()
        {
            homeScreen = false;
            gamePlaying = true;
            playedOnce = true;
            autoPilot = false;
            Points = 0;
            numOfCoins = 0;
            label.Text = Points.ToString();
            DownVelocity = 13;
            bird = new Actor(MAX_WIDTH / 8, MAX_HEIGHT / 3, SQUARE_SIZE, SQUARE_SIZE - 12);
            topTerrain = new List<Tunnel>();
            bottomTerrain = new List<Tunnel>();
            Coins = new List<Coin>();
            timer = new Timer();
            LOCAL_MAX = 10;
            FRAMES_PER_SECOND = 40;
            timer.Interval = FRAMES_PER_SECOND;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            autoPilotTimer = new Timer();
            autoPilotTimer.Tick += new EventHandler(autoPilotTimer_Tick);
            autoPilotTimer.Interval = 1000;

            homeBut = Resources.home_button;
            exitBut = Resources.exit2_button;

            generateTerain();

        }

        private void autoPilotTimer_Tick(object sender, EventArgs e)
        {
            if (seconds + 1 == MAX_POWERUP_SECONDS)
            {
                autoPilotTimer.Stop();
                DownVelocity = 0;
                autoPilot = false;
                labelAutoPilot.Visible = false;
                bird.Model = Resources.bird_fall;
            }

            seconds++;
            labelAutoPilot.Text = Convert.ToString(MAX_POWERUP_SECONDS - seconds);
        }

        public void generateTerain()
        {
            Random rnd = new Random();


            for (int i = 0; i < 4; i++)
            {
                //pipes
                int X = 500 + i * pipeDistanceX;
                int Y = rnd.Next(pipeMinHeight + pipeDistanceY, MAX_HEIGHT - (pipeMinHeight + pipeDistanceY));
                topTerrain.Add(new Tunnel(X, 0, Y - pipeDistanceY, Tunnel.TunnelType.Top));
                bottomTerrain.Add(new Tunnel(X, Y, MAX_HEIGHT - Y, Tunnel.TunnelType.Bottom));


                //coins
                int X1 = 300 + i * coinDistance;
                int Y1 = rnd.Next(pipeMinHeight, MAX_HEIGHT - pipeMinHeight);

                Coin.setCoins(ref X1, ref Y1);

                Coin c = new Coin(X1, Y1);
                if (i == 3)
                    c.isPowerUP = true;
                Coins.Add(c);
            }
        }

        

        public bool gameOver()
        {
            Point birdLeftTop = new Point(bird.X, bird.Y);
            Point birdRightBottom = new Point(bird.X + SQUARE_SIZE, bird.Y + SQUARE_SIZE - 12);

            if (birdRightBottom.Y >= MAX_HEIGHT) //check if u fell off the map
                return true;

            if (birdLeftTop.Y + SQUARE_SIZE < 0) //bitch overflew
                return true;

            if (bird.State != Actor.STATE.EXPIRED)
            {
                //check overlap of 2 rectangles
                foreach (Tunnel t in topTerrain)
                {
                    Point tunnelLeftTop = new Point(t.X, t.Y);
                    Point tunnelRightBottom = new Point(t.X + t.Width, t.Y + t.Height);
                    if (birdLeftTop.X < tunnelRightBottom.X && birdRightBottom.X > tunnelLeftTop.X && birdLeftTop.Y < tunnelRightBottom.Y && birdRightBottom.Y > tunnelLeftTop.Y)
                    {
                        gameOverSound.Play();
                        bird.State = Actor.STATE.EXPIRED;

                    }
                    //return true;
                }

                foreach (Tunnel t in bottomTerrain)
                {
                    Point tunnelLeftTop = new Point(t.X, t.Y);
                    Point tunnelRightBottom = new Point(t.X + t.Width, t.Y + t.Height);
                    if (birdLeftTop.X < tunnelRightBottom.X && birdRightBottom.X > tunnelLeftTop.X && birdLeftTop.Y < tunnelRightBottom.Y && birdRightBottom.Y > tunnelLeftTop.Y)
                    {
                        gameOverSound.Play();
                        bird.State = Actor.STATE.EXPIRED;

                    }
                    //return true;
                }
            }

            return false;
        }

        private bool gotPoint(Tunnel t)
        {
            return ((bird.X + SQUARE_SIZE >= t.X + t.Width / 2) && (bird.X + SQUARE_SIZE <= t.X + t.Width));
        }

        private bool gotCoin(Coin c)
        {
            Point birdLeftTop = new Point(bird.X, bird.Y);
            Point birdRightBottom = new Point(bird.X + SQUARE_SIZE, bird.Y + SQUARE_SIZE - 12);

            Point coinLeftTop = new Point(c.X, c.Y);
                Point coinRightBottom = new Point(c.X + c.size, c.Y + c.size);

            
            
                if (birdLeftTop.X < coinRightBottom.X && birdRightBottom.X > coinLeftTop.X && birdLeftTop.Y < coinRightBottom.Y && birdRightBottom.Y > coinLeftTop.Y)
                {
                    coinSound.Play();
                    return true;
                }
            return false;
           }


            
        
       private void timer_Tick(object sender, EventArgs e)
        {
            if (gamePlaying)
            {

                if (Points >= LOCAL_MAX)
                {
                    LOCAL_MAX += 10;
                    if (FRAMES_PER_SECOND - 2 > 0)
                    {
                        FRAMES_PER_SECOND -= 2;
                        timer.Interval = FRAMES_PER_SECOND;
                    }

                }

                if (!autoPilot)
                {
                    DownVelocity += Actor.VELOCITY;
                    bird.Move(DownVelocity);
                }
                else
                    findYourWay();

                if (bird.State != Actor.STATE.EXPIRED)
                {
                    if (bird.State == Actor.STATE.POWERED_UP)
                    {
                        bird.Angle = 0;
                    }
                    else if (bird.Angle + 5 <= 30)
                    {
                        bird.Angle += 5;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        topTerrain[i].Move(9);
                        bottomTerrain[i].Move(9);
                        Coins[i].Move(9);

                        if (gotPoint(topTerrain[i]) && !topTerrain[i].pointCounted)
                        {
                            Points++;
                            label.Text = Points.ToString();
                            topTerrain[i].pointCounted = true;
                        }

                        if (!Coins[i].pointCollected)
                        {
                            if (gotCoin(Coins[i]))
                            {
                                if (!Coins[i].isPowerUP)
                                {
                                    Points += 2;
                                    label.Text = Points.ToString();
                                    Coins[i].pointCollected = true;
                                    numOfCoins++;

                                }
                                else
                                {
                                    Coins[i].pointCollected = true;
                                    powerupSound.Play();
                                    bird.Model = Resources.empowered;
                                    bird.State = Actor.STATE.POWERED_UP;
                                    seconds = 0;
                                    labelAutoPilot.Text = MAX_POWERUP_SECONDS.ToString();
                                    labelAutoPilot.Visible = true;
                                    autoPilot = true;
                                    autoPilotTimer.Start();

                                }
                            }
                        }
                    }
                        
                }
                else
                {
                    autoPilot = false;
                    bird.Angle += 30;

                    if (bird.SquareSizeX > 0)
                        bird.SquareSizeX -= 5;

                    if (bird.SquareSizeY > 0)
                        bird.SquareSizeY -= 5;
                }


                if (gameOver())
                {

                    gamePlaying = false;
                    endGame = true;   

                }

            }

            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (gamePlaying)
            {
                bird.Draw(e.Graphics);

                foreach (Tunnel t in topTerrain)
                {
                    t.Draw(e.Graphics);
                }
                foreach (Tunnel t in bottomTerrain)
                {
                    t.Draw(e.Graphics);
                }

                foreach (Coin c in Coins)
                {
                    c.Draw(e.Graphics);
                }
            }
            else if (endGame || scoreScreen)
            {
                e.Graphics.DrawImageUnscaled(homeBut, new Rectangle(420, 390, Width, Height));
                e.Graphics.DrawImageUnscaled(exitBut, new Rectangle(300, 390, Width, Height));
            }
            else if (homeScreen)
            {
                if (count == 30 || playedOnce)
                    e.Graphics.DrawImage(homeSc, new Rectangle(0, 0, Width, Height - 38));
                else
                {
                    if (count >= 0)
                        e.Graphics.DrawImage(Resources.home_background, new Rectangle(0, 0, Width, Height - 38));
                    if (count >= 6)
                        e.Graphics.DrawImage(Resources.F, new Rectangle(0, 0, Width, Height - 38));
                    if (count >= 12)
                        e.Graphics.DrawImage(Resources.l, new Rectangle(0, 0, Width, Height - 38));
                    if (count >= 18)
                        e.Graphics.DrawImage(Resources.y, new Rectangle(0, 0, Width, Height - 38));
                    if (count >= 24)
                        e.Graphics.DrawImage(Resources.xcl_mark, new Rectangle(0, 0, Width, Height - 38));
                }

            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gamePlaying)
            {
                if (e.KeyCode == Keys.Space)
                {
                    if (bird.State != Actor.STATE.EXPIRED && !autoPilot)
                    {
                        DownVelocity = -15;
                        birdFlapSound.Play();
                        bird.Model = Resources.bird_fly;
                        bird.State = Actor.STATE.FLY;
                        bird.Angle = -30;

                    }

                }
            }
            /* else if (e.KeyCode == Keys.Up)
             {
                 bird.State = Actor.STATE.POWERED_UP;
                 seconds = 0;
                 labelAutoPilot.Text = MAX_POWERUP_SECONDS.ToString();
                 labelAutoPilot.Visible = true;
                 autoPilot = true;
                 autoPilotTimer.Start();
             }
             else if (e.KeyCode == Keys.Down)
             {
                 //autoPilotTimer.Stop();
                 autoPilot = false;
             }*/

        }

        private Tunnel findMostOptimal()
        {
            Tunnel t = null;
            int minDist = Int32.MaxValue;
            for (int i = 0; i < topTerrain.Count; i++)
            {
                //find the most optimal
                if ((topTerrain[i].X - bird.X) < minDist)
                {
                    minDist = topTerrain[i].X - bird.X;
                    t = topTerrain[i];
                }
            }

            return t;
        }

        private void findYourWay()
        {
            autoPilot = true;
            Tunnel t = findMostOptimal();
            int validState = (t.Y + t.Height) + (pipeDistanceY - SQUARE_SIZE - 12) / 2;
            bird.Y = validState;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bestScore.Font = new Font(fonts.Families[0], 50); ;
            loadHC();
            //newGame();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (gamePlaying)
            {
                if (bird.State != Actor.STATE.EXPIRED && !autoPilot)
                {
                    bird.Model = Resources.bird_fall;
                    bird.State = Actor.STATE.FALL;
                }
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (endGame || scoreScreen)
            {
                if ((e.X > 300 && e.X < 410) && (e.Y > 390 && e.Y < 500))
                {
                    exitBut = Resources.exit2_button_hover;
                }
                else if (!((e.X > 300 && e.X < 410) && (e.Y > 390 && e.Y < 500)))
                {
                    exitBut = Resources.exit2_button;
                }
                if ((e.X > 420 && e.X < 560) && (e.Y > 390 && e.Y < 470))
                {
                    homeBut = Resources.home_button_hover;
                }
                else if (!((e.X > 420 && e.X < 560) && (e.Y > 390 && e.Y < 470)))
                {
                    homeBut = Resources.home_button;
                }
            }
            if (homeScreen)
            {
                if ((e.X > 80 && e.X < 220) && (e.Y > 400 && e.Y < 470))
                {
                    homeSc = Resources.home_screen_hover1;
                }
                else if ((e.X > 370 && e.X < 500) && (e.Y > 400 && e.Y < 470))
                {
                    homeSc = Resources.home_screen_hover2;
                }
                else if ((e.X > 200 && e.X < 350) && (e.Y > 540 && e.Y < 600))
                {
                    homeSc = Resources.home_screen_hover3;
                }
                else
                    homeSc = Resources.home_screen_modified;
            }

        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (endGame || scoreScreen)
            {
                if ((e.X > 300 && e.X < 410) && (e.Y > 390 && e.Y < 500))
                {
                    Application.Exit();
                }
                if ((e.X > 420 && e.X < 560) && (e.Y > 390 && e.Y < 470))
                {
                    bestScore.Visible = false;
                    loadHC();
                }
            }
            else if (count == 30 || playedOnce)
            {
                if ((e.X > 80 && e.X < 220) && (e.Y > 400 && e.Y < 470))
                {
                    hcTimer.Stop();
                    newGame();
                }
                else if ((e.X > 200 && e.X < 350) && (e.Y > 540 && e.Y < 600))
                {
                    Application.Exit();
                }
                else if ((e.X > 370 && e.X < 500) && (e.Y > 400 && e.Y < 470))
                {
                    scoreScreen = true;
                    

                    if (Points > highScore)
                    {
                        highScore = Points;
                        StreamWriter fo = new StreamWriter("highScore.txt", false);
                        fo.WriteLine(highScore);
                        fo.Flush();
                        fo.Close();
                    }

                    bestScore.Visible = true;
                    bestScore.Text = "Highscore: " + highScore.ToString();

                }
            }
        }
        private void hcTimer_Tick(object sender, EventArgs e)
        {
            if (count < 30)
                count++;
            Invalidate();

        }
        private void loadHC()
        {
            homeSc = Resources.home_screen_modified;
            label.Text = null;
            homeScreen = true;
            if (timer != null)
                timer.Stop();
            if (autoPilotTimer != null)
                autoPilotTimer.Stop();

            if(scoreScreen)
            {
                count = 30;
            }
            else
            {
                count = 0;
                hcTimer = new Timer();
                hcTimer.Interval = 40;
                hcTimer.Tick += new EventHandler(hcTimer_Tick);
                hcTimer.Start();
            }

            endGame = false;
            scoreScreen = false;
            
        }


    }
}
