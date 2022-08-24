using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCR
{
    public partial class ImgShowDialog : Form
    {
        private string recString;
        public ImgShowDialog(string selectedPath)
        {
            InitializeComponent();
            recString = selectedPath;
        }

        private void ImgShowDialog_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = Image.FromFile(recString);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.Text = recString;
        }
    }
}
