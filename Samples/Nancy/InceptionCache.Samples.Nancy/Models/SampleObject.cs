using System;

namespace InceptionCache.Samples.Nancy.Models
{
    [Serializable]
    public class SampleObject
    {
        public SampleObject(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}