using System;

namespace AirService.Web.Helpers
{
    [Serializable]
    public class FileUploadResult
    {
        public bool success { get; set; }
        public string url { get; set; }
        public int dataIndex { get; set; }
    }
}
