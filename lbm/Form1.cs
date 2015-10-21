

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;


using Emgu.CV;
using Emgu.CV.UI;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;


namespace LaserBeamMeasurement
{


    public partial class Form1 : Form
    {

        bool hand_mode = false;

        int mouse_x = 0;
        int mouse_y = 0;
       
       
        double pixsize = 2.2;

        ImageData _imagedata = new ImageData();
        BeamParameters _beamparameters = new BeamParameters();

        private Capture _capture = null;

        private bool _captureInProgress;

        public Form1()
        {
            
            InitializeComponent();

            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.RowHeadersWidth = 78;
            dataGridView1.Rows[0].HeaderCell.Value = "FWHM";
            dataGridView1.Rows[1].HeaderCell.Value = "1/e^2";

            tabControl1.SelectedIndex = 1;

            CvInvoke.UseOpenCL = false;

            try
            {
                _capture = new Capture();

                _capture.ImageGrabbed += ProcessFrame;

            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        public void ProcessFrame(object sender, EventArgs arg) // will be simplified
        {

            //get frame from the Camera

            Mat frame = new Mat();
            Mat grayFrame = new Mat();

            double thresh_med;
            double thresh_e2;

            System.Drawing.Image OrgImage;
            System.Drawing.Image OrgImage1;

            _capture.Retrieve(frame);

            CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
            Image<Rgb, Byte> tothermo = grayFrame.ToImage<Rgb, Byte>(); // original
            Image<Gray, Byte> tothermo1 = grayFrame.ToImage<Gray, Byte>();

            OrgImage = tothermo.ToBitmap();
            OrgImage1 = tothermo.ToBitmap();

            pictureBox4.Image = OrgImage1;

            _imagedata.MakeFalse((Bitmap)OrgImage);
            pictureBox1.Image = OrgImage;

            pictureBox1.Refresh();
            pictureBox4.Refresh();


            double[] minVal;
            double[] maxVal;
            System.Drawing.Point[] minLoc;
            System.Drawing.Point[] maxLoc;
            grayFrame.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);

            _imagedata.sizex = OrgImage.Width;
            _imagedata.sizey = OrgImage.Height;

            if (hand_mode)
            {
                _imagedata.centerx = mouse_x;
                _imagedata.centery = mouse_y;
            }
            else
            {
                _imagedata.centerx = maxLoc[0].X;
                _imagedata.centery = maxLoc[0].Y;
            }


            _imagedata.GraphFill(tothermo1);



            // thresh calculate
            if (hand_mode)
            {
                thresh_med = grayFrame.Bitmap.GetPixel(_imagedata.centerx, _imagedata.centery).R / 2;
                thresh_e2 = grayFrame.Bitmap.GetPixel(_imagedata.centerx, _imagedata.centery).R / Math.Exp(2);
            }
            else
            {
                thresh_med = maxVal[0] / 2;
                thresh_e2 = maxVal[0] / Math.Exp(2);
            }

            _beamparameters.BeamSizeDetect(thresh_med, thresh_e2, _imagedata);



        }

        public void ProcessStaticFrame(Image ImFromFile, bool filt)
        {
            double thresh_med;
            double thresh_e2;
            // Converting the master image to a bitmap
            Bitmap masterImage = (Bitmap)ImFromFile;

            // Normalizing it to grayscale
            Image<Gray, Byte> grayFrame = new Image<Gray, Byte>(masterImage);
            Image<Gray, Byte> grayFramefilt = new Image<Gray, Byte>(masterImage);

            double[] minVal;
            double[] maxVal;
            System.Drawing.Point[] minLoc;
            System.Drawing.Point[] maxLoc;
            grayFrame.MinMax(out minVal, out maxVal, out minLoc, out maxLoc);

            if (filt)
            CvInvoke.MedianBlur(grayFrame, grayFrame, 7);

            _imagedata.sizex = grayFrame.Width;
            _imagedata.sizey = grayFrame.Height;

            if (hand_mode)
            {
                _imagedata.centerx = mouse_x;
                _imagedata.centery = mouse_y;
            }
            else
            {
                _imagedata.centerx = maxLoc[0].X;
                _imagedata.centery = maxLoc[0].Y;
            }

            _imagedata.GraphFill(grayFrame);     
           
            if (!checkBox2.Checked)
            _imagedata.zero_level = Convert.ToInt32(textBox2.Text);



            // thresh calculate
            if (hand_mode)
            {
                thresh_med = (grayFrame.Bitmap.GetPixel(_imagedata.centerx, _imagedata.centery).R- _imagedata.zero_level) / 2 + _imagedata.zero_level;
                thresh_e2 = (grayFrame.Bitmap.GetPixel(_imagedata.centerx, _imagedata.centery).R- _imagedata.zero_level) / Math.Exp(2) + _imagedata.zero_level;
            }
            else
            {
                thresh_med = (maxVal[0]- _imagedata.zero_level)/ 2 + _imagedata.zero_level;
                thresh_e2 = (maxVal[0]- _imagedata.zero_level) / Math.Exp(2) + _imagedata.zero_level;
            }
     
            _beamparameters.BeamSizeDetect(thresh_med, thresh_e2, _imagedata);

            for (int i=0;i<_imagedata.TreshE2X.Length;i++)
            {
                _imagedata.TreshE2X[i] = (int)thresh_e2;
                _imagedata.TreshMedX[i] = (int)thresh_med;
                _imagedata.zero[i] = _imagedata.zero_level;

            }


            if (filt)
            {
                label3.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizex_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizex_e2 * pixsize) + " um";
                label4.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizey_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizey_e2 * pixsize) + " um";
                dataGridView1[2, 0].Value = Convert.ToString(_beamparameters.sizex_med * pixsize);
                dataGridView1[3, 0].Value = Convert.ToString(_beamparameters.sizey_med * pixsize);
                dataGridView1[2, 1].Value = Convert.ToString(_beamparameters.sizex_e2 * pixsize);
                dataGridView1[3, 1].Value = Convert.ToString(_beamparameters.sizey_e2 * pixsize);
                chart3.Series["x filter"].Points.DataBindY(_imagedata.ChartX);
                chart4.Series["y filter"].Points.DataBindY(_imagedata.ChartY);
                chart3.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
                chart3.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
                chart4.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
                chart4.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
                chart3.Series["zero"].Points.DataBindY(_imagedata.zero);
                chart4.Series["zero"].Points.DataBindY(_imagedata.zero);
            }
            else
            {
                label1.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizex_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizex_e2 * pixsize) + " um";
                label2.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizey_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizey_e2 * pixsize) + " um";

                chart1.Series["x"].Points.DataBindY(_imagedata.ChartX);
                chart2.Series["y"].Points.DataBindY(_imagedata.ChartY);
                chart1.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
                chart1.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
                chart2.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
                chart2.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
                chart1.Series["zero"].Points.DataBindY(_imagedata.zero);
                chart2.Series["zero"].Points.DataBindY(_imagedata.zero);

                dataGridView1[0, 0].Value = Convert.ToString(_beamparameters.sizex_med * pixsize);
                dataGridView1[1, 0].Value = Convert.ToString(_beamparameters.sizey_med * pixsize);
                dataGridView1[0, 1].Value = Convert.ToString(_beamparameters.sizex_e2 * pixsize);
                dataGridView1[1, 1].Value = Convert.ToString(_beamparameters.sizey_e2 * pixsize);
            }
      
         
           

        }
 
        private void button1_Click(object sender, EventArgs e)
        {


            if (_capture != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    button1.Text = "Start Capture";
                    _capture.Pause();

                }
                else
                {
                    //start the capture
                    button1.Text = "Stop";
                    _capture.Start();

                }

                _captureInProgress = !_captureInProgress;
            }
        }

   

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            mouse_x = e.X;
            mouse_y = e.Y;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            label3.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizex_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizex_e2 * pixsize) + " um";
            label4.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizey_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizey_e2 * pixsize) + " um";
            dataGridView1[2, 0].Value = Convert.ToString(_beamparameters.sizex_med * pixsize);
            dataGridView1[3, 0].Value = Convert.ToString(_beamparameters.sizey_med * pixsize);
            dataGridView1[2, 1].Value = Convert.ToString(_beamparameters.sizex_e2 * pixsize);
            dataGridView1[3, 1].Value = Convert.ToString(_beamparameters.sizey_e2 * pixsize);
            chart3.Series["x filter"].Points.DataBindY(_imagedata.ChartX);
            chart4.Series["y filter"].Points.DataBindY(_imagedata.ChartY);
            chart3.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
            chart3.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
            chart4.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
            chart4.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
            chart3.Series["zero"].Points.DataBindY(_imagedata.zero);
            chart4.Series["zero"].Points.DataBindY(_imagedata.zero);

            label1.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizex_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizex_e2 * pixsize) + " um";
            label2.Text = "FWHM:  " + Convert.ToString(_beamparameters.sizey_med * pixsize) + " um           1/e^2:  " + Convert.ToString(_beamparameters.sizey_e2 * pixsize) + " um";

            chart1.Series["x"].Points.DataBindY(_imagedata.ChartX);
            chart2.Series["y"].Points.DataBindY(_imagedata.ChartY);
            chart1.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
            chart1.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
            chart2.Series["fwhm"].Points.DataBindY(_imagedata.TreshMedX);
            chart2.Series["1/e^2"].Points.DataBindY(_imagedata.TreshE2X);
            chart1.Series["zero"].Points.DataBindY(_imagedata.zero);
            chart2.Series["zero"].Points.DataBindY(_imagedata.zero);

            dataGridView1[0, 0].Value = Convert.ToString(_beamparameters.sizex_med * pixsize);
            dataGridView1[1, 0].Value = Convert.ToString(_beamparameters.sizey_med * pixsize);
            dataGridView1[0, 1].Value = Convert.ToString(_beamparameters.sizex_e2 * pixsize);
            dataGridView1[1, 1].Value = Convert.ToString(_beamparameters.sizey_e2 * pixsize);


            Graphics gr = e.Graphics;
            drawmark(gr, Color.Red);

        }


        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (hand_mode) hand_mode = false;
            else hand_mode = true;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            button1.Text = "Start Capture";
            _capture.Pause();

            openFileDialog1.Filter = "beam picture|*.jpg;*.png;*.gif;*.bmp| All (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Image t = Image.FromFile(openFileDialog1.FileName);
                pictureBox2.Image = t;

                Image tf = (Image)t.Clone();
                _imagedata.MakeFalse((Bitmap)tf);
                pictureBox3.Image = tf;

                _imagedata.ImageFromFile = t;
             
                mouse_x = (int)(_imagedata.ImageFromFile.Size.Width/2);
                mouse_y = (int)(_imagedata.ImageFromFile.Size.Height/2);
                ProcessStaticFrame(t,true);
                ProcessStaticFrame(t, false);
                          
                pictureBox4.Refresh();
                pictureBox3.Refresh();

            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {   

        }


        private void pictureBox4_Paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            drawmark(gr, Color.Red);
            Pen rect = new Pen(Color.White);
            rect.Width = 2;
            e.Graphics.DrawRectangle(rect, _imagedata.graphstartx, _imagedata.graphstarty, _imagedata.spotsize<<1, _imagedata.spotsize<<1);
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            drawmark(gr,Color.Red);
            Pen rect = new Pen(Color.White);
            rect.Width = 2;
            e.Graphics.DrawRectangle(rect, _imagedata.graphstartx, _imagedata.graphstarty, _imagedata.spotsize<<1, _imagedata.spotsize<<1);
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            mouse_x = e.X;
            mouse_y = e.Y;
            ProcessStaticFrame(_imagedata.ImageFromFile,true);
            ProcessStaticFrame(_imagedata.ImageFromFile, false);
            pictureBox2.Refresh(); pictureBox3.Refresh();
        }

        private void pictureBox3_MouseClick(object sender, MouseEventArgs e)
        {

            mouse_x = e.X;
            mouse_y = e.Y;
            ProcessStaticFrame(_imagedata.ImageFromFile,true);
            ProcessStaticFrame(_imagedata.ImageFromFile,false);
            pictureBox3.Refresh(); pictureBox2.Refresh();
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            drawmark(gr, Color.White);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {  
            pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBox3.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                pixsize = Convert.ToDouble(textBox1.Text);
                ProcessStaticFrame(_imagedata.ImageFromFile, true);
                ProcessStaticFrame(_imagedata.ImageFromFile, false);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
           
            if (char.IsDigit(e.KeyChar) == true || e.KeyChar == (char)Keys.Back ||  e.KeyChar == ',') return;
            if (e.KeyChar == (char)Keys.Back) return;
            e.Handled = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                _imagedata.zero_level = Convert.ToInt16(textBox2.Text);
                ProcessStaticFrame(_imagedata.ImageFromFile, true);
                ProcessStaticFrame(_imagedata.ImageFromFile, false);
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) == true || e.KeyChar == (char)Keys.Back) return;
            e.Handled = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            ProcessStaticFrame(_imagedata.ImageFromFile, true);
            ProcessStaticFrame(_imagedata.ImageFromFile, false);
        }

        private void pictureBox4_MouseClick(object sender, MouseEventArgs e)
        {
            mouse_x = e.X;
            mouse_y = e.Y;
            ProcessStaticFrame(_imagedata.ImageFromFile, true);
            ProcessStaticFrame(_imagedata.ImageFromFile, false); pictureBox2.Refresh(); pictureBox3.Refresh();
        }

         private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                _imagedata.spotsize = Convert.ToInt16(textBox3.Text);
                if (_imagedata.spotsize > _imagedata.maxspotsize) _imagedata.spotsize = _imagedata.maxspotsize;
                chart1.ChartAreas[0].AxisX.Maximum = _imagedata.spotsize<<1;
                chart2.ChartAreas[0].AxisX.Maximum = _imagedata.spotsize<<1;
                chart3.ChartAreas[0].AxisX.Maximum = _imagedata.spotsize<<1;
                chart4.ChartAreas[0].AxisX.Maximum = _imagedata.spotsize<<1;
                ProcessStaticFrame(_imagedata.ImageFromFile, true);
                ProcessStaticFrame(_imagedata.ImageFromFile, false);
                pictureBox2.Refresh(); pictureBox3.Refresh();
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) == true || e.KeyChar == (char)Keys.Back) return;
            e.Handled = true;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0) { button1.Visible = true; groupBox1.Visible = false; }
            else { button1.Visible = false; groupBox1.Visible = true; }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void drawmark (Graphics gr, Color cl)
        {
            Pen axis = new Pen(cl);

            axis.Width = 2;

            Point p1 = new Point(_imagedata.centerx - 100, _imagedata.centery);
            Point p2 = new Point(_imagedata.centerx + 100, _imagedata.centery);
            Point p3 = new Point(_imagedata.centerx, _imagedata.centery - 100);
            Point p4 = new Point(_imagedata.centerx, _imagedata.centery + 100);

            gr.DrawLine(axis, p1, p2);
            gr.DrawLine(axis, p3, p4);
        }


    }

   
    }








