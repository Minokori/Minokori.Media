using Minokori.Media.Photoshop.Extensions;
using Minokori.Media.Photoshop.Interfaces;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop;

public sealed partial class PsdLayer : IPsdLayer
    {
    public int Depth => Document.Depth;
    public SectionType SectionType
        {
        get
            {
            var type = Records.GetValue<string>(["Resources.SectionDividerSetting.SectionType", "Resources.lsdk.SectionType"]);
            return string.IsNullOrEmpty(type) ? SectionType.Normal : Enum.Parse<SectionType>(type);
            }
        }
    public JObject Records { get; internal set; } = [];

    public override string ToString() => Name;
    }
