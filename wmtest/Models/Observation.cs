using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrafficLight.Models
{
    public class Observation
    {
        public string color { get; set; }
        public List<string> numbers { get; set; }

        public Observation()
        {
            numbers = new List<string>();
        }
    }

    public class ObservationWithSequence
    {
        public Observation observation { get; set; }
        public Guid sequence { get; set; }
    }
}
