using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Cloudsdale.FayeConnector;

namespace Cloudsdale.Models {
    [DataContract(Name="Avatar")]
    public class Avatar {
        [DataMember]
        public virtual Uri Normal { get; set; }
        [DataMember]
        public virtual Uri Mini { get; set; }
        [DataMember]
        public virtual Uri Thumb { get; set; }
        [DataMember]
        public virtual Uri Preview { get; set; }
        [DataMember]
        public virtual Uri Chat { get; set; }
    }
}
