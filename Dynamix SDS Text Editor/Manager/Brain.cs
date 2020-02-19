namespace Manager
{
    public class Brain
    {
        public FileFormat.Chunks.SDS SDSOpened { get; set; }

        public Brain(FileFormat.Chunks.SDS sdsOpened)
        {
            SDSOpened = sdsOpened;
        }
    }
}
