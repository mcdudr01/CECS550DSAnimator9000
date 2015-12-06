using Microsoft.Win32;
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

namespace DSAnimator9000
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {

        }

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

        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_forward_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void txtWindow_Loaded(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
