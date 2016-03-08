using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ImageRequest
    {
        public string tmpPath { get; set; }
        public string url { get; set; }
        public int? position { get; set; }
        public string ImageName { get; set; }
        public int? ImageId;
        public string EnTh { get; set; }
    }
}
