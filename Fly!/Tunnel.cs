using Fly.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fly
{
    public class Tunnel
    {
        
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public TunnelType Type { get; set; }

        public bool pointCounted { get; set; }

        private readonly Image model;

        public readonly int Width = 100;
        public enum TunnelType
        {
            Bottom,
            Top
        }
        public Tunnel(int x, int y, int height, TunnelType type)
        {
            X = x;
            Y = y;
            Height = height;
            Type = type;
            if (Type == TunnelType.Bottom)
                model = Resources.pipe;
            else
                model = Resources.pipeD;

            pointCounted = false;
        }

        public void Draw(Graphics g)
        {
            g.DrawImage(model, new Rectangle(X, Y, Width, Height));

        }

        public void Move(int x)
        {
            X -= x;
            if (X + Width < 0)
            {
                Regenerate();
            }
        }

        private void Regenerate()
        {
            Random rnd = new Random();
            X = Form1.MAX_WIDTH + Width + 400;

            int tmp = rnd.Next(Form1.pipeMinHeight + Form1.pipeDistanceY, Form1.MAX_HEIGHT - (Form1.pipeMinHeight + Form1.pipeDistanceY));

            if (Type == TunnelType.Top)
            {
                Height = tmp;
            }
            else if (Type == TunnelType.Bottom)
            {
                Y = tmp + Form1.pipeDistanceY;
                Height = Form1.MAX_HEIGHT - tmp;
            }

            pointCounted = false;
        }
    }
}
