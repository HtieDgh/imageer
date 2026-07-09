using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace image_proccessing.model
{
    internal class Pixel
    {

        public int X;//координаты
        public int Y;
        public byte R, G, B, A; //цвет
        public double Brigthness;//яркость

        private PixelFormat fmt_;//формат преобразования
        public Pixel(BitmapSource src, int x, int y)
        {
            if (x < 0 || x > src.PixelWidth || y < 0 || y > src.PixelHeight)
                  throw new ArgumentException($"Параметры X, Y вне диапазона ({x},{y})");

            Processing.chackFormat(src);

            fmt_ = src.Format;

            byte[] pixels = new byte[4];
            src.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);//запись в буфер информации о цвте
            //извлечение из буфера информации о цвете
            X = x;
            Y = y;
            B = pixels[0];
            G = pixels[1];
            R = pixels[2];
            Brigthness = (R + G + B) / 3.0;
        }
        /// <summary>
        /// Возвращает Color из пикселя, для вывода в интерфейсе
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            if (fmt_ == PixelFormats.Bgr32 || fmt_ == PixelFormats.Bgra32) 
                return Color.FromRgb(R, G, B);

            return Color.FromRgb(0,0,0);
        }
    }
}
