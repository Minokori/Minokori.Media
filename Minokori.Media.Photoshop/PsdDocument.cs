using Minokori.Media.Photoshop.Extensions;
using Minokori.Media.Photoshop.Interfaces;

namespace Minokori.Media.Photoshop;

public sealed partial class PsdDocument : IPsdLayer
    {
    public PsdDocument(string filename)
        {
        using Stream stream = File.OpenRead(filename);
        using PhotoshopBinaryReader reader = new(stream) { Document = this };
        FileHeader = reader.ReadFileHeaderSection();
        reader.Depth = FileHeader.Depth;
        ColorModeDataSection = reader.ReadColorModeDataSection();
        ImageResourcesSection = reader.ReadImageResourcesSection();
        LayerAndMaskSection = reader.ReadLayerAndMaskInformationSection();
        ImageDataSection = reader.ReadImageDataSection(FileHeader);
        }
    }
