using System;
using System.Threading;
using CaptureProxy;
using Win32Proxy;

namespace SimpleWindowCapture
{
    internal sealed class CaptureHelper
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern int WindowFromPoint(int xPoint, int yPoint);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool IsIconic(IntPtr hWnd);

        public event Action<string, IntPtr, Win32Types.BitmapInfo,bool> CaptureDone =
            (captureName, bitmapPtr, bitmapInfo,b) => { };

        public int Fps { get; set; } = 15;

        private double TimerInterval => 1000.0 / Fps;
        private string _captureName;
        private Timer _timer;
        IntPtr hptr;

        public bool Start(string captureName, IntPtr handle)
        {
            hptr = handle;
            if (!CaptureService.Instance.RegisterCapture(captureName, handle))
            {
                return false;
            }

            _captureName = captureName;

            //创建守护定时器，马上执行
            _timer = new Timer(CaptureFunc, null,
                TimeSpan.FromMilliseconds(0), Timeout.InfiniteTimeSpan);

            return true;
        }

        public void Stop()
        {
            //移除定时器
            _timer?.Dispose();
            _timer = null;

            CaptureService.Instance.UnRegisterCapture(_captureName);
            _captureName = string.Empty;
        }

        private void CaptureFunc(object state)
        {
            Capture();

            //执行下次定时器
            _timer?.Change(TimeSpan.FromMilliseconds(TimerInterval), Timeout.InfiniteTimeSpan);
        }

        private void Capture()
        {
            IntPtr bitsPtr;
            var bitmapPtr = CaptureService.Instance.GetBitmapPtr(_captureName);
            var bitmapInfo = CaptureService.Instance.GetBitmapInfo(_captureName);
            if (!CaptureService.Instance.Capture(_captureName, out bitsPtr))
            {
                CaptureDone.Invoke(_captureName, bitmapPtr, bitmapInfo, false);
                return;
            }
            if(IsIconic(hptr))
            {
                CaptureDone.Invoke(_captureName, bitmapPtr, bitmapInfo, false);
                return;
            }
            CaptureDone.Invoke(_captureName, bitmapPtr, bitmapInfo, true);
        }
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public void MouseMove(int x, int y, int sx, int sy)
        {
            Win32Types.Rect rect;
            Win32Funcs.GetWindowRectWrapper(hptr, out rect);
            int tx = x + rect.Left;

            int ty = y + rect.Top;
          
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, tx * 65535 / sx, ty * 65535 / sy, 0, 0);
        }

        public void MouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 1, 0);
        }

        public void MoveClickBack(int x, int y, int yx, int yy, int sx, int sy)
        {
            IntPtr hWndFromPoint = (IntPtr)WindowFromPoint(x, y);
            SetForegroundWindow(hptr);
            x = x - x / 5;
            y = y - y / 5;
            MouseMove(x, y, sx, sy);
            Thread.Sleep(25);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 1, 0);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, yx * 65535 / sx, yy * 65535 / sy, 0, 0);
            Thread.Sleep(25);
            SetForegroundWindow(hWndFromPoint);
            //Console.WriteLine(hWndFromPoint.ToString());
        }
    }
}
