using System;
using Microsoft.VisualBasic;

namespace ImageOverlayFunctionApp
{
    public class ImageOverlay
    {
        public string ItemPk { get; set; }
        public string ItemLongDescription { get; set; }
        public string StartDate { get; set; }
        public string LocationDescription { get; set; }
        public string ItemClassDescription { get; set; }
        public string SourceItemPk { get; set; }
        public int SrcItemPkCount { get; set; }
    }
}
