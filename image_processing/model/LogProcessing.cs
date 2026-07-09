using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace image_proccessing.model
{
    internal class LogProcessing : Processing
    {
        private byte zmax_;
        private byte zmin_;
        private double zsr_;//средняя яркость
        private double us_;//верхнаяя граница по умолчанию 255 - zsr_
        private double ds_;//нижняя граница по умолчанию 0 - zsr_

        public LogProcessing(BitmapSource src, Image dst, byte zmax, byte zmin,byte us=255,byte ds=0) : base(src, dst)
        {
            zmax_ = zmax;
            zmin_ = zmin;
            zsr_ = ((double)zmin_ + zmax_) / 2.0;
            us_ = (us - zsr_);
            ds_ = (ds - zsr_);
        }
        public override void run() {
            var u = Math.Log(zmax_ - zsr_);//здесь это постоянные значения, можно не считать каждый раз в цикле
            var d = Math.Log(zsr_ - zmin_);
            if (u == 0.0 || d == 0.0) throw new ArgumentException($"Возможное деление на 0 при (zmin,zmax) = ({zmin_}, {zmax_})");

            //обработка
            for (int i = 0; i < pixels_.Length; i += 4)
            {
                //B
                if (pixels_[i] > zsr_) pixels_[i] = (byte)(zsr_ + us_ * (Math.Log(pixels_[i] - zsr_) / u));
                else 
                if (pixels_[i] < zsr_) pixels_[i] = (byte)(zsr_ + ds_ * (Math.Log(zsr_ - pixels_[i]) / d));

                pixels_[i + 1] = pixels_[i];//G
                pixels_[i + 2] = pixels_[i];//R
            }
            dst_.Source = getResult();//вывод результата обработки
        }
    }
}
