using Minokori.Media.Photoshop.Interfaces;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Sections;

public class ImageDataSection(int width, int height, int depth, Channel[] channels) : IPsdLayer
    {
    public BlendMode BlendMode => BlendMode.Normal;

    public IPsdLayer[] Childs => [];

    public bool IsClipping => false;

    public ILinkedLayer? LinkedLayer => null;

    public string Name => "Preview Image";

    public IPsdLayer Parent => Document;

    public JObject Resources => [];

    public required PsdDocument Document { get; init; }

    public int Left => 0;

    public int Top => 0;

    public int Right => width;

    public int Bottom => height;

    public int Width => width;

    public int Height => height;

    public int Depth => depth;

    public Channel[] Channels => channels;

    public float Opacity => 1;

    public bool HasImage => true;

    public bool HasMask => Channels.Length > 4;

    public byte[] MergedImage { get; set; } = [];

    public bool IsVisible { get; set; } = true;
    }
