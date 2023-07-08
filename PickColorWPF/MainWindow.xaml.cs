using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using System.Threading.Tasks;

namespace PickColorWPF
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        static extern int BitBlt(nint hDC, int x, int y, int nWidth, int nHeight, nint hSrcDC, int xSrc, int ySrc, int dwRop);

        Bitmap screenPixel = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int VirtualKeyPressed);

        bool loop = true;
        int cont = 0;

        public MainWindow()
        {
            InitializeComponent();
            Topmost = true;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            var rgb = $"rgb({R_Label.Content},{G_Label.Content},{B_Label.Content})";
            Clipboard.SetText(rgb);
        }

        private void CopyBtn_Hex_Click(object sender, RoutedEventArgs e)
        {
            var hex = $"#{Hex_Label.Content}";
            Clipboard.SetText(hex);
        }

        private async void PickColorBtn_Click(object sender, RoutedEventArgs e)
        {
            await GetPixel();
        }

        internal async Task GetPixel()
        {
            while (loop)
            {
                UserClicked();

                if (cont > 1)
                    loop = false;

                System.Drawing.Point cursor = new System.Drawing.Point();
                GetCursorPos(ref cursor);

                var c = GetColorAt(cursor);
                var hex = c.Name.Remove(0,2);

                ActualizarLabels(c.A, c.R, c.G, c.B, hex);
                await Task.Delay(30);
            }
            cont = 0;
            loop = true;
        }

        System.Drawing.Color GetColorAt(System.Drawing.Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(nint.Zero))
                {
                    nint hSrcDC = gsrc.GetHdc();
                    nint hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        private void ActualizarLabels(byte a, byte r, byte g, byte b, string hex)
        {
            R_Label.Dispatcher.Invoke(() => R_Label.Content = r);
            G_Label.Dispatcher.Invoke(() => G_Label.Content = g);
            B_Label.Dispatcher.Invoke(() => B_Label.Content = b);

            Hex_Label.Dispatcher.Invoke(() => Hex_Label.Content = hex);

            ShowColor.Dispatcher.Invoke(() => ShowColor.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(a,r,g,b)));
        }

        private bool UserClicked()
        {
            if (GetAsyncKeyState(0x01) == 0)
                return false;

            cont++;
            return true;
        }
    }
}
