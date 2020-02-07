namespace FileUploadApi.ApiServices
{
    public class FileTransferObject
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public string UploadDate { get; set; }

        public int NumOfAllRecords { get; set; }

        public int NumOfValidRecords { get; set; }
    }
}