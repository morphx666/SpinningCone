using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpinningCone {
    public partial class FormMain : Form {
        private bool savePng = false;

        private readonly List<RectangleF> ellipses = new List<RectangleF>();
        private readonly Pen p = new Pen(Color.FromArgb(200, 200, 200));

        private float w2;
        private float h2;

        private int ellipsesPerFrame;
        private int totalFrames;
        private int frame;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);

            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;

            this.Resize += (_, __) => PreComputeEllipses();
            this.Paint += (s, e) => RenderCone(e);

            PreComputeEllipses();

            Task.Run(() => {
                while(true) {
                    Thread.Sleep(8);
                    this.Invalidate();
                }
            });
        }

        private void PreComputeEllipses() {
            double angleStep = 0.072;

            double w = this.DisplayRectangle.Width;
            double h = this.DisplayRectangle.Height;
            w2 = (float)(w / 2);
            h2 = (float)(h / 2);
            double hc = h * 0.8;

            Bitmap bmp = new Bitmap((int)w, (int)h);
            Graphics g = Graphics.FromImage(bmp);
            g.TranslateTransform(w2, h2);

            ellipses.Clear();
            frame = 0;
            totalFrames = 0;
            ellipsesPerFrame = 0;

            for(double angle = 0; angle < 2 * Math.PI; angle += angleStep) {
                double s = Math.Sin(angle);
                double xc = 0.6 * hc * s;
                double wc = hc * Math.Cos(angle);
                double xo = 0.16 * w * s;

                double k = 0.05;
                for(double i = 0; i < 5; i += k) {
                    double n = Math.Exp(i);

                    double ew = wc / n;         // wc / Math.Sqrt(n)    | wc / Math.Pow(i + 1, 1.7)
                    double eh = hc / (n + i);   // hc / n               | hc / Math.Pow(i + 1, 2)
                    ellipses.Add(new RectangleF((float)(xc / Math.Sqrt(n / 2) - xo - ew / 2), (float)(-eh / 2),
                                                (float)ew, (float)eh));

                    k += 0.006;
                }
                if(ellipsesPerFrame == 0) ellipsesPerFrame = ellipses.Count;
                totalFrames++;

                if(savePng) {
                    g.Clear(Color.Black);
                    int f = frame * ellipsesPerFrame;
                    for(int i = 0; i < ellipsesPerFrame; i++)
                        g.DrawEllipse(p, ellipses[i + f]);
                    string fileName = $"cone{frame++:000}.png";
                    if(File.Exists(fileName)) File.Delete(fileName);
                    bmp.Save(fileName, ImageFormat.Png);
                }
            }

            frame = 0;
            g.Dispose();
            bmp.Dispose();
        }

        private void RenderCone(PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.TranslateTransform(w2, h2);

            int f = frame * ellipsesPerFrame;
            for(int i = 0; i < ellipsesPerFrame; i++)
                g.DrawEllipse(p, ellipses[i + f]);
            frame++;
            frame %= totalFrames;
        }
    }
}