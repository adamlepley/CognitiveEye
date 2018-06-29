using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CongnitiveEye.Forms.Services
{
    public enum ModelType
    {
        General,
        Landscape,
        Retail
    }

    public struct ImageClassification
    {
        public ImageClassification(string tag, double probability)
        {
            Tag = tag;
            Probability = probability;
        }

        public string Tag { get; }
        public double Probability { get; }

        public override string ToString() => $"Tag={Tag}, Probability={Probability:N2}";
    }

    public class ImageClassifierException : Exception
    {
        public ImageClassifierException(string message) : base(message) { }
        public ImageClassifierException(string message, Exception innerException) : base(message, innerException) { }
    }



    public interface IImageClassifier
    {
        void Init(string modelUrl, ModelType modelType);
        Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream);
    }

}
