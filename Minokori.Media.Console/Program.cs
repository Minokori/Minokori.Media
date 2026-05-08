using Minokori.Media.Photoshop;
using Minokori.Media.Photoshop.Extensions;
Console.WriteLine(Path.Exists("./Assets/依神紫苑.psd"));

PsdDocument psd = new("./Assets/依神紫苑.psd");
Console.WriteLine("read complete.");
Console.WriteLine(psd.GetStructureString());

Console.WriteLine(psd.GetCompleteProperties());

var invaidChars = Path.GetInvalidFileNameChars();

foreach (var item in psd.ImageLayers)
    {
    if (item.HasImage)
        {
        var image = item.MergeChannelsToCVImage();
        string name = new(
    [.. item.Name.Where(c => !Path.GetInvalidFileNameChars().Contains(c))]);
        _ = Emgu.CV.CvInvoke.Imwrite($"./Assets/{name}.png", image);

        }
    }

Console.ReadLine();