﻿namespace Colsp.Model.Requests
{
    public class SEORequest
    {
        public string MetaTitleEn           { get; set; }
        public string MetaTitleTh           { get; set; }
        public string MetaDescriptionEn     { get; set; }
        public string MetaDescriptionTh     { get; set; }
        public string MetaKeywordEn         { get; set; }
        public string MetaKeywordTh         { get; set; }
        public string ProductUrlKeyTh       { get; set; }
        public string ProductUrlKeyEn       { get; set; }
        public int    ProductBoostingWeight { get; set; }

        public SEORequest()
        {
            MetaTitleEn           = string.Empty;
            MetaTitleTh           = string.Empty;
            MetaDescriptionEn     = string.Empty;
            MetaDescriptionTh     = string.Empty;
            MetaKeywordEn         = string.Empty;
            MetaKeywordTh         = string.Empty;
            ProductUrlKeyTh       = string.Empty;
            ProductUrlKeyEn       = string.Empty;
            ProductBoostingWeight = 0;
        }
    }
}
