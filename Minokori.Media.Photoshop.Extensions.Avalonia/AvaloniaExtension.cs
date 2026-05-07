using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Minokori.Media.Photoshop.Interfaces;

namespace Minokori.Media.Photoshop.Extensions;

public static class AvaloniaExtension
    {
    extension(IPsdLayer layer)
        {
        /// <summary>
        ///
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        /// <remarks>
        /// see <see href="https://github.com/AvaloniaUI/Avalonia/discussions/9873"/>
        /// </remarks>
        public WriteableBitmap ToBitmap(WriteableBitmap? bitmap = null)
            {
            var ImageRawBytes = layer.MergeChannels();
            bitmap ??= new WriteableBitmap(
                new Avalonia.PixelSize(layer.Width, layer.Height),
                new Avalonia.Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Unpremul
            );
            using var frameBuffer = bitmap.Lock();
            Marshal.Copy(ImageRawBytes, 0, frameBuffer.Address, ImageRawBytes.Length);
            return bitmap;
            }
        }
    }
