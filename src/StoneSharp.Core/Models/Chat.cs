using System;

namespace StoneSharp.Core.Models
{
    public class Chat
    {
        public Chat()
        {

        }

        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime Time { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Chat);
        }

        public bool Equals(Chat other)
        {
            if (other is null)
                return false;

            return Id == other.Id &&
                   Name == other.Name &&
                   Time == other.Time;
        }
    }
}
