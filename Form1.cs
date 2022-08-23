using Microsoft.VisualBasic;
using PaddleOCRSharp;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace OCR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;



        }

        Thread drawThread = null;


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



        }
        /// <summary>
        /// 单个识别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.*|*.bmp;*.jpg;*.jpeg;*.tiff;*.tiff;*.png";
            if (ofd.ShowDialog() != DialogResult.OK) return;




            resultEntry entry = new resultEntry();

            List<string> resultList = HandleImg(ofd.FileName);

            HandleResult(resultList);


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
        /// 批量处理
        /// </summary>
        public void handleImgBatch()
        {
            int len = listBox1.Items.Count;

            for (int i = 0; i < len; i++)
            {
                List<string> resultList = HandleImg((string)listBox1.Items[i]);
                HandleResult(resultList);
                label4.Text = (i + 1).ToString() + "/" + len;
                progressBar1.Value = i + 1;


            }
        }

        /// <summary>
        /// 识别文件结果
        /// </summary>
        public List<string> HandleImg(string filePath)
        {

            _result = _engine.DetectText(filePath);
            StringBuilder @string = new StringBuilder();

            _result.TextBlocks.ForEach(block =>
            {
                @string.AppendLine(block.Text);

            });

            bool isStart = false;


            List<string> lines = new List<string>();


            resultEntry entry = new resultEntry();


            int len = _result.TextBlocks.Count;

            //结果处理
            for (int i = 0; i < len; i++)
            {

                //获取查询时间、查询人
                if (_result.TextBlocks[i].Text.Contains('月') && _result.TextBlocks[i].Text.Contains('日'))
                {
                    lines.Add(_result.TextBlocks[i].Text);
                    lines.Add(_result.TextBlocks[i + 1].Text);
                    isStart = true;

                }
                //获取查询身份证号
                if (Regex.IsMatch(_result.TextBlocks[i].Text, @"(^[1-9]\d{5}(18|19|20)\d{2}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$)"))
                {
                    lines.Add(_result.TextBlocks[i].Text);
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
            if (isStart)
            {
                try
                {
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show("在识别文件：" + filePath + "时发生错误");
                    // return entry;
                    return lines;
                }
            }
            else
            {
                MessageBox.Show("截图不完整");
            }

            // return entry;
            return lines;
        }

        /// <summary>
        /// 结果填充到界面
        /// </summary>
        /// <param name="result"></param>
        public void HandleResult(List<string> result)
        {




            int indexs = dataGridView1.Rows.Add();

            for (int i = 0; i < result.Count; i++)
            {
                dataGridView1.Rows[indexs].Cells[i].Value = result[i];
            }


            foreach (DataGridViewColumn item in dataGridView1.Columns)
            {
                item.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
        /// <summary>
        /// 选择识别路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK) return;
            string path = fbd.SelectedPath;
            textBox1.Text = path;
            // GetFileNameListFromFolder(path);

            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*", System.IO.SearchOption.AllDirectories);

            listBox1.Items.Clear();

            foreach (FileInfo file in files)
            {
                if (Regex.IsMatch(file.Name, @"^.+\.(jpg|png|bmp|jpeg|tiff)$"))
                {
                    listBox1.Items.Add(file.FullName);

                }

            }


        }

        /// <summary>
        /// 获取文件夹中的文件名
        /// </summary>
        public List<string> GetFileNameListFromFolder(string folderPath)
        {


            List<string> fileNameList = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles("", System.IO.SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                fileNameList.Add(file.FullName);
            }
            return fileNameList;
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        /// <summary>
        /// 调用图片识别函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {

            int len = listBox1.Items.Count;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = len;

            //Thread t = new Thread(new ThreadStart(handleImgBatch));

            drawThread = new Thread(new ThreadStart(handleImgBatch));

            drawThread.IsBackground = true;

            drawThread.Start();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}