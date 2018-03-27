namespace StreamingServices.Models
{
    public class Channel
    {
        public int Id => id;
        private int id;

        public string Name => name;
        private string name;

        public Channel(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
