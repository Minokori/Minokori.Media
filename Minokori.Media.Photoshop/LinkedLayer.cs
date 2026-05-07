using Minokori.Media.Photoshop.Interfaces;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop;

/// <summary>
/// 除了 EmbeddedLayer, 其他的链接图层实现
/// </summary>
/// <param name="name"></param>
/// <param name="id"></param>
/// <param name="documentReader"></param>
/// <param name="fileHeaderReader"></param>
internal class LinkedLayer(JObject info) : ILinkedLayer
    {
    public JObject Properties { get; init; } = info;

    public PsdDocument? Document => null;

    public string Path => "";

    public bool HasDocument => false;

    public Guid ID => Properties.Value<Guid>("UniqueId");

    public string Name => Properties.Value<string>("OriginalFileName") ?? "";

    public int Width => -1;

    public int Height => -1;
    }
