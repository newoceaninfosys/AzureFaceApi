using System.Collections.Generic;

namespace Face.Service.Models
{
    public class DetectResult
    {
        public string FaceId { get; set; }
        public FaceRectangle FaceRectangle { get; set; }
        public FaceAttributes FaceAttributes { get; set; }
    }

    public class FaceRectangle
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
    }

    public class FaceAttributes
    {
        public double Age { get; set; }
        public string Gender { get; set; }
        public double Smile { get; set; }
        public FacialHair FacialHair { get; set; }
        public string Glasses { get; set; }
        public Emotion Emotion { get; set; }
        public Hair Hair { get; set; }
        public Makeup Makeup { get; set; }
        public Occlusion Occlusion { get; set; }
        public List<Accessory> Accessories { get; set; }
        public Blur Blur { get; set; }
        public Exposure Exposure { get; set; }
        public Noise Noise { get; set; }
        public HeadPose HeadPose { get; set; }
    }

    public class FacialHair
    {
        public double Moustache { get; set; }
        public double Beard { get; set; }
        public double Sideburns { get; set; }
    }

    public class HeadPose
    {
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }

    }

    public class Emotion
    {
        public double Anger { get; set; }
        public double Contempt { get; set; }
        public double Disgust { get; set; }
        public double Fear { get; set; }
        public double Happiness { get; set; }
        public double Neutral { get; set; }
        public double Sadness { get; set; }
        public double Surprise { get; set; }
    }

    public class Hair
    {
        public double Bald { get; set; }
        public bool Invisible { get; set; }
        public List<HairColor> HairColor { get; set; }
    }

    public class HairColor
    {
        public string Color { get; set; }
        public double Confidence { get; set; }
    }

    public class Makeup
    {
        public bool EyeMakeup { get; set; }
        public bool LipMakeup { get; set; }
    }

    public class Occlusion
    {
        public bool ForeheadOccluded { get; set; }
        public bool EyeOccluded { get; set; }
        public bool MouthOccluded { get; set; }
    }

    public class Accessory
    {
        public string Type { get; set; }
        public double Confidence { get; set; }
    }

    public class Blur
    {
        public string BlurLevel { get; set; }
        public double Value { get; set; }
    }

    public class Exposure
    {
        public string ExposureLevel { get; set; }
        public double Value { get; set; }
    }

    public class Noise
    {
        public string NoiseLevel { get; set; }
        public double Value { get; set; }
    }
}
