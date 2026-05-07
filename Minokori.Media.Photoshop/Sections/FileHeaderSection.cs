using Minokori.Media.Photoshop.Extensions;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Sections;

/// <summary>
/// 文件的第一部分,包含文件的基本信息
/// </summary>
/// <remarks>
/// 原则上, 不包括图层之间相互引用的纯结构化数据直接用 <see cref="JObject"/> 表示, 考虑到链接 Psd 文件的加载, 为文件头部分提供了一个专门的类 <see cref="FileHeaderSection"/>。
/// </remarks>
public sealed class FileHeaderSection : JObject
    {
    public string Signature => Value<string>(nameof(Signature))!;

    public int Version => Value<int>(nameof(Version));

    public int Reserved => Value<int>(nameof(Reserved));

    public int NumberOfChannels => Value<int>(nameof(NumberOfChannels));

    public int Height => Value<int>(nameof(Height));

    public int Width => Value<int>(nameof(Width));

    public int Depth => Value<int>(nameof(Depth));

    public ColorMode ColorMode => Enum.Parse<ColorMode>(Value<string>(nameof(ColorMode)) ?? "RGB");

    public static FileHeaderSection FromFile(string filename)
        {
        using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new PhotoshopBinaryReader(stream);
        return reader.ReadFileHeaderSection();
        }
    }
