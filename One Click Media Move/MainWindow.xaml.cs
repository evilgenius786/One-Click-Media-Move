using System;
using System.Collections.Generic;
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

namespace One_Click_Media_Move
{
    class action
    {
        public string source;
        public string destination;
        public action(string s,string d)
        {
            source = s;
            destination = d;
        }
    };
    public partial class MainWindow : Window
    {
        List<string> files;
        List<string> images;
        List<string> videos;
        int count = 0;
        List<action> actions;
        public MainWindow()
        {
            InitializeComponent();
            images = new List<string>();
            videos = new List<string>();
            actions = new List<action>();
            images.Add(".jpg");
            images.Add(".png");
            images.Add(".bmp");
            images.Add(".tif");
            images.Add(".gif");
            videos.Add(".mp4");
            videos.Add(".avi");
            videos.Add(".mpg");
            videos.Add(".3gp");
            loadCollection(new object(),new RoutedEventArgs());
        }
        private void loadCollection(object sender, RoutedEventArgs e)
        {
            try{
                string[] lines = File.ReadAllLines("buttonsCollection.csv");
                gridMain.Children.Clear();
                foreach (var line in lines)
                {
                    Button button = new Button();
                    string[] temp = line.Split(',')[1].Split('/');
                    int x=temp.Length;
                    if(x > 3)button.Content = line.Split(',')[0] + '\n' + "../" +temp[x - 3] + "/" + temp[x - 2] + "/" + temp[x - 1];
                    else button.Content = line.Split(',')[0] + '\n' + line.Split(',')[1];
                    button.HorizontalContentAlignment = HorizontalAlignment.Center;
                    button.HorizontalAlignment = HorizontalAlignment.Center;
                    button.Width = 150;
                    button.Height = 50;
                    button.Click += move;
                    button.ToolTip = line.Split(',')[1];
                    button.Padding = new Thickness(6.0);
                    gridMain.Children.Add(button);
                }
            }
            catch(IOException){
                MessageBox.Show("Error in opening buttons collection csv");
            }
        }
        private void setOrigin(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = "Select a Directory";
            dialog.Filter = "Directory|*.this.directory";
            dialog.FileName = "select";
            string path="";
            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                
            }
            files = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList();
            Update();
        }
        void setImage()
        {
            button.Visibility = Visibility.Hidden;
            video.Visibility = Visibility.Hidden;
            image.Visibility = Visibility.Visible;

            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri(files[count]);
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.EndInit();
            image.Source = bmi;
        }
        void setVideo()
        {
            button.Visibility = Visibility.Hidden;
            video.Visibility = Visibility.Visible;
            image.Visibility = Visibility.Hidden;
            video.Source = (new Uri(@files[count]));
        }
        void setButton()
        {
            button.Visibility = Visibility.Visible;
            video.Visibility = Visibility.Hidden;
            image.Visibility = Visibility.Hidden;
        }
        void Update()
        {
            if (count < files.Count)
            {
                if (video.Source != null) video.Source = null;
                filename.Text = Path.GetFileName(files.ElementAt(count));
                origin.Text = Path.GetDirectoryName(files.ElementAt(count));
                if (images.Contains(Path.GetExtension(files[count].ToLower())))
                    setImage();
                else if (videos.Contains(Path.GetExtension(files[count]).ToLower()))
                    setVideo();
                else
                    setButton();
            }
            else MessageBox.Show("Last file reached!","Error");
        }
        private void skip(object sender, RoutedEventArgs e)
        {
            if (files != null)
            {
                actions.Add(new action(files.ElementAt(count), ""));
                count++;
                Update();
            }
            else MessageBox.Show("Please select origin first", "Error");
        }
        private void undo(object sender, RoutedEventArgs e)
        {
            if (actions.Count != 0 && count > 0)
            {
                if (actions[count-1].destination != "")
                {
                    if (move(actions[count - 1].destination, actions[count - 1].source))
                        actions.RemoveAt(actions.Count - 1);
                    else MessageBox.Show("Error in undo");
                }
                count--;
                Update();
            }
            else MessageBox.Show("No action to undo", "Error");
        }
        Boolean move(string source,string dest)
        {
            if (File.Exists(dest))
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(dest + " already exists, Do you want to overwrite it?", "Destination already exists.", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes){
                    File.Delete(dest);
                    if(move(source, dest))
                        return true;
                }
                else return false;
            }
            if (!Directory.Exists(Path.GetDirectoryName(dest)))
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(Path.GetDirectoryName(dest) + " doesnt exist, Do you want to create it?", "Destination directory doesnt exists.", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    if (move(source, dest))
                        return true;
                }
                else return false;
            }
            try
            {
                File.Move(source, dest);
                return true;
            }
            catch (IOException e)
            {
                MessageBox.Show(e.ToString(),"Src: "+source+"Dest: "+dest);
                return false;
            }
        }
        private void move(object sender, RoutedEventArgs e)
        {
            if (files != null)
            {
                Button btn = sender as Button;
                if (move(files.ElementAt(count), btn.ToolTip.ToString() + "//" + Path.GetFileName(files.ElementAt(count))))
                {
                    actions.Add(new action(files.ElementAt(count), btn.ToolTip.ToString() + "//" + Path.GetFileName(files.ElementAt(count))));
                    skip(sender,e);
                }
                else MessageBox.Show("Error in moving file!");
            }
            else MessageBox.Show("Please select an origin folder", "Error!");
        }

        private void openFile(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@files[count]);
        }
    }
}
