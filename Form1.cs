using PaddleOCRSharp;
using System.Security.Cryptography.Xml;
using System.Text;

namespace OCR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //使用默认中英文V3模型
            //  OCRModelConfig config = null;
            //使用默认参数
            // OCRParameter oCRParameter = new OCRParameter();
            //识别结果对象
            // OCRResult ocrResult = new OCRResult();
            //PaddleOCREngine engine = new PaddleOCREngine(config, oCRParameter);



        }

        private OCRModelConfig _modelConfig;

        private OCRParameter _parameter;

        private OCRResult _result;

        private PaddleOCREngine _engine;

        private List<resultEntry> resultEntries = new List<resultEntry>();

        private void Form1_Load(object sender, EventArgs e)
        {
            _modelConfig = null;

            _parameter = new OCRParameter();

            _result = new OCRResult();

            _engine = new PaddleOCREngine(_modelConfig, _parameter);


            //建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。     
            //  PaddleOCREngine engine = new PaddleOCREngine(config, oCRParameter);
            // {
            //     ocrResult = engine.DetectText(ofd.FileName);
            // }
            // if (ocrResult != null) MessageBox.Show(ocrResult.Text, "识别结果");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.*|*.bmp;*.jpg;*.jpeg;*.tiff;*.tiff;*.png";
            if (ofd.ShowDialog() != DialogResult.OK) return;




            resultEntry entry = new resultEntry();

            entry = HandleImg(ofd.FileName);


            resultEntries.Add(entry);

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = resultEntries;


            foreach (DataGridViewColumn item in dataGridView1.Columns)
            {
                item.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            //textBox1.Text = result.ToString();
            // MessageBox.Show(@string.ToString(), "识别结果");
        }
        /// <summary>
        /// 处理时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string convertToDateTime(string str)
        {
            string[] strs = str.Split('.');
            string year = strs[0].Substring(0, 4);
            string month = strs[0].Substring(5, 2);
            string day = strs[0].Substring(8, 2);
            string hour = strs[0].Substring(10, 2);
            string minute = strs[0].Substring(13, 2);
            string second = strs[0].Substring(16, 2);
            return year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second;
        }
        /// <summary>
        /// 识别文件结果
        /// </summary>
        public resultEntry HandleImg(string filePath)
        {

            _result = _engine.DetectText(filePath);
            StringBuilder @string = new StringBuilder();

            _result.TextBlocks.ForEach(block =>
            {
                @string.AppendLine(block.Text);

            });

            List<string> lines = new List<string>();


            resultEntry entry = new resultEntry();


            int len = _result.TextBlocks.Count;

            //结果处理
            for (int i = 0; i < len; i++)
            {
                if (_result.TextBlocks[i].Text == "核酸检测信息")
                {
                    //result.AppendLine("查询时间：" + _result.TextBlocks[i + 1].Text);

                    //result.AppendLine("查询人：" + _result.TextBlocks[i + 2].Text);
                    //result.AppendLine("身份证号：" + _result.TextBlocks[i + 4].Text);

                    lines.Add(_result.TextBlocks[i + 1].Text);
                    lines.Add(_result.TextBlocks[i + 2].Text);
                    lines.Add(_result.TextBlocks[i + 4].Text);



                }
                if (_result.TextBlocks[i].Text.Contains("检测时间："))
                {
                    //获取第一个：的位置
                    int index = _result.TextBlocks[i].Text.IndexOf("：");
                    //截取第一个：的位置之后的字符串
                    string str = _result.TextBlocks[i].Text.Substring(index + 1);
                    str = convertToDateTime(str);
                    //result.AppendLine("检测时间：" + str);
                    lines.Add(str);

                }
                if (_result.TextBlocks[i].Text.Contains("检测机构："))
                {
                    //获取第一个：的位置
                    int index = _result.TextBlocks[i].Text.IndexOf("：");
                    //截取第一个：的位置之后的字符串
                    string str = _result.TextBlocks[i].Text.Substring(index + 1);
                    // result.AppendLine("检测机构：" + str);
                    lines.Add(str);
                }
                if (_result.TextBlocks[i].Text.Contains("检测结果："))
                {
                    //获取第一个：的位置
                    int index = _result.TextBlocks[i].Text.IndexOf("：");
                    //截取第一个：的位置之后的字符串
                    string str = _result.TextBlocks[i].Text.Substring(index + 1);
                    //result.AppendLine("检测结果：" + str);
                    lines.Add(str);

                }
            }
            //实体类赋值
            entry.queryTime = lines[0];
            entry.queryUser = lines[1];
            entry.userIDcard = lines[2];
            entry.checkTime1 = lines[3];
            entry.chackLocation1 = lines[4];
            entry.checkResult1 = lines[5];
            entry.checkTime2 = lines[6];
            entry.checkLocation2 = lines[7];
            entry.checkResult2 = lines[8];
            entry.checkTime3 = lines[9];
            entry.checkLocation3 = lines[10];
            entry.checkResult3 = lines[11];


            return entry;


        }

        //打开文件夹
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK) return;
            string path = fbd.SelectedPath;
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                resultEntry entry = new resultEntry();
                entry = HandleImg(file.FullName);
                resultEntries.Add(entry);
            }
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = resultEntries;
            foreach (DataGridViewColumn item in dataGridView1.Columns)
            {
                item.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        /// <summary>
        /// 获取文件夹中的文件名
        /// </summary>
        public List<string> GetFileNameListFromFolder(string folderPath)
        {

            List<string> fileNameList = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                fileNameList.Add(file.Name);
                //fileNameList += file.FullName + ";";
            }
            return fileNameList;
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

     
    }
}