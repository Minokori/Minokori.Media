using System.Diagnostics;
using Minokori.Media.Photoshop.Interfaces;
using Minokori.Media.Photoshop.Sections;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Extensions;

internal static partial class PhotoshopBinaryReaderExtension
    {
    private static ChannelType[] ChannelTypes { get; } = [ChannelType.Red, ChannelType.Green, ChannelType.Blue, ChannelType.Alpha];
    private static string[] DoubleTypeKeys { get; } =
    ["LMsk", "Lr16", "Lr32", "Layr", "Mt16", "Mt32", "Mtrn", "Alph", "FMsk", "lnk2", "FEid", "FXid", "PxSD", "lnkE", "extd"];

    extension(PhotoshopBinaryReader binaryReader)
        {
        #region Read Sections
        internal FileHeaderSection ReadFileHeaderSection(int? startPosition = null)
            {
            binaryReader.Position = startPosition ?? binaryReader.Position;
            // 26 = 4 + 2 + 6 + 2 + 4 + 4 + 2 + 2
            FileHeaderSection fileHeader = new()
                {
                ["StartPosition"] = binaryReader.Position,
                ["Signature"] = binaryReader.ReadAsType(),
                ["Version"] = binaryReader.ReadInt16(),
                ["Reserved"] = binaryReader.ReadBytes(6).Sum(b => b),
                ["NumberOfChannels"] = binaryReader.ReadInt16(),
                ["Height"] = binaryReader.ReadInt32(),
                ["Width"] = binaryReader.ReadInt32(),
                ["Depth"] = binaryReader.ReadInt16(),
                ["ColorMode"] = Enum.GetName(binaryReader.ReadAsColorMode()),
                ["EndPosition"] = binaryReader.Position,
                };
            Debug.Assert(fileHeader.Value<int>("EndPosition") - fileHeader.Value<int>("StartPosition") == 26);
            return fileHeader;
            }

        /// <summary>
        /// 读取颜色模式数据部分<para/>
        /// 只有索引颜色和双色调（请参阅文件头部分中的模式字段）具有颜色模式数据。对于所有其他模式，此部分只是 4 字节长度的字段，该字段设置为零
        /// </summary>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        internal JObject ReadColorModeDataSection(int? startPosition = null)
            {
            // 初始化
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject colorModeData = [];

            // 读取数据
            colorModeData["StartPosition"] = binaryReader.Position;
            var length = binaryReader.ReadAsStreamLength();
            colorModeData["ColorModeData"] = length > 0 ? binaryReader.ReadBytes((int)length) : [];
            colorModeData["EndPosition"] = binaryReader.Position;

            // 断言
            Debug.Assert(colorModeData.Value<int>("EndPosition") - colorModeData.Value<int>("StartPosition") == length + 4/*4是表示length的int的长度*/);
            return colorModeData;
            }

        internal JObject ReadImageResourcesSection(int? startPosition = null)
            {
            // 初始化
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject value = [];

            // 读取起止位置
            value["StartPosition"] = binaryReader.Position;
            var length = binaryReader.ReadAsStreamLength();
            value["EndPosition"] = value.Value<int>("StartPosition") + length + 4/*4是表示length的int的长度*/;

            // 读取 Image Resources
            while (binaryReader.Position < value.Value<int>("EndPosition"))
                {
                _ = binaryReader.VerifySignatureIs("8BIM"); // signature, 4 bytes
                var resourceID = binaryReader.ReadInt16().ToString(); //Unique identifier for the resource. Image resource IDs contains a list of resource IDs used by Photoshop.
                _ = binaryReader.ReadAsPascalString(2); //Name: Pascal string, padded to make the size even
                long resourceLength = binaryReader.ReadInt32().PadToEven(); // Actual size of resource data that follows (even)
                var (name, result) = binaryReader.ReadResource(resourceID, resourceLength);
                if (name.Length > 0)
                    {
                    value[name] = result;
                    }
                }

            // 断言
            Debug.Assert(value.Value<int>("EndPosition") == binaryReader.Position);
            return value;
            }

        internal LayerAndMaskInformationSection ReadLayerAndMaskInformationSection(int? startPosition = null)
            {
            // 初始化
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject layerAndMaskInformationSection = [];

            // 读取起止位置
            layerAndMaskInformationSection["StartPosition"] = binaryReader.Position;
            var length = binaryReader.ReadAsStreamLength();
            layerAndMaskInformationSection["EndPosition"] = layerAndMaskInformationSection.Value<int>("StartPosition") + length + 4/*4是表示length的int的长度*/;

            // 读取 Layer Info
            var (layerInfo, layers) = binaryReader.ReadLayerInfo();


            //读取 Global Layer Mask Info
            JObject globalLayerMaskInfo = []; //1927726
            if (binaryReader.Position + 4 < layerAndMaskInformationSection.Value<int>("EndPosition"))
                {
                globalLayerMaskInfo = binaryReader.ReadGlobalLayerMaskInfo();
                }



            // 读取 Additional Info (JObeject) & Linked/Embedded Layers (ILinkedLayer)
            JObject additionalInfo = [];
            LinkedLayer[] linkedLayers = [];
            EmbeddedLayer[] embeddedLayers = []; //1927730
            if (binaryReader.Position + 4 < layerAndMaskInformationSection.Value<int>("EndPosition"))
                {
                (additionalInfo, linkedLayers, embeddedLayers) = binaryReader.ReadDocumentResource(
                    layerAndMaskInformationSection.Value<int>("EndPosition") - binaryReader.Position
                );
                }

            // 断言
            Debug.Assert(binaryReader.Position == layerAndMaskInformationSection.Value<int>("EndPosition"));

            return new(layerInfo, globalLayerMaskInfo, additionalInfo)
                {
                LinkedLayers = [.. linkedLayers.Cast<ILinkedLayer>(), .. embeddedLayers.Cast<ILinkedLayer>()],
                Layers = layers.BuildLayerTreeAndComputeBounds(),
                };
            }

        internal ImageDataSection ReadImageDataSection(FileHeaderSection fileHeader, int? startPosition = null)
            {
            // 初始化
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject imageDataSection = [];

            // 读取起止位置
            imageDataSection["StartPosition"] = binaryReader.Position;
            imageDataSection["EndPosition"] = binaryReader.Length;

            // 读取通道数, 宽度, 高度, 深度, 压缩类型
            var channelCount = fileHeader.NumberOfChannels;
            var width = fileHeader.Width;
            var height = fileHeader.Height;
            var depth = fileHeader.Depth;
            var compressionType = binaryReader.ReadAsCompressionType();

            // 读取每个通道的 RLE 包长度（如果适用）, 初始化 channel
            var channels = new Channel[channelCount];
            int[][] rlePackLengthsList = new int[channelCount][];
            for (var i = 0; i < channels.Length; i++)
                {
                var channelType = i < ChannelTypes.Length ? ChannelTypes[i] : ChannelType.Mask;
                var rlePackLengths = compressionType == CompressionType.RLE ? binaryReader.ReadAsChannelRlePackLengths(height) : [];
                channels[i] = new Channel(channelType, width, height, depth);
                rlePackLengthsList[i] = rlePackLengths;
                }

            // 读取每个通道的图像数据, 填充入 channel
            foreach (var (channel, rle) in channels.Zip(rlePackLengthsList))
                {
                channel.Data = binaryReader.ReadImageStream(width, depth, height, 1, compressionType, rle);
                }

            Debug.Assert(binaryReader.Position == binaryReader.Length);
            return new(width, height, depth, [.. channels.OrderBy(item => item.Type)]) { Document = binaryReader.Document };
            }
        #endregion

        /// <summary>
        /// 根据 resourceID 读取 resource 占用的字节长度 (4的倍数)
        /// </summary>
        /// <param name="resourceID"></param>
        /// <returns></returns>
        private long ReadResourceLength(string resourceID)
            {
            var length =
                DoubleTypeKeys.Contains(resourceID) && binaryReader.Version == 2 ? binaryReader.ReadInt64()
                : binaryReader.Version == 2 ? binaryReader.ReadInt64()
                : binaryReader.ReadInt32();
            return length.PadToFour();
            }

        private Tuple<JObject, LinkedLayer[], EmbeddedLayer[]> ReadDocumentResource(long dataLength, int? startPosition = null)
            {
            //初始化
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject props = [];

            // 读取起止位置
            long endPosition = binaryReader.Position + dataLength;
            props["StartPosition"] = binaryReader.Position;
            props["EndPosition"] = endPosition;

            List<LinkedLayer> linkedLayers = [];
            List<EmbeddedLayer> embeddedLayers = [];

            while (binaryReader.Position < endPosition)
                {
                _ = binaryReader.VerifySignatureIs("8BIM", "8B64");
                var resourceID = binaryReader.ReadAsType();
                var length = binaryReader.ReadResourceLength(resourceID);
                var (resourceName, resource) = binaryReader.ReadResource(resourceID, length);

                switch (resourceName)
                    {
                    case "LinkedLayer":
                        {
                        var items = (JArray)resource;
                        foreach (var item in items)
                            {
                            var linkedLayer = new LinkedLayer((JObject)item);
                            linkedLayers.Add(linkedLayer);
                            }

                        props[resourceName] = items;
                        continue;
                        }
                    case "EmbeddedLayer":
                        {
                        var items = (JArray)resource;
                        foreach (var item in items)
                            {
                            var embeddedLayer = new EmbeddedLayer((JObject)item);
                            embeddedLayers.Add(embeddedLayer);
                            }

                        props[resourceName] = items;
                        continue;
                        }
                    default:
                        {
                        props[resourceName] = resource;
                        continue;
                        }
                    }
                }

            Debug.Assert(binaryReader.Position == endPosition);
            return new(props, [.. linkedLayers], [.. embeddedLayers]);
            }
        }
    }
