using System;

namespace Network
{
    [Serializable]
    public class RoomProfile
    {
        public string name;

        public RoomProfile(string _name)
        {
            this.name = _name;
        }
    }
}