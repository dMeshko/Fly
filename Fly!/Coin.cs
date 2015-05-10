using Fly.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fly
{
    public class Coin
    {
        public int X { get; set; }
        public int Y { get; set; }
        public readonly int size = 20;
        private Image Model;
        public bool isPowerUP { get; set; }

        public bool pointCollected;

        public Coin(int x, int y)
        {
            X = x;
            Y = y;

            pointCollected = false;
            isPowerUP = false;
            Model = Resources.coin;
        }

        public void Draw(Graphics g)
        {
            if (isPowerUP)
                Model = Resources.powerup;

            if (!pointCollected)
                //g.FillEllipse(new SolidBrush(Color.Yellow), X, Y, size, size);
                g.DrawImageUnscaled(Model, X, Y);
        }

        public void Move(int n)
        {
            X -= n;

            if (X + size < 0)
            {
                Regenerate();
            }
        }

        private void Regenerate()
        {
            Random rnd = new Random();

            if (Form1.numOfCoins == 5 * rnd.Next(1, 3))
            {
                Model = Resources.powerup;
                isPowerUP = true;
                Form1.numOfCoins = 0;

            }
            else
            {
                Model = Resources.coin;
                isPowerUP = false;
            }


            pointCollected = false;
            int newX = X + Form1.MAX_WIDTH + Form1.coinDistance;
            int newY = rnd.Next(Form1.pipeMinHeight, Form1.MAX_HEIGHT - Form1.pipeMinHeight);

            setCoins(ref newX, ref newY);

            X = newX;
            Y = newY;
        }

        public static void setCoins(ref int x, ref int y)
        {
            foreach (Tunnel t in Form1.topTerrain)
            {
                if (x >= t.X - 50 && x <= t.X + t.Width)
                {
                    if (y <= t.Y + t.Height + 50)
                    {
                        while (y <= t.Y + t.Height + 50)
                        {
                            y++;
                        }
                    }
                }


            }

            foreach (Tunnel t in Form1.bottomTerrain)
            {
                if (x >= t.X - 50 && x <= t.X + t.Width)
                {

                    if (y >= t.Y - 50)
                    {
                        while (y >= t.Y - 50)
                        {
                            y--;
                        }
                    }
                }
            }
        }
    }
}
