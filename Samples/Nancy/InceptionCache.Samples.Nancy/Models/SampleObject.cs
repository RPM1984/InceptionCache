using System;
using ProtoBuf;

namespace InceptionCache.Samples.Nancy.Models
{
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SampleObject
    {
        public SampleObject(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}