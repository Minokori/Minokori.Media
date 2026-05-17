using System.Diagnostics;
using Emgu.CV.Structure;
using Minokori.Media.Photoshop.Extensions;

namespace Minokori.Media.Demo.Godot;

public partial class Character : Node2D
    {
    internal Sprite2D Tachie { get; init; }
    internal Sprite2D Avatar { get; init; }

    private ImageTexture Texture { get; set; }

    private RichTextLabel TextBubble { get; init; }

    public Character(string name, string path)
        {
        Debug.Assert(System.IO.File.Exists(path), $"Photoshop 文档不存在: {path}");
        Name = name;
        File = new(path);
        var image = Image.CreateFromData(
            File.Width,
            File.Height,
            false,
            Image.Format.Rgba8,
            File.MergeChannelsToCVImage().Convert<Rgba, byte>().Bytes
        );
        Texture = ImageTexture.CreateFromImage(image);
        TextBubble = new RichTextLabel
            {
            Text = "你好, 世界!",
            Position = new(100, 200),
            Size = new(200, 50),
            BbcodeEnabled = true,
            };
        AddChild(TextBubble);
        Tachie = new Sprite2D
            {
            Texture = Texture,
            Position = Vector2.Zero,
            Centered = true
            };
        AddChild(Tachie);
        Avatar = new Sprite2D
            {
            Texture = Texture,
            Position = new Vector2(600, 0),
            Centered = true,
            RegionEnabled = true,
            RegionRect = new(288, 66, 265, 265)
            };
        AddChild(Avatar);
        }

    public void Say(string message) => TextBubble.Text = message;

    }
