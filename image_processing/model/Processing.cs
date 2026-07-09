using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace image_proccessing.model
{
    abstract class Processing
    {
        public byte[] pixels_;
        protected BitmapSource src_;//источник пикселей
        protected Image dst_;       //куда поместить результат обработки

        protected Processing(BitmapSource src, Image dst) {
            src_ = src;
            dst_ = dst;
            pixels_ = bitmapToPixels(src);
        }
        public abstract void run();
        /// <summary>
        /// Получить обработаное изображение
        /// </summary>
        /// <returns></returns>
        public BitmapSource getResult()
        {
            return BitmapSource.Create(
                src_.PixelWidth, src_.PixelHeight,
                src_.DpiX, src_.DpiY,
                src_.Format, src_.Palette,
                pixels_, src_.PixelWidth * 4);
        }
        /// <summary>
        /// Перевести bitmap в массив пикселей
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte[] bitmapToPixels(BitmapSource src)
        {
            chackFormat(src);
            var buffer = new byte[src.PixelHeight * src.PixelWidth * 4];
            src.CopyPixels(buffer, src.PixelWidth * 4, 0);
            return buffer;
        }
        /// <summary>
        /// Обрабатывать только BGR32 - если формат другой то извлекать цвета надо по другому
        /// </summary>
        /// <param name="src"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void chackFormat(BitmapSource src)
        {
            if (src.Format != PixelFormats.Bgr32 && src.Format != PixelFormats.Bgra32)
                throw new ArgumentException($"Формат изображения не поддерживается({src.Format.ToString()})");
        }
    }
}
