using Minokori.Media.Photoshop.Interfaces;
using Minokori.Media.Photoshop.Specifications;

namespace Minokori.Media.Photoshop.Extensions;

internal static class PsdUtility
    {
    extension(string text)
        {
        /// <summary>
        /// 快速把读取到的原始字符串转换为 <see cref="BlendMode"/> 枚举。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 不应在 <see cref="PhotoshopBinaryReader.ReadAsBlendMode"/> 以外使用此方法。
        /// </remarks>
        internal BlendMode ToBlendMode()
            {
            return text.Trim() switch
                {
                    "pass" => BlendMode.PassThrough,
                    "norm" => BlendMode.Normal,
                    "diss" => BlendMode.Dissolve,
                    "dark" => BlendMode.Darken,
                    "mul" => BlendMode.Multiply,
                    "idiv" => BlendMode.ColorBurn,
                    "lbrn" => BlendMode.LinearBurn,
                    "dkCl" => BlendMode.DarkerColor,
                    "lite" => BlendMode.Lighten,
                    "scrn" => BlendMode.Screen,
                    "div" => BlendMode.ColorDodge,
                    "lddg" => BlendMode.LinearDodge,
                    "lgCl" => BlendMode.LighterColor,
                    "over" => BlendMode.Overlay,
                    "sLit" => BlendMode.SoftLight,
                    "hLit" => BlendMode.HardLight,
                    "vLit" => BlendMode.VividLight,
                    "lLit" => BlendMode.LinearLight,
                    "pLit" => BlendMode.PinLight,
                    "hMix" => BlendMode.HardMix,
                    "diff" => BlendMode.Difference,
                    "smud" => BlendMode.Exclusion,
                    "fsub" => BlendMode.Subtract,
                    "fdiv" => BlendMode.Divide,
                    "hue" => BlendMode.Hue,
                    "sat" => BlendMode.Saturation,
                    "colr" => BlendMode.Color,
                    "lum" => BlendMode.Luminosity,
                    _ => BlendMode.Normal,
                    };
            }
        /// <summary>
        /// 快速把读取到的原始字符串转换为 <see cref="UnitType"/> 枚举。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 不应在 <see cref="StructureReader"/> 以外使用此方法。
        /// </remarks>
        internal UnitType ToUnitType()
            {
            return text switch
                {
                    "#Ang" => UnitType.Angle,
                    "#Rsl" => UnitType.Density,
                    "#Rlt" => UnitType.Distance,
                    "#Nne" => UnitType.None,
                    "#Prc" => UnitType.Percent,
                    "#Pxl" => UnitType.Pixels,
                    "#Pnt" => UnitType.Points,
                    "#Mlm" => UnitType.Millimeters,
                    _ => throw new NotSupportedException($"{text} 目前还不支持."),
                    };
            }
        }

    extension(int width)
        {
        /// <summary>
        /// 根据位深度决定一行需要多少字节
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        internal int ToPitch(int depth)
            {
            return depth switch
                {
                    1 => width, //NOT Sure
                    8 => width,
                    16 => width * 2,
                    _ => throw new NotSupportedException(),
                    };
            }
        }

    extension(PsdLayer[] layers)
        {
        internal PsdLayer[] BuildLayerTreeAndComputeBounds(PsdLayer? parent = null)
            {
            Stack<PsdLayer?> stack = new();
            List<PsdLayer> rootLayers = [];
            Dictionary<PsdLayer, List<PsdLayer>> layerToChilds = [];

            foreach (var layer in layers.Reverse())
                {
                if (layer.SectionType == SectionType.Divider)
                    {
                    parent = stack.Pop();
                    continue;
                    }

                if (parent != null)
                    {
                    if (layerToChilds.ContainsKey(parent) == false)
                        layerToChilds.Add(parent, []);

                    var children = layerToChilds[parent];
                    children.Insert(0, layer);
                    layer.Parent = parent;
                    }
                else
                    {
                    rootLayers.Insert(0, layer);
                    }

                if (layer.SectionType is SectionType.Open or SectionType.Closed)
                    {
                    stack.Push(parent);
                    parent = layer;
                    }
                }

            foreach (var item in layerToChilds)
                {
                item.Key.Childs = [.. item.Value];
                }


            foreach (var item in rootLayers.SelectMany(item => ((IPsdLayer)item).Descendants()).Cast<PsdLayer>().Reverse())
                {
                item.ComputeBounds();
                }

            return [.. rootLayers];


            }
        }

    extension(PsdLayer layer)
        {
        /// <summary>
        /// 计算边距(TOP, Bottom, Left, right)
        /// </summary>
        internal void ComputeBounds()
            {
            if (layer.SectionType is not SectionType.Open and not SectionType.Closed)
                {
                return;
                }
            //整个图层组的边距
            var left = int.MaxValue;
            var top = int.MaxValue;
            var right = int.MinValue;
            var bottom = int.MinValue;

            var isSet = false;

            foreach (
                var childLayer in ((IPsdLayer)layer)
                    .Descendants()
                    .Cast<PsdLayer>()
                    .Where(i => i != layer || !i.HasImage)
            )
                {
                switch (childLayer.Records.GetValue<double[]>(["Resources.PlacedLayer.Transformation"]))
                    {
                    case null:
                        {
                        left = Math.Min(childLayer.Left, left);
                        top = Math.Min(childLayer.Top, top);
                        right = Math.Max(childLayer.Right, right);
                        bottom = Math.Max(childLayer.Bottom, bottom);
                        break;
                        }
                    case double[] transforms:
                        {
                        var xx = transforms.Where((i, idx) => idx % 2 == 0).OrderBy(i => i).Select(i => (int)Math.Ceiling(i));
                        var yy = transforms.Where((i, idx) => idx % 2 == 1).OrderBy(i => i).Select(i => (int)Math.Ceiling(i));
                        left = Math.Min(xx.First(), left);
                        top = Math.Min(xx.Last(), top);
                        right = Math.Max(yy.First(), right);
                        bottom = Math.Max(yy.Last(), bottom);
                        break;
                        }
                    }

                isSet = true;
                }

            if (isSet == false)
                return;

            layer.Left = left;
            layer.Top = top;
            layer.Right = right;
            layer.Bottom = bottom;
            }
        }
    }
