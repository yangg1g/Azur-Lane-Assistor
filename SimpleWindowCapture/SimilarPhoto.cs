using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;


namespace SimpleWindowCapture
{
    class SimilarPhoto
    {
        public static bool Zoom(Bitmap srcBmp, double ratioW, double ratioH, out Bitmap dstBmp)
        {
            int height = Convert.ToInt16(ratioH * (double)srcBmp.Height);
            int width = Convert.ToInt16(ratioW * (double)srcBmp.Width);
            dstBmp = new Bitmap((int)width, (int)height);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int R = 0; int G = 0; int B = 0; int c = 0;
                    for (int ii = 0; ii < 1.0 / ratioH; ii++)
                    {
                        int x = Convert.ToInt16(i / ratioH + ii);
                        if (x >= srcBmp.Height)
                            break;

                        for (int jj = 0; jj < 1.0 / ratioW; jj++)
                        {
                            int y = Convert.ToInt16(j / ratioW + jj);
                            if (y >= srcBmp.Width)
                                break;
                            R += srcBmp.GetPixel(y, x).R;
                            G += srcBmp.GetPixel(y, x).G;
                            B += srcBmp.GetPixel(y, x).B;
                            c++;
                        }
                    }
                    dstBmp.SetPixel(j, i, Color.FromArgb(R / c, G / c, B / c));
                }
            return true;
        }
        public static void BlackWhite2(Bitmap b)
        {
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color c = b.GetPixel(i, j);
                    int t = (c.R + c.G + c.B) / 3;
                    b.SetPixel(i, j, Color.FromArgb(t, t, t));
                }
            }
        }
        public static ArrayList FindP(Image i1)
        {
            Bitmap bmp = (Bitmap)i1.Clone();
            bmp.Save(@"C:\temp\Picture1.bmp");
            Bitmap bmp1 = (Bitmap)Image.FromFile(@"C:\temp\qj.bmp");
            Bitmap bmp2 = (Bitmap)Image.FromFile(@"C:\temp\zj.bmp");
            Bitmap bmp3 = (Bitmap)Image.FromFile(@"C:\temp\hm.bmp");
            //Bitmap bmp4 = (Bitmap)Image.FromFile(@"D:\Downloads\Compressed\SimpleWindowCapture-master\res\ysc.bmp");
            Bitmap bmp5 = (Bitmap)Image.FromFile(@"C:\temp\boss.bmp");
            Zoom(bmp, 0.1, 0.1, out bmp);
            Zoom(bmp1, 0.1, 0.1, out bmp1);
            Zoom(bmp2, 0.1, 0.1, out bmp2);
            Zoom(bmp3, 0.1, 0.1, out bmp3);
            //Zoom(bmp4, 0.1, 0.1, out bmp4);
            Zoom(bmp5, 0.1, 0.1, out bmp5);
            ArrayList m_list1 = FindEm2(1, bmp, bmp1);
            ArrayList m_list2 = FindEm2(2, bmp, bmp2);
            ArrayList m_list3 = FindEm2(3, bmp, bmp3);
            //ArrayList m_list4 = FindEm2(4, bmp, bmp4);
            ArrayList m_list5 = FindEm2(5, bmp, bmp5);
            ArrayList m_list = m_list1;
            for (int ti = 0; ti < m_list2.Count; ti++)
                m_list.Add(m_list2[ti]);
            for (int ti = 0; ti < m_list3.Count; ti++)
                m_list.Add(m_list3[ti]);
            //for (int ti = 0; ti < m_list4.Count; ti++)
              //  m_list.Add(m_list4[ti]);
            for (int ti = 0; ti < m_list5.Count; ti++)
                m_list.Add(m_list5[ti]);
            ResultInfoAscent sa = new ResultInfoAscent();
            m_list.Sort(sa);
            return m_list;
        }
        public static ArrayList FindEm2(int type,Bitmap re1,Bitmap re2)
        {
            Bitmap res1 = (Bitmap)re1.Clone();
            Bitmap ress1 = (Bitmap)res1.Clone();
            Bitmap res2 = (Bitmap)re2.Clone();
            Bitmap ress2 = (Bitmap)res2.Clone();
            BlackWhite2(res1);
            BlackWhite2(res2);
            int recw = res2.Width;
            int rech = res2.Height;
            int[,] map = new int[res1.Width, res1.Height];
            int[,] map2 = new int[res1.Width, res1.Height];
            map[0, 0] = res1.GetPixel(0, 0).R;
            map2[0, 0] = res1.GetPixel(0, 0).R * res1.GetPixel(0, 0).R;
            int org = 0;
            int org2 = 0;
            for (int ii = 0; ii < res2.Width; ii++)
                for (int jj = 0; jj < res2.Height; jj++)
                {
                    org += res2.GetPixel(ii, jj).R;
                    org2 += res2.GetPixel(ii, jj).R * res2.GetPixel(ii, jj).R;
                }
            org = org / (res2.Width * res2.Height);
            org2 = org2 / (res2.Width * res2.Height);
            org2 = org2 - org * org;
            int i = 1;
            int j = 1;
            while (i < res1.Width && j < res1.Height)
            {
                map[0, j] = map[0, j - 1] + res1.GetPixel(0, j).R;
                map2[0, j] = map2[0, j - 1] + map2[0, j] + res1.GetPixel(0, j).R * res1.GetPixel(0, j).R;
                for (int ii = 1; ii < i; ii++)
                {
                    map[ii, j] = map[ii, j - 1] + map[ii - 1, j] - map[ii - 1, j - 1] + res1.GetPixel(ii, j).R;
                    map2[ii, j] = map2[ii, j - 1] + map2[ii - 1, j] - map2[ii - 1, j - 1] + res1.GetPixel(ii, j).R * res1.GetPixel(ii, j).R;
                }
                map[i, 0] = map[i - 1, 0] + res1.GetPixel(i, 0).R;
                map2[i, 0] = map2[i - 1, 0] + res1.GetPixel(i, 0).R * res1.GetPixel(i, 0).R;
                for (int jj = 1; jj < j; jj++)
                {
                    map[i, jj] = map[i, jj - 1] + map[i - 1, jj] - map[i - 1, jj - 1] + res1.GetPixel(i, jj).R;
                    map2[i, jj] = map2[i, jj - 1] + map2[i - 1, jj] - map2[i - 1, jj - 1] + res1.GetPixel(i, jj).R * res1.GetPixel(i, jj).R;
                }
                map[i, j] = map[i, j - 1] + map[i - 1, j] - map[i - 1, j - 1] + res1.GetPixel(i, j).R;
                map2[i, j] = map2[i, j - 1] + map2[i - 1, j] - map2[i - 1, j - 1] + res1.GetPixel(i, j).R * res1.GetPixel(i, j).R;
                i++;
                j++;
            }
            i--;
            j--;
            while (i < res1.Width && j == res1.Height - 1)
            {
                map[i, 0] = map[i - 1, 0] + res1.GetPixel(i, 0).R;
                map2[i, 0] = map2[i - 1, 0] + res1.GetPixel(i, 0).R * res1.GetPixel(i, 0).R;
                for (int jj = 1; jj <= j; jj++)
                {
                    map[i, jj] = map[i, jj - 1] + map[i - 1, jj] - map[i - 1, jj - 1] + res1.GetPixel(i, jj).R;
                    map2[i, jj] = map2[i, jj - 1] + map2[i - 1, jj] - map2[i - 1, jj - 1] + res1.GetPixel(i, jj).R * res1.GetPixel(i, jj).R;
                }
                i++;
            }
            while (j < res1.Height && i == res1.Width - 1)
            {
                map[0, j] = map[0, j - 1] + res1.GetPixel(0, j).R;
                map2[0, j] = map2[0, j - 1] + res1.GetPixel(0, j).R * res1.GetPixel(0, j).R;
                for (int ii = 1; ii <= i; ii++)
                {
                    map[ii, j] = map[ii, j - 1] + map[ii - 1, j] - map[ii - 1, j - 1] + res1.GetPixel(ii, j).R;
                    map2[ii, j] = map2[ii, j - 1] + map2[ii - 1, j] - map2[ii - 1, j - 1] + res1.GetPixel(ii, j).R * res1.GetPixel(ii, j).R;
                }
                j++;
            }
            ArrayList m_list = new ArrayList();
            for (i = res2.Width; i < res1.Width; i++)
            {
                for (j = res2.Height; j < res1.Height; j++)
                {
                    int t = map[i, j] - map[i - res2.Width, j] - map[i, j - res2.Height] + map[i - res2.Width, j - res2.Height];
                    int t2 = map2[i, j] - map2[i - res2.Width, j] - map2[i, j - res2.Height] + map2[i - res2.Width, j - res2.Height];
                    t = t / (res2.Width * res2.Height);
                    t2 = t2 / (res2.Width * res2.Height);
                    t2 = t2 - t * t;
                    if (i == 69 && j == 58)
                        i = i;
                    if (Math.Abs(org - t) < 20 && Math.Abs(org2 - t2) < 1000)
                    {
                        Color c = res1.GetPixel(i, j);
                        int res = 0;

                        for (int ii = i - res2.Width; ii < i; ii++)
                        {
                            for (int jj = j - res2.Height; jj < j; jj++)
                            {
                                int R = Math.Abs(ress1.GetPixel(ii, jj).R - ress2.GetPixel(ii - i + ress2.Width, jj - j + ress2.Height).R);
                                int G = Math.Abs(ress1.GetPixel(ii, jj).G - ress2.GetPixel(ii - i + ress2.Width, jj - j + ress2.Height).G);
                                int B = Math.Abs(ress1.GetPixel(ii, jj).B - ress2.GetPixel(ii - i + ress2.Width, jj - j + ress2.Height).B);
                                int lt = Math.Max(R, Math.Max(G, B));
                                lt = lt / 10;
                                lt = lt * lt * lt;
                                lt = lt / 10;
                                res += lt * lt;
                            }
                        }
                        res = res / (res2.Width * res2.Height);
                        if (res < 2000)
                        {
                            ResultInfo data = new ResultInfo();
                            data.x = i - res2.Width / 2;
                            data.y = j - res2.Height / 2;
                            data.type = type;
                            data.score = res;
                            m_list.Add(data);
                        }

                    }
                }
            }
            ResultInfoAscent sa = new ResultInfoAscent();
            m_list.Sort(sa);
            i = 0;
            while (i < m_list.Count)
            {
                j = i + 1;
                while (j < m_list.Count)
                {
                    int ttx = ((ResultInfo)m_list[i]).x - ((ResultInfo)m_list[j]).x;
                    int tty = ((ResultInfo)m_list[i]).y - ((ResultInfo)m_list[j]).y;
                    if (ttx * ttx + tty * tty < res2.Width * res2.Height)
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
