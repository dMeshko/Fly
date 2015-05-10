using Fly.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fly
{
    public class Actor
    {
        public Image Model { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int SquareSizeX { get; set; }
        public int SquareSizeY { get; set; }
        public static readonly int VELOCITY = 2;
        public int Angle { get; set; }
        public enum STATE
        {
            POWERED_UP,
            FLY,
            FALL,
            EXPIRED //lol :D
        }
        public STATE State { get; set; }

        public Actor(int x, int y, int sizeX, int sizeY)
        {
            X = x;
            Y = y;
            SquareSizeX = sizeX;
            SquareSizeY = sizeY;
            Model = Resources.bird_fall;
            State = STATE.FALL;
            Angle = -30;
        }

        public void Draw(Graphics g)
        {
            g.TranslateTransform(X, Y);
            g.RotateTransform(Angle);
            g.DrawImage(Model, new Rectangle(0, 0, SquareSizeX, SquareSizeY));
            g.ResetTransform();
        }

        public void Move(int y)
        {
            Y += y;
        }
    }
}
