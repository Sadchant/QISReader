using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QisReaderClassLibrary
{
    [DataContract]
    [KnownTypeAttribute(typeof(NotenData))]
    class NotenData
    {
        [DataMember]
        public int AnzahlEinträge { get; set; }
        [DataMember]
        public int AnzahlNoten { get; set; }
        [DataMember]
        public DateTime lastRefreshTime { get; set; }
    }
}
