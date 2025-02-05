using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


using Compunet.YoloSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO.Ports;
using Microsoft.Win32;
using Compunet.YoloSharp.Data;
using System.Timers;
using System.Net.Sockets;
using System.Net;
namespace gprs
{
    public partial class Form1 : Form
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////鼠标驱动
        [System.Runtime.InteropServices.DllImport("user32.dll")] //导入user32.dll函数库
        public static extern bool GetCursorPos(out System.Drawing.Point lpPoint);//获取鼠标坐标

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, UIntPtr extraInfo);

        const uint MOUSEEVENTF_MOVE = 0x0001;            //移动鼠标 
        const uint MOUSEEVENTF_LEFTDOWN = 0x02;          //模拟鼠标左键按下
        const uint MOUSEEVENTF_LEFTUP = 0x04;            //模拟鼠标左键抬起
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;       //模拟鼠标右键按下
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;         //模拟鼠标右键抬起
        const uint MOUSEEVENTF_WHEEL = 0x0800;           //模拟鼠标滚轮事件
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;        //标示是否采用绝对坐标 

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////键盘驱动
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys key);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////游戏操作相关变
        System.Drawing.Point mousePositionJiaDian = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2);
        bool jiaDianMode = false;

        int fireZhu = 2;
        int fireFu = 2;

        bool fireEn = false;
        bool fireEnLast = false;

        ////////////////////////////////////////////////////////
        int count;
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch stopwatchJianGe = new Stopwatch();

        SerialPort serialPort1 = new SerialPort();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////yolo
        YoloPredictor predictor = new YoloPredictor("./yolov8m-pose.onnx");


        private UdpClient udpData = null;
        /// <summary>
        static int width = 640, height = 640;
        Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        Graphics graphics;
        Pen pen = new Pen(System.Drawing.Color.Yellow); // 点的大小设置为1像素
        Pen pen2 = new Pen(System.Drawing.Color.Green); // 点的大小设置为1像素
        System.Drawing.Point pointMirror = new System.Drawing.Point(width / 2 + 3, height / 2 + 3);
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown2.Value = 165;//156
            numericUpDown3.Value = 222;//146
            radioButton1.Checked = true;
            radioButton5.Checked = true;
            radioButton6.Checked = true;
            udpData = new UdpClient(new IPEndPoint(IPAddress.Any, 9002));
            port_cbb.Items.Clear();
            GetComList();     //获取设备串口号，并放在下拉列表
            Task.Run(() =>
            {
                //predictor.PoseAndSave("./bus.jpg");
                graphics = Graphics.FromImage(bitmap);
                while (true)
                {
                    if (radioButton10.Checked)//灵敏度调试模式
                    {
                        moveMouse();
                        //jiaDian(new System.Drawing.Point(10, 29), new System.Drawing.Point(13, 29));
                    }
                    else //游戏模式
                    {
                        int xSensitivity = 0;
                        int ySensitivity = 0;
                        if (radioButton1.Checked)//穿越火线
                        {
                            xSensitivity = 90;//2560P 165  1080P 124//1080p 83
                            ySensitivity = 117;//2560P 222  1080P 157//1080p 102
                        }
                        else if (radioButton2.Checked)
                        {
                            xSensitivity = 160;//2554p 160;//1080p 215
                            ySensitivity = 158;//2554p 158;//1080p 207
                        }
                        else if (radioButton3.Checked)
                        {
                            xSensitivity = 240;
                            ySensitivity = 227;
                        }

                        if (radioButton11.Checked)//狙-头
                        {
                            fireZhu = 1;
                            Yolo(xSensitivity, ySensitivity, 1, 0, 0);
                        }
                        else if (radioButton5.Checked)//狙-身子
                        {
                            fireZhu = 2;
                            Yolo(xSensitivity, ySensitivity, 1, 0, 1);
                        }
                        else if (radioButton4.Checked)//点射-头
                        {
                            fireZhu = 3;
                            Yolo(xSensitivity, ySensitivity, 2, 30, 0);
                        }
                        else if (radioButton7.Checked)//点射-身子
                        {
                            fireZhu = 4;
                            Yolo(xSensitivity, ySensitivity, 2, 30, 1);
                        }
                        else if (radioButton8.Checked)//连发-头
                        {
                            fireZhu = 5;
                            Yolo(xSensitivity, ySensitivity, 3, 30, 0);
                        }
                        else if (radioButton12.Checked)//连发-身子
                        {
                            fireZhu = 6;
                            Yolo(xSensitivity, ySensitivity, 3, 30, 1);
                        }


                        if (radioButton9.Checked)//手枪-头
                        {
                            fireFu = 1;
                            Yolo(xSensitivity, ySensitivity, 2, 30, 0);
                        }
                        else if (radioButton6.Checked)//手枪-身子
                        {
                            fireFu = 2;
                            Yolo(xSensitivity, ySensitivity, 2, 30, 1);
                        }
                    }
                    Thread.Sleep(0);
                    //Delay(1);
                    count++;
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    Action action = () =>
                    {
                        textBox2.Text = "每秒检测次数:" + count;
                    };
                    Invoke(action);
                    count = 0;
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1);

                    if (GetAsyncKeyState(Keys.D1) < 0)
                    {
                        Action action = () =>
                        {
                            switch (fireZhu)
                            {
                                case 1: radioButton11.Checked = true; break;
                                case 2: radioButton5.Checked = true; break;
                                case 3: radioButton4.Checked = true; break;
                                case 4: radioButton7.Checked = true; break;
                                case 5: radioButton8.Checked = true; break;
                                case 6: radioButton12.Checked = true; break;
                            }
                        };
                        Invoke(action);
                    }
                    else if (GetAsyncKeyState(Keys.D2) < 0)
                    {
                        Action action = () =>
                        {
                            switch (fireFu)
                            {
                                case 1: radioButton9.Checked = true; break;
                                case 2: radioButton6.Checked = true; break;
                            }
                        };
                        Invoke(action);
                    }
                    else if ((GetAsyncKeyState(Keys.D3) < 0) || (GetAsyncKeyState(Keys.D4) < 0))
                    {
                        Action action = () =>
                        {
                            radioButton11.Checked = false;
                            radioButton5.Checked = false;
                            radioButton4.Checked = false;
                            radioButton7.Checked = false;
                            radioButton8.Checked = false;
                            radioButton12.Checked = false;
                            radioButton9.Checked = false;
                            radioButton6.Checked = false;
                        };
                        Invoke(action);
                    }
                }
            });

        }
        public void moveMouse()
        {
            //if (Control.MouseButtons == MouseButtons.Right)
            if (GetAsyncKeyState(Keys.F) < 0)
            {
                jiaDianMode = true;
            }
            if (!jiaDianMode)
                return;

            int x = 155; //(int)numericUpDown2.Value;
            int y = 155;// (int)numericUpDown3.Value;

            int x1 = (int)(x * (((int)numericUpDown2.Value) / 100.0f));
            int y1 = (int)(y * (((int)numericUpDown3.Value) / 100.0f));

            string data = "{1," + x1 + "," + (-y1) + "}";
            serialPort1.Write(data);
            Thread.Sleep(2500);

            serialPort1.Write("{1," + (-x1) + "," + y1 + "}");
            Thread.Sleep(2500);

            //进入架点
            System.Drawing.Rectangle region2 = new System.Drawing.Rectangle(mousePositionJiaDian.X - 160, mousePositionJiaDian.Y - 160, 320, 320);
            using (Bitmap bitmap = new Bitmap(region2.Width, region2.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(region2.X, region2.Y, 0, 0, region2.Size);

                    Pen pen = new Pen(System.Drawing.Color.Red, 1); // 点的大小设置为1像素
                    //
                    using (Brush brush = new SolidBrush(System.Drawing.Color.Black))
                    {
                        graphics.FillRectangle(brush, new System.Drawing.Rectangle(160 + x - 2, 160 - y - 2, 4, 4));
                    }

                    Action action = () =>
                    {
                        pictureBox1.Image = new Bitmap(bitmap);
                    };
                    Invoke(action);

                    jiaDianMode = false;
                    return;
                }
            }
        }

        public void Yolo1(int xSensitivity, int ySensitivity, int fireMode, int delayRun, int location)
        {
            // 获取准心坐标
            System.Drawing.Point mousePosition = new System.Drawing.Point(mousePositionJiaDian.X, mousePositionJiaDian.Y);
            System.Drawing.Rectangle region2 = new System.Drawing.Rectangle(mousePosition.X - width / 2, mousePosition.Y - height / 2, width, height);

            graphics.CopyFromScreen(region2.X, region2.Y, 0, 0, region2.Size);

            System.Drawing.Color ColorMirror = bitmap.GetPixel(pointMirror.X, pointMirror.Y);
            graphics.DrawLine(pen, pointMirror.X, pointMirror.Y, pointMirror.X, pointMirror.Y - 1);

            if (serialPort1.IsOpen == false)
            {
                return;
            }
            if (!(ColorMirror.R < 240 || ColorMirror.G < 240 || ColorMirror.B < 240))
                return;
            
            int x = 2;
            int y = 2;

            int x1 = (int)(x * xSensitivity / 100.0f);
            int y1 = (int)(y * ySensitivity / 100.0f);


            if (fireMode == 1)//狙击模式，打完自动切手枪
            {                
                serialPort1.Write("{1,0,0}");
                Thread.Sleep(100);
            }
            else if (fireMode == 2)//点射模式
            {
                //IPEndPoint remotePoint = new(IPAddress.Parse("192.168.1.183"), 9000);
                //byte[] portTemp = System.Text.Encoding.Default.GetBytes("{1,0,0}");
                //udpData.Send(portTemp, portTemp.Length, remotePoint);//将数据发送到远程端点 
                serialPort1.Write("{1,2,2}");                               
                Thread.Sleep(100);
            }
            else if (fireMode == 3)//连发模式，只移动准心
            {
                if (stopwatchJianGe.IsRunning)
                {
                    if (stopwatchJianGe.ElapsedMilliseconds > 100)//隔100ms吸附1次
                    {
                        stopwatchJianGe.Restart();
                        serialPort1.Write("{0," + x1 + "," + y1 + "}");
                    }
                }
                else
                {
                    serialPort1.Write("{0," + x1 + "," + y1 + "}");
                }

                if (!stopwatchJianGe.IsRunning)
                {
                    stopwatchJianGe.Restart();
                }
            }

        }
        public void Yolo(int xSensitivity, int ySensitivity, int fireMode, int delayRun, int location)  
        {
            // 获取准心坐标
            System.Drawing.Point mousePosition = new System.Drawing.Point(mousePositionJiaDian.X, mousePositionJiaDian.Y);
            System.Drawing.Rectangle region2 = new System.Drawing.Rectangle(mousePosition.X - width / 2, mousePosition.Y - height / 2, width, height);

            graphics.CopyFromScreen(region2.X, region2.Y, 0, 0, region2.Size);

            System.Drawing.Color ColorMirror = bitmap.GetPixel(pointMirror.X, pointMirror.Y);
            graphics.DrawLine(pen, pointMirror.X, pointMirror.Y, pointMirror.X, pointMirror.Y - 1);

            // 屏蔽枪械区域，防止干扰
            using (Brush brush = new SolidBrush(System.Drawing.Color.Black))
            {
                graphics.FillRectangle(brush, new System.Drawing.Rectangle(width / 2 + 100, height / 2 + 90, 250, 250));
            }
            
            if (serialPort1.IsOpen == false)
            {
                return;
            }
            
            //根据枪械选择不同的触发方式                  
            if (fireMode == 1)//狙击模式
            {
                if (!(ColorMirror.R >= 170 && ColorMirror.G < 32 && ColorMirror.B < 35))
                    return;
            }
            else if (fireMode == 2)//点射模式
            {
                if ((Control.MouseButtons == MouseButtons.Right) || (Control.MouseButtons == (MouseButtons.Right | MouseButtons.Left)))
                {
                    stopwatch.Restart();
                    jiaDianMode = true;
                }

                if (stopwatch.ElapsedMilliseconds > delayRun)
                {
                    stopwatch.Reset();
                    jiaDianMode = false;
                }

                if (!jiaDianMode)
                    return;
            }
            else if (fireMode == 3)//连发模式
            {
                fireEn = jiaDianMode;
                if (fireEn != fireEnLast)
                {
                    fireEnLast = fireEn;
                    if (fireEn)
                    {
                        //--stopwatchJianGe.Restart();

                        serialPort1.Write("{2," + 0 + "," + 0 + "}");
                    }
                    else
                    {
                        stopwatchJianGe.Reset();
                        serialPort1.Write("{3," + 0 + "," + 0 + "}");
                    }

                }

                if ((Control.MouseButtons == MouseButtons.Right) || (Control.MouseButtons == (MouseButtons.Right | MouseButtons.Left)))
                {
                    stopwatch.Restart();
                    jiaDianMode = true;
                }

                if (stopwatch.ElapsedMilliseconds > delayRun)
                {
                    stopwatch.Reset();
                    jiaDianMode = false;
                }
                //架点自动开枪
                if (!jiaDianMode)
                    return;
            }
            
            //开始识别2
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Bmp); // 可以改变ImageFormat来保存为其他格式，如Jpeg, Bmp等
            memoryStream.Position = 0; // 重设流的位置，以确保从头开始读取
            SixLabors.ImageSharp.Image img = SixLabors.ImageSharp.Image.Load(memoryStream);
            YoloResult<Pose> result = predictor.Pose(img);
            //memoryStream.Position = 54; // 重设流的位置，以确保从头开始读取
            //byte[] buffer = new byte[memoryStream.Length - 54];
            //int bytesRead = memoryStream.Read(buffer, 0, (int)memoryStream.Length - 54);
            //把识别结果画到图像上，并寻找离准心最近的敌人
            int id = -1, min = 1000, id2 = 0;
            for (int i = 0; i < result.Count; i++)// foreach (var item in result)
            {
                if (result[i].Confidence < 0.5)
                    continue;


                /*
                bool isTeammate = false;
                int x_left = result[i].Bounds.X, x_right = result[i].Bounds.X + result[i].Bounds.Width, y_top = result[i].Bounds.Y - result[i].Bounds.Height / 2, y_bott = result[i][1].Point.Y;               
                if (x_left < 0) {
                    x_left = 0;
                }
                if (x_right > 640)
                {
                    x_left = 640;
                }
                if(y_top< 0)
                {
                    y_top = 0;
                }
                if (y_bott > 640)
                {
                    y_bott = 640;
                }

                
                for (int y_ = y_top; y_ < y_bott; y_++)
                {
                    for (int x_ = x_left; x_ < x_right; x_++)
                    {
                        int R1 = buffer[(639 - y_) * 640 * 3 + (x_ * 3) + 2], G1 = buffer[(639 - y_) * 640 * 3 + (x_ * 3) + 1], B1 = buffer[(639 - y_) * 640 * 3 + (x_ * 3) + 0];
                        //int R2 = bitmap.GetPixel(x_,y_).R, G2 = bitmap.GetPixel(x_, y_).G, B2 = bitmap.GetPixel(x_, y_).B;
                        if (Math.Abs(R1 - 133) < 12 && Math.Abs(G1 - 190) < 12 && Math.Abs(B1 - 180) < 12)
                        {                            
                           
                            isTeammate = true;
                            x_ = 640;
                            y_ = 640;
                            //graphics.DrawEllipse(pen, x_, y_, 5, 5);
                        }
                    }
                }
                
                if (isTeammate)
                {
                    graphics.DrawRectangle(pen2, result[i].Bounds.X, y_top, result[i].Bounds.Width, y_bott- y_top);
                    continue;
                }
                else
                {
                    graphics.DrawRectangle(pen, result[i].Bounds.X, result[i].Bounds.Y, result[i].Bounds.Width, result[i].Bounds.Height);
                }
                */

                foreach (var po in result[i])
                {
                    //graphics.DrawLine(pen, po.Point.X, po.Point.Y, po.Point.X, po.Point.Y - 1); // 绘制一条宽度为1的线，因为点是1x1的
                    //graphics.DrawString("" + id2++, new System.Drawing.Font("Arial", 16), new SolidBrush(System.Drawing.Color.Black), po.Point.X, po.Point.Y);
                }
                
                if (Math.Abs(result[i][0].Point.X - width / 2) < min)
                {
                    min = Math.Abs(result[i][0].Point.X - width / 2);
                    id = i;
                }
            }

            /*
            //显示图像
            Action action = () =>
            {
                ///textBox1.Text = result.Boxes[id].Bounds.X + "";//  result.ToString();
                textBox1.Text = ColorMirror.ToString();               

                //textBox1.Text = result.Speed.ToString();
                //if (id >= 0)
                    //textBox1.Text = result[id].Confidence + "";

                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
                pictureBox1.Image = (Bitmap)bitmap.Clone();
            };
            Invoke(action);
            */
            if (id < 0)
                return;
            
            //根据识别坐标，进行瞄准杀敌操作                   

            int x = 0;
            int y = 0;

            switch (location)
            {
                case 0:
                    x = result[id][0].Point.X - width / 2;
                    y = (result[id].Bounds.Y + result[id][1].Point.Y) / 2 - height / 2;
                    break;
                case 1:
                    x = (result[id][5].Point.X + result[id][6].Point.X) / 2 - width / 2;
                    y = (result[id][6].Point.Y * 2 + result[id][11].Point.Y) / 3 - height / 2;
                    break;
                case 2:
                    x = result[id][1].Point.X - width / 2;
                    y = result[id][1].Point.Y - height / 2;
                    break;
            }
            graphics.DrawEllipse(pen, x + width / 2, y + height / 2,5,5);
            graphics.DrawString("" + result[id].Confidence, new System.Drawing.Font("Arial", 16), new SolidBrush(System.Drawing.Color.Black), 0, 0);

            int x1 = (int)(x * xSensitivity / 100.0f);
            int y1 = (int)(y * ySensitivity / 100.0f);
            if (0 <= y1 && y1 <= 1)
            {
                y1 = 2;
            }
            if (-1 <= y1 && y1 <= 0)
            {
                y1 = -2;
            }
            if (fireMode == 1)//狙击模式，打完自动切手枪
            {
                serialPort1.Write("{4," + x1 + "," + y1 + "}");
                string time1 = "./img/" + DateTime.Now.Ticks / 10000 + " " + x1 + "," + y1 + ".bmp";
                //记录未操作前的图像                               
                bitmap.Save(time1, ImageFormat.Bmp);
                jiaDianMode = false;
                stopwatch.Reset();

                int time = new Random().Next(120, 150);
                Thread.Sleep(time);
            }
            else if (fireMode == 2)//点射模式
            {
                serialPort1.Write("{1," + x1 + "," + y1 + "}");

                string time1 = "./img/" + DateTime.Now.Ticks / 10000 + " " + x1 + "," + y1 + ".bmp";
                //记录未操作前的图像                               
                bitmap.Save(time1, ImageFormat.Bmp);
                jiaDianMode = false;
                stopwatch.Reset();

                int time = new Random().Next(70, 110);
                Thread.Sleep(time);
            }
            else if (fireMode == 3)//连发模式，只移动准心
            {
                if (stopwatchJianGe.IsRunning)
                {
                    if (stopwatchJianGe.ElapsedMilliseconds > 100)//隔100ms吸附1次
                    {
                        stopwatchJianGe.Restart();
                        serialPort1.Write("{0," + x1 + "," + y1 + "}");
                    }
                }
                else
                {
                    serialPort1.Write("{0," + x1 + "," + y1 + "}");
                }

                if (!stopwatchJianGe.IsRunning)
                {
                    stopwatchJianGe.Restart();
                }
            }
        }
        public bool IsTeammate(MemoryStream memoryStream,SixLabors.ImageSharp.Rectangle Bounds, int x,int y,int width,int height)
        {
            memoryStream.Position = 54; // 重设流的位置，以确保从头开始读取
            byte[] buffer = new byte[memoryStream.Length - 54];
            int bytesRead = memoryStream.Read(buffer, 0, (int)memoryStream.Length - 54);


            for (int y_ = 0; y_ < Bounds.Y; y_++)
            {
                for (int x_ = Bounds.X; x_ < Bounds.X+ Bounds.Width; x_++)
                {
                    int R1 = buffer[((Bounds.Y-1) - y_) * Bounds.Width * 3 + (x_ * 3) + 2], G1 = buffer[((Bounds.Y - 1) - y_) * Bounds.Width * 3 + (x_ * 3) + 1], B1 = buffer[((Bounds.Y - 1) - y_) * Bounds.Width * 3 + (x_ * 3) + 0];
                    if (Math.Abs(R1 - 133) < 12 && Math.Abs(G1 - 190) < 12 && Math.Abs(B1 - 180) < 12)
                    {
                        graphics.DrawEllipse(pen, x_, y_, 5, 5);
                        Action action2 = () =>
                        {
                            if (pictureBox1.Image != null)
                            {
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;
                            }
                            pictureBox1.Image = (Bitmap)bitmap.Clone();
                        };
                        //Invoke(action2);
                        //y_ = 640;
                        //x_ = 640;
                    }
                }
            }
            return false;
        }
        public void Delay(int delayMilliseconds)
        {
            // 创建一个Stopwatch实例来测量时间
            Stopwatch stopwatch = new Stopwatch();

            // 开始Stopwatch
            stopwatch.Start();

            // 当Stopwatch记录的时间小于设定的延迟时间时，持续循环
            while (stopwatch.ElapsedMilliseconds < delayMilliseconds)
            {
                // 通过Thread.Sleep(0)释放当前线程的时间片，防止占用CPU
                Thread.Sleep(0);
            }
            // 停止Stopwatch
            stopwatch.Stop();
        }
        Stopwatch stopwatchTest = new Stopwatch();
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            return;
            if (stopwatchTest.IsRunning == false)
            {
                Action action = () =>
                {
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }
                    pictureBox1.Image = System.Drawing.Image.FromFile("1.jpg");
                };
                Invoke(action);
                stopwatchTest.Restart();               
            }
            else
            {
                long i = stopwatchTest.ElapsedMilliseconds;
                Action action = () =>
                {
                    textBox1.Text = i + "";
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }
                };
                Invoke(action);
                stopwatchTest.Stop();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
        
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void open_btn_Click(object sender, EventArgs e)
        {
            try
            {
                //如果串口是关闭的和端口号不为空,就给串口配置赋值
                if (serialPort1.IsOpen == false && port_cbb.Text != "")
                {
                    serialPort1.PortName = port_cbb.Text;
                    serialPort1.BaudRate = 921600;
                    serialPort1.DataBits = 8;

                    //校验位
                    serialPort1.Parity = System.IO.Ports.Parity.None;
                    //停止位
                    serialPort1.StopBits = StopBits.One;

                    //打开串口
                    serialPort1.Open();
                    open_btn.Text = "关闭串口";
                }
                else
                {
                    serialPort1.Close();//关闭串口
                    open_btn.Text = "打开串口";
                    GetComList();//刷新获取设备串口号
                }
            }
            catch (Exception ex)
            {
                GetComList();   //刷新获取设备串口号
                MessageBox.Show(ex.ToString() + serialPort1.PortName.ToString());//抛出报错信息和端口号
            }
        }

        public void GetComList()
        {
            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
            if (keyCom != null)
            {
                string[] sSubKeys = keyCom.GetValueNames();
                port_cbb.Items.Clear(); //清除端口号列表
                foreach (string sName in sSubKeys)
                {
                    string sValue = (string)keyCom.GetValue(sName);
                    port_cbb.Items.Add(sValue);
                }
            }
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {

        }
    }


}