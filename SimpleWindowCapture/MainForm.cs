using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CaptureProxy;
using Win32Proxy;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using Tesseract;

namespace SimpleWindowCapture
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern int WindowFromPoint(int xPoint, int yPoint);

        [System.Runtime.InteropServices.DllImport("user32")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool SetActiveWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool SetFocus(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lppt);

        [System.Runtime.InteropServices.DllImport("user32")]
        public extern static System.IntPtr GetDC(System.IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32")]//拖动无窗体的控件
        public static extern bool ReleaseCapture();

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

        private readonly SynchronizationContext _syncContext;
        private CaptureHelper _captureHelper;
        TesseractEngine ocr;
        TesseractEngine ocrn;

        public MainForm()
        {
            InitializeComponent();

            _syncContext = SynchronizationContext.Current;
        }
        Dictionary<string, Point> gkzb = new Dictionary<string, Point>();
        private void MainForm_Load(object sender, EventArgs e)
        {
            TopMost = true;
            MinimumSize = new Size(Width, Height);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            ocr = new TesseractEngine(@"C:\Users\10947\Downloads\tessdata", "chi_sim", EngineMode.Default);
            ocr.SetVariable("tessedit_char_whitelist", "SAB无尽呆雪女新手SKIP出击无限时战斗评价大获全胜停止自律OFFON");
            ocrn = new TesseractEngine(@"C:\Users\10947\Downloads\tessdata", "chi_sim", EngineMode.Default);
            ocrn.SetVariable("tessedit_char_whitelist", "01l23456789口");

            //gkzb.Add("1-1", new Point(169, 658));
            //gkzb.Add("1-2", new Point(556, 424));
            //gkzb.Add("1-3", new Point(811, 746));
            gkzb.Add("1-4", new Point(778, 254));

            for (int i = 0; i < 57; i++)
            {
                string path = @"D:\Downloads\Compressed\SimpleWindowCapture-master\res\新建文件夹 (3)\" + string.Format("_{0:D4}_图层 ", i) + (57 - i).ToString() + ".bmp";
                ig[i] = (Bitmap)Image.FromFile(path);
            }

            timer1.Start();
            textBox1.Visible = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            RemoveCapture();
        }

        private void RemoveCapture()
        {
            _captureHelper?.Stop();
            _captureHelper = null;
        }

        private void AddCapture(IntPtr handle)
        {
            _captureHelper = new CaptureHelper();
            _captureHelper.CaptureDone += OnCaptureDone;
            _captureHelper.Start(Guid.NewGuid().ToString(), handle);
            CheckForIllegalCrossThreadCalls = false;
        }

        private void OnCaptureDone(string captureName, IntPtr bitmapPtr, Win32Types.BitmapInfo bitmapInfo,bool b)
        {
            try
            {
                if (b)
                {
                    var image = Image.FromHbitmap(bitmapPtr);
                    _syncContext.Post(OnCaptureDone1SafePost, image);
                }
                else
                {
                    mytext += "窗口关闭或最小化\r\n已退出游戏\r\n";
                    gamemode = false;
                    RemoveCapture();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 截取图像的矩形区域
        /// </summary>
        /// <param name="source">源图像对应picturebox1</param>
        /// <param name="rect">矩形区域，如上初始化的rect</param>
        /// <returns>矩形区域的图像</returns>
        public static Image AcquireRectangleImage(Image source, Rectangle rect)
        {
            if (source == null || rect.IsEmpty) return null;
            Bitmap bmSmall = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            //Bitmap bmSmall = new Bitmap(rect.Width, rect.Height, source.PixelFormat);
            using (Graphics grSmall = Graphics.FromImage(bmSmall))
            {

                grSmall.DrawImage(source, new System.Drawing.Rectangle(0, 0, bmSmall.Width, bmSmall.Height), rect, GraphicsUnit.Pixel);
                grSmall.Dispose();
            }
            return bmSmall;
        }

        Bitmap nbmp;
        private void OnCaptureDone1SafePost(object state)
        {
            var image = pictureBox.Image;
            pictureBox.Image = (Bitmap)state;
            nbmp = (Bitmap)state;
            image?.Dispose();

            //image = pictureBox1.Image;
            //Rectangle rect = new Rectangle(193, 314, 560, 130);
            //pictureBox1.Image = AcquireRectangleImage((Bitmap)state,rect);
            //image?.Dispose();
            //label5.Text= BaiduHelper.Ocr((Bitmap)state, "CHN_ENG");


        }

        private void checkBoxTopMost_Click(object sender, EventArgs e)
        {
            TopMost = checkBoxTopMost.Checked;
        }
        Bitmap[] ig = new Bitmap[57];
        private void buttonTitle_Click(object sender, EventArgs e)
        {
            var hWnd = Win32Funcs.FindWindowWrapper(null, "逍遥模拟器".Trim());
            if (string.IsNullOrEmpty("逍遥模拟器") || hWnd.Equals(IntPtr.Zero))
            {
                MessageBox.Show("无效的窗口标题");
                return;
            }
            for (int i = 0; i < 57; i++)
            {
                string path = "C:\\Users\\10947\\Desktop\\新建文件夹 (2)\\" + string.Format("_{0:D4}_图层 ", i) + (57 - i).ToString() + ".bmp";
                ig[i] = (Bitmap)Image.FromFile(path);
            }
            timer1.Start();
            AddCapture(hWnd);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            RemoveCapture();
        }

        static void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo)
        {
            int x = dx, y = dy;
            edit_position(dwFlags, dx, dy, ref x, ref y);
            IntPtr hWndFromPoint = (IntPtr)WindowFromPoint(x, y);
            SetForegroundWindow(hWndFromPoint);
            //screen_to_client(hWndFromPoint, ref x, ref y);
            send_message(hWndFromPoint, dwFlags, cButtons, x, y);
        }

        static void edit_position(int dwFlags, int dx, int dy, ref int x, ref int y)
        {
            Point pos = MousePosition;
            x = x + 100;
            y = y + 100;
            if ((dwFlags | MOUSEEVENTF_ABSOLUTE) == dwFlags)
                SetCursorPos(dx, dy);
            if ((dwFlags | MOUSEEVENTF_MOVE) == dwFlags)
                SetCursorPos(x, y);
        }

        static void send_message(IntPtr hWnd, int dwFlags, int cButtons, int x, int y)
        {
            if ((dwFlags | MOUSEEVENTF_LEFTDOWN) == dwFlags)
                SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)cButtons, (IntPtr)MakeDWord(x, y));
            if ((dwFlags | MOUSEEVENTF_LEFTUP) == dwFlags)
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)cButtons, (IntPtr)MakeDWord(x, y));
            if ((dwFlags | MOUSEEVENTF_RIGHTDOWN) == dwFlags)
                SendMessage(hWnd, WM_RBUTTONDOWN, (IntPtr)cButtons, (IntPtr)MakeDWord(x, y));
            if ((dwFlags | MOUSEEVENTF_RIGHTUP) == dwFlags)
                SendMessage(hWnd, WM_RBUTTONUP, (IntPtr)cButtons, (IntPtr)MakeDWord(x, y));
            if ((dwFlags | MOUSEEVENTF_MIDDLEDOWN) == dwFlags)
                SendMessage(hWnd, WM_MBUTTONDOWN, (IntPtr)cButtons, (IntPtr)MakeDWord(x, y));
            if ((dwFlags | MOUSEEVENTF_MIDDLEUP) == dwFlags)
                SendMessage(hWnd, WM_MBUTTONUP, (IntPtr)cButtons, (IntPtr)MakeDWord(x, y));
        }

        static int MakeDWord(int low, int high)
        {
            return low + (high * Abs(~ushort.MaxValue));
        }

        static int Abs(int value)
        {
            return ((value >> 31) ^ value) - (value >> 31);
        }

        static bool screen_to_client(IntPtr hwnd, ref int x, ref int y)
        {
            bool bRetVal = false;
            Point lpptPos = new Point(x, y);
            if ((bRetVal = ScreenToClient(hwnd, ref lpptPos)))
            {
                x = lpptPos.X;
                y = lpptPos.Y;
            }
            return bRetVal;
        }

        public const int WM_LBUTTONDOWN = 513; // 鼠标左键按下
        public const int WM_LBUTTONUP = 514; // 鼠标左键抬起
        public const int WM_RBUTTONDOWN = 516; // 鼠标右键按下
        public const int WM_RBUTTONUP = 517; // 鼠标右键抬起
        public const int WM_MBUTTONDOWN = 519; // 鼠标中键按下
        public const int WM_MBUTTONUP = 520; // 鼠标中键抬起

        public const int MOUSEEVENTF_MOVE = 0x0001; // 移动鼠标       
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002; // 鼠标左键按下      
        public const int MOUSEEVENTF_LEFTUP = 0x0004; // 鼠标左键抬起      
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008; // 鼠标右键按下     
        public const int MOUSEEVENTF_RIGHTUP = 0x0010; // 鼠标右键抬起        
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020; // 鼠标中键按下  
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040; // 鼠标中键抬起         
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000; // 绝对坐标 

        private void button1_Click(object sender, EventArgs e)
        {
            var hWnd = Win32Funcs.FindWindowWrapper(null, "逍遥模拟器".Trim());
            if (string.IsNullOrEmpty("逍遥模拟器") || hWnd.Equals(IntPtr.Zero))
            {
                MessageBox.Show("无效的窗口标题");
                return;
            }
            AddCapture(hWnd);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            nbmp.Save("C:\\temp\\Picture1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

        }

        private string GetText(Rectangle rect)
        {
            Bitmap tbmp = (Bitmap)AcquireRectangleImage(nbmp, rect);
            string ts = BaiduHelper.Ocr(tbmp, "CHN_ENG");
            tbmp.Dispose();
            return ts;
        }
        public void BlackWhite(Bitmap b, int v)
        {
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color c = b.GetPixel(i, j);
                    int t = (c.R + c.G + c.B) / 3;
                    //b.SetPixel(i, j, Color.FromArgb(t, t, t));
                    if (t > v)
                        b.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    else
                        b.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                }
            }
        }
        private string GetText2(Rectangle rect,int v)
        {
            Bitmap tbmp = (Bitmap)AcquireRectangleImage(nbmp, rect);
            tbmp.Save(@"C:\Users\10947\Desktop\text.bmp");
            BlackWhite(tbmp, v);
            tbmp.Save(@"C:\Users\10947\Desktop\text2.bmp");
            var img = new Bitmap(tbmp);
            var page = ocr.Process(img);
            tbmp.Dispose();
            img.Dispose();
            string res = page.GetText();
            page.Dispose();
            res = res.Replace(" ", "");
            return res;
        }
        private string GetText3(Rectangle rect, int v)
        {
            Bitmap tbmp = (Bitmap)AcquireRectangleImage(nbmp, rect);
            tbmp.Save(@"C:\Users\10947\Desktop\text.bmp");
            BlackWhite(tbmp, v);
            tbmp.Save(@"C:\Users\10947\Desktop\text2.bmp");
            var img = new Bitmap(tbmp);
            var page = ocrn.Process(img);
            string res = page.GetText();
            res = res.Replace('l', '1');
            res = res.Replace('口', '0');
            res = res.Replace(" ", "");
            tbmp.Dispose();
            img.Dispose();
            page.Dispose();
            return res;
        }

        private string GetText(Rectangle rect,Bitmap bmp)
        {
            Bitmap tbmp = (Bitmap)AcquireRectangleImage(bmp, rect);
            string ts = BaiduHelper.Ocr(tbmp, "CHN_ENG");
            tbmp.Dispose();
            return ts;
        }

        int oil = 0;
        int material = 0;
        int diamond = 0;
        int lv = 0;

        private void zycx()
        {
            mytext += "资源查询：\r\n";
            Rectangle rect = new Rectangle(650, 60, 70, 38);
            string ts = GetText3(rect,100);
            if (ts != "")
            {
                int t = 0;
                oil = 0;
                while (ts[t] >= '0' && ts[t] <= '9')
                {
                    int tlv = ts[t] - '0';
                    oil = oil * 10 + tlv;
                    t++;
                }

            }
            mytext += "油：" + oil.ToString() + "\r\n";
            rect = new Rectangle(850, 60, 70, 38);
            ts = GetText3(rect,100);
            if (ts != "")
            {
                int t = 0;
                material = 0;
                while (ts[t] >= '0' && ts[t] <= '9')
                {
                    int tlv = ts[t] - '0';
                    material = material * 10 + tlv;
                    t++;
                }

            }
            mytext += "物资：" + material.ToString() + "\r\n";
            rect = new Rectangle(1065, 65, 70, 20);
            Delay(1);
            ts = GetText3(rect,120);
            if (ts != "")
            {
                int t = 0;
                diamond = 0;
                while (ts[t] >= '0' && ts[t] <= '9')
                {
                    int tlv = ts[t] - '0';
                    diamond = diamond * 10 + tlv;
                    t++;
                }

            }
            mytext += "钻石：" + diamond.ToString() + "\r\n";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path = @"C:\temp\Picture1.bmp";
            Bitmap ig = (Bitmap)Image.FromFile(path);
            //SimilarPhoto.FindEm2(ig);
        }

        /*
        private void xxcx()
        {
            mytext += "当前消息：\r\n";
            Rectangle rect = new Rectangle(774, 867, 34, 34);
            Image i2 = Image.FromFile(@"C:\Users\10947\Desktop\res1.bmp");
            Int32 t = SimilarPhoto.Calc(AcquireRectangleImage(nbmp, rect), i2);
            if (t <= 30)
                mytext += "生活区有消息\r\n";
            rect = new Rectangle(1362, 867, 34, 34);
            t = SimilarPhoto.Calc(AcquireRectangleImage(nbmp, rect), i2);
            if (t <= 30)
                mytext += "建筑有消息\r\n";
        }
        */

        public static bool Delay(int delayTime)
        {
            DateTime now = DateTime.Now;
            int s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Seconds;
                Application.DoEvents();
            }
            while (s < delayTime);
            return true;
        }

        private void shws()
        {
            _captureHelper.MoveClickBack(38, 200, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Delay(1);
            Rectangle rect = new Rectangle(148, 132, 54, 30);
            string ts = GetText3(rect,222);
            int toil = 0;
            if (ts != "")
            {
                int t = 0;
                while (ts[t] >= '0' && ts[t] <= '9')
                {
                    int tlv = ts[t] - '0';
                    toil = toil * 10 + tlv;
                    t++;
                }

            }
            mytext += "可收获油：" + toil.ToString() + "\r\n";
            rect = new Rectangle(390, 131, 54, 30);
            ts = GetText3(rect,222);
            int tmat = 0;
            if (ts != "")
            {
                int t = 0;
                tmat = 0;
                while (ts[t] >= '0' && ts[t] <= '9')
                {
                    int tlv = ts[t] - '0';
                    tmat = tmat * 10 + tlv;
                    t++;
                }

            }
            mytext += "可收获物资：" + tmat.ToString() + "\r\n";

            mytext += "收获石油与物资\r\n";
            _captureHelper.MoveClickBack(172, 117, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Delay(1);
            /*
            rect = new Rectangle(585, 424, 470, 44);
            Delay(1);
            ts = GetText(rect);
            if(ts.Contains("上限"))
            {
                mytext += "石油资源达到上限\r\n";
            }
            else
                mytext += "成功获取石油\r\n";
             */
            _captureHelper.MoveClickBack(422, 116, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Delay(1);
            /*
            rect = new Rectangle(585, 424, 470, 44);
            Delay(1);
            ts = GetText(rect);
            if (ts.Contains("上限"))
            {
                mytext += "物资资源达到上限\r\n";
            }
            else
                mytext += "成功获取物资\r\n";
             */
            _captureHelper.MoveClickBack(900, 400, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Delay(1);
        }

        private void cj()
        {
            mytext += "正在扫描敌军……\r\n";
            Application.DoEvents();
            Delay(2);
            ArrayList res_list = SimilarPhoto.FindP(nbmp);
            mytext += "发现" + res_list.Count.ToString() + "个敌军\r\n";
            for (int i = 0; i < res_list.Count; i++)
            {
                mytext += (i + 1).ToString() + "号：";
                //mytext += ((ResultInfo)res_list[i]).star.ToString() + "星";
                switch (((ResultInfo)res_list[i]).type)
                {
                    case 1:
                        mytext += "轻甲,";
                        break;
                    case 2:
                        mytext += "重甲,";
                        break;
                    case 3:
                        mytext += "航母,";
                        break;
                    case 4:
                        mytext += "运输车,";
                        break;
                    case 5:
                        mytext += "BOSS,";
                        break;
                }
                mytext += "坐标：(" + ((ResultInfo)res_list[i]).x * 10 + "," + ((ResultInfo)res_list[i]).y * 10 + ")";
                mytext += "\r\n";
            }
            int ti = 0;
            for(;ti<res_list.Count;ti++)
            {
                mytext += "尝试向"+ti.ToString()+"号进攻。\r\n";
                _captureHelper.MoveClickBack(((ResultInfo)res_list[0]).x * 10, ((ResultInfo)res_list[0]).y * 10, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Delay(1);
                Rectangle rectt = new Rectangle(673, 413, 262, 51);
                string tst = GetText2(rectt, 100);
                if (tst.Contains("无法"))
                {
                    mytext += "距离不够\r\n";
                    continue;
                }
                break;
            }
            Delay(3);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            picprotemp();
        }

        private void picprotemp()
        {
            for (int i = 0; i < 57; i++)
            {
                string path = "C:\\Users\\10947\\Desktop\\新建文件夹 (2)\\" + string.Format("_{0:D4}_图层 ", i) + (57 - i).ToString() + ".bmp";
                string pathy = "C:\\Users\\10947\\Desktop\\新建文件夹\\" + string.Format("_{0:D4}_图层 ", i) + (57 - i).ToString() + ".bmp";
                Bitmap ig = (Bitmap)Image.FromFile(path);
                Bitmap igy = (Bitmap)Image.FromFile(pathy);
                for (int k = 0; k < ig.Width; k++)
                {
                    for (int j = 0; j < ig.Height; j++)
                    {
                        Color color = ig.GetPixel(k, j);
                        Color colory = igy.GetPixel(k, j);
                        if(color.R==0)
                        {
                            if(k>0 && k<ig.Width-1)
                            {
                                Color color1 = ig.GetPixel(k-1, j);
                                Color color2 = ig.GetPixel(k+1, j);
                                if (color1.R != 0 && color2.R != 0)
                                    ig.SetPixel(k, j, colory);
                            }
                            if (j > 0 && j < ig.Height - 1)
                            {
                                Color color1 = ig.GetPixel(k, j - 1);
                                Color color2 = ig.GetPixel(k, j + 1);
                                if (color1.R != 0 && color2.R != 0)
                                    ig.SetPixel(k, j, colory);
                            }
                        }
                    }
                }
                ig.Save(@"D:\Downloads\Compressed\SimpleWindowCapture-master\res\新建文件夹 (3)\" + string.Format("_{0:D4}_图层 ", i) + (57 - i).ToString() + ".bmp");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            mytext += "a\r\na";
        }

        private void checkBoxTopMost_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        int tit = -1;
        int texti = 0;
        int hs = 0;
        string mytext = "";
        string pictext = "";

        private void TextShow()
        {
            tit = (tit + 1) % 57;
            Bitmap newBitMap = new Bitmap(ig[0].Width + ig[0].Width/3, ig[0].Height + ig[0].Height/3);
            Graphics g = Graphics.FromImage(newBitMap);
            g.DrawImage(ig[tit], new PointF(0, 0));
            //newBitMap.Save(@"C:\temp\Picture1.bmp");

            if (mytext.Length > 0 && mytext.Length > pictext.Length)
            {
                Bitmap dia = (Bitmap)Image.FromFile(@"D:\Downloads\Compressed\SimpleWindowCapture-master\res\dig.png");
                g.DrawImage(dia, new PointF(0, 600));

                FontFamily fm = new FontFamily("黑体");
                Font font = new Font(fm, 50, FontStyle.Bold, GraphicsUnit.Pixel);
                SolidBrush sb = new SolidBrush(Color.White);
                g.DrawString(pictext, font, sb, new PointF(10, 610));
                if (mytext[texti] == '\n')
                {
                    hs++;
                }
                if (hs < 3)
                {
                    texti++;
                    pictext = mytext.Substring(0, texti);
                }
                else
                {
                    textBox1.Text += pictext;
                    mytext = mytext.Remove(0, pictext.Length + 1);
                    pictext = "";
                    hs = 0;
                    texti = 0;
                }
            }
            else
            {
                textBox1.Text += pictext;
                mytext = "";
                pictext = "";
                hs = 0;
                texti = 0;
            }
            g.Dispose();
            pictureBox2.Image = newBitMap;
            
        }
        int tcount = 0;
        bool gamemode = false;

        private void EnterGame()
        {
            mytext += "尝试进入游戏……\r\n";
            var hWnd = Win32Funcs.FindWindowWrapper(null, "逍遥模拟器".Trim());
            if (string.IsNullOrEmpty("逍遥模拟器") || hWnd.Equals(IntPtr.Zero))
            {
                mytext += "进入游戏失败\r\n未打开逍遥模拟器\r\n";
                return;
            }
            AddCapture(hWnd);
            mytext += "已进入游戏\r\n";
            gamemode = true;
            return;
        }
        int res = 1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            TextShow();
            if (tcount<10)
            {
                tcount++;
                return;
            }
            tcount = 0;
            if (res == 0)
                return;
            res = 0;
            if(!gamemode)
            {
                EnterGame();
                res = 1;
            }
            else
            {
                mytext += "分析当前界面：\r\n";
                //_captureHelper.MoveClickBack(700, 500, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Rectangle rect = new Rectangle(116, 50, 143, 30);
                string ts = GetText2(rect,126);
                if (ts.Contains("无尽呆雪女"))
                {
                    mytext += "为主界面\r\n";
                    zycx();
                    shws();
                    mytext += "出击\r\n";
                    _captureHelper.MoveClickBack(1090, 415, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                }
                rect = new Rectangle(118, 51, 62, 30);
                ts = GetText2(rect,210);
                if (ts.Contains("出击"))
                {
                    mytext += "为出击页面\r\n";
                    mytext += "刷图1-4：\r\n";
                    _captureHelper.MoveClickBack(gkzb["1-4"].X, gkzb["1-4"].Y, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    Delay(1);
                    _captureHelper.MoveClickBack(949, 562, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    Delay(1);
                    _captureHelper.MoveClickBack(1075, 647, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    Delay(1);
                }
                rect = new Rectangle(120, 53, 64, 34);
                ts = GetText2(rect,200);
                if (ts.Contains("限"))
                {
                    mytext += "为战斗页面\r\n";
                    cj();
                }
                rect = new Rectangle(1057, 649, 76, 42);
                ts = GetText2(rect, 200);
                if (ts.Contains("出击"))
                {
                    mytext += "为出击准备页面\r\n";
                    rect = new Rectangle(741, 143, 68, 34);
                    ts = GetText2(rect,80);
                    if (ts.Contains("ON"))
                    {
                        mytext += "自律已开启\r\n";
                    }
                    else
                    {
                        mytext += "自律未开启\r\n";
                        mytext += "自动开启自律\r\n";
                        _captureHelper.MoveClickBack(836, 163, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        Delay(1);
                    }
                    mytext += "出击！\r\n";
                    _captureHelper.MoveClickBack(1155, 681, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    Delay(1);
                }
                rect = new Rectangle(611, 388, 181, 53);
                ts = GetText2(rect,155);
                if (ts.Contains("战斗"))
                {
                    mytext += "为战斗结束页面：\r\n";
                    rect = new Rectangle(138, 250, 460, 100);
                    ts = GetText2(rect,128);
                    if (ts.Contains("大获全胜"))
                    {
                        mytext += "S胜！\r\n";
                    }
                    _captureHelper.MoveClickBack(700, 600, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    Delay(1);
                    _captureHelper.MoveClickBack(700, 600, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    Delay(2);
                    /*
                    rect = new Rectangle(700, 290, 215, 67);
                    Delay(1);
                    ts = GetText(rect);
                    if (ts.Contains("得"))
                    {
                        mytext += "获得道具:\r\n";
                        rect = new Rectangle(477, 542, 616, 46);
                        Delay(1);
                        ts = GetText(rect);
                        ts.Replace("\r\n", ",");
                        mytext += ts;
                        mytext += "\r\n";
                        _captureHelper.MoveClickBack(700, 600, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        Delay(1);
                    }
                    rect = new Rectangle(121, 625, 1380, 94);
                    ResultInfo re = SimilarPhoto.FindMVP(AcquireRectangleImage(nbmp, rect));
                    rect = new Rectangle(re.x + 121, re.y + 625, 117, 31);
                    //Image tempi = AcquireRectangleImage(nbmp, rect);
                    //tempi.Save(@"C:\temp\tempi.bmp");
                    Delay(1);
                    ts = GetText(rect);
                    mytext += "MVP为:\r\n" + ts+"\r\n";
                    */
                    _captureHelper.MoveClickBack(1093, 704, Control.MousePosition.X, Control.MousePosition.Y, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    Delay(5);
                }
                rect = new Rectangle(12, 74, 91, 26);
                ts = GetText2(rect,210);
                if (ts.Contains("自律"))
                {
                    mytext += "正在战斗……\r\n";
                    Delay(7);
                }
                res = 1;
            }
        }

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {

            //Point p = pictureBox2.PointToScreen(this.FindForm().Location);
            //p.X = MousePosition.X - p.X;
            //p.Y = MousePosition.Y - p.Y;
            //MessageBox.Show(p.X.ToString() + "," + p.Y.ToString());
            //textBox1.Visible = true;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            //textBox1.Visible = false;
        }

        private void textBox1_MouseLeave(object sender, EventArgs e)
        {
            textBox1.Visible = false;
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 切换置顶ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, (IntPtr)SC_MOVE + HTCAPTION, IntPtr.Zero);
        }
    }
}
