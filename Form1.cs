using Microsoft.VisualBasic;
using PaddleOCRSharp;
using System.Data;
using System.Drawing;
using System.Security.AccessControl;
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
            this.DoubleBuffered = true;


        }

        Thread drawThread = null;


        private OCRModelConfig _modelConfig;

        private OCRParameter _parameter;

        private OCRResult _result;

        private PaddleOCREngine _engine;

        private List<resultEntry> resultEntries = new List<resultEntry>();

        private int currentIndex = 0;

        private int totalIndex = 0;

        /// <summary>
        /// 选中的图片路径
        /// </summary>
        public string selectedPath = "";

        private void Form1_Load(object sender, EventArgs e)
        {
            // _modelConfig = null;
            try
            {
                _modelConfig = new OCRModelConfig();
                string root = System.IO.Path.GetDirectoryName(typeof(OCRModelConfig).Assembly.Location);
                string modelPathroot = root + @"\server";
                _modelConfig.det_infer = modelPathroot + @"\ch_PP-OCRv3_det_infer";
                _modelConfig.cls_infer = modelPathroot + @"\ch_ppocr_mobile_v2.0_cls_infer";
                _modelConfig.rec_infer = modelPathroot + @"\ch_PP-OCRv3_rec_infer";
                _modelConfig.keys = modelPathroot + @"\ppocr_keys.txt";


                _parameter = new OCRParameter();



                _result = new OCRResult();

                _engine = new PaddleOCREngine(_modelConfig, _parameter);

                auth formauth = new auth();

                //激活认证
                if (formauth.authGUID())
                {
                    this.Text = "核酸检测结果识别及导出软件-状态：已激活";
                }
                else
                {
                    this.Text = "核酸检测结果识别及导出软软件-状态：未激活，禁止导出";
                    button3.Enabled = false;
                    button4.Enabled = false;
                }

            }
            catch (Exception)
            {

                MessageBox.Show("模型加载失败，请检查模型是否存在");
                
            }
           

            //listView2.Columns.Add("状态",100,HorizontalAlignment.Left);
            //listView2.Columns.Add("路径",300,HorizontalAlignment.Left);


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

            List<string> resultList = HandleImg(ofd.FileName,0);

            HandleResult(resultList);


        }
        /// <summary>
        /// 处理时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string convertToDateTime(string str)
        {
            str = str.Replace(" ","");
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
            int len = listView2.Items.Count;
            totalIndex = len;
            for (int i = 0; i < len; i++)
            {
                currentIndex = i;

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                sw.Start();
                List<string> resultList = HandleImg(listView2.Items[i].SubItems[1].Text);
                sw.Stop();
                TimeSpan timeSpan = sw.Elapsed;

                double handleTime = timeSpan.TotalMilliseconds;

                labelHandleTime.Text = handleTime.ToString() + "ms";

                HandleResult(resultList);
                label4.Text = (i + 1).ToString() + "/" + len;
                progressBar1.Value = i + 1;
            }

        }

        /// <summary>
        /// 识别文件结果
        /// </summary>
        public List<string> HandleImg(string filePath,int? type = 1)
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

            if (type == 0)
            {
                for (int i = 0; i < _result.TextBlocks.Count; i++)
                {
                    @string.AppendLine(_result.TextBlocks[i].ToString());
                }
                MessageBox.Show(@string.ToString());
            }

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
                listView2.Items[currentIndex].ImageIndex = 2;
                
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

            if (result.Count != 12)
            {
                listView2.Items[currentIndex].ImageIndex = 2;

                return;
                
            }

            for (int i = 0; i < result.Count; i++)
            {
                dataGridView1.Rows[indexs].Cells[i].Value = result[i];
                listView2.Items[currentIndex].ImageIndex = 1;

            }

           

            foreach (DataGridViewColumn item in dataGridView1.Columns)
            {
                item.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            //全部完成后提示
            if (currentIndex == totalIndex - 1)
            {
              //  MessageBox.Show(totalIndex + "个图片识别完成");
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
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
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);

           

            listView2.Clear();
            listView2.Columns.Add("状态", 100, HorizontalAlignment.Left);
            listView2.Columns.Add("路径",750,HorizontalAlignment.Left);


            listView2.BeginUpdate();

            foreach (FileInfo file in files)
            {
                if (Regex.IsMatch(file.Name, @"^.+\.(jpg|png|bmp|jpeg|tiff)$"))
                {
                    ListViewItem viewItem = new ListViewItem();

                    viewItem.ImageIndex = 0;

                    viewItem.Text = "待处理";
                    viewItem.SubItems.Add(file.FullName);
                    listView2.Items.Add(viewItem);
                }

            }
            listView2.EndUpdate();


        }

        /// <summary>
        /// 获取文件夹中的文件名
        /// </summary>
        public List<string> GetFileNameListFromFolder(string folderPath)
        {


            List<string> fileNameList = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles("", SearchOption.AllDirectories);
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
            int len = listView2.Items.Count;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = len;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;


            drawThread = new Thread(new ThreadStart(handleImgBatch));

            drawThread.IsBackground = true;

            drawThread.Start();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
        /// <summary>
        /// 导出EXCEL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {

            string path = Environment.CurrentDirectory + "/template.xlsx";

            List<resultEntry> resultEntries = new List<resultEntry>();


            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                resultEntry result = new resultEntry();
                result.queryTime = Convert.ToString( dataGridView1.Rows[i].Cells[0].Value);
                result.queryUser = Convert.ToString( dataGridView1.Rows[i].Cells[1].Value);
                result.userIDcard = Convert.ToString( dataGridView1.Rows[i].Cells[2].Value);
                result.checkTime1 = Convert.ToString( dataGridView1.Rows[i].Cells[3].Value);
                result.chackLocation1 = Convert.ToString( dataGridView1.Rows[i].Cells[4].Value);
                result.checkResult1 = Convert.ToString( dataGridView1.Rows[i].Cells[5].Value);
                result.checkTime2 = Convert.ToString( dataGridView1.Rows[i].Cells[6].Value);
                result.checkLocation2 = Convert.ToString( dataGridView1.Rows[i].Cells[7].Value);
                result.checkResult2 = Convert.ToString( dataGridView1.Rows[i].Cells[8].Value);
                result.checkTime3 = Convert.ToString( dataGridView1.Rows[i].Cells[9].Value);
                result.checkLocation3 = Convert.ToString( dataGridView1.Rows[i].Cells[10].Value);
                result.checkResult3 = Convert.ToString( dataGridView1.Rows[i].Cells[11].Value);
                resultEntries.Add(result);
            }



            var value = new Dictionary<string, object>()
            {
                ["resultList"] = resultEntries
            };
           


           
            

            if (File.Exists(path))
            {
                // MessageBox.Show(path);
                try
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    // saveFileDialog.FileName = "xxx.xlsx";
                    saveFileDialog.Filter = "Excel2007(*.xlsx)|*.xlsx";
                    saveFileDialog.FileName = DateTime.Now.ToString("yyyymmddHHmmss") + ".xlsx";
                    saveFileDialog.ShowDialog();

                    MiniExcelLibs.MiniExcel.SaveAsByTemplate(saveFileDialog.FileName, path, value);

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.ToString(), "出错了", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("模板不存在，无法导出数据");
            }

            //MiniExcelLibs.MiniExcel.SaveAsByTemplate(textBox2.Text,, value);

        }
        /// <summary>
        /// 选择输出文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK) return;
            string path = fbd.SelectedPath;
            //textBox2.Text = path;
        }

       

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            selectedPath = listView2.SelectedItems[0].SubItems[1].Text;
           // MessageBox.Show(selectedPath);

            ImgShowDialog imgShow = new ImgShowDialog(selectedPath);
            imgShow.Show();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

       

        private void 菜单ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 激活软件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            authDialog authDialog = new authDialog();
            authDialog.Show();
        }
    }
}