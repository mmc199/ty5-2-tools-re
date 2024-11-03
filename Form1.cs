using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ty5_2_tools_re
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            // 设置窗体和textBox_Link允许拖放
            this.AllowDrop = true;
            this.textBox1_Link.AllowDrop = true;
            this.textBox2_Target.AllowDrop = true;

            // 为textBox_Link和textBox_Target添加事件处理程序
            this.textBox1_Link.DragEnter += new DragEventHandler(textBox_DragEnter);
            this.textBox1_Link.DragDrop += new DragEventHandler(textBox_DragDrop);
            this.textBox2_Target.DragEnter += new DragEventHandler(textBox_DragEnter);
            this.textBox2_Target.DragDrop += new DragEventHandler(textBox_DragDrop);


            this.linkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel1_LinkClicked);


            this.backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);

            this.comboBox1.TextChanged += ComboBox1_TextChanged;

            this.textBox2_Target.TextChanged += TextBox2_Target_TextChanged;

            //// 重定向 Console 的输出到 textBox3_Log
            TextWriter writer = new TextBoxWriter(textBox3_Log);
            Console.SetOut(writer);
            Console.SetError(writer); // 重定向错误输出


        }

        // 定义 textBox2 的 TextChanged 事件处理程序
        private void TextBox2_Target_TextChanged(object sender, EventArgs e)
        {
            // 触发 ComboBox1_TextChanged 以更新相关控件
            ComboBox1_TextChanged(comboBox1, EventArgs.Empty);
        }

        private void ComboBox1_TextChanged(object sender, EventArgs e)
        {
            // 获取输入或选择的扩展名
            string extension = comboBox1.Text;

            var extensionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jgw", ".jpg" },
            { ".pgw", ".png" },
            { ".tfw", ".tif" }
        };



            // 获取文件扩展名对应的值
            string fileExtension = extensionMap.TryGetValue(extension, out var mappedExtension) ? mappedExtension : null;

            // 构建文件路径并更新 textBox1_Link 和 openFileDialog1
            string filePath = Path.Combine(textBox2_Target.Text, $"*{fileExtension}");
            textBox1_Link.Text = filePath;
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
            openFileDialog1.FileName = Path.GetFileName(filePath);

        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // 将链接标记为已访问
            this.linkLabel1.LinkVisited = true;

            // 使用浏览器打开链接
            System.Diagnostics.Process.Start(this.linkLabel1.Text);
        }

        // 创建一个安全更新控件的TextWriter类
        public class TextBoxWriter : TextWriter
        {
            private System.Windows.Forms.TextBox _textBox;
            private StringBuilder _buffer;

            public TextBoxWriter(System.Windows.Forms.TextBox textBox)
            {
                _textBox = textBox;
                _buffer = new StringBuilder();
            }

            public override void Write(char value)
            {
                _buffer.Append(value);
                if (value == '\n')
                {
                    Flush();
                }
            }

            public override void Flush()
            {
                _textBox.BeginInvoke(new Action(() =>
                {
                    _textBox.AppendText(_buffer.ToString());
                    _buffer.Remove(0, _buffer.Length);
                }));
            }

            public override Encoding Encoding => Encoding.UTF8;
        }




        private void textBox_DragEnter(object sender, DragEventArgs e)
        {
            // 检查拖动数据是否包含文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // 允许复制操作
            }
            else
            {
                e.Effect = DragDropEffects.None; // 不允许其他类型的拖动操作
            }
        }

        private void textBox_DragDrop(object sender, DragEventArgs e)
        {
            // 获取拖动的文件数组
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // 如果是文件，根据文件类型更新文本框内容
            if (files != null && files.Length > 0)
            {
                // 由于sender是object类型，我们需要将其转换为TextBox
                System.Windows.Forms.TextBox textBox = (System.Windows.Forms.TextBox)sender;

                if (sender == textBox1_Link)
                {
                    //// 检查文件是否为.xls扩展名
                    //if (!Path.GetExtension(files[0]).ToLower().Equals(".xls") && !Path.GetExtension(files[0]).ToLower().Equals(".xlsx"))
                    //{
                    //    // 异步显示消息提示，避免锁死文件浏览窗口
                    //    this.BeginInvoke(new MethodInvoker(delegate
                    //    {
                    //        MessageBox.Show("请选择.xls文件");
                    //    }));
                    //    return; // 终止处理
                    //}

                    textBox.Text = files[0]; // 添加文件路径


                    openFileDialog1.FileName = files[0];

                }
                else if (sender == textBox2_Target)
                {
                    if (File.Exists(files[0]))
                    {
                        // 获取文件所在目录
                        string directory = Path.GetDirectoryName(files[0]);
                        textBox.Text = directory; // 设置为文件所在目录
                        folderBrowserDialog1.SelectedPath = directory;
                    }
                    else if (Directory.Exists(files[0]))
                    {
                        // 如果拖入的是目录，直接设置为该目录
                        textBox.Text = files[0];
                        folderBrowserDialog1.SelectedPath = files[0];
                    }
                }
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {


            // 动态添加选项
            comboBox1.Items.Add(".jgw");
            comboBox1.Items.Add(".pgw");
            comboBox1.Items.Add(".tfw");

            // 强制用户选择索引0，防止不选择任何选项
            comboBox1.SelectedIndex = 0;

            SetLinkLabel1TextAsync("http://sasmaptools.mmc199.com");
            //CheckFileAndFolder();

            ComboBox1_TextChanged(comboBox1, EventArgs.Empty);

        }

        private void SetLinkLabel1TextAsync(string url)
        {
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error == null)
                {
                    linkLabel1.Text = e.Result;
                }
                else
                {
                    linkLabel1.Text = "https://github.com/mmc199/sasmap/releases/latest/download/ty5-2-tools.zip";
                }
                client.Dispose();
            };
            client.DownloadStringAsync(new Uri(url));
        }


        //private void CheckFileAndFolder()
        //{
        //    string appPath = AppDomain.CurrentDomain.BaseDirectory;
        //    string filePath = Path.Combine(appPath, "ty5-2-tools.zip");
        //    string folderPath = Path.Combine(appPath, "ty5-2-Maps");

        //    // 检查文件是否存在并更新UI
        //    if (File.Exists(filePath))
        //    {
        //        textBox1_Link.Text = filePath;
        //        openFileDialog1.InitialDirectory = Path.GetDirectoryName(filePath);
        //        openFileDialog1.FileName = Path.GetFileName(filePath);
        //    }

        //    // 如果文件夹不存在则创建
        //    Directory.CreateDirectory(folderPath);

        //    //// 更新UI
        //    //textBox2_Target.Text = folderPath;
        //    //folderBrowserDialog1.SelectedPath = folderPath;
        //}










        private void button1_Click(object sender, EventArgs e)
        {
            //openFileDialog1.Filter = "图源xls|*.xls;*.xlsx";

            // 检查 textBox1_Link 中的路径是否存在
            if (File.Exists(textBox1_Link.Text))
            {
                // 设置 OpenFileDialog 的初始目录为 textBox1_Link 中的路径
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(textBox1_Link.Text);
            }
            else
            {
                // 如果 textBox1_Link 中的路径不存在，可以设置为默认路径或给出提示
                openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                // MessageBox.Show("指定的路径不存在，将使用默认目录。");
            }

            // 显示 OpenFileDialog
            DialogResult result = openFileDialog1.ShowDialog();

            // 如果用户选择了文件，更新 textBox1_Link 的内容
            if (result == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                textBox1_Link.Text = openFileDialog1.FileName;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            // 检查 textBox2_Target 中的路径是否存在
            if (Directory.Exists(textBox2_Target.Text))
            {
                // 设置 FolderBrowserDialog 的初始目录为 textBox2_Target 中的路径
                folderBrowserDialog1.SelectedPath = textBox2_Target.Text;
            }

            // 显示 FolderBrowserDialog
            DialogResult result = folderBrowserDialog1.ShowDialog();

            // 如果用户选择了文件夹，更新 textBox2_Target 的内容
            if (result == DialogResult.OK && !string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
            {
                textBox2_Target.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {


            bool error = false;

            if (textBox1_Link.Text == "" || textBox2_Target.Text == "") // 检查是否在未做任何操作的情况下点击了提交
            {
                error = true;
                MessageBox.Show("必须指定文件/目录。");
            }
            if (!Directory.Exists(textBox2_Target.Text)) // 检查目标目录是否存在
            {
                error = true;
                MessageBox.Show("您在“位置”文本框中指定的文件夹不存在。");
            }
            //if (!File.Exists(textBox1_Link.Text)) // 检查目标目录是否存在
            //{
            //    error = true;
            //    MessageBox.Show("您在“源”文本框中指定的文件不存在。");
            //}
            // 如果没有错误，我们现在创建实际链接
            if (error == false)
            {

                //string scriptDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
                //Console.WriteLine($"Script directory: {scriptDir}");

                // 获得Excel文件路径
                ////string excelPath = Path.Combine(scriptDir, "ty5-2.xls");
                //string excelPath = textBox1_Link.Text;
                //string scriptDir = textBox2_Target.Text;


                // 获取目标目录和扩展名
                string sourceFilePath = textBox1_Link.Text;
                string targetDirectory = textBox2_Target.Text;
                string extension = comboBox1.Text;

                // 构建目标子目录路径为 targetDirectory-3
                string targetSubDirectory = targetDirectory + "-3";

                Directory.CreateDirectory(targetSubDirectory);



                // 定义文件扩展名与配套文件扩展名的映射，支持多种配套扩展名
                var extensionMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jgw", new List<string> { ".jpg" } },
            { ".pgw", new List<string> { ".png" } },
            { ".tfw", new List<string> { ".tif", ".tiff" } } // 支持 .tif 和 .tiff
        };



                // 查找所有指定扩展名的文件
                var files = Directory.GetFiles(targetDirectory, $"*{extension}");

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string targetFilePath = Path.Combine(targetSubDirectory, fileName);

                    // 复制扩展名文件到目标子目录
                    try
                    {
                        File.Copy(file, targetFilePath, true);
                        Console.WriteLine($"已复制 {file} 到 {targetFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"复制文件时出错: {ex.Message}");
                    }


                    // 处理对应的 .prj 文件
                    string correspondingPrjFilePath = Path.ChangeExtension(file, ".prj");
                    if (File.Exists(correspondingPrjFilePath))
                    {
                        string targetPrjPath = Path.Combine(targetSubDirectory, Path.GetFileName(correspondingPrjFilePath));
                        try
                        {
                            File.Copy(correspondingPrjFilePath, targetPrjPath, true);
                            Console.WriteLine($"已复制 {correspondingPrjFilePath} 到 {targetPrjPath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"复制 .prj 文件时出错: {ex.Message}");
                        }
                    }



                    // 创建软链接部分 - 手动检查映射扩展名
                    List<string> companionExtensions = null;
                    if (extensionMap.ContainsKey(extension))
                    {
                        companionExtensions = extensionMap[extension];
                    }

                    if (companionExtensions != null)
                    {
                        // 遍历配套扩展名列表，尝试创建每种可能的软链接
                        foreach (var companionExtension in companionExtensions)
                        {
                            // 构建对应的映射扩展名文件路径
                            string correspondingFilePath = Path.ChangeExtension(file, companionExtension);
                            string targetLinkPath = Path.Combine(targetSubDirectory, Path.GetFileName(correspondingFilePath));


                            // 检查链接或文件是否已存在，并删除
                            if (File.Exists(targetLinkPath) || Directory.Exists(targetLinkPath))
                            {
                                File.Delete(targetLinkPath); // 删除文件或符号链接
                            }

                            // 检查映射扩展名文件是否存在，若存在则创建软链接
                            if (File.Exists(correspondingFilePath))
                            {
                                try
                                {
                                    Process processSymlink = new Process();
                                    ProcessStartInfo startInfoSymlink = new ProcessStartInfo
                                    {
                                        FileName = "cmd.exe",
                                        CreateNoWindow = true,
                                        UseShellExecute = false,
                                        RedirectStandardOutput = true
                                    };

                                    // 构建 MKLINK 命令参数
                                    startInfoSymlink.Arguments = $"/C MKLINK \"{targetLinkPath}\" \"{correspondingFilePath}\"";

                                    processSymlink.StartInfo = startInfoSymlink;
                                    processSymlink.Start();
                                    processSymlink.WaitForExit();

                                    string output = processSymlink.StandardOutput.ReadToEnd();
                                    Console.WriteLine(output);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"创建符号链接时出错: {ex.Message}");
                                }
                            }
                        }
                    }


                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string downloadUrl = linkLabel1.Text;
            string fileName = "ty5-2-tools.zip";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            if (File.Exists(filePath))
            {
                var downloadresult = MessageBox.Show($"本地已经存在 {fileName} 文件, 解压前是否要重新下载?", "覆盖下载?", MessageBoxButtons.YesNoCancel);

                if (downloadresult == DialogResult.No)
                {
                    var extractResult = MessageBox.Show("是否要解压现有的压缩文件?", "确认解压", MessageBoxButtons.YesNo);

                    if (extractResult == DialogResult.Yes)
                    {
                        ExtractFile(filePath);
                    }
                    return; // 用户选择不覆盖且不下载
                }
                else if (downloadresult == DialogResult.Cancel)
                {
                    return; // 用户选择取消，退出操作
                }
                // 用户选择是（覆盖），继续下载
            }

            // 用户选择覆盖或文件不存在时开始下载


            button4.Text = "下载中…";
            //UpdateButtonText("下载中…");
            button4.Enabled = false; // 禁用下载按钮

            backgroundWorker1.RunWorkerAsync(new string[] { downloadUrl, filePath });
        }




        public void ExtractFile(string filePath)
        {
            string extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(filePath));

            try
            {
                // 创建目标目录（如果不存在）
                Directory.CreateDirectory(extractPath);

                using (ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(filePath)))
                {
                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        string entryFileName = Path.Combine(extractPath, entry.Name);
                        if (entry.IsDirectory)
                        {
                            Directory.CreateDirectory(entryFileName);
                        }
                        else
                        {
                            using (FileStream streamWriter = File.Create(entryFileName))
                            {
                                byte[] buffer = new byte[4096];
                                int size;
                                while ((size = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    streamWriter.Write(buffer, 0, size);
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("文件解压成功。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解压过程中出现异常: {ex.Message}");
            }
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args = e.Argument as string[];
            string downloadUrl = args[0];
            string filePath = args[1];

            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                string redirectedUrl = GetRedirectedUrl(downloadUrl);


                DownloadFile(redirectedUrl, filePath);


                e.Result = new DownloadResult { Success = true, FilePath = filePath };
            }
            catch (Exception ex)
            {
                e.Result = new DownloadResult { Success = false, ErrorMessage = ex.Message };
            }
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            button4.Text = "解压模板";
            //UpdateButtonText("下载模板");
            button4.Enabled = true; // 重新启用下载按钮

            DownloadResult result = e.Result as DownloadResult;
            if (result.Success)
            {
                //MessageBox.Show("文件下载成功。");

                //textBox1_Link.Text = result.FilePath;
                //openFileDialog1.InitialDirectory = Path.GetDirectoryName(result.FilePath);
                //openFileDialog1.FileName = Path.GetFileName(result.FilePath);
            }
            else
            {
                MessageBox.Show("下载过程中出现异常: " + result.ErrorMessage);
            }
        }

        private string GetRedirectedUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                return (response.StatusCode == HttpStatusCode.MovedPermanently ||
                    response.StatusCode == HttpStatusCode.Found ||
                    response.StatusCode == HttpStatusCode.Redirect) ?
                    response.Headers["Location"] : url;
            }
        }

        private void DownloadFile(string url, string outputPath)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, outputPath);
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private class DownloadResult
        {
            public bool Success { get; set; }
            public string FilePath { get; set; }
            public string ErrorMessage { get; set; }
        }

        //private void UpdateButtonText(string text)
        //{
        //    if (button4.InvokeRequired)
        //    {
        //        button4.Invoke(new Action<string>(UpdateButtonText), text);

        //        MessageBox.Show("Invoke成功。");
        //    }
        //    else
        //    {
        //        button4.Text = text;
        //    }
        //}

    }

}
