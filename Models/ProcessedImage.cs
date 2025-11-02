using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BDM_P.Models
{
    public class ProcessedImage
    {
        public int Id { get; set; }
        // replaced VideoName with VideoId (foreign key to videos table)
        public int? VideoId { get; set; }
        public byte[] Img { get; set; }
    }
}