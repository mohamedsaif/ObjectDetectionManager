using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace CognitivePipeline.Functions.Models
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstructionFlag
    {
        AnalyzeImage,
        AnalyzeText,
        Thumbnail,
        FaceAuthentication,
        ShelfCompliance,
        GenericObjectDetection,
        CustomObjectDetection
    }
}