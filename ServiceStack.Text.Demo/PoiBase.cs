using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaPoco;

namespace ServiceStack.Text.Demo
{
    public class PoiBase
    {
        [Column]
        public int id { get; set; }
        [Column]
        public string name { get; set; }
        [Column]
        public string address { get; set; }
        [Column]
        public double x { get; set; }
        [Column]
        public double y { get; set; }
        [Column]
        public string uid { get; set; }
        [Column]
        public string type { get; set; }
        [Column]
        public DateTime time { get; set; }
        [Column]
        public int taskid { get; set; }
    }
}
