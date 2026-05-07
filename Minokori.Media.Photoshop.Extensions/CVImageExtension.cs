using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Minokori.Media.Photoshop.Extensions;

public static class CVImageExtension
    {
    extension(Image<Bgra, byte> self)
        {
        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <exception cref="CvException">
        /// 若在 Psd 文件中, 子图层超过了画布边缘, 会导致报错
        /// </exception>
        public Image<Bgra, byte> AddImage(Image<Bgra, byte> other, int left, int top)
            {
            Emgu.CV.Structure.Range rowRange = new(left, left + other.Width);
            Emgu.CV.Structure.Range colRange = new(top, top + other.Height);
            Mat roi = new(self.Mat, rowRange, colRange);
            var size = roi.Size;
            // 拆分通道
            var backgroundMat = roi.Split();
            var otherMat = other.Mat.Split();
            // 用于存放各通道计算结果
            Mat[] outputChannels =
            [
                new Mat(size, DepthType.Cv16U, 1),
                new Mat(size, DepthType.Cv16U, 1),
                new Mat(size, DepthType.Cv16U, 1),
                new Mat(size, DepthType.Cv16U, 1),
            ];

            //临时变量
            Mat white = new(size, DepthType.Cv16U, 1);
            white.SetTo(new MCvScalar(256));
            Mat temp = new(size, DepthType.Cv16U, 1);
            Mat temp2 = new(size, DepthType.Cv16U, 1);

            CvInvoke.Multiply(backgroundMat[3], otherMat[3], temp, dtype: DepthType.Cv16U);
            CvInvoke.Divide(temp, white, temp, dtype: DepthType.Cv16U);

            var a1 = backgroundMat[3];
            Mat a2 = new(size, DepthType.Cv16U, 1);
            Mat a = new(size, DepthType.Cv16U, 1);
            CvInvoke.Subtract(otherMat[3], temp, a2, dtype: DepthType.Cv16U);
            CvInvoke.Add(a1, a2, a, dtype: DepthType.Cv16U);

            // B G R 通道计算
            for (var i = 0; i < 3; i++)
                {
                CvInvoke.Multiply(backgroundMat[i], a1, temp, dtype: DepthType.Cv16U);
                CvInvoke.Multiply(otherMat[i], a2, temp2, dtype: DepthType.Cv16U);
                CvInvoke.Add(temp, temp2, outputChannels[i], dtype: DepthType.Cv16U);
                CvInvoke.Divide(outputChannels[i], a, outputChannels[i], dtype: DepthType.Cv16U);
                }

            outputChannels[3] = a;
            // 通道合并输出
            Mat output = new(size, DepthType.Cv16U, 4);

            CvInvoke.Merge(new VectorOfMat(outputChannels), output);

            // 释放资源
            temp.Dispose();
            temp2.Dispose();
            white.Dispose();
            a.Dispose();
            a1.Dispose();
            a2.Dispose();

            for (var i = 0; i < 4; i++)
                {
                backgroundMat[i].Dispose();
                otherMat[i].Dispose();
                outputChannels[i].Dispose();
                }

            output.ConvertTo(output, DepthType.Cv8U);
            output.CopyTo(roi);
            output.Dispose();

            return self;
            }
        }
    }
