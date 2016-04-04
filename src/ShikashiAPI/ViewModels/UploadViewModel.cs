using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ShikashiAPI.ViewModels
{
    public class UploadViewModel
    {
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public DateTime Uploaded { get; set; }
        public int ViewCount { get; set; }
        public long FileSize { get; set; }
        public string Key { get; set; }
    }
}
