using System.Diagnostics;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Extensions;

internal static partial class PhotoshopBinaryReaderExtension
    {
    extension(PhotoshopBinaryReader GlobalReader)
        {

        private JObject ReadSliceResourceBlock()
            {
            var props = new JObject
                {
                ["ID"] = GlobalReader.ReadInt32(),
                ["GroupID"] = GlobalReader.ReadInt32(),
                };


            var origin = GlobalReader.ReadInt32();
            if (origin == 1)
                {
                _ = GlobalReader.ReadInt32();
                }

            props["Name"] = GlobalReader.ReadString();
            _ = GlobalReader.ReadInt32();

            props["Left"] = GlobalReader.ReadInt32();
            props["Top"] = GlobalReader.ReadInt32();
            props["Right"] = GlobalReader.ReadInt32();
            props["Bottom"] = GlobalReader.ReadInt32();

            props["Url"] = GlobalReader.ReadString();
            props["Target"] = GlobalReader.ReadString();
            props["Message"] = GlobalReader.ReadString();
            props["AltTag"] = GlobalReader.ReadString();
            _ = GlobalReader.ReadBoolean();
            _ = GlobalReader.ReadString();

            props["Horizontal Alignment"] = GlobalReader.ReadInt32();
            props["Vertical Alignment"] = GlobalReader.ReadInt32();

            props["Alpha"] = GlobalReader.ReadByte();
            props["Red"] = GlobalReader.ReadByte();
            props["Green"] = GlobalReader.ReadByte();
            props["Blue"] = GlobalReader.ReadByte();

            return props;
            }

        private static JObject ReadSliceInfo(JToken properties)
            {
            var props = new JObject
                {
                ["ID"] = properties["sliceID"],
                ["GroupID"] = properties["groupID"],
                };
            if (properties.Contains("Nm") == true)
                props["Name"] = properties["Nm"];

            props["Left"] = properties.SelectToken("bounds.Left");
            props["Top"] = properties.SelectToken("bounds.Top");
            props["Right"] = properties.SelectToken("bounds.Rght");
            props["Bottom"] = properties.SelectToken("bounds.Btom");
            props["Url"] = properties["url"];
            props["Target"] = properties["null"];
            props["Message"] = properties["Msge"];
            props["AltTag"] = properties["altTag"];

            if (properties.Contains("bgColor") == true)
                {
                props["Alpha"] = properties.SelectToken("bgColor.alpha");
                props["Red"] = properties.SelectToken("bgColor.Rd");
                props["Green"] = properties.SelectToken("bgColor.Grn");
                props["Blue"] = properties.SelectToken("bgColor.Bl");
                }

            return props;
            }


        internal (string displayName, JToken resource) ReadResource(string resourceID, long dataLength, long? startPosition = null)
            {
            return resourceID switch
                {
                    "lnkE" => ("EmbeddedLayer", GlobalReader.ReadEmbedded(dataLength, startPosition)),
                    "fxlr" => ("EffectsLayer", GlobalReader.ReadEffectsLayer(dataLength, startPosition)),
                    "1032" => ("GridAndGuides", GlobalReader.ReadGridAndGuides(dataLength, startPosition)),
                    "lyid" => ("LayerID", GlobalReader.ReadLayerId(dataLength, startPosition)),
                    "lnsr" => ("LayerNameSourceSetting", GlobalReader.ReadLayerNameSourceSetting(dataLength, startPosition)),
                    "lyvr" => ("LayerVersion", GlobalReader.ReadLayerVersion(dataLength, startPosition)),
                    "lnkD" => ("LinkedLayer", GlobalReader.ReadLinkedLayer(dataLength, startPosition)),
                    "lnk2" => ("LinkedLayer", GlobalReader.ReadLinkedLayer(dataLength, startPosition)),
                    "lnk3" => ("LinkedLayer", GlobalReader.ReadLinkedLayer(dataLength, startPosition)),
                    "shmd" => ("MetadataSetting", GlobalReader.ReadMetadataSetting(dataLength, startPosition)),
                    "lfx2" => ("ObjectBasedEffectsLayerInfo", GlobalReader.ReadObjectBasedEffectsLayerInfo(dataLength, startPosition)),
                    "PlLd" => ("PlacedLayer", GlobalReader.ReadPlacedLayer(dataLength, startPosition)),
                    "SoLd" => ("PlacedLayerData", GlobalReader.ReadPlacedLayerData(dataLength, startPosition)),
                    "iOpa" => ("iOpa", GlobalReader.ReadiOpa(dataLength, startPosition)),
                    "lsdk" => ("lsdk", GlobalReader.Readlsdk(dataLength, startPosition)),
                    "fxrp" => ("ReferencePoint", GlobalReader.ReadReferencePoint(dataLength, startPosition)),
                    "1005" => ("ResolutionInfo", GlobalReader.ReadResolutionInfo(dataLength, startPosition)),
                    "lsct" => ("SectionDividerSetting", GlobalReader.ReadSectionDividerSetting(dataLength, startPosition)),
                    "1050" => ("SlicesInfo", GlobalReader.ReadSlicesInfo(dataLength, startPosition)),
                    "1057" => ("VersionInfo", GlobalReader.ReadVersionInfo(dataLength, startPosition)),
                    "luni" => ("UnicodeLayerName", GlobalReader.ReadUnicodeLayerName(dataLength, startPosition)),
                    "TySh" => ("TypeToolObjectSetting", GlobalReader.ReadTypeToolObjectSetting(dataLength, startPosition)),
                    "SoLE" => ("SmartObjectLayerData", GlobalReader.ReadSmartObjectLayerData(dataLength, startPosition)),
                    _ => ("", GlobalReader.ReadEmpty(dataLength, startPosition))
                    };
            }

        internal JObject ReadEffectsLayer(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            JObject value = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["EndPosition"] = GlobalReader.Position + dataLength
                };
            _ = GlobalReader.ReadInt16();
            int count = GlobalReader.ReadInt16();

            for (var i = 0; i < count; i++)
                {
                _ = GlobalReader.ReadAsAscii(4);
                var effectType = GlobalReader.ReadAsAscii(4);
                var size = GlobalReader.ReadInt32();
                var p = GlobalReader.Position;

                switch (effectType)
                    {
                    case "dsdw":
                        {
                        //ShadowInfo.Parse(_reader);
                        }

                    break;
                    case "sofi":
                        {
                        //this.solidFillInfo = SolidFillInfo.Parse(_reader);
                        }

                    break;
                    }

                GlobalReader.Position = p + size;
                }

            Debug.Assert(GlobalReader.Position == value.Value<long>("EndPosition"));
            return value;
            }

        internal JArray ReadEmbedded(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var EndPosition = GlobalReader.Position + dataLength;
            JArray embeddedLayerInfoList = [];
            while (GlobalReader.Position < EndPosition)
                {
                var endPosition = GlobalReader.ReadInt64().PadToFour() + GlobalReader.Position;
                JObject embeddedLayerInfo = new()
                    {
                    //'liFD' linked file data, 'liFE' linked file external or 'liFA' linked file alias
                    ["Type"] = GlobalReader.VerifySignatureIs("liFD", "liFA", "liFE"),
                    // Version ( = 1 to 7 )
                    ["Version"] = GlobalReader.ReadInt32(),
                    ["UniqueId"] = GlobalReader.ReadAsPascalString(),
                    ["OriginalFileName"] = GlobalReader.ReadString(),
                    ["FileType"] = GlobalReader.ReadAsType(),
                    ["FileCreator"] = GlobalReader.ReadAsType(),
                    };

                // length of data below
                _ = GlobalReader.ReadInt64();
                var fileOpenDescriptor = GlobalReader.ReadBoolean();
                if (fileOpenDescriptor)
                    {
                    _ = GlobalReader.ReadDescriptor();//StructureReader.ReadDescriptor(GlobalReader);
                    }
                // in document :If the type is 'liFE' then a linked file Descriptor is next.
                embeddedLayerInfo["DescriptorOfLinkedFile"] = GlobalReader.ReadDescriptor();

                if (embeddedLayerInfo.Value<int>("Version") > 3)
                    {
                    embeddedLayerInfo["Year"] = GlobalReader.ReadInt32();
                    embeddedLayerInfo["Month"] = GlobalReader.ReadByte();
                    embeddedLayerInfo["Day"] = GlobalReader.ReadByte();
                    embeddedLayerInfo["Hour"] = GlobalReader.ReadByte();
                    embeddedLayerInfo["Minute"] = GlobalReader.ReadByte();
                    embeddedLayerInfo["Seconds"] = GlobalReader.ReadDouble();
                    }

                embeddedLayerInfo["FileSize"] = GlobalReader.ReadInt64();
                if (embeddedLayerInfo.Value<int>("Version") >= 5)
                    {
                    embeddedLayerInfo["ChildDocumentId"] = GlobalReader.ReadString();
                    }

                if (embeddedLayerInfo.Value<int>("Version") >= 6)
                    {
                    embeddedLayerInfo["AssetModTime"] = GlobalReader.ReadDouble();
                    }

                if (embeddedLayerInfo.Value<int>("Version") >= 7)
                    {
                    embeddedLayerInfo["AssetLockedState"] = GlobalReader.ReadBoolean();
                    }

                embeddedLayerInfoList.Add(embeddedLayerInfo);
                GlobalReader.Position = endPosition;
                }

            Debug.Assert(GlobalReader.Position == EndPosition);
            return embeddedLayerInfoList;

            }

        internal JArray ReadEmpty(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            GlobalReader.Position += dataLength;
            // Many Unknown id!
            return [];
            }
        internal JObject ReadGridAndGuides(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                };
            _ = GlobalReader.VerifyIntIs(1); // version
            props["HorizontalGrid"] = GlobalReader.ReadInt32();
            props["VerticalGrid"] = GlobalReader.ReadInt32();
            var guideCount = GlobalReader.ReadInt32();
            List<int> horizontalGrids = [];
            List<int> verticalGrids = [];
            for (var i = 0; i < guideCount; i++)
                {
                var n = GlobalReader.ReadInt32();
                var t = GlobalReader.ReadByte();
                if (t == 0)
                    verticalGrids.Add(n);
                else
                    horizontalGrids.Add(n);
                }

            props["HorizontalGuides"] = new JArray(horizontalGrids);
            props["VerticalGuides"] = new JArray(verticalGrids);
            props["EndPosition"] = endPosition;
            Debug.Assert(GlobalReader.Position == props.Value<long>("EndPosition"));
            return props;
            }

        internal JObject ReadLayerId(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["ID"] = GlobalReader.ReadInt32()
                };
            props["EndPosition"] = endPosition;
            Debug.Assert(GlobalReader.Position == props.Value<long>("EndPosition"));
            return props;
            }
        internal JObject ReadLayerNameSourceSetting(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;

            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["Name"] = GlobalReader.ReadAsAscii(4)
                };
            props["EndPosition"] = endPosition;
            Debug.Assert(GlobalReader.Position == props.Value<long>("EndPosition"));
            return props;
            }

        internal JObject ReadLayerVersion(long dataLength, long? startPostion = null)
            {
            GlobalReader.Position = startPostion ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["Version"] = GlobalReader.ReadInt32()
                };
            props["EndPosition"] = endPosition;
            Debug.Assert(GlobalReader.Position == props.Value<long>("EndPosition"));
            return props;
            }

        internal JArray ReadLinkedLayer(long dataLength, long? startPostion = null)
            {
            GlobalReader.Position = startPostion ?? GlobalReader.Position;
            var EndPosition = GlobalReader.Position + dataLength;
            JArray info = [];
            while (GlobalReader.Position < EndPosition)
                {
                // 注意: 先读取 **长度**, 使 GlobalReader.Position 移动后再计算 endPosition
                var endPosition = GlobalReader.ReadInt64().PadToFour() + GlobalReader.Position;
                JObject linkedLayerInfo = new()
                    {
                    //'liFD' linked file data, 'liFE' linked file external or 'liFA' linked file alias
                    ["Type"] = GlobalReader.VerifySignatureIs("liFD", "liFA", "liFE"),
                    // Version ( = 1 to 7 )
                    ["Version"] = GlobalReader.ReadInt32(),
                    ["UniqueId"] = GlobalReader.ReadAsPascalString(),
                    ["OriginalFileName"] = GlobalReader.ReadString(),
                    ["FileType"] = GlobalReader.ReadAsType(),
                    ["FileCreator"] = GlobalReader.ReadAsType(),
                    };
                // length of data below
                _ = GlobalReader.ReadInt64();

                var fileOpenDescriptor = GlobalReader.ReadBoolean();
                if (fileOpenDescriptor)
                    {
                    _ = GlobalReader.ReadDescriptor(); //StructureReader.ReadDescriptor(GlobalReader);
                    }

                #region TODO maybe raw bytes for linked img, ,linked psd turns to lnkE
                //var isDocument = IsDocument(GlobalReader);
                //LinkedDocumentReader documentReader = null;
                //LinkedDocumentFileHeaderReader fileHeaderReader = null;
                //if (lengthOfDataBelow > 0 && isDocument == true)
                //    {
                //    var position = GlobalReader.Position;
                //    documentReader = new LinkedDocumentReader(GlobalReader, lengthOfDataBelow);
                //    GlobalReader.Position = position;
                //    fileHeaderReader = new LinkedDocumentFileHeaderReader(GlobalReader, lengthOfDataBelow);
                //    }
                #endregion

                info.Add(linkedLayerInfo);
                GlobalReader.Position = endPosition;
                }

            return info;
            }

        internal JObject ReadMetadataSetting(long dataLength, long? startPostion = null)
            {
            GlobalReader.Position = startPostion ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                };
            var count = GlobalReader.ReadInt32();

            JArray dss = [];

            for (var i = 0; i < count; i++)
                {
                _ = GlobalReader.ReadAsAscii(4);
                _ = GlobalReader.ReadAsAscii(4);
                _ = GlobalReader.ReadByte();
                _ = GlobalReader.ReadBytes(3);
                var l = GlobalReader.ReadInt32();
                var p2 = GlobalReader.Position;
                var ds = GlobalReader.ReadDescriptor();//StructureReader.ReadDescriptor(GlobalReader);
                dss.Add(ds);
                GlobalReader.Position = p2 + l;
                }

            props["Items"] = dss;
            //props["Items"] = dss;
            props["EndPosition"] = endPosition;
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;

            }

        internal JObject ReadObjectBasedEffectsLayerInfo(long dataLength, long? startPostion = null)
            {
            GlobalReader.Position = startPostion ?? GlobalReader.Position;
            _ = GlobalReader.Position + dataLength;
            _ = new JObject()
                {
                ["StartPosition"] = GlobalReader.Position,
                };
            _ = GlobalReader.VerifyIntIs(0);
            return GlobalReader.ReadDescriptor();//StructureReader.ReadDescriptor(GlobalReader);
            }

        internal JObject ReadPlacedLayer(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = [];
            props["StartPosition"] = GlobalReader.Position;
            _ = GlobalReader.VerifySignatureIs("plcL");
            props["Version"] = GlobalReader.ReadInt32();
            props["UniqueId"] = GlobalReader.ReadAsPascalString(1);
            props["PageNumbers"] = GlobalReader.ReadInt32();
            props["Pages"] = GlobalReader.ReadInt32();
            props["AntiAlias"] = GlobalReader.ReadInt32();
            props["LayerType"] = GlobalReader.ReadInt32();
            //props["Transformation"] = GlobalReader.ReadDoubles(8);
            props["Transformation"] = new JArray(GlobalReader.ReadDoubles(8));
            _ = GlobalReader.VerifyIntIs(0);
            props["Warp"] = GlobalReader.ReadDescriptor();//StructureReader.ReadDescriptor(GlobalReader);
            props["EndPosition"] = endPosition;
            return props;

            }

        internal JObject ReadPlacedLayerData(long dataLength, long? startPosition = null)
            {
            // BUG
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            _ = GlobalReader.VerifySignatureIs("soLD");
            _ = GlobalReader.VerifyIntIs(4);
            JObject descriptor = GlobalReader.ReadDescriptor();//StructureReader.ReadDescriptor(GlobalReader);
            // TODO 权益之计. 后期需要调整
            if (endPosition - GlobalReader.Position == 2)
                {
                GlobalReader.Position = endPosition;
                }

            Debug.Assert(GlobalReader.Position == endPosition);
            return descriptor;
            }
        internal JObject ReadiOpa(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["Opacity"] = GlobalReader.ReadByte(),
                ["EndPosition"] = GlobalReader.Position
                };
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }

        internal JObject Readlsdk(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["SectionType"] = GlobalReader.ReadInt32(),
                ["EndPosition"] = GlobalReader.Position
                };
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }

        internal JObject ReadReferencePoint(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["ReferencePoint"] = new JArray(GlobalReader.ReadDoubles(2)),
                ["EndPosition"] = GlobalReader.Position
                };
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }
        internal JObject ReadResolutionInfo(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["HorizontalRes"] = GlobalReader.ReadInt16(),
                ["HorizontalResUnit"] = GlobalReader.ReadInt32(),
                ["WidthUnit"] = GlobalReader.ReadInt16(),
                ["VerticalRes"] = GlobalReader.ReadInt16(),
                ["VerticalResUnit"] = GlobalReader.ReadInt32(),
                ["HeightUnit"] = GlobalReader.ReadInt16(),
                ["EndPosition"] = endPosition
                };
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }

        internal JObject ReadSectionDividerSetting(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["SectionType"] = Enum.GetName((SectionType)GlobalReader.ReadInt32()),
                ["EndPosition"] = endPosition
                };
            // TODO 
            GlobalReader.Position = endPosition;
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }

        internal JObject ReadSlicesInfo(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = [];
            props["StartPosition"] = GlobalReader.Position;
            var version = GlobalReader.ReadInt32();
            if (version == 6)  // Photoshop<=7.0
                {
                //Bounding rectangle for all of the slices: top, left, bottom, right of all the slices
                _ = GlobalReader.ReadInt32();
                _ = GlobalReader.ReadInt32();
                _ = GlobalReader.ReadInt32();
                _ = GlobalReader.ReadInt32();
                // Name of group of slices
                _ = GlobalReader.ReadString();
                // Number of slices to follow
                var count = GlobalReader.ReadInt32();


                // slice resource blocks
                JArray slices = [];
                for (var i = 0; i < count; i++)
                    {
                    slices.Add(GlobalReader.ReadSliceResourceBlock());
                    }
                }

            // Photoshop>=CS
                {
                var descriptor = GlobalReader.ReadDescriptor();
                var items = descriptor.Value<JArray>("slices") ?? [];

                JArray slices = [];//items.Length
                foreach (var item in items)
                    {
                    slices.Add(ReadSliceInfo(item));
                    }

                props["Items"] = slices;
                }

            props["EndPosition"] = endPosition;
            // TODO : verify position
            GlobalReader.Position = endPosition;
            Debug.Assert(GlobalReader.Position == props.Value<long>("EndPosition"));
            return props;
            }

        internal JObject ReadVersionInfo(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["Version"] = GlobalReader.ReadInt32(),
                ["HasRealMergedData"] = GlobalReader.ReadBoolean(),
                ["WriterName"] = GlobalReader.ReadString(),
                ["ReaderName"] = GlobalReader.ReadString(),
                ["FileVersion"] = GlobalReader.ReadInt32(),
                ["EndPosition"] = endPosition
                };
            // TODO Position 少一位
            GlobalReader.Position = endPosition;
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }
        internal JObject ReadUnicodeLayerName(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                ["Name"] = GlobalReader.ReadString(),
                ["EndPosition"] = endPosition
                };
            // TODO Position 少2位
            GlobalReader.Position = endPosition;
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }
        internal JObject ReadTypeToolObjectSetting(long dataLength, long? startPosition = null)
            {
            GlobalReader.Position = startPosition ?? GlobalReader.Position;
            var endPosition = GlobalReader.Position + dataLength;
            JObject props = new()
                {
                ["StartPosition"] = GlobalReader.Position,
                };
            _ = GlobalReader.VerifyIntIs<short>(1);
            props["Transforms"] = new JArray(GlobalReader.ReadDoubles(6));
            props["TextVersion"] = GlobalReader.ReadInt16();
            props["Text"] = GlobalReader.ReadDescriptor();//StructureReader.ReadDescriptor(GlobalReader);
            props["WarpVersion"] = GlobalReader.ReadInt16();
            props["Warp"] = GlobalReader.ReadDescriptor();//StructureReader.ReadDescriptor(GlobalReader);
            props["Bounds"] = new JArray(GlobalReader.ReadDoubles(2));
            props["EndPosition"] = endPosition;
            Debug.Assert(GlobalReader.Position == endPosition);
            return props;
            }

        internal JObject ReadSmartObjectLayerData(long dataLength, long? startPosition = null) => GlobalReader.ReadPlacedLayerData(dataLength, startPosition);
        }
    }