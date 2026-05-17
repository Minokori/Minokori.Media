using Avalonia.Controls;
using Minokori.Media.Photoshop;
using Minokori.Media.Photoshop.Extensions;
namespace Minokori.Media.Demo.Avalonia;

public partial class MainWindow : Window
    {
    public MainWindow()
        {
        InitializeComponent();
        PsdDocument psd = new PsdDocument("D:\\Lenovo\\Documents\\Repositories\\Minokori.Media\\Minokori.Media.Console\\Assets\\依神紫苑.psd");

        image.Source = psd.ToBitmap();
        }
    }