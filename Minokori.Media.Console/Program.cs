
using Minokori.Media.Photoshop;
using Minokori.Media.Photoshop.Extensions;
Console.WriteLine(Path.Exists("./Assets/依神紫苑.psd"));

PsdDocument psd = new("./Assets/依神紫苑.psd");
Console.WriteLine("read complete.");
Console.WriteLine(psd.GetStructureString());

Console.WriteLine(psd.GetCompleteProperties());