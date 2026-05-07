namespace Minokori.Media.Photoshop.Interfaces;

/// <summary>
/// 链接图层信息接口
/// </summary>
/// <remarks>
/// 不包含具体的图层数据, 只包含链接到的 PSD 文档信息。
/// </remarks>
public interface ILinkedLayer
    {
    /// <summary>
    /// 链接到的 Psd文档.
    /// </summary>
    /// <remarks>
    /// 若链接图层是嵌入的其他 PSD 文档, 则为该 PSD 的文档对象。否则为 null。
    /// </remarks>
    PsdDocument? Document { get; }

    /// <summary>
    /// 链接对象的绝对路径 URI。
    /// </summary>
    /// <remarks>
    /// 若链接图层是嵌入的其他 PSD 文档, 则为该 PSD 文档的绝对路径
    /// </remarks>
    string Path { get; }

    /// <summary>
    /// 是否是实际的 Psd 文档, 或只是 Psd 文档内的链接图层
    /// </summary>
    bool HasDocument { get; }

    /// <summary>
    /// 链接对象的唯一标识符
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// 链接对象的名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 链接对象的宽度
    /// </summary>
    int Width { get; }

    /// <summary>
    /// 链接对象的高度
    /// </summary>
    int Height { get; }
    }
