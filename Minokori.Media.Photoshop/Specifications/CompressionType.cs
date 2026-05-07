namespace Minokori.Media.Photoshop.Specifications;

/// <summary>
/// 压缩类型
/// </summary>
public enum CompressionType
    {
    /// <summary>
    /// 原始数据
    /// </summary>
    Raw = 0,

    /// <summary>
    /// RLE压缩
    /// </summary>
    RLE = 1,

    /// <summary>
    /// 无预测Zip
    /// </summary>
    Zip = 2,

    /// <summary>
    /// 有预测Zip
    /// </summary>
    ZipPrediction = 3,
    }
