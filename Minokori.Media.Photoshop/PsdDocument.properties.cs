using Minokori.Media.Photoshop.Extensions;
using Minokori.Media.Photoshop.Interfaces;
using Minokori.Media.Photoshop.Sections;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;
namespace Minokori.Media.Photoshop;

public partial class PsdDocument
    {
    // Stream Reader
    //internal PsdBinaryReader BinaryReader { get; init; }

    // sections
    public FileHeaderSection FileHeader { get; private set; }

    public JObject ColorModeDataSection { get; private set; }

    public LayerAndMaskInformationSection LayerAndMaskSection { get; private set; }
    internal ImageDataSection ImageDataSection { get; private set; }

    public byte[] ColorModeData => (byte[])ColorModeDataSection.Value<JArray>("ColorMode")!.Values<byte>();

    public int Width => FileHeader.Width;

    public int Height => FileHeader.Height;

    public int Depth => FileHeader.Depth;

    public IPsdLayer[] Childs => LayerAndMaskSection.Layers; //LayerAndMaskSection.GetValue.Layers;

    public bool IsVisible { get; set; } = true;

    public IEnumerable<ILinkedLayer> LinkedLayers => LayerAndMaskSection.LinkedLayers; //LayerAndMaskSection.GetValue.LinkedLayers;

    public JObject Resources => LayerAndMaskSection.AdditionalLayerInformation; //LayerAndMaskSection.GetValue.AdditionalLayerInfomation;

    public JObject ImageResourcesSection { get; private set; }

    public bool HasImage =>
        ImageResourcesSection.Contains("Version") != false
        && ImageResourcesSection.GetValue<bool>(["Version.HasRealMergedData"]);

    #region IPsdLayer

    IPsdLayer? IPsdLayer.Parent => null;

    bool IPsdLayer.IsClipping => false;

    PsdDocument IPsdLayer.Document => this;

    ILinkedLayer? IPsdLayer.LinkedLayer => null;

    string IPsdLayer.Name => "Document";

    int IPsdLayer.Left => 0;

    int IPsdLayer.Top => 0;

    int IPsdLayer.Right => Width;

    int IPsdLayer.Bottom => Height;

    BlendMode IPsdLayer.BlendMode => BlendMode.Normal;

    Channel[] IPsdLayer.Channels => ImageDataSection.Channels;

    // TODO This makes MergeChannels on PsdDocument class no opacity
    float IPsdLayer.Opacity => 1.0f;

    bool IPsdLayer.HasMask => FileHeader.NumberOfChannels > 4;

    public byte[] MergedImage { get; set; } = [];
    #endregion
    }
