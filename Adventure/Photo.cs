using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    [DataContract]
    public class Photo
    {
        [DataMember(Name = "image")]
        public string Image { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
