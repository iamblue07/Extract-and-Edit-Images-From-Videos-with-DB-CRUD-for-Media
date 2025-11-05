using System;

namespace BDM_P.Models
{
    public class SemanticSearchResult
    {
        public int Id { get; set; }
        public string ImageType { get; set; }
        public int? VideoId { get; set; }
        public double SimilarityScore { get; set; }
        public byte[] ImageData { get; set; }
    }
}