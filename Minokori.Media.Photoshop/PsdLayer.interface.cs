using Minokori.Media.Photoshop.Extensions;
using Minokori.Media.Photoshop.Interfaces;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop;

public partial class PsdLayer
    {
    // IPsdLayer 接口实现

    #region 元数据(图层信息)
    public BlendMode BlendMode => Enum.Parse<BlendMode>(Records.GetValue<string>(["BlendMode"]) ?? "Normal");
    public int Bottom
        {
        get
            {
            if (field < 0)
                field = Records.GetValue<int>(["Bottom"]);
            return field;
            }
        internal set;
        } = -1;

    /// <summary>
    /// 是否有图像数据, 一般情况下为 <see cref="true"/>, 但 特殊的图层类型如 <see cref="SectionType.Divider"/> 可能没有图像数据
    /// </summary>
    public bool HasImage => SectionType == SectionType.Normal && Width != 0 && Height != 0;
    public bool HasMask => Records.Contains("Mask");
    public int Height => Bottom - Top;
    public bool IsClipping => Records.GetValue<bool>(["Clipping"]);
    public int Left
        {
        get
            {
            if (field < 0)
                field = Records.GetValue<int>(["Left"]);
            return field;
            }
        internal set;
        } = -1;
    public string Name => Records.GetValue<string>(["Resources.UnicodeLayerName.Name"]) ?? "";
    public float Opacity => Records.GetValue<float>(["Opacity"]) / 255f;
    public JObject Resources => Records.GetValue<JObject>(["Resources"]) ?? [];
    public int Right
        {
        get
            {
            if (field < 0)
                field = Records.GetValue<int>(["Right"]);
            return field;
            }
        internal set;
        } = -1;
    public int Top
        {
        get
            {
            if (field < 0)
                field = Records.GetValue<int>(["Top"]);
            return field;
            }
        internal set;
        } = -1;
    public int Width => Right - Left;

    /// <summary>
    /// 初始值为Psd 文档中的设置. 但此属性被设置为 <b>可读写的</b>, 以便在需要时修改图层的可见性.
    /// </summary>
    public bool IsVisible { get; set; }
    #endregion

    #region 引用信息
    public IPsdLayer[] Childs { get; set; } = [];
    public required PsdDocument Document { get; init; }
    public ILinkedLayer? LinkedLayer
        {
        get
            {
            var guidString = Records.GetValue<string>(["Resources.SmartObjectLayerData.Idnt"]);
            if (guidString is null)
                return null;
            var placeID = new Guid(guidString);

            field ??= Document.LinkedLayers.Where(i => i.ID == placeID && i.HasDocument).FirstOrDefault();
            return field;
            }
        }
    public IPsdLayer Parent
        {
        get
            {
            field ??= Document;
            return field;
            }
        set;
        }

    #endregion

    #region 图像信息
    public Channel[] Channels { get; internal set; } = [];
    public byte[] MergedImage { get; set; } = [];

    #endregion
    }
