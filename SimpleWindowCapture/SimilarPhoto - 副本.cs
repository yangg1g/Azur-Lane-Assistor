using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;


namespace SimpleWindowCapture
{
    class SimilarPhoto
    {

        private static String GetHash(Image SourceImg)
        {
            Image image = ReduceSize(SourceImg);
            Byte[] grayValues = ReduceColor(image);
            Byte average = CalcAverage(grayValues);
            String reslut = ComputeBits(grayValues, average);
            return reslut;
        }

        private static String GetHash2(Image SourceImg)
        {
            Byte[] grayValues = ReduceColor(SourceImg);
            Byte average = CalcAverage(grayValues);
            String reslut = ComputeBits(grayValues, average);
            return reslut;
        }

        // Step 1 : Reduce size to 8*8
        private static Image ReduceSize(Image SourceImg, int width = 8, int height = 8)
        {
            Image image = SourceImg.GetThumbnailImage(width, height, () => { return false; }, IntPtr.Zero);
            return image;
        }

        // Step 2 : Reduce Color
        private static Byte[] ReduceColor(Image image)
        {
            Bitmap bitMap = new Bitmap(image);
            Byte[] grayValues = new Byte[image.Width * image.Height];

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    Color color = bitMap.GetPixel(x, y);
                    byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    grayValues[x * image.Height + y] = grayValue;
                }
            return grayValues;
        }

        // Step 3 : Average the colors
        private static Byte CalcAverage(byte[] values)
        {
            int sum = 0;
            for (int i = 0; i < values.Length; i++)
                sum += (int)values[i];
            return Convert.ToByte(sum / values.Length);
        }

        // Step 4 : Compute the bits
        private static String ComputeBits(byte[] values, byte averageValue)
        {
            char[] result = new char[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < averageValue)
                    result[i] = '0';
                else
                    result[i] = '1';
            }
            return new String(result);
        }

        // Compare hash
        private static Int32 CalcSimilarDegree(string a, string b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException();
            int count = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    count++;
            }
            return count;
        }

        private static Int32 CalcSimilarDegree2(Image image1, Image image2)
        {
            Bitmap bitMap1 = new Bitmap(image1);
            Bitmap bitMap2 = new Bitmap(image2);
            int res = 0;
            int res2 = 0;
            for (int x = 0; x < image1.Width; x++)
                for (int y = 0; y < image2.Height; y++)
                {
                    Color color1 = bitMap1.GetPixel(x, y);
                    Color color2 = bitMap2.GetPixel(x, y);
                    if (color2.R == 255 && color2.G == 0 && color2.B == 255)
                    {
                        continue;
                    }
                    double K = color1.R;
                    if (K < color1.B)
                        K = color1.B;
                    if (K < color1.G)
                        K = color1.G;
                    K = 1 - K / 255;
                    double Y = (1.0 - color1.B / 255.0 - K) / (1.0 - K);
                    if (Y > 0.5)
                        res2++;
                    int t = (color1.R - color2.R) * (color1.R - color2.R) + (color1.G - color2.G) * (color1.G - color2.G) + (color1.B - color2.B) * (color1.B - color2.B);
                    if (t > 10000)
                        res++;
                }
            if (res2 > 10)
                return res;
            else
                return 255;
        }

        private static Image AcquireRectangleImage(Image source, Rectangle rect)
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

        public static Int32 Calc(Image i1, Image i2)
        {
            string s1 = GetHash(i1);
            string s2 = GetHash(i2);
            return CalcSimilarDegree(s1, s2);
        }

        public static Bitmap ToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    int gray = (int)(color.R + color.G + color.B)/3;
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        public static Bitmap ToGrayt(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel
(i, j);
                    //利用公式计算灰度值
                    int gray = (color.R + color.G + color.B) / 3;
                    if (gray > 128)
                        gray = 255;
                    else
                        gray = 0;
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        public static Bitmap ToRed(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel
(i, j);
                    //利用公式计算灰度值
                    int gray = color.R;
                    if (gray > 240)
                        gray = 255;
                    else
                        gray = 0;
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        public static ArrayList tFind(int type,Bitmap bmp1,Bitmap bmp2,Bitmap tb1,Bitmap tb2,Bitmap tb3,Bitmap xbmp)
        {
            ArrayList m_list = new ArrayList();
            for (int ii = 0; ii < bmp1.Width - bmp2.Width; ii++)
            {
                for (int j = 0; j < bmp1.Height - bmp2.Height; j++)
                {
                    Rectangle rect = new Rectangle(ii, j, bmp2.Width, bmp2.Height);
                    int res = 0;
                    for (int k2 = 0; k2 < bmp2.Width; k2++)
                    {
                        for (int j2 = 0; j2 < bmp2.Height; j2++)
                        {
                            Color color1 = bmp1.GetPixel(ii + k2, j + j2);
                            Color color2 = bmp2.GetPixel(k2, j2);
                            res += (color1.R - color2.R) * (color1.R - color2.R);
                        }
                    }
                    if (res < 310000)
                    {
                        ResultInfo data = new ResultInfo();
                        data.x = ii;
                        data.y = j;
                        data.score = res;
                        data.type = type;
                        m_list.Add(data);
                    }
                }
            }
            ResultInfoAscent sa = new ResultInfoAscent();
            m_list.Sort(sa);
            if(type==5)
            {
                m_list.RemoveRange(1, m_list.Count - 1);
                if(((ResultInfo)m_list[0]).score<= 210000)
                {
                    ((ResultInfo)m_list[0]).star = -1;
                    ((ResultInfo)m_list[0]).x = (((ResultInfo)m_list[0]).x + bmp2.Width / 2) * 10;
                    ((ResultInfo)m_list[0]).y = (((ResultInfo)m_list[0]).y + bmp2.Height / 2) * 10;
                }
                else
                {
                    m_list.Clear();
                }
                return m_list;
            }
            int i = 0;
            while (i < m_list.Count)
            {
                int j = i + 1;
                while (j < m_list.Count)
                {
                    int ttx = ((ResultInfo)m_list[i]).x - ((ResultInfo)m_list[j]).x;
                    int tty = ((ResultInfo)m_list[i]).y - ((ResultInfo)m_list[j]).y;
                    if (ttx * ttx + tty * tty < bmp2.Width * bmp2.Height)
                    {
                        m_list.RemoveAt(j);
                    }
                    else
                        j++;
                }
                if(type>4)
                {
                    ((ResultInfo)m_list[i]).star = -1;
                    ((ResultInfo)m_list[i]).x = (((ResultInfo)m_list[i]).x + bmp2.Width / 2) * 10;
                    ((ResultInfo)m_list[i]).y = (((ResultInfo)m_list[i]).y + bmp2.Height / 2) * 10;
                    i++;
                    continue;
                }
                int tx = ((ResultInfo)m_list[i]).x*2 - 20;
                int ty = ((ResultInfo)m_list[i]).y*2 - 20;
                ArrayList m_list2 = new ArrayList();
                for (int ii = tx; ii < tx + 20; ii++)
                {
                    for (int jj = ty; jj < ty + 20; jj++)
                    {
                        Rectangle rect = new Rectangle(ii, jj, 5, 5);
                        ResultInfo data1 = new ResultInfo();
                        ResultInfo data2 = new ResultInfo();
                        ResultInfo data3 = new ResultInfo();
                        int res = 0;
                        data1.x = ii;
                        data1.y = jj;
                        data1.type = -1;
                        data2.x = ii;
                        data2.y = jj;
                        data2.type = -1;
                        data3.x = ii;
                        data3.y = jj;
                        data3.type = -1;

                        for (int k2 = 0; k2 < 10; k2++)
                        {
                            for (int j2 = 0; j2 < 10; j2++)
                            {
                                Color color1 = xbmp.GetPixel(ii + k2, jj + j2);
                                Color color2 = tb1.GetPixel(k2, j2);
                                res += (color1.R - color2.R) * (color1.R - color2.R);
                            }
                        }
                        data1.score = res;
                        data1.star = 1;
                        m_list2.Add(data1);

                        res = 0;
                        for (int k2 = 0; k2 < 10; k2++)
                        {
                            for (int j2 = 0; j2 < 10; j2++)
                            {
                                Color color1 = xbmp.GetPixel(ii + k2, jj + j2);
                                Color color2 = tb2.GetPixel(k2, j2);
                                res += (color1.R - color2.R) * (color1.R - color2.R);
                            }
                        }
                        data2.score = res;
                        data2.star = 2;
                        m_list2.Add(data2);

                        res = 0;
                        for (int k2 = 0; k2 < 10; k2++)
                        {
                            for (int j2 = 0; j2 < 10; j2++)
                            {
                                Color color1 = xbmp.GetPixel(ii + k2, jj + j2);
                                Color color2 = tb3.GetPixel(k2, j2);
                                res += (color1.R - color2.R) * (color1.R - color2.R);
                            }
                        }
                        data3.score = res;
                        data3.star = 3;
                        m_list2.Add(data3);

                    }
                }
                m_list2.Sort(sa);
                ((ResultInfo)m_list[i]).star = ((ResultInfo)m_list2[i]).star;
                ((ResultInfo)m_list[i]).x = (((ResultInfo)m_list[i]).x + bmp2.Width / 2) * 10;
                ((ResultInfo)m_list[i]).y = (((ResultInfo)m_list[i]).y + bmp2.Height / 2) * 10;
                i++;
            }
            return m_list;
        }

        public static ResultInfo pFind(Bitmap bmp, Bitmap bmp1)
        {
            ArrayList m_list = new ArrayList();
            for (int ii = 0; ii < bmp.Width - bmp1.Width; ii++)
            {
                for (int j = 0; j < bmp.Height - bmp1.Height; j++)
                {
                    Rectangle rect = new Rectangle(ii, j, bmp1.Width, bmp1.Height);
                    int res = 0;
                    for (int k2 = 0; k2 < bmp1.Width; k2++)
                    {
                        for (int j2 = 0; j2 < bmp1.Height; j2++)
                        {
                            Color color1 = bmp.GetPixel(ii + k2, j + j2);
                            Color color2 = bmp1.GetPixel(k2, j2);
                            res += (color1.R - color2.R) * (color1.R - color2.R);
                        }
                    }
                    if (res < 2000000)
                    {
                        ResultInfo data = new ResultInfo();
                        data.x = ii;
                        data.y = j;
                        data.score = res;
                        m_list.Add(data);
                    }
                }
            }
            ResultInfoAscent sa = new ResultInfoAscent();
            m_list.Sort(sa);
            return (ResultInfo)m_list[0];
        }

        public static ArrayList FindP(Image i1)
        {
            Bitmap bmp = ToGray((Bitmap)i1);
            Bitmap bmp1 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\qj.bmp");
            Bitmap bmp2 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\zj.bmp");
            Bitmap bmp3 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\hm.bmp");
            Bitmap bmp4 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\ysc.bmp");
            Bitmap bmp5 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\boss.bmp");
            Bitmap bmp6 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\wh.bmp");
            Bitmap tb1 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\xj1.bmp");
            Bitmap tb2 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\xj2.bmp");
            Bitmap tb3 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\xj3.bmp");
            tb1 = ToGray(tb1);
            tb2 = ToGray(tb2);
            tb3 = ToGray(tb3);
            bmp1 = ToGray(bmp1);
            bmp2 = ToGray(bmp2);
            bmp3 = ToGray(bmp3);
            bmp4 = ToGray(bmp4);
            bmp5 = ToGray(bmp5);
            bmp6 = ToGray(bmp6);
            Bitmap xbmp = (Bitmap)ReduceSize(i1, i1.Width / 5, i1.Height / 5);
            bmp = (Bitmap)ReduceSize(i1, i1.Width / 10, i1.Height / 10);
            bmp1 = (Bitmap)ReduceSize(bmp1, bmp1.Width / 10, bmp1.Height / 10);
            bmp2 = (Bitmap)ReduceSize(bmp2, bmp2.Width / 10, bmp2.Height / 10);
            bmp3 = (Bitmap)ReduceSize(bmp3, bmp3.Width / 10, bmp3.Height / 10);
            bmp4 = (Bitmap)ReduceSize(bmp4, bmp4.Width / 10, bmp4.Height / 10);
            bmp5 = (Bitmap)ReduceSize(bmp5, bmp5.Width / 10, bmp5.Height / 10);
            bmp6 = (Bitmap)ReduceSize(bmp6, bmp6.Width / 10, bmp6.Height / 10);
            //tb1.Save("C:\\temp\\tPicture1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //tb3.Save("C:\\temp\\tPicture3.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            tb1 = (Bitmap)ReduceSize(tb1, tb1.Width / 5, tb1.Height / 5);
            tb2 = (Bitmap)ReduceSize(tb2, tb2.Width / 5, tb2.Height / 5);
            tb3 = (Bitmap)ReduceSize(tb3, tb3.Width / 5, tb3.Height / 5);
            //xbmp.Save("C:\\temp\\tPicturex.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //bmp5.Save("C:\\temp\\tPicture5.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //tb3.Save("C:\\temp\\tPicture3.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            ArrayList m_list1 = tFind(1, bmp, bmp1, tb1,tb2,tb3,xbmp);
            ArrayList m_list2 = tFind(2, bmp, bmp2, tb1,tb2,tb3,xbmp);
            ArrayList m_list3 = tFind(3, bmp, bmp3, tb1,tb2,tb3,xbmp);
            ArrayList m_list4 = tFind(4, bmp, bmp4, tb1, tb2, tb3, xbmp);
            ArrayList m_list5 = tFind(5, bmp, bmp5, tb1,tb2,tb3,xbmp);
            ArrayList m_list6 = tFind(6, bmp, bmp6, tb1,tb2,tb3,xbmp);
            ArrayList m_list = m_list1;
            for (int ti = 0; ti < m_list2.Count; ti++)
                m_list.Add(m_list2[ti]);
            for (int ti = 0; ti < m_list3.Count; ti++)
                m_list.Add(m_list3[ti]);
            for (int ti = 0; ti < m_list4.Count; ti++)
                m_list.Add(m_list4[ti]);
            for (int ti = 0; ti < m_list5.Count; ti++)
                m_list.Add(m_list5[ti]);
            for (int ti = 0; ti < m_list6.Count; ti++)
                m_list.Add(m_list6[ti]);
            ResultInfoAscent sa = new ResultInfoAscent();
            m_list.Sort(sa);
            return m_list;
        }

        public static bool Zoom(Bitmap srcBmp, double ratioW, double ratioH, out Bitmap dstBmp)
        {//ZoomType为自定义的枚举类型
            if (srcBmp == null)
            {
                dstBmp = null;
                return false;
            }
            //若缩放大小与原图一样，则返回原图不做处理
            if ((ratioW == 1.0) && ratioH == 1.0)
            {
                dstBmp = new Bitmap(srcBmp);
                return true;
            }
            //计算缩放高宽
            double height = ratioH * (double)srcBmp.Height;
            double width = ratioW * (double)srcBmp.Width;
            dstBmp = new Bitmap((int)width, (int)height);

            BitmapData srcBmpData = srcBmp.LockBits(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData dstBmpData = dstBmp.LockBits(new Rectangle(0, 0, dstBmp.Width, dstBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* srcPtr = null;
                byte* dstPtr = null;
                int srcI = 0;
                for (int i = 0; i < dstBmp.Height; i++)
                {
                    srcI = (int)(i / ratioH);//srcI是此时的i对应的原图像的高
                    srcPtr = (byte*)srcBmpData.Scan0 + srcI * srcBmpData.Stride;
                    dstPtr = (byte*)dstBmpData.Scan0 + i * dstBmpData.Stride;
                    for (int j = 0; j < dstBmp.Width; j++)
                    {
                        dstPtr[j * 3] = srcPtr[(int)(j / ratioW) * 3];//j / ratioW求出此时j对应的原图像的宽
                        dstPtr[j * 3 + 1] = srcPtr[(int)(j / ratioW) * 3 + 1];
                        dstPtr[j * 3 + 2] = srcPtr[(int)(j / ratioW) * 3 + 2];
                    }
                }
                
            }
            srcBmp.UnlockBits(srcBmpData);
            dstBmp.UnlockBits(dstBmpData);
            return true;
        }

        public static ArrayList FindEm(Image i1)
        {
            Bitmap bmpt;
            Zoom((Bitmap)i1, 0.3, 0.3, out bmpt);
            Bitmap back = (Bitmap)i1.Clone();
            bmpt = ToGrayt(bmpt);
            bmpt.Save(@"C:\Users\10947\Desktop\bmp.bmp");
            Bitmap bmp = bmpt;
            //bmpt.Save(@"C:\Users\10947\Desktop\Picture2p.bmp");

            Bitmap em = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\Picturep.bmp");
            ArrayList m_list = new ArrayList();
            for (int ii = 150; ii < bmp.Width - 100; ii++)
            {
                for (int j = 150; j < bmp.Height - 100; j++)
                {
                    Color colort = bmp.GetPixel(ii, j);
                    if (colort.R == 0)
                        continue;
                    int res = 0;
                    int soc = 0;
                    for (int k2 = 0; k2 < em.Width; k2++)
                    {
                        for (int j2 = 0; j2 < em.Height; j2++)
                        {
                            Color color1 = bmp.GetPixel(ii + k2, j + j2);
                            Color color2 = em.GetPixel(k2, j2);
                            if (color1.R == color2.R)
                            {
                                res += 1;
                                if (color1.R == 255)
                                    soc++;
                            }
                            
                        }
                    }
                    if (res>700)
                    {
                        ResultInfo data = new ResultInfo();
                        data.x = ii;
                        data.y = j;
                        data.score = res;
                        m_list.Add(data);
                    }
                }
            }
            ResultInfoAscent sa = new ResultInfoAscent();
            m_list.Sort(sa);
            int i = 0;
            while (i < m_list.Count)
            {
                int j = i + 1;
                while (j < m_list.Count)
                {
                    int ttx = ((ResultInfo)m_list[i]).x - ((ResultInfo)m_list[j]).x;
                    int tty = ((ResultInfo)m_list[i]).y - ((ResultInfo)m_list[j]).y;
                    if (ttx * ttx + tty * tty < em.Width * em.Height)
                    {
                        m_list.RemoveAt(j);
                    }
                    else
                        j++;
                }
                int tx = ((ResultInfo)m_list[i]).x - 105;
                int ty = ((ResultInfo)m_list[i]).y - 125;
                Rectangle rect = new Rectangle(tx, ty, 50, 50);
                Bitmap tbmp = (Bitmap)AcquireRectangleImage(back, rect);
                tbmp = ToRed(tbmp);
                tbmp.Save(@"C:\Users\10947\Desktop\tbmp.bmp");
                int dot = 0;
                for (int ii = 0; ii < 50; ii++)
                {
                    for (int jj = 0; jj < 50; jj++)
                    {
                        Color c = tbmp.GetPixel(ii, jj);
                        if (c.R == 255)
                            dot++;
                    }
                }
                if(dot<210)
                    ((ResultInfo)m_list[i]).star = 1;
                else if(dot<230)
                    ((ResultInfo)m_list[i]).star = 2;
                else
                    ((ResultInfo)m_list[i]).star = 3;
                i++;
            }
            return m_list;
        }
        static Bitmap bmp;
        static Bitmap em;
        public static ArrayList Prefind(int t, ArrayList m_list)
        {
            bmp.Save(@"C:\Users\10947\Desktop\bmp.bmp");
            em.Save(@"C:\Users\10947\Desktop\em.bmp");
            int i = 0;
            while (i < m_list.Count)
            {
                int ii = ((ResultInfo)m_list[i]).x;
                int j = ((ResultInfo)m_list[i]).y;
                int res = 0;
                for (int k2 = 0; k2 < 2*t+1; k2++)
                {
                    for (int j2 = 0; j2 < 2*t+1; j2++)
                    {
                        Color color1 = bmp.GetPixel(ii + k2, j + j2);
                        Color color2 = em.GetPixel(em.Width / 2 - t + k2, em.Height / 2 - t + k2);
                        if (Math.Abs(color1.R - color2.R) <= 10)
                        {
                            res += 1;
                        }

                    }
                }
                if (res <= t * t)
                    m_list.RemoveAt(i);
                else
                    i++;
            }
            return m_list;
        }

        public static ArrayList FindEm2(Image i1)
        {
            //Bitmap bmpt;
            //Zoom((Bitmap)i1, 0.3, 0.3, out bmpt);
            //Bitmap back = (Bitmap)i1.Clone();
            //bmpt = ToGrayt(bmpt);
            //bmpt.Save(@"C:\Users\10947\Desktop\bmp.bmp");
            bmp = ToGray((Bitmap)i1);
            //bmpt.Save(@"C:\Users\10947\Desktop\Picture2p.bmp");

            em = ToGray((Bitmap)Image.FromFile(@"C:\Users\10947\Downloads\Compressed\SimpleWindowCapture-master\res\qj.bmp"));
            bmp.Save(@"C:\Users\10947\Desktop\bmp.bmp");
            em.Save(@"C:\Users\10947\Desktop\em.bmp");
            ArrayList m_list = new ArrayList();
            for (int ii = 150; ii < bmp.Width - 100; ii++)
            {
                for (int j = 150; j < bmp.Height - 100; j++)
                {
                    Color colort = bmp.GetPixel(ii, j);

                    Color color1 = bmp.GetPixel(ii, j);
                    Color color2 = em.GetPixel(em.Width / 2, em.Height / 2);
                    if (color1.R==color2.R)
                    {
                        ResultInfo data = new ResultInfo();
                        data.x = ii;
                        data.y = j;
                        m_list.Add(data);
                    }
                }
            }
            ResultInfoAscent sa = new ResultInfoAscent();
            for (int ti = 1; ti < 20; ti++)
            {
                m_list = Prefind(ti, m_list);
                if (m_list.Count < 10)
                    break;
            }
            m_list.Sort(sa);
            int i = 0;
            while (i < m_list.Count)
            {
                int j = i + 1;
                while (j < m_list.Count)
                {
                    int ttx = ((ResultInfo)m_list[i]).x - ((ResultInfo)m_list[j]).x;
                    int tty = ((ResultInfo)m_list[i]).y - ((ResultInfo)m_list[j]).y;
                    if (ttx * ttx + tty * tty < em.Width * em.Height)
                    {
                        m_list.RemoveAt(j);
                    }
                    else
                        j++;
                }
                i++;
            }
            return m_list;
        }

        public static ResultInfo FindMVP(Image i1)
        {
            Bitmap bmp = ToGray((Bitmap)i1);
            bmp.Save(@"C:\temp\tmvp.bmp");
            Bitmap bmp1 = (Bitmap)Image.FromFile(@"C:\Users\10947\Desktop\mvp.bmp");
            bmp1 = ToGray(bmp1);
            bmp = (Bitmap)ReduceSize(bmp, bmp.Width / 5, bmp.Height / 5);
            bmp1 = (Bitmap)ReduceSize(bmp1, bmp1.Width / 5, bmp1.Height / 5);
            bmp.Save("C:\\temp\\tPicture1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            bmp1.Save("C:\\temp\\tPicture2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            ResultInfo res = (ResultInfo)pFind(bmp, bmp1);
            res.x = (res.x) * 5;
            res.y = (res.y + bmp1.Height) * 5;
            return res;
        }

        public static void FindP2(Image i1, Image i2)
        {
            Bitmap bmp1 = (Bitmap)ReduceSize(i1, i1.Width / 12, i1.Height / 12);
            Bitmap bmp2 = (Bitmap)ReduceSize(i2, i2.Width / 12, i2.Height / 12);
            bmp1.Save("C:\\temp\\tPicture3.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            ArrayList m_list = new ArrayList();
            for (int ii = 0; ii < bmp1.Width - bmp2.Width; ii++)
            {
                for (int j = 0; j < bmp1.Height - bmp2.Height; j++)
                {
                    Rectangle rect = new Rectangle(ii, j, bmp2.Width, bmp2.Height);
                    Bitmap tbmp = (Bitmap)AcquireRectangleImage(bmp1, rect);
                    Int32 res = CalcSimilarDegree2(tbmp, bmp2);
                    if (res < bmp2.Width * bmp2.Height / 2)
                    {
                        ResultInfo data = new ResultInfo();
                        data.x = ii;
                        data.y = j;
                        data.score = res;
                        m_list.Add(data);
                    }
                }
            }
            ResultInfoAscent sa = new ResultInfoAscent();
            m_list.Sort(sa);
            int i = 0;
            while (i < m_list.Count)
            {
                int j = i + 1;
                while (j < m_list.Count)
                {
                    int tx = ((ResultInfo)m_list[i]).x - ((ResultInfo)m_list[j]).x;
                    int ty = ((ResultInfo)m_list[i]).y - ((ResultInfo)m_list[j]).y;
                    if (tx * tx + ty * ty < bmp2.Width * bmp2.Height)
                    {
                        m_list.RemoveAt(j);
                    }
                    else
                        j++;
                }

                i++;
            }
        }
    }

    public class ResultInfo
    {
        public int x;
        public int score;
        public int y;
        public int star;
        public int type;
    }

    public class ResultInfoAscent : IComparer
    {
        public int Compare(object x, object y)
        {
            if (((ResultInfo)x).type == ((ResultInfo)x).type)
                return ((ResultInfo)y).score.CompareTo(((ResultInfo)x).score);
            else
                return ((ResultInfo)x).type.CompareTo(((ResultInfo)y).type);
        }
    }

}
