using Minokori.Media.Photoshop.Extensions;
using Minokori.Media.Photoshop.Interfaces;
using Minokori.Media.Photoshop.Sections;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop;

/// <summary>
/// 嵌入的其他 PSD document 的信息.
/// </summary>
internal class EmbeddedLayer(JObject info) : ILinkedLayer
    {
    public FileHeaderSection FileHeader
        {
        get
            {
            field ??= FileHeaderSection.FromFile(Path);
            return field;
            }
        }
    public JObject Properties { get; } = info;
    public PsdDocument Document
        {
        get
            {
            field ??= new(Path);
            return field;
            }
        }

    //from Properties
    public string Path
        {
        get
            {
            field ??= Properties.GetValue<string>(["DescriptorOfLinkedFile.fullPath", "DescriptorOfLinkedFile.relPath", "DescriptorOfLinkedFile.Nm"]);
            return field ?? string.Empty;
            }
        }


    public bool HasDocument => true;

    public Guid ID => Properties.Value<Guid>("UniqueId");

    public string Name => System.IO.Path.GetFileName(Path);

    public int Width => FileHeader.Width;

    public int Height => FileHeader.Height;
    }
