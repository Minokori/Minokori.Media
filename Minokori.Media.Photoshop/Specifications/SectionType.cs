namespace Minokori.Media.Photoshop.Specifications;

/// <summary>
/// 图层组状态 (分节分隔线设置, Section divider setting)
/// </summary>
/// <remarks>
/// key = lsct
/// </remarks>
public enum SectionType
    {
    /// <summary>
    /// 任何其他类型的图层
    /// </summary>
    Normal = 0,
    /// <summary>
    /// 打开的 “文件夹”
    /// </summary>
    Open = 1,
    /// <summary>
    /// 关闭的 “文件夹”
    /// </summary>
    Closed = 2,
    /// <summary>
    /// 边界部分分隔线，隐藏在 UI 中
    /// </summary>
    Divider = 3,
    }
