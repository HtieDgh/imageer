using image_proccessing.model;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace image_proccessing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource? defaultBitmapSource_;//Картинка до обработки
        private Dictionary<string,double> cmbPowers_;//словарь для перобразования cmbPower.SelectedValue в double
        private const string histPath_= "\\hist.csv"; //путь для данных пикселей
        private const string octaveScriptPath_= "\\make_z_hist_plot.m"; //путь для скрипта octave
        private bool isPixelSelected_=false;//флаг что пиксель выбран
        private Pixel? pixel_;
        private string? lastOperation_;//Подпись графика в Octave
        public MainWindow()
        {
            InitializeComponent();
            //заполнить combobox элементами
            cmbPowers_= new Dictionary<string, double>
            {
                { "1/3", 1.0 / 3 },
                { "1/5", 1.0 / 5 },
                { "1/7", 1.0 / 7 },
                { "1/9", 1.0 / 9 },
                { "1/11", 1.0 / 11 },
                { "1/13", 1.0 / 13 },
                { "1/15", 1.0 / 15 },
                { "1/17", 1.0 / 17 },
                { "1/19", 1.0 / 19 }
            };
           

        }
        /// <summary>
        /// Клик на кнопку "Загрузить Изображение"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onBtnLoadImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения|*.bmp;";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Прямая загрузка из файла на image
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Важно для освобождения файла
                    bitmap.EndInit();

                    imgDisplay.Source = bitmap;//отобразить картинку

                    defaultBitmapSource_ = bitmap;

                    btnResetImage.Visibility = Visibility.Visible;
                    lastOperation_ = "Loaded Image";
                    pixel_ = null;//сбросить пиксель
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Сгенерировать и отобразить изображение белого шума
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onBtnGenerateImageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //равномерное расплределение пикселей
                var pxs = new byte[250 * 250 * 4];
                int zmin = byte.Parse(txtbGZmin.Text); int zmax = byte.Parse(txtbGZmax.Text);

                var rand = new Random();
                for (int i = 0; i < pxs.Length; i += 4)
                {
                    pxs[i] = pxs[i + 1] = pxs[i + 2] = (byte)rand.Next(zmin,zmax);
                }
                defaultBitmapSource_ = BitmapSource.Create(250, 250, 96, 96, PixelFormats.Bgr32, BitmapPalettes.Gray256, pxs, 250 * 4);
                imgDisplay.Source = defaultBitmapSource_;
                lastOperation_ = $"Generated image (uniform) with zmin = {zmin}, zmax = {zmax}";
                btnResetImage.Visibility = Visibility.Visible;
                pixel_ = null;//сбросить пиксель
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}
        /// <summary>
        /// Клик на кнопку "Сохранить изображение"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onBtnDownloadImageClick(object sender, RoutedEventArgs e)
        {
            if (imgDisplay.Source == null)
            {
                MessageBox.Show("Нет изображения для сохранения!");
                return;
            }
            //допустимы только bmp файлы
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "BMP Image|*.bmp|Все файлы|*.*";
            saveFileDialog.DefaultExt = "bmp";
            saveFileDialog.FileName = "image.bmp";

            if (saveFileDialog.ShowDialog() == true)//после того как путь выбран
            {
                try
                {

                    SaveBitmapSourceToFile(imgDisplay.Source, saveFileDialog.FileName);//сохранить файл
                    MessageBox.Show("Изображение успешно сохранено!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        /// <summary>
        /// Сохранить изображение в файл по заданому пути
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        /// <exception cref="Exception"></exception>
        private void SaveBitmapSourceToFile(ImageSource image, string filePath)
        {
            if(image==null) throw new Exception("Перед сохранением требуется загрузить изображение");
            
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image));

            FileStream stream = new FileStream(filePath, FileMode.Create);
            encoder.Save(stream);
            stream.Close();//закрыть поток после сохранения
        }
        /// <summary>
        /// Получение данных о пикселе изображения в элементе Image, наведя на него мышкой 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onImgDisplayMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var b = imgDisplay.Source as BitmapSource;
                if (b == null || isPixelSelected_) return;//если картинка еще не загружена или пиксель уже выбран, то ничего не делать

                Point clickPoint = e.GetPosition(imgDisplay);// Здесь Point - позиция мышки над изображением в device-independent пиеселях

                // Получить координаты пикселя из Device-independent-пикселей
                int x = (int)(clickPoint.X * b.PixelWidth / imgDisplay.ActualWidth);
                int y = (int)(clickPoint.Y * b.PixelHeight / imgDisplay.ActualHeight);

                if (x == b.PixelWidth) x--;//Иногда WPF высчитывает Point.X Point.Y чуть более чем реальное изображение, что дает пограничное значение при округлении, 
                if (y == b.PixelHeight) y--;//здесь решение - просто вычесть единицу если x и y равен границе изображения

                pixel_ = new Pixel(b, x, y);//получение данных пикселя

                changePixelPreview(pixel_);               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// При клике на картинку не дает менять выбраный цвет
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onImgDisplayClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isPixelSelected_ = !isPixelSelected_;
        }
        /// <summary>
        /// Отобразить текущую информацию о пикселе в UI
        /// </summary>
        /// <param name="p"></param>
        private void changePixelPreview(Pixel p)
        {
            // Вывод информации о пикселе
            txtCoordinates.Text = $"Координаты: X={p.X}, Y={p.Y}";
            txtColor.Text = $"Цвет: R={p.R}, G={p.G}, B={p.B}";
            txtBrightness.Text = $"Яркость (средняя): {p.Brigthness:F2}\n";
            //смена цвета у образца пикселя
            colorPreview.Fill = new SolidColorBrush(p.ToColor());
        }
        /// <summary>
        /// Обработка методом Степенного преобразования со смещенным нулем для n<1 нечетной степени
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onBtnPowerClick(object sender, RoutedEventArgs e)
        {
            try
            { 
                var b = imgDisplay.Source as BitmapSource;
                if (b == null) return;

                if (cmbPower.SelectedValue == null) throw new Exception("Укажите степень для преобразования");

                Algorithm.i().setProccessing(new PowerProcessing(b, imgDisplay, cmbPowers_[(string)cmbPower.SelectedValue]));

                Algorithm.i().run();
                //изменить информацию о пикселе
                if (pixel_ != null)
                {
                    pixel_ = new Pixel(imgDisplay.Source as BitmapSource, pixel_.X, pixel_.Y);
                    changePixelPreview(pixel_);
                }
                lastOperation_ = $"Power transformation with n = {cmbPower.SelectedValue}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Обработка методом Логарифмизации шкалы яркости
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onBtnLogClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var b = imgDisplay.Source as BitmapSource;
                if (b == null) return;
                //если не получится распарсить то бросится исключение
                Algorithm.i().setProccessing(
                    new LogProcessing(b,
                        imgDisplay,
                        byte.Parse(txtbZmax.Text),
                        byte.Parse(txtbZmin.Text)
                        )
                    );

                Algorithm.i().run();

                //изменить информацию о пикселе
                if (pixel_ != null)
                {
                    pixel_ = new Pixel(imgDisplay.Source as BitmapSource, pixel_.X, pixel_.Y);
                    changePixelPreview(pixel_);
                }
                lastOperation_ = $"Logarithmization of the brightness zmax = {txtbZmax.Text}, zmin = {txtbZmin.Text}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Сброс обработки у изображения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetImageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                imgDisplay.Source = defaultBitmapSource_;
            //изменить информацию о пикселе
            if (pixel_ != null)
            {
                pixel_ = new Pixel(imgDisplay.Source as BitmapSource, pixel_.X, pixel_.Y);
                changePixelPreview(pixel_);
            }
            lastOperation_ = "Image after reset";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}
        /// <summary>
        /// Использует GNU\Octave для построения гистограммы 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownloadHistClick(object sender, RoutedEventArgs e)
        {
            var b = imgDisplay.Source as BitmapSource;
            if (b == null) return;
            ///////////////////////////////////////////////////////////////////////////////////////
            //Сохранить файл для octave
            var pixels=Processing.bitmapToPixels(b);//получть массив пикселей текущего изображения

            using (StreamWriter writer = new StreamWriter(histPath_, false, Encoding.ASCII))
            {
                for (int y = 0; y < b.PixelHeight; y++)
                {
                    for (int x = 0; x < b.PixelWidth; x++)
                    {
                        writer.Write(pixels[(y * b.PixelWidth + x) * 4]);
                        writer.Write(',');
                    }
                }
                writer.Write($"\"{lastOperation_}\"");
            }
            ///////////////////////////////////////////////////////////////////////////////////////
            //Вызвать Octave и получить график гистограммы яркостей

            Process.Start("octave-launch.exe", $" --no-gui  {octaveScriptPath_}");// Запуск с параметрами
        }
    }
}