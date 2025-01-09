using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Aruco;
using Emgu.CV.Structure;
using AForge.Video.DirectShow;
using AForge.Video;
using Emgu.CV.Util;
using System.Runtime.InteropServices;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace ArucoPointer
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private bool _isCapturing = false;
        private bool isConnected = false;
        private VideoCaptureDevice videodevice;
        private FilterInfoCollection cameraDevices;

        private int imagesCaptured = 0;
        private bool capturingImages = false;
        //private List<Mat> capturedImages = new List<Mat>();

        // 距离测算相关变量
        private int _shotCount;
        private int _frameCount;
        private List<Mat> _capturedImages = new List<Mat>();

        private Size _imageSize = Size.Empty;

        private Dictionary _dictionary;
        private GridBoard _gridBoard;
        private DetectorParameters _detectorParameters;
        private Mat _cameraMatrix = new Mat();
        private Mat _distCoeffs = new Mat();
        private List<int> _markerCounterPerFrame = new List<int>();

        double[,] read_cameraMatrix = new double[3, 3];
        double[] read_distCoeffs = new double[5];

        private BackgroundWorker backgroundWorker;
        public Form1()
        {
            InitializeComponent();
            InitializeDevices();
            InitializeAruco();

            menuStrip1.Visible = false;
            panel1.Visible = false;
            panel2.Visible = false;
            progressBar1.Visible = false;

            pictureBox1.Image = Properties.Resources.icon;
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

            button2.Text = "カメラ接続";
            label1.Text = "カメラを選択して接続してください";
        }

        private void InitializeDevices()
        {
            cameraDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            comboBox1.Items.Clear();
            if (cameraDevices.Count == 0)
            {
                MessageBox.Show("ビデオ入力装置は見つかりません。");
                comboBox1.Enabled = false;
            }
            else
            {
                foreach (FilterInfo device in cameraDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
                comboBox1.SelectedIndex = 1;
                comboBox1.Enabled = true;
            }
        }

        private void InitializeAruco()
        {
            _dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_50);
            _gridBoard = new GridBoard(5, 7, 0.04f, 0.01f, _dictionary);
            _detectorParameters = DetectorParameters.GetDefault();
        }

        private void videodevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = frame;

            if (capturingImages && imagesCaptured < _shotCount)
            {
                Mat matFrame = frame.ToMat();
                _capturedImages.Add(matFrame);
                imagesCaptured++;
                if (imagesCaptured >= _shotCount)
                {
                    capturingImages = false;
                    // 调用相机标定方法
                    CalibrateCamera();
                }
            }
            //if (_cameraMatrix != null && _distCoeffs != null)
            //{
            //    Mat matFrame = frame.ToMat();
            //    DetectAruco(matFrame); // 检测 ArUco 标记并绘制箭头
            //}
        }

        // 连接相机按钮
        private void button2_Click(object sender, EventArgs e)
        {
            string selectedText = comboBox1.SelectedItem.ToString();
            if (isConnected)
            {
                DisconnectCamera();
                button1.Visible = false;
                label1.Text = $"カメラを切断しました";
                button2.Text = "カメラ接続";
                isConnected = false;
                comboBox1.Enabled = true;
            }
            else
            {
                ConnectCamera();
                button1.Visible = true;
                label1.Text = $"{selectedText}を接続しました";
                button2.Text = "カメラ切断";
                isConnected = true;
                comboBox1.Enabled = false;
            }
            // Form2 form2 = new Form2();
            // form2.ShowDialog(); // 模态显示新页面（阻塞当前页面的交互）
        }

        private bool ConnectCamera()
        {
            try
            {
                label2.Visible = true;
                comboBox1.Visible = true;
                //连接相机部分
                int deviceIndex = comboBox1.SelectedIndex;
                if (deviceIndex < 0 || deviceIndex >= cameraDevices.Count)
                {
                    MessageBox.Show("無効なデバイスの選択です。");
                    return false;
                }
                
                string devicePath = cameraDevices[deviceIndex].MonikerString;
                _capture = new VideoCapture(deviceIndex, VideoCapture.API.DShow);

                if (!_capture.IsOpened)
                {
                    MessageBox.Show("接続失敗: Unable to open camera.");
                    return false;
                }
                _capture.ImageGrabbed += ProcessedFrame;
                _capture.Start();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("接続失敗: " + ex.Message);
                return false;
            }

            //videodevice = new VideoCaptureDevice(devicePath);

            ////int selectedDeviceIndex = comboBox1.SelectedIndex;
            ////_videoDevice = new VideoCaptureDevice(_videoDevices[deviceIndex].MonikerString);
            //videodevice.NewFrame += videodevice_NewFrame;
            //videodevice.Start();

            // 订阅 NewFrame 事件
            //videodevice.NewFrame += new NewFrameEventHandler(videodevice_NewFrame);
            //videodevice.Start();

            //return true;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("接続失敗: " + ex.Message);
            //    return false;
            //}
        }

        private void DisconnectCamera()
        {
            try
            {
                if (_capture != null && _capture.IsOpened)
                {
                    // 切断相机部分代码
                    //videodevice.SignalToStop();
                    //videodevice.WaitForStop();
                    //videodevice = null;
                    _capture.ImageGrabbed -= ProcessedFrame;
                    _capture.Stop();
                    _capture.Dispose();
                    _capture = null;
                }
                pictureBox1.Invoke((MethodInvoker)delegate
                {
                    pictureBox1.Image?.Dispose(); 
                    pictureBox1.Image = Properties.Resources.icon; 
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                });
                
                isConnected = false;
                menuStrip1.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("切断失敗: " + ex.Message);
            }
        }
        private void ProcessedFrame(object sender, EventArgs e)
        {
            if (_capture != null && _capture.IsOpened)
            {
                Mat frame = new Mat();
                _capture.Retrieve(frame);

                if (!frame.IsEmpty)
                {
                    pictureBox1.Invoke((MethodInvoker)delegate
                    {
                        // Detect Aruco markers
                        using (VectorOfInt ids = new VectorOfInt())
                        using (VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF())
                        using (VectorOfMat rvecs = new VectorOfMat())
                        using (VectorOfMat tvecs = new VectorOfMat())
                        {
                            ArucoInvoke.DetectMarkers(frame, _dictionary, corners, ids, _detectorParameters);

                            if (ids.Size > 0)
                            {
                                // Draw detected markers
                                ArucoInvoke.DrawDetectedMarkers(frame, corners, ids, new MCvScalar(0, 255, 0));

                                // Store the frame for calibration
                                _capturedImages.Add(frame.Clone());
                            }
                            if (isPoseEstimationEnabled)
                            {
                                EstimatePoseAndProcess(corners, ids, frame);
                            }
                            //EstimatePoseAndProcess(corners, ids, frame);
                        }

                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = frame.ToBitmap();
                    });

                    if (capturingImages && imagesCaptured < _shotCount)
                    {
                        _capturedImages.Add(frame.Clone());
                        imagesCaptured++;
                        if (imagesCaptured >= _shotCount)
                        {
                            capturingImages = false;
                            CalibrateCamera();
                        }
                    }
                }
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        // 输入的拍照次数和帧数
        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int _shotCount))
            {
                Console.WriteLine("摄影枚数： " + _shotCount);
            }
            else
            {
                Console.WriteLine("请重新输入");
            }
        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            if (int.TryParse(textBox2.Text, out int _frameCount))
            {
                Console.WriteLine("摄影间隔： " + _frameCount);
            }
            else
            {
                Console.WriteLine("请重新输入");
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = comboBox1.SelectedItem.ToString();
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            toolStripMenuItem1_Click(sender, e);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            toolStripMenuItem2.BackColor = Color.FromArgb(227, 227, 227);
            toolStripMenuItem1.BackColor = Color.FromArgb(240, 240, 240);
            menuStrip1.Visible = true;
            panel1.Visible = false;
            panel2.Visible = true;
            button1.Visible = false;
            progressBar1.Visible = false;
            label1.Text = $"测距balabal";
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.BackColor = Color.FromArgb(227, 227, 227);
            toolStripMenuItem2.BackColor = Color.FromArgb(240, 240, 240);
            menuStrip1.Visible = true;
            panel1.Visible = true;
            panel2.Visible = false;
            button1.Visible = false;
            label1.Text = $"指示棒でPivot操作の準備ができたら開始ボタンを押してください";


        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        // 校准
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // 相机参数计算
        private async void button3_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out _shotCount) && int.TryParse(textBox2.Text, out _frameCount))
            {
                button3.Enabled = false;
                progressBar1.Value = 0;
                
                //PauseRealTimeCapture();

                _capturedImages.Clear();
                _isCapturing = true;
                await CaptureImagesAsync();
            }
            else
            {
                MessageBox.Show("请输入有效的拍照次数和帧间隔。");
            }
        }
        private void PauseRealTimeCapture()
        {
            if (_capture != null && _capture.IsOpened)
            {
                _capture.ImageGrabbed -= ProcessedFrame; // 暂停实时画面
            }
        }
        private void ResumeRealTimeCapture()
        {
            if (_capture != null && _capture.IsOpened)
            {
                _capture.ImageGrabbed += ProcessedFrame; // 恢复实时画面
            }
        }

        private async Task CaptureImagesAsync()
        {
            //_capture = new VideoCapture(0);
            //_capture.ImageGrabbed += ProcessFrame;
            //_capture.Start();
           
            _isCapturing = true;
            _capturedImages.Clear();

            progressBar1.Visible = true;
            progressBar1.Invoke((MethodInvoker)(() =>
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = _shotCount; // 总步骤数
                progressBar1.Value = 0; // 初始值
            }));

            for (int i = 0; i < _shotCount; i++)
            {
                await Task.Delay(_frameCount * 1000);

                if (_capture.IsOpened)
                {
                    Mat frame = new Mat();
                    _capture.Grab();
                    _capture.Retrieve(frame);

                    if (!frame.IsEmpty)
                    {
                        //_capturedImages.Add(frame.Clone());
                        _imageSize = frame.Size;

                        progressBar1.Invoke((MethodInvoker)(() =>
                        {
                            progressBar1.Value = i + 1;
                            label1.Text = "正在捕获图像，请勿移动摄像机！";
                        }));
                    }
                }
            }
            _isCapturing = false;

            if (_capturedImages.Count > 0)
            {
                CalibrateCamera();
            }
            else
            {
                MessageBox.Show("未捕捉到有效的图像。");
            }
            //ResumeRealTimeCapture();
        }

      

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (_isCapturing)
            {
                Mat frame = new Mat();
                _capture.Retrieve(frame);

                // Detect Aruco markers
                using (VectorOfInt ids = new VectorOfInt())
                using (VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF())
                {
                    ArucoInvoke.DetectMarkers(frame, _dictionary, corners, ids, _detectorParameters);

                    if (ids.Size > 0)
                    {
                        // Draw detected markers
                        ArucoInvoke.DrawDetectedMarkers(frame, corners, ids, new MCvScalar(0, 255, 0));

                        // Store the frame for calibration
                        _capturedImages.Add(frame.Clone());
                    }
                }

                // Display the frame
                pictureBox1.Invoke((MethodInvoker)(() =>
                {
                    pictureBox1.Image?.Dispose();
                    pictureBox1.Image = frame.ToBitmap();
                }));
            }
        }

        private void CalibrateCamera()
        {
            VectorOfVectorOfPointF allCorners = new VectorOfVectorOfPointF();
            VectorOfInt allIds = new VectorOfInt();

            // 创建一个 1x1 的 GridBoard，标记之间的间距非常小
            //Size boardSize = new Size(1, 1);  // 1x1 网格
            //float markerLength = 0.03f;  // 标记的边长
            //float markerSeparation = 0.02f;  // 标记之间的间隔

            //// 创建 GridBoard 对象
            //GridBoard gridBoard = new GridBoard(boardSize.Width, boardSize.Height, markerLength, markerSeparation, _dictionary);

            _markerCounterPerFrame.Clear();

            foreach (var image in _capturedImages)
            {
                using (VectorOfInt ids = new VectorOfInt())
                using (VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF())
                {
                    ArucoInvoke.DetectMarkers(image, _dictionary, corners, ids, _detectorParameters);
                    if (ids.Size > 0)
                    {
                        allCorners.Push(corners);
                        allIds.Push(ids);
                        _markerCounterPerFrame.Add(ids.Size);
                    }
                }
            }
            try
            {
                if (allIds.Size > 0)
                {
                    Size imageSize = _capturedImages[0].Size;

                    VectorOfInt markerCounterPerFrameVector = new VectorOfInt(_markerCounterPerFrame.ToArray());
                    var calibrationFlags = Emgu.CV.CvEnum.CalibType.FixPrincipalPoint | Emgu.CV.CvEnum.CalibType.ZeroTangentDist;

                    // 调用标定方法
                    double error = ArucoInvoke.CalibrateCameraAruco(
                        allCorners,
                        allIds,
                        markerCounterPerFrameVector,
                        _gridBoard,
                        _imageSize,
                        _cameraMatrix,
                        _distCoeffs,
                        null,
                        null,
                        calibrationFlags,
                        //Emgu.CV.CvEnum.CalibType.Default,
                        new MCvTermCriteria(30, 1e-3)
                    ); 

                    // 显示标定结果
                    MessageBox.Show($"标定完成！\n重投影误差：{error}\n相机矩阵：\n{_cameraMatrix.ToString()}\n畸变系数：\n{_distCoeffs.ToString()}");
                    button3.Enabled = true;

                }
                else
                {
                    MessageBox.Show("未检测到足够的标记进行标定。");
                }
            }
            catch (Exception ex)
            {
                // 捕获异常并显示错误信息
                MessageBox.Show($"标定失败！\n错误信息：{ex.Message}\n堆栈信息：{ex.StackTrace}");
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_cameraMatrix != null && !_cameraMatrix.IsEmpty && _cameraMatrix.Rows > 0 && _cameraMatrix.Cols > 0 &&
        _distCoeffs != null && !_distCoeffs.IsEmpty && _distCoeffs.Rows > 0 && _distCoeffs.Cols > 0)            
                {
                string filePath = "camera_parameters.txt";

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("相机矩阵");

                    // 获取相机矩阵的行数和列数
                    int rows = _cameraMatrix.Rows;
                    int cols = _cameraMatrix.Cols;

                    // 将相机矩阵的数据复制到二维数组中
                    double[,] cameraMatrixData = new double[rows, cols];
                    GCHandle handle = GCHandle.Alloc(cameraMatrixData, GCHandleType.Pinned);

                    using (Mat tempMat = new Mat(rows, cols, DepthType.Cv64F, 1, handle.AddrOfPinnedObject(), cols * sizeof(double)))
                    {
                        _cameraMatrix.CopyTo(tempMat);
                    }
                    handle.Free();

                    // 打印相机矩阵
                    Console.WriteLine("相机矩阵：");
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            string value = cameraMatrixData[i, j].ToString("F6") + " ";
                            writer.Write(value);
                            Console.Write($"{cameraMatrixData[i, j]:F6}\t");
                        }
                        writer.WriteLine();
                    }

                    writer.WriteLine("畸变系数");

                    // 获取畸变系数的长度
                    int length = _distCoeffs.Rows * _distCoeffs.Cols;

                    // 将畸变系数的数据复制到数组中
                    double[] distCoeffsData = new double[length];
                    _distCoeffs.CopyTo(distCoeffsData);

                    // 打印畸变系数
                    Console.WriteLine("畸变系数：");
                    foreach (var coeff in distCoeffsData)
                    {
                        string value = coeff.ToString("F6") + " ";
                        writer.Write(value);
                        Console.WriteLine($"{coeff:F6}");
                    }
                }
                MessageBox.Show("已成功保存到 {filePath}。");
            }
            else
            {
                MessageBox.Show("没有可以保存的数据。");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            menuStrip1.Visible = false;
            panel1.Visible = false;
            button1.Visible = true;
            progressBar1.Visible = false;
            label1.Text = $"カメラを接続しました";
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        // 读取数据按钮
        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                int rowIndex = 0;
                bool isDistCoeffs = false;

                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    MessageBox.Show("相机参数如下:\n" + fileContent);

                    string[] lines = File.ReadAllLines(filePath);

                    foreach (string line in lines)
                    {
                        // 跳过空行或无效行
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        if (line.Contains("畸变系数"))
                        {
                            isDistCoeffs = true; // 开始读取畸变系数
                            continue; // 跳过"畸变系数"这一行
                        }

                        // 如果是相机矩阵的内容
                        if (!isDistCoeffs)
                        {
                            // 将每行的数据拆分并填充到相机矩阵中
                            var values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (values.Length == 3) // 相机矩阵每行有三个值
                            {
                                for (int col = 0; col < values.Length; col++)
                                {
                                    if (double.TryParse(values[col], out double result))
                                    {
                                        read_cameraMatrix[rowIndex, col] = result;
                                    }
                                    else
                                    {
                                        MessageBox.Show($"相机矩阵中的数据格式不正确: {values[col]}", "格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                                rowIndex++;
                            }
                        }
                        // 如果是畸变系数的内容
                        else
                        {
                            // 将畸变系数的数据填充到数组中
                            var values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (values.Length == 5) // 畸变系数通常是5个值
                            {
                                for (int i = 0; i < values.Length; i++)
                                {
                                    if (double.TryParse(values[i], out double result))
                                    {
                                        read_distCoeffs[i] = result;
                                    }
                                    else
                                    {
                                        MessageBox.Show($"畸变系数中的数据格式不正确: {values[i]}", "格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                        }
                        DisplayResults(read_cameraMatrix, read_distCoeffs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取文件时发生错误: {ex.Message}");
                }
            }
        }

        // 打印读取的数据
        private void DisplayResults(double[,] cameraMatrix, double[] distCoeffs)
        {
            Console.WriteLine("相机矩阵：");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write($"{cameraMatrix[i, j]:F6}\t");
                }
                Console.WriteLine();
            }

            Console.WriteLine("畸变系数：");
            foreach (var coeff in distCoeffs)
            {
                Console.WriteLine($"{coeff:F6}");
            }
        }

        //    private double? MeasureDistance(Mat frame)
        //    {
        //        // 检测 ArUco 标记
        //        VectorOfInt ids = new VectorOfInt();
        //        VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF();
        //        ArucoInvoke.DetectMarkers(frame, _dictionary, corners, ids, _detectorParameters);

        //        if (ids.Size < 2)
        //        {
        //            // 如果未检测到至少两个标记，则无法计算距离
        //            return null;
        //        }

        //        // 获取两个标记的中心点
        //        PointF center1 = GetMarkerCenter(corners[0]);
        //        PointF center2 = GetMarkerCenter(corners[1]);

        //        // 转换到相机坐标
        //        Point3D p1 = ConvertToCameraCoordinates(center1, _cameraMatrix, _distCoeffs);
        //        Point3D p2 = ConvertToCameraCoordinates(center2, _cameraMatrix, _distCoeffs);

        //        // 计算两点之间的欧几里得距离
        //        double distance = Math.Sqrt(
        //            Math.Pow(p2.X - p1.X, 2) +
        //            Math.Pow(p2.Y - p1.Y, 2) +
        //            Math.Pow(p2.Z - p1.Z, 2)
        //        );

        //        return distance;
        //    }

        public class Point3D
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public Point3D(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            // 方便显示点的信息
            public override string ToString()
            {
                return $"({X:F2}, {Y:F2}, {Z:F2})";
            }
        }

        private Point3D point1 = null;
        private Point3D point2 = null;
        private bool isMeasuring = false;
        private Point3D startPoint = null;


        private void DetectAruco(Mat frame)
        {
            using (VectorOfInt ids = new VectorOfInt())
            using (VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF())
            {
                // 检测 ArUco 标记
                ArucoInvoke.DetectMarkers(frame, _dictionary, corners, ids, _detectorParameters);

                if (ids.Size > 0)
                {
                    // 用于存储旋转和位移向量
                    VectorOfMat rvecs = new VectorOfMat();
                    VectorOfMat tvecs = new VectorOfMat();

                    // 使用 EstimatePoseSingleMarkers 估算位姿
                    ArucoInvoke.EstimatePoseSingleMarkers(corners, 0.3f, _cameraMatrix, _distCoeffs, rvecs, tvecs);

                    // 获取第一个（也是唯一一个）标记的旋转向量和位移向量
                    Mat tvecMat = tvecs[0];
                    float[] tvecData = new float[3];
                    tvecMat.CopyTo(tvecData);

                    float x = tvecData[0];
                    float y = tvecData[1];
                    float z = tvecData[2];

                    // 计算物体与相机的距离
                    double distance = Math.Sqrt(x * x + y * y + z * z);
                    Console.WriteLine($"标记的距离：{distance:F2} 米");

                    // 画出标记和位姿 
                    ArucoInvoke.DrawDetectedMarkers(frame, corners, ids, new MCvScalar(0, 255, 0));

                    // 绘制标记的坐标轴
                    float axisLength = 0.1f;
                    MCvPoint3D32f origin = new MCvPoint3D32f(0, 0, 0);
                    MCvPoint3D32f xAxisEnd = new MCvPoint3D32f(axisLength, 0, 0);
                    MCvPoint3D32f yAxisEnd = new MCvPoint3D32f(0, axisLength, 0);
                    MCvPoint3D32f zAxisEnd = new MCvPoint3D32f(0, 0, axisLength);

                    MCvPoint3D32f[] axis3D = new MCvPoint3D32f[] { origin, xAxisEnd, yAxisEnd, zAxisEnd };
                    VectorOfPointF axis2D = new VectorOfPointF();

                    Mat rvec = rvecs[0];
                    Mat tvec = tvecs[0];
                    CvInvoke.ProjectPoints(axis3D, rvec, tvec, _cameraMatrix, _distCoeffs, axis2D);

                    //CvInvoke.ProjectPoints(axis3D, rvecs, tvecs, _cameraMatrix, _distCoeffs, axis2D);

                    CvInvoke.Line(frame, Point.Round(axis2D[0]), Point.Round(axis2D[1]), new MCvScalar(255, 0, 0), 2); // X轴，红色
                    CvInvoke.Line(frame, Point.Round(axis2D[0]), Point.Round(axis2D[2]), new MCvScalar(0, 255, 0), 2); // Y轴，绿色
                    CvInvoke.Line(frame, Point.Round(axis2D[0]), Point.Round(axis2D[3]), new MCvScalar(0, 0, 255), 2); // Z轴，蓝色

                }
                else
                {
                    Console.WriteLine("没有检测到 ArUco 标记。");
                }
            }
        }

        private Point3D CalculateArrowPosition(Mat rvec, Mat tvec, double rulerLength)
        {
            // 将 rvec 转换为旋转矩阵
            Mat rotationMatrix = new Mat();
            CvInvoke.Rodrigues(rvec, rotationMatrix);

            // 提取旋转矩阵数据到一维数组
            double[] rotationArray = new double[9];
            rotationMatrix.CopyTo(rotationArray);

            // 转换为二维数组
            double[,] rotation2DArray = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    rotation2DArray[i, j] = rotationArray[i * 3 + j];
                }
            }

            // 提取 tvec 数据到数组
            double[] tvecArray = new double[3];
            tvec.CopyTo(tvecArray);

            // 假设箭头沿直尺负 Z 轴方向偏移
            double[] localArrowPosition = { 0, 0, -rulerLength };

            // 计算箭头的全局坐标
            double[] globalArrowPosition = new double[3];
            for (int i = 0; i < 3; i++)
            {
                globalArrowPosition[i] =
                    tvecArray[i] +
                    rotation2DArray[i, 0] * localArrowPosition[0] +
                    rotation2DArray[i, 1] * localArrowPosition[1] +
                    rotation2DArray[i, 2] * localArrowPosition[2];
            }

            return new Point3D(globalArrowPosition[0], globalArrowPosition[1], globalArrowPosition[2]);
        }


        private void calculateDistance(object sender, EventArgs e)
        {
            if (point1 != null && point2 != null)
            {
                double distance = Math.Sqrt(
                    Math.Pow(point2.X - point1.X, 2) +
                    Math.Pow(point2.Y - point1.Y, 2) +
                    Math.Pow(point2.Z - point1.Z, 2)
                );

                MessageBox.Show($"两点之间的距离为: {distance:F2} 米");
            }
            else
            {
                MessageBox.Show("请先记录两个点的位置。");
            }
        }


        private void EstimatePoseAndProcess(VectorOfVectorOfPointF corners, VectorOfInt ids, Mat frame)
        {
            if (_cameraMatrix == null || _cameraMatrix.IsEmpty || _distCoeffs == null || _distCoeffs.IsEmpty)
            {
                MessageBox.Show("相机矩阵或畸变参数未初始化，请先进行相机标定。");
                return;
            }

            if (corners.Size == 0 || ids.Size == 0)
            {
                MessageBox.Show("未检测到有效的 ArUco 标记。");
                return;
            }

            using (VectorOfMat rvecs = new VectorOfMat())
            using (VectorOfMat tvecs = new VectorOfMat())
            {
                // 姿态估计
                ArucoInvoke.EstimatePoseSingleMarkers(corners, 0.03f, _cameraMatrix, _distCoeffs, rvecs, tvecs);

                if (rvecs.Size > 0 && tvecs.Size > 0)
                {
                    Mat rvec = rvecs[0];
                    Mat tvec = tvecs[0];

                    // 计算箭头的全局位置
                    Point3D currentPoint = CalculateArrowPosition(rvec, tvec, 1.0f);

                    // 如果启用了测距模式，则计算距离
                    if (isMeasuring && startPoint != null)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(currentPoint.X - startPoint.X, 2) +
                            Math.Pow(currentPoint.Y - startPoint.Y, 2) +
                            Math.Pow(currentPoint.Z - startPoint.Z, 2)
                        );

                        // 更新距离到 UI
                        textBox3.Invoke((MethodInvoker)(() =>
                        {
                            textBox3.Text = $"实时距离: {distance:F2} 米";
                        }));
                    }
                }
                else
                {
                    MessageBox.Show("未能生成有效的位姿，请确保标记清晰可见。");
                }
            }
        }

        bool isPoseEstimationEnabled = false;
        // 距离测算
        private void button7_Click(object sender, EventArgs e)
        {
            isPoseEstimationEnabled = true;
            if (_capture != null && _capture.IsOpened)
            {
                Mat frame = new Mat();
                _capture.Retrieve(frame);

                using (VectorOfInt ids = new VectorOfInt())
                using (VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF())
                using (VectorOfMat rvecs = new VectorOfMat())
                using (VectorOfMat tvecs = new VectorOfMat())
                {
                    ArucoInvoke.DetectMarkers(frame, _dictionary, corners, ids, _detectorParameters);

                    if (ids.Size > 0)
                    {
                        ArucoInvoke.EstimatePoseSingleMarkers(corners, 0.03f, _cameraMatrix, _distCoeffs, rvecs, tvecs);

                        // 假设第一个标记是直尺标记
                        Mat rvec = rvecs[0];
                        Mat tvec = tvecs[0];

                        startPoint = CalculateArrowPosition(rvec, tvec, 0.1f);
                        isMeasuring = true; // 开始测量
                        MessageBox.Show("开始测量，请移动到目标点。");
                    }
                    else
                    {
                        MessageBox.Show("未检测到标记，请重试。");
                    }
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
