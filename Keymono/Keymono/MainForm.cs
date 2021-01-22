using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keymono
{
    public partial class MainForm : Form
    {
        NativeLibraries.WindowHelper wh = new NativeLibraries.WindowHelper();
        RawHID rawHid = new RawHID(0xFEED, 0x6060);

        public MainForm()
        {
            InitializeComponent();
            wh.WindowChanged += wh_WindowChanged;
            rawHid.MessageReceived += RawHid_MessageReceived;
            rawHid.Start();
        }

        private void RawHid_MessageReceived(byte[] Report)
        {
            Console.WriteLine(BitConverter.ToString(Report).Replace("-", " "));
            //throw new NotImplementedException();
        }

        private void wh_WindowChanged(IntPtr Handle)
        {
            if (Handle != this.Handle)
            {
                //Bitmap icon = wh.GetWindowIconHarder(Handle);
                Bitmap icon = wh.GetIconFromProcess(wh.GetProcess(Handle));

                Image bw_icon = GraphicsLibraries.Effects.ConvertToBlackAndWhite(icon, (float)trackBar1.Value / 10);

                GraphicsLibraries.Analysis.Analyse(icon);
                Color color = GraphicsLibraries.Analysis.MostUsedVisibleColor;
                Color color2 = color;
                foreach (Color c in GraphicsLibraries.Analysis.TenMostUsedColors)
                    Console.WriteLine(c);

                textBox1.Text = wh.GetWindowTitle(Handle);
                textBox2.Text = wh.GetProcess(Handle).ProcessName;

                pictureBox1.Image = icon;
                pictureBox2.Image = GenerateImage(GraphicsLibraries.Effects.ConvertToBlackAndWhite(pictureBox1.Image, (float)trackBar1.Value / 10));

                
                pictureBox3.BackColor = color;
                label1.Text = color.ToString();

                GraphicsLibraries.ColorHelper.HSVColor hsv = GraphicsLibraries.ColorHelper.ColorToHSV(color);
                hsv.Saturation = 1;
                hsv.Value = 1;
                color2 = GraphicsLibraries.ColorHelper.ColorFromHSV(hsv);
                pictureBox4.BackColor = color2;

                rawHid.SendRawPayload(RawHID.Commands.RGB_SetColorRGB, new byte[] { color.R, color.G, color.B });

            }
            
        }

        private Image GenerateImage(Image org)
        {
            Image result = new Bitmap(64, 48);

            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(result))
            {
                gr.Clear(Color.Black);
                gr.DrawImage(org, result.Width / 2 - org.Width / 2, result.Height / 2 - org.Height / 2);
            }
            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            pictureBox2.Image = GenerateImage(GraphicsLibraries.Effects.ConvertToBlackAndWhite(pictureBox1.Image, (float)trackBar1.Value / 10));
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            rawHid.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            rawHid.SendRawPayload(RawHID.Commands.RGB_Step, new byte[] {  });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            rawHid.SendRawPayload(RawHID.Commands.RGB_Mode, new byte[] { 0x01 });
        }
    }
}
