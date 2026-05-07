using System.Diagnostics;
using Minokori.Media.Photoshop.Exceptions;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Extensions;

internal static partial class PhotoshopBinaryReaderExtension
    {
    // BUG
    extension(PhotoshopBinaryReader binaryReader)
        {
        internal (JObject LayerInfo, PsdLayer[] Layers) ReadLayerInfo(int? startPosition = null)
            {
            // 初始化
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject layerInfo = [];

            // 读取起止位置
            layerInfo["StartPosition"] = binaryReader.Position;
            var length = binaryReader.ReadAsStreamLength();
            layerInfo["EndPosition"] = layerInfo.Value<int>("StartPosition") + length + 4;

            // 读取图层数
            var layerCount = Math.Abs((int)binaryReader.ReadInt16());
            layerInfo["LayerCount"] = layerCount;


            var layers = new PsdLayer[layerCount];
            for (var i = 0; i < layerCount; i++)
                {
                layers[i] = new PsdLayer() { Document = binaryReader.Document };
                }

            // 读取每个图层的 layer record
            var layerRecords = new JArray();
            for (var i = 0; i < layerCount; i++)
                {
                var record = binaryReader.ReadLayerRecords();
                layerRecords.Add(record);
                layers[i].Records = record;
                }

            layerInfo["LayerRecord"] = layerRecords;

            // 读取每个图层的 channel image data
            JArray channelsImageDatas = [];

            foreach (var (layerRecord, layerIndex) in layerRecords.Select((layerRecord, layerIndex) => ((JObject)layerRecord, layerIndex)))
                {
                var channelsTotalLength = layerRecord.Value<JArray>("ChannelDataLength")!.Values<long>()!.Sum();
                var items = layerRecord.ToChannelInfo();
                var (channelImageData, imageBytes) = binaryReader.ReadChannelImageData(channelsTotalLength, items);
                channelImageData.Add(channelImageData);
                layers[layerIndex].Channels = new Channel[items.Count];
                foreach (var (item, channelIndex) in items.Select((item, channelIndex) => (item, channelIndex)))
                    {
                    var (type, width, height) = item;
                    layers[layerIndex].Channels[channelIndex] = new Channel(type, width, height, binaryReader.Depth)
                        {
                        Data = imageBytes[channelIndex],
                        };
                    if (type == ChannelType.Alpha)
                        {
                        var opacity = layerRecord.GetValue<byte>(["Opacity", "Resources.iOpa.Opacity"]);
                        layers[layerIndex].Channels[channelIndex].Opacity = opacity == 0 ? 1.0f : opacity / 255.0f;
                        }
                    }

                var isLayerVisible = (layerRecord.GetValue<byte>(["Flags"]) & (byte)LayerFlags.Visible) != 0;
                layers[layerIndex].IsVisible = isLayerVisible;
                }


            layerInfo["ChannelsImageData"] = channelsImageDatas;

            // 后续处理
            binaryReader.Position = layerInfo.Value<int>("StartPosition") + length + 4; // 由于补位的存在,
            return (layerInfo, layers);
            }

        /// <summary>
        /// 读取1个图层的 Layer Record.<para/>
        /// </summary>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        /// <remarks>
        /// 包括
        /// <list type="bullet">
        /// <layerRecord>图层边界信息 (Top, Left, Bottom, Right)</layerRecord>
        /// <layerRecord>通道信息 (ChannelCount[], ChannelID[], ChannelDataLength[])</layerRecord>
        /// <layerRecord>混合模式信息 (BlendMode, Opacity, Clipping, Flags, Filler)</layerRecord>
        /// <layerRecord>图层遮罩信息 (Mask)</layerRecord>
        /// <layerRecord>图层混合范围信息 (BlendingRanges)</layerRecord>
        /// <layerRecord>图层名称 (PascalName)</layerRecord>
        /// <layerRecord>图层资源信息 (Resources)</layerRecord>
        /// </list>
        /// </remarks>
        internal JObject ReadLayerRecords(int? startPosition = null)
            {
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject records = new()
                {
                ["StartPosition"] = binaryReader.Position,
                ["Top"] = binaryReader.ReadInt32(),
                ["Left"] = binaryReader.ReadInt32(),
                ["Bottom"] = binaryReader.ReadInt32(),
                ["Right"] = binaryReader.ReadInt32(),
                ["ChannelCount"] = binaryReader.ReadUInt16(),
                };

            records["ChannelID"] = new JArray();
            records["ChannelDataLength"] = new JArray();

            for (var i = 0; i < records.Value<int>("ChannelCount"); i++)
                {
                var id = binaryReader.ReadAsChannelType();
                (records["ChannelID"] as JArray)!.Add(Enum.GetName(id));
                var l = binaryReader.ReadAsStreamLength();
                (records["ChannelDataLength"] as JArray)!.Add(l);
                }

            _ = binaryReader.VerifySignatureIs("8BIM");

            records["BlendMode"] = Enum.GetName(binaryReader.ReadAsBlendMode());
            records["Opacity"] = binaryReader.ReadByte();
            records["Clipping"] = binaryReader.ReadBoolean();
            records["Flags"] = (byte)binaryReader.ReadAsLayerFlags();
            records["Filler"] = binaryReader.ReadByte();

            // 计算 总长度(前面读取过的长度 + 可变长度部分的长度)
            // 可变部分: 5个, mask + blendingRanges + pascalName + resources
            var StreamLength = 16 + 2 + (6 * records.Value<int>("ChannelCount")) + 4 + 4 + 1 + 1 + 1 + 1 + 4 + binaryReader.ReadInt32();

            records["EndPosition"] = records.Value<int>("StartPosition") + StreamLength;

            records["Mask"] = binaryReader.ReadLayerMask();
            records["BlendingRanges"] = binaryReader.ReadLayerBlendingRanges();
            // BUG 乱码?
            records["PascalName"] = binaryReader.ReadAsPascalString(4);

            var resources = binaryReader.ReadLayerResource(records.Value<long>("EndPosition") - binaryReader.Position);
            records["Resources"] = resources;

            Debug.Assert(binaryReader.Position == records.Value<int>("EndPosition"));
            return records;
            }

        internal JObject ReadGlobalLayerMaskInfo(int? startPosition = null)
            {
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject globalLayerMaskInfo = [];
            globalLayerMaskInfo["StartPosition"] = binaryReader.Position;
            var length = binaryReader.ReadInt32();
            if (length == 0)
                return [];
            globalLayerMaskInfo["OverlayColorSpace"] = binaryReader.ReadInt16();

            var fillerLength = length - (2 + 8 + 2 + 1);
            JArray colorComponents = [];

            for (var i = 0; i < 4; i++)
                {
                colorComponents.Add(binaryReader.ReadInt16());
                }

            globalLayerMaskInfo["ColorComponents"] = colorComponents;

            globalLayerMaskInfo["Opacity"] = binaryReader.ReadInt16();

            globalLayerMaskInfo["Kind"] = binaryReader.ReadByte();

            globalLayerMaskInfo["Filler"] = binaryReader.ReadBytes(fillerLength);
            globalLayerMaskInfo["EndPosition"] = globalLayerMaskInfo.Value<int>("StartPosition") + length + 4;
            Debug.Assert(binaryReader.Position == globalLayerMaskInfo.Value<int>("EndPosition"));
            return globalLayerMaskInfo;
            }

        internal JObject ReadDocumentResourse(int? startPosition = null) => throw new NotImplementedException();

        /// <summary>
        /// 读取1个图层的 Channel Image Data.<para/>
        /// </summary>
        /// <param name="dataLength"></param>
        /// <param name="layerRecord"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        internal (JArray ChannelImageData, List<byte[]> ImageData) ReadChannelImageData(long dataLength, List<(ChannelType type, int width, int height)> items, int? startPosition = null)
            {
            binaryReader.Position = startPosition ?? binaryReader.Position;
            var EndPosition = binaryReader.Position + dataLength;

            JArray ImageMetaData = []; //channel 的heigt, width,depth ,count
            List<byte[]> ImageData = [];
            foreach (var (_, width, height) in items)
                {
                JObject data = [];
                // 1. 读压缩类型
                var compressionType = binaryReader.ReadAsCompressionType();
                data["CompressionType"] = Enum.GetName(compressionType);

                // 2. 读压缩长度
                var rlePackLength = compressionType == CompressionType.RLE ? binaryReader.ReadAsChannelRlePackLengths(height) : [];
                data["RlePackLengths"] = new JArray(rlePackLength);

                // 3. 解压数据
                // 读取的长度:
                // RAW: depth * Width * Height
                // RLE: 每行的长度 Rle 相加
                data["StartPosition"] = binaryReader.Position;
                var channelTotalLength =
                    compressionType == CompressionType.Raw ? width.ToPitch(binaryReader.Depth) * height : rlePackLength.Sum();
                data["StreamLength"] = channelTotalLength;

                // 4. 读取图像数据
                var imageBytes = binaryReader.ReadImageStream(width, binaryReader.Depth, height, 1, compressionType, rlePackLength);
                ImageData.Add(imageBytes);
                Debug.Assert(binaryReader.Position == data.Value<int>("StartPosition") + channelTotalLength);
                data["EndPosition"] = binaryReader.Position;
                ImageMetaData.Add(data);
                }

            Debug.Assert(binaryReader.Position == EndPosition);
            return new(ImageMetaData, ImageData);
            }

        internal JObject ReadLayerMask(int? startPosition = null)
            {
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject layerMask = new() { ["StartPosition"] = binaryReader.Position };
            var length = binaryReader.ReadInt32();
            switch (length)
                {
                case 0:
                    layerMask.RemoveAll();
                    break;
                case 20:
                    {
                    layerMask["Top"] = binaryReader.ReadInt32();
                    layerMask["Left"] = binaryReader.ReadInt32();
                    layerMask["Bottom"] = binaryReader.ReadInt32();
                    layerMask["Right"] = binaryReader.ReadInt32();
                    layerMask["EndPosition"] = binaryReader.Position;
                    break;
                    }
                case 40:
                    {
                    layerMask["Top"] = binaryReader.ReadInt32();
                    layerMask["Left"] = binaryReader.ReadInt32();
                    layerMask["Bottom"] = binaryReader.ReadInt32();
                    layerMask["Right"] = binaryReader.ReadInt32();
                    layerMask["Color"] = binaryReader.ReadByte();
                    layerMask["Flags"] = binaryReader.ReadByte();
                    layerMask["UserMaskDensity"] = binaryReader.ReadByte();
                    layerMask["UserMaskFeather"] = binaryReader.ReadDouble();
                    layerMask["VectorMaskDensity"] = binaryReader.ReadByte();
                    layerMask["VectorMaskFeather"] = binaryReader.ReadDouble();
                    layerMask["EndPosition"] = binaryReader.Position;
                    break;
                    }
                default:
                    throw new InvalidFormatException(
                        $"LayerMaskReader: Invalid stream length {length} for LayerMask. Expected 4, 20, or 40 bytes."
                    );
                }

            return layerMask;
            }

        internal JObject ReadLayerResource(long dataLength, int? startPosition = null)
            {
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject blendingRanges = [];
            blendingRanges["StartPosition"] = binaryReader.Position;
            var endPosition = binaryReader.Position + dataLength;
            while (binaryReader.Position < endPosition)
                {
                _ = binaryReader.VerifySignatureIs("8BIM");
                var resourceID = binaryReader.ReadAsType();
                long length = binaryReader.ReadInt32().PadToEven();
                var (resourceName, result) = binaryReader.ReadResource(resourceID, length);
                blendingRanges[resourceName] = result;
                }

            blendingRanges["EndPosition"] = binaryReader.Position;
            Debug.Assert(binaryReader.Position == endPosition);
            return blendingRanges;
            }

        internal JObject ReadLayerBlendingRanges(int? startPosition = null)
            {
            binaryReader.Position = startPosition ?? binaryReader.Position;
            JObject blendingRanges = [];
            blendingRanges["StartPosition"] = binaryReader.Position;
            var length = binaryReader.ReadInt32();
            var endPosition = blendingRanges.Value<int>("StartPosition") + length + 4;

            blendingRanges["CompositeGrayBlendSource"] = binaryReader.ReadInt32();
            blendingRanges["CompositeGrayDestinationRange"] = binaryReader.ReadInt32();

            var channelSourceRange = new JArray();
            var channelDestinationRange = new JArray();
            while (binaryReader.Position < endPosition)
                {
                channelSourceRange.Add(binaryReader.ReadInt32());
                channelDestinationRange.Add(binaryReader.ReadInt32());
                }

            blendingRanges["ChannelSourceRange"] = channelSourceRange;
            blendingRanges["ChannelDestinationRange"] = channelDestinationRange;
            blendingRanges["EndPosition"] = binaryReader.Position;
            Debug.Assert(binaryReader.Position == endPosition);
            return blendingRanges;
            }
        }
    }
