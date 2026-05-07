namespace Minokori.Media.Photoshop.Specifications;

/// <summary>
/// 图层标志位，表示图层的各种状态
/// </summary>
[Flags]
public enum LayerFlags : byte
    {
    /// <summary>
    /// 透明度受保护（位0）
    /// </summary>
    TransparencyProtected = 1 << 0,

    /// <summary>
    /// 可见（位1）
    /// </summary>
    Visible = 1 << 1,

    /// <summary>
    /// 过时（位2）
    /// </summary>
    Obsolete = 1 << 2,

    /// <summary>
    /// Photoshop 5.0及更高版本，指示位4是否包含有用信息（位3）
    /// </summary>
    HasUsefulBit4 = 1 << 3,

    /// <summary>
    /// 像素数据与文档外观无关（位4）
    /// </summary>
    PixelDataIrrelevant = 1 << 4,
    }
