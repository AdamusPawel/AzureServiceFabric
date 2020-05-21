using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Communication
{
    [DataContract] // allows us to serialize our data in reliable state
    public class Product
    {
        [DataMember] // required to make whole object serializable
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }

        // we want serializable objects because:
        // - they will be written on disk
        // - they will be written on memory
        // - service fabric reliable state & connection -> to let them be replicated from diff instances of our service
        // = we provide high reliability because of all above
    }
}
