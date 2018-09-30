using StreamingServices.Chats.Abstractions;

namespace StreamingServices.Chats.Models
{
    public class Channel : IChannel
    {
        public int? Id => _id;
        private int? _id;

        public string Name => _name;
        private string _name;

        public Channel(int? id, string name)
        {
            _id = id;
            _name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var model = (Channel)obj;

            return (_id.Equals(model.Id) && _name.Equals(model.Name));
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode() + _name.GetHashCode();
        }
    }
}
