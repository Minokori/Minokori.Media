using Minokori.Media.Photoshop.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Sections;

public partial class LayerAndMaskInformationSection : JObject
    {
    public LayerAndMaskInformationSection(JObject layerInfo, JObject globalLayerMask, JObject additionalLayerInfo)
        {
        this[nameof(LayerInformation)] = layerInfo;
        this[nameof(GlobalLayerMask)] = globalLayerMask;
        this[nameof(AdditionalLayerInformation)] = additionalLayerInfo;
        }

    public JObject LayerInformation => Value<JObject>("LayerInformation") ?? [];
    public JObject GlobalLayerMask => Value<JObject>("GlobalLayerMask") ?? [];
    public JObject AdditionalLayerInformation => Value<JObject>("AdditionalLayerInformation") ?? [];

    [JsonIgnore]
    public PsdLayer[] Layers { get; internal set; } = [];

    [JsonIgnore]
    public ILinkedLayer[] LinkedLayers { get; init; } = [];
    }
