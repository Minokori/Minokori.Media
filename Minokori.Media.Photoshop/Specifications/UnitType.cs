namespace Minokori.Media.Photoshop.Specifications;

/// <summary>
/// 单位浮动结构
/// </summary>
public enum UnitType
    {

    /// <summary>
    /// angle: base degrees
    /// </summary>
    Angle,

    /// <summary>
    /// density: base per inch
    /// </summary>
    Density,

    /// <summary>
    /// distance: base 72ppi
    /// </summary>
    Distance,

    /// <summary>
    /// none: coerced.
    /// </summary>
    None,

    /// <summary>
    /// percent: unit value
    /// </summary>
    Percent,

    /// <summary>
    /// pixels: tagged unit value
    /// </summary>
    Pixels,

    /// <summary>
    /// points: tagged unit value
    /// </summary>
    Points,

    /// <summary>
    /// : tagged unit value
    /// </summary>
    Millimeters,
    }
