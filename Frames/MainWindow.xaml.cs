﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Configuration;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace DSAnimator9000
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timerImageChange;
        private Image[] ImageControls;
        private List<ImageSource> Images = new List<ImageSource>();
        private static string[] ValidImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        private static string[] TransitionEffects = new[] { "Fade" };
        private string TransitionType, strImagePath = "";
        private int CurrentSourceIndex, CurrentCtrlIndex, EffectIndex = 0, IntervalTimer = 2;

        public MainWindow()
        {
            InitializeComponent();

            //Initialize Image control, Image directory path and Image timer.
            //IntervalTimer = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalTime"]);
            IntervalTimer = 2;
            //strImagePath = ConfigurationManager.AppSettings["ImagePath"];
            strImagePath = Directory.GetCurrentDirectory() + "\\Output\\png\\";
            ImageControls = new[] { myImage, myImage2 };
            
            LoadImageFolder(strImagePath);

            timerImageChange = new DispatcherTimer();
            timerImageChange.Interval = new TimeSpan(0, 0, IntervalTimer);
            timerImageChange.Tick += new EventHandler(timerImageChange_Tick);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlaySlideShow();
            timerImageChange.IsEnabled = true;
        }

        private void Menu_New_Click(object sender, RoutedEventArgs e)
        {
            txt_code.Text = "";
        }

        private void Menu_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"c:\temp\";
            openFileDialog.Filter = "CSharp files (*.cs)|*.cs|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                txt_code.Text = File.ReadAllText(openFileDialog.FileName);
        }

        private void Menu_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = @"c:\temp\";
            saveFileDialog.Filter = "CSharp file (*.cs)|*.cs|Text file (*.txt)|*.txt|C# file (*.cs)|*.cs";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, txt_code.Text);
        }

        //private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        //{

        //}

        private void Menu_Animate_Click(object sender, RoutedEventArgs e)
        {
            //Logic to run poor_man_lexer and pass code present in the txt_code textbox
            //string path = "C:\\Users\\Dooder\\Documents\\GitHub\\DSAnimator9000\\lexer\\poor_man_lexer\\bin\\Debug\\";
            string path = Directory.GetCurrentDirectory() + "\\..\\..\\lexer\\poor_man_lexer\\bin\\Debug\\";

            Process p = new Process();
            p.StartInfo.FileName = path + "poor_man_lexer.exe";
            p.StartInfo.Arguments = path + "Input\\List.cs";
            //p.StartInfo.Arguments = txt_code.Text;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            //string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            timerImageChange.IsEnabled = false;
            try
            {
                if (Images.Count == 0)
                    return;
                var oldCtrlIndex = CurrentCtrlIndex;
                CurrentCtrlIndex = (CurrentCtrlIndex + 1) % 2;
                if (CurrentSourceIndex == 0)
                    CurrentSourceIndex = 0;
                else
                    CurrentSourceIndex = (CurrentSourceIndex - 1) % Images.Count;

                Image imgFadeOut = ImageControls[oldCtrlIndex];
                Image imgFadeIn = ImageControls[CurrentCtrlIndex];
                ImageSource newSource = Images[CurrentSourceIndex];
                imgFadeIn.Source = newSource;

                TransitionType = TransitionEffects[EffectIndex].ToString();

                Storyboard StboardFadeOut = (Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();
                StboardFadeOut.Begin(imgFadeOut);
                Storyboard StboardFadeIn = Resources[string.Format("{0}In", TransitionType.ToString())] as Storyboard;
                StboardFadeIn.Begin(imgFadeIn);
            }
            catch (Exception ex) { }
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            timerImageChange.IsEnabled = false;
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            IntervalTimer = 0;
            timerImageChange.IsEnabled = true;
            IntervalTimer = 2;
        }

        private void btn_forward_Click(object sender, RoutedEventArgs e)
        {
            timerImageChange.IsEnabled = false;
            try
            {
                if (Images.Count == 0)
                    return;
                var oldCtrlIndex = CurrentCtrlIndex;
                CurrentCtrlIndex = (CurrentCtrlIndex + 1) % 2;
                CurrentSourceIndex = (CurrentSourceIndex + 1) % Images.Count;

                Image imgFadeOut = ImageControls[oldCtrlIndex];
                Image imgFadeIn = ImageControls[CurrentCtrlIndex];
                ImageSource newSource = Images[CurrentSourceIndex];
                imgFadeIn.Source = newSource;

                TransitionType = TransitionEffects[EffectIndex].ToString();

                Storyboard StboardFadeOut = (Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();
                StboardFadeOut.Begin(imgFadeOut);
                Storyboard StboardFadeIn = Resources[string.Format("{0}In", TransitionType.ToString())] as Storyboard;
                StboardFadeIn.Begin(imgFadeIn);
            }
            catch (Exception ex) { }
        }

        private void LoadImageFolder(string folder)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            // Can change directory here
           // if (!System.IO.Path.IsPathRooted(folder))
            folder = Directory.GetCurrentDirectory() + "\\Output\\png\\";
            if (!System.IO.Directory.Exists(folder))
            {
                ErrorText.Text = "The specified folder does not exist: " + Environment.NewLine + folder;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            Random r = new Random();
            var sources = from file in new System.IO.DirectoryInfo(folder).GetFiles().OrderBy(p => p.Name) // .AsParallel()
                          where ValidImageExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                          //orderby r.Next()
                          select CreateImageSource(file.FullName, true);
            Images.Clear();
            Images.AddRange(sources);
            sw.Stop();
            Console.WriteLine("Total time to load {0} images: {1}ms", Images.Count, sw.ElapsedMilliseconds);
        }

        private ImageSource CreateImageSource(string file, bool forcePreLoad)
        {
            if (forcePreLoad)
            {
                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(file, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze();
                return src;
            }
            else
            {
                var src = new BitmapImage(new Uri(file, UriKind.Absolute));
                src.Freeze();
                return src;
            }
        }

        private void timerImageChange_Tick(object sender, EventArgs e)
        {
            PlaySlideShow();
        }

        private void PlaySlideShow()
        {
            try
            {
                if (Images.Count == 0)
                    return;
                var oldCtrlIndex = CurrentCtrlIndex;
                CurrentCtrlIndex = (CurrentCtrlIndex + 1) % 2;

                ImageSource newSource = Images[CurrentSourceIndex];
                CurrentSourceIndex = (CurrentSourceIndex + 1) % Images.Count;

                Image imgFadeOut = ImageControls[oldCtrlIndex];
                Image imgFadeIn = ImageControls[CurrentCtrlIndex];

                imgFadeIn.Source = newSource;
                TransitionType = TransitionEffects[EffectIndex].ToString();

                Storyboard StboardFadeOut = (Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();
                StboardFadeOut.Begin(imgFadeOut);
                Storyboard StboardFadeIn = Resources[string.Format("{0}In", TransitionType.ToString())] as Storyboard;
                StboardFadeIn.Begin(imgFadeIn);
            }
            catch (Exception ex) { }
        }
    }
}
