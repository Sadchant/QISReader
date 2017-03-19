using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    [DataContract]
    [KnownTypeAttribute(typeof(FachInhalt))]
    public class Fach
    {
        [DataMember]
        public int? Id { get; set; }
        [DataMember]
        public string FachName { get; set; }
        [DataMember]
        public float? Note { get; set; }
        [DataMember]
        public float? Cp { get; set; }
        [DataMember]
        public bool? Bestanden { get; set; }

    }

    [DataContract]
    [KnownTypeAttribute(typeof(FachInhalt))]
    public class FachInhalt : Fach
    {
        [DataMember]
        public string Semester { get; set; }
        [DataMember]
        public int? Versuch { get; set; }
    }
}
