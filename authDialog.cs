using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32;
using System.Configuration;

namespace OCR
{
    public partial class authDialog : Form
    {
        public authDialog()
        {
            InitializeComponent();
        }

        private string cpuid;

        private string checkid ;

        private Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        private void authDialog_Load(object sender, EventArgs e)
        {
            SystemInfo info = new SystemInfo();
            cpuid = info.processor.SerialNumber.ToString();
            textBox1.Text = cpuid;

           
            checkid = configuration.AppSettings.Settings["license"].Value;
            textBox2.Text = checkid;
            bool isCheck = checkAuth();
            if (isCheck)
            {
                button1.Enabled = false;
                button1.Text = "软件已激活";
            }

        }

        public bool checkAuth()
        {
                       


            if (textBox1.Text == textBox2.Text)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkAuth())
            {
                configuration.AppSettings.Settings["license"].Value = textBox2.Text;
                ConfigurationManager.RefreshSection("appSettings");
                configuration.Save(ConfigurationSaveMode.Modified);
                MessageBox.Show("激活成功，请退出后重新打开软件","成功",MessageBoxButtons.OK,MessageBoxIcon.Information);
                Application.Exit();
            }
            else
            {
                MessageBox.Show("序列号不正确，请重新输入","错误",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// 对CPUID进行加密
        /// </summary>
        /// <param name="cpuid"></param>
        /// <returns></returns>
        private string encoding(string cpuid)
        {

        }
    }
}
