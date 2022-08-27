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
using System.Security.Cryptography;

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


            string calc_check = encoding(textBox1.Text);
            if (calc_check == textBox2.Text)
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
            //string md5_str = encry_MD5(cpuid);
            //string check_code = encry_DES(md5_str);
            //return check_code;
            string des_str = encry_DES(cpuid);
             string check_code = encry_SHA256(des_str);
            return check_code;
            
        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string encry_SHA256(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = SHA256.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="data">加密数据</param>
        /// <param name="key">8位字符的密钥字符串</param>
        /// <param name="iv">8位字符的初始化向量字符串</param>
        /// <returns></returns>
        public static string encry_DES(string sInputString, string key= "2bSzfR94", string iv= "qGkBcYfm")
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(sInputString);

                // DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
                TripleDES DES = TripleDES.Create();

                DES.Key = ASCIIEncoding.ASCII.GetBytes(key);

                DES.IV = ASCIIEncoding.ASCII.GetBytes(iv);

                ICryptoTransform desencrypt = DES.CreateEncryptor();

                byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);

                return BitConverter.ToString(result);
            }
            catch { }

            return "转换出错！";

        }


    }
}
