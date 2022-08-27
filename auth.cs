using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using Win32;
using System.Configuration;
using System.Security.Cryptography;

namespace OCR
{
    public class auth
    {
        
        public string getGUID()
        {
            SystemInfo systemInfo = new SystemInfo();
            //richTextBox1.AppendText("操作系统：" + systemInfo.operatingSystem.Caption + "\n");
            //richTextBox1.AppendText("系统ID：" + systemInfo.operatingSystem.SerialNumber + "\n");
            //richTextBox1.AppendText("操作系统平台：" + systemInfo.operatingSystem.OSLevel + "\n");
            //richTextBox1.AppendText("系统安装时间：" + systemInfo.operatingSystem.InstallDate + "\n");
            //richTextBox1.AppendText("系统最近启动时间：" + systemInfo.operatingSystem.LastBootUpTime + "\n");
            //richTextBox1.AppendText("系统时间：" + systemInfo.operatingSystem.LocalDateTime + "\n");
            //richTextBox1.AppendText("CPU：" + systemInfo.processor.Name + "\n");
            //richTextBox1.AppendText("CPU厂商：" + systemInfo.processor.Manufacturer + "\n");
            //richTextBox1.AppendText("CPU序列号：" + systemInfo.processor.SerialNumber + "\n");
            //richTextBox1.AppendText("物理内存：" + systemInfo.memory.TotalPhysicalMemory + "\n");

            string cpuid = systemInfo.processor.SerialNumber;
            
            return cpuid.ToString();
        }

        public bool authGUID()
        {
           Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string cpuid = getGUID();

            string checkid = configuration.AppSettings.Settings["license"].Value;

            if(checkid  == encoding(cpuid))
            {
                return true;
            }
            else
            {
                return false;
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
        public static string encry_DES(string sInputString, string key = "2bSzfR94", string iv = "qGkBcYfm")
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
