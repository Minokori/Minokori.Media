namespace Minokori.Media.Photoshop.Specifications;

/// <summary>
/// 色彩模式
/// </summary>
public enum ColorMode
    {
    /// <summary>
    /// 位图
    /// </summary>
    Bitmap = 0,

    /// <summary>
    /// 灰度
    /// </summary>
    GrayScale = 1,

    /// <summary>
    /// 索引
    /// </summary>
    Indexed = 2,

    /// <summary>
    /// RGB
    /// </summary>
    RGB = 3,

    /// <summary>
    /// CMWK
    /// </summary>
    CMYK = 4,

    /// <summary>
    /// 多通道
    /// </summary>
    MultiChannel = 7,

    /// <summary>
    /// 双色调
    /// </summary>
    DUOTONE = 8,

    /// <summary>
    /// 实验室
    /// </summary>
    LAB = 9,
    }
