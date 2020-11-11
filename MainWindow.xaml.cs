namespace VoiceRecorder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using NAudio.Wave;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using MFCC;
    using System.Drawing;
    using System.IO;

    public class Coord
    { 
        public double X { get; set; }
        public double Y { get; set; }


        public Coord()
        {
            this.X = 0;
            this.Y = 0;
        }
        public Coord(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private WaveIn waveIn;
        private List<double> recordData = new List<double>();
        public PlotModel Model { get; set; }
        public PlotModel MelChart2D { get; set; }
        private LineSeries Line = new LineSeries();
        private System.Windows.Media.GradientStopCollection gsc1 = new System.Windows.Media.GradientStopCollection(3);


        private MFCC mfcc;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsRecord { get; set; }
        public BitmapImage MfccBitmap { get; set; }
        public BitmapImage WordBitmap { get; set; }
        
        private int sliderValue = 0;

        public int SliderMax { get;set; } = 0;
        public int SliderValue
        {
            get
            {
                return this.sliderValue;
            }

            set
            {
                this.sliderValue = value;
                MfccBitmap = GetImage(mel);

            }
        }

        List<double[]> mel = new List<double[]>();
        public ICommand StartRecordCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    IsRecord = true;
                    Line.Points.Clear();

                    recordData.Clear();
                    mel.Clear();
                    Model.InvalidatePlot(true);

                    Model.InvalidatePlot(true);
                   // waveIn.StartRecording();
                });
            }
        }
        public ICommand StopRecordCommand
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    IsRecord = false;
                    mel = mfcc.GetCoeff(recordData, false);
                    int x = 0;
                    foreach (var item in recordData)
                    {
                        Line.Points.Add(new DataPoint(x++,item));
                    }
                    SliderMax = mel.Count();
                    MfccBitmap = GetImage(mel);
                  //  WordBitmap = GetImage(SplitWord(mel));
                    // makeImage(mel);

                });
            }
        }


        private List<double[]> SplitWord(List<double[]> mel)
        {
            int height = mel[0].Length;
            List<double[]> buff = new List<double[]>();
            List<double[]> res = new List<double[]>();
           
            int trash = 5;
            Coord lastCoord = new Coord();
            for (int i = 1; i < mel.Count(); i++)
            {
                //Coord nowCoord = new Coord();
                //double delta = 0;
                bool err = false;
                double power = 0;

                for (int y = 0; y < height; y++)
                {

                        //if (y < (height / 2))
                        //    nowCoord.X += mel[i][y];
                        //else
                        //    nowCoord.Y += mel[i][y];
                        power += Math.Abs(mel[i][y]);

                    if (mel[i][y] > mel[i - 1][y] + trash || mel[i][y] < mel[i - 1][y] - trash)
                    {
                        err = true;
                        double[] red = new double[mel[i].Length];
                        for (int a = 0; a < height; a++)
                        {
                            red[a] = -50;
                        }
                        res.Add(red);
                        break;
                    }

                }

                // double power = nowCoord.X + nowCoord.Y;
                if (!err && power > 10)
                    res.Add(mel[i]);

                //if (power > 10) 
                //{

                //        if (getDist(nowCoord, lastCoord) < 25 || !first)
                //        {
                //            first = true;
                //            res.Add(mel[i]);
                //            lastCoord = nowCoord;
                //        }
                //}
                //else
                //{

                //    double[] red = new double[mel[i].Length];
                //    for (int a = 0; a < red.Length; a++)
                //    {
                //        red[a] = -50;
                //    }
                //    res.Add(red);
                //    lastCoord = new Coord();
                //    first = false;
                //}


            }
            return res;
        }
        private double getDist(Coord a, Coord b) => Math.Abs(Math.Sqrt(Math.Pow(a.X - b.X,2) + Math.Pow(a.Y - b.Y,2)));


        //  | (a — b) / [ (a + b) / 2 ] | * 100 %
        private double Compare(double a, double b)
        {
            double a2 = a;
            double b2 = b;


            if (a > 0 && b < 0 || a < 0 && b > 0) return 100;
            
            if (a < 0 && b < 0) { a2 = Math.Abs(a); b2 = Math.Abs(b); };


            double a3;
            double b3;
            if (a2 > b2) { a3 = a2; b3 = b2; }
            else
            {
                a3 = b2; b3 = a2;
            }
            // if (a > 0 && b < 0 || a < 0 && b > 0) return 100; 

            // double res = Math.Abs((a2 - b2) / ((a2 + b2) / 2)) * 100;
            double res =((a3 - b3) / ((a3 + b3) / 2)) * 100;

            return res;
        }



        private BitmapImage GetImage(List<double[]> mel)
        {
            int height = mel[0].Length;
            int width = mel.Count;
            Bitmap btm = new Bitmap(width, height);
            int x = 0;
            foreach (var item in mel)
            {
                int y = 0;
                foreach (var value in item)
                    {
                       int col = (int)value*20;
                    if (col < 0)
                        {

                            col = Math.Abs(col);
                            if (col > 255) col = 255;
                         btm.SetPixel(x, y, System.Drawing.Color.FromArgb(255, col, 0, 0));

                    }
                        else
                        {
                            if (col > 255) col = 255;
                        btm.SetPixel(x, y, System.Drawing.Color.FromArgb(255, 0, col, 0));
                    }
                        y++;
                    }
                    x++;
               
            }

            
            double[] red = new double[height];
            if (sliderValue >= width) sliderValue = width - 1;
            for (int a = 0; a < red.Length; a++)
            {
                btm.SetPixel(SliderValue, a, System.Drawing.Color.FromArgb(180, 255, 255, 255));
            }
            return BitmapToImage(btm, x);
        }
     

        public MainWindow()
        {
            waveIn = new WaveIn();
            waveIn.DeviceNumber = 0;
            waveIn.BufferMilliseconds = 50;
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.WaveFormat = new WaveFormat(22050, 1);
            waveIn.StartRecording();
            initChar();
           // mfcc = new MFCC(1102, 100, 8000, 22050, 13, WindowFunction.Hamming, 128);
            mfcc = new MFCC(551, 100, 11000, 22050, 23, WindowFunction.Hamming, 220);
            #region Gsc
            gsc1.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Black, 0));
            gsc1.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.DarkCyan, 0.2));
            gsc1.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Blue, 0.35));
            gsc1.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.LightGreen, 0.53));
            gsc1.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Yellow, 0.65));
            gsc1.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Red, 0.9));
            gsc1.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.DarkRed, 2));
            #endregion
            InitializeComponent();
            this.DataContext = this;

        }

        private void InitMelChart2D()
        {
            MelChart2D = new PlotModel();
        }



        private void initChar() { 
             Model = new PlotModel();
            Model.Background = OxyColor.FromRgb(0, 0, 50);
            Model.TextColor = OxyColors.White;

            Line.Color = OxyColor.FromRgb(0, 255, 0);

            Model.Series.Add(Line);

            LinearAxis xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorStep = 5000,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                //AbsoluteMaximum = 1000000,
                //AbsoluteMinimum = 0

            };
        Model.Axes.Add(xAxis);

            LinearAxis YAxis = new LinearAxis()
            {
                IsAxisVisible = true,
                Position = AxisPosition.Left,
                IsPanEnabled = false,
                MaximumPadding = 1,
                Minimum = -1,
                Maximum = 1,
                MajorStep = 5000,
                MajorGridlineStyle = LineStyle.Solid,
            };
            Model.Axes.Add(YAxis);
            Model.InvalidatePlot(true);

        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (IsRecord)
            {
                byte[] buffer = e.Buffer;

                int N = buffer.Length;
                int record = e.BytesRecorded;
                double[] sig = new double[record / 2];
                for (int i = 0, j = 0; i < record; i += 2, j++)
                {
                    short sample = (short)((buffer[i + 1] << 8) | buffer[i + 0]);
                    sig[j] = (sample / 32768f);
                }
                recordData.AddRange(sig);
            }
        }


        public BitmapImage BitmapToImage(object o, int width)
        {
           // Bitmap b = new Bitmap(o as Bitmap, new System.Drawing.Size(150, 150));
            Bitmap b = new Bitmap(o as Bitmap, new System.Drawing.Size(width, (o as Bitmap).Height));

            BitmapImage btm = new BitmapImage();
            using (MemoryStream memStream2 = new MemoryStream())
            {
                (b as Bitmap).Save(memStream2, System.Drawing.Imaging.ImageFormat.Png);
                memStream2.Position = 0;
                btm.BeginInit();
                btm.CacheOption = BitmapCacheOption.OnLoad;
                btm.UriSource = null;
                btm.StreamSource = memStream2;
                btm.EndInit();
            }

            return btm;
        }

        public System.Windows.Media.Color GetRelativeColor(double offset)
        {
            System.Windows.Media.GradientStopCollection gsc = gsc1;
            var point = gsc.SingleOrDefault(f => f.Offset == offset);
            if (point != null) return (System.Windows.Media.Color)System.Windows.Media.Colors.Black;

            System.Windows.Media.GradientStop before = gsc.Where(w => w.Offset == gsc.Min(m => m.Offset)).First();
            System.Windows.Media.GradientStop after = gsc.Where(w => w.Offset == gsc.Max(m => m.Offset)).First();

            foreach (var gs in gsc)
            {
                if (gs.Offset < offset && gs.Offset > before.Offset)
                {
                    before = gs;
                }
                if (gs.Offset > offset && gs.Offset < after.Offset)
                {
                    after = gs;
                }
            }

            var color = new System.Windows.Media.Color();

            color.ScA = (float)((offset - before.Offset) * (after.Color.ScA - before.Color.ScA) / (after.Offset - before.Offset) + before.Color.ScA);
            color.ScR = (float)((offset - before.Offset) * (after.Color.ScR - before.Color.ScR) / (after.Offset - before.Offset) + before.Color.ScR);
            color.ScG = (float)((offset - before.Offset) * (after.Color.ScG - before.Color.ScG) / (after.Offset - before.Offset) + before.Color.ScG);
            color.ScB = (float)((offset - before.Offset) * (after.Color.ScB - before.Color.ScB) / (after.Offset - before.Offset) + before.Color.ScB);

            return color;
        }

    }
}
