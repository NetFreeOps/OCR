using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using Win32;
using System.Configuration;

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

            if(checkid  == cpuid)
            {
                return true;
            }
            else
            {
                return false;
            }

           
        }
    }
}
