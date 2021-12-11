using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Model
{
    public class UploadedContent
    {
        public int Id { get; set; }
        public string MimeType { get; set; }
        public string UploaderIP { get; set; }
        public string FileName { get; set; }
        public DateTime Uploaded { get; set; }
        public User Owner { get; set; }
        public long FileSize { get; set; }
    }
}
