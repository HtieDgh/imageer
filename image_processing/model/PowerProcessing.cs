using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace image_proccessing.model
{
    internal class PowerProcessing:Processing
    {

        private byte zmax_;
        private double pow_;

        /// <summary>
        /// Степеное преобразование по формуле
        /// z' = 0,5zm(1 + (2zom – 1)^n)
        /// Обрабатывает только черно белое изображение
        /// </summary>
        /// <param name="src">источник пикселей</param>
        /// <param name="dst">назначение результата</param>
        public PowerProcessing(BitmapSource src, Image dst,double pow) : base( src,  dst)
        {
            pow_ = pow;          
            //поиск наивысшей яркости (только черно белый формат изображения)
            zmax_ = 0;
            foreach (var z in pixels_)
            {
                zmax_ = z > zmax_ ? z : zmax_;
            }
        }
        public override void run()
        {
            double zot; //относительная яркость
            double halfzmax = zmax_ / 2;
            //обработка
            for (int i = 0; i < pixels_.Length; i+=4)
            {
                zot = (double)pixels_[i] / zmax_;//вычисление относительной яркости
                //B
                if(pixels_[i] > halfzmax) pixels_[i] = (byte)(halfzmax * (1.0 + Math.Pow((2.0 * zot - 1.0), pow_)));
                else                      pixels_[i] = (byte)(halfzmax * (1.0 - Math.Pow((1.0 - 2.0 * zot), pow_)));

                pixels_[i+1] = pixels_[i];//G
                pixels_[i+2] = pixels_[i];//R
            }
            dst_.Source = getResult();//вывод результата обработки
        }
    }
}
