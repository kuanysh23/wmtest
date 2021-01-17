using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficLight.Controllers;

namespace TrafficLight.Models
{
    public class SeqData
    {
        public List<int> StartNumbers { get; set; }
        public List<int> CurrentNumbers { get; set; }
        public List<StringBuilder> BadSectionsStatuses { get; set; }
        public List<List<StringBuilder>> BadSectionsStartNumbers { get; set; }
        public string LastColor { get; set; }
        public Observation LastObservation { get; set; }

        public SeqData()
        {
            StartNumbers = new List<int>();
            CurrentNumbers = new List<int>();

            //считаем сначала что все секции рабочие
            BadSectionsStatuses = new List<StringBuilder>();
            BadSectionsStartNumbers = new List<List<StringBuilder>>();

            for (int i = 0; i < ObservationController.N; i++)
                BadSectionsStatuses.Add(new StringBuilder("       "));//пробел - статус неизвестен, 0 - рабочая, 1 - нерабочая

            LastColor = "green";

            LastObservation = new Observation();
        }
    }
}
