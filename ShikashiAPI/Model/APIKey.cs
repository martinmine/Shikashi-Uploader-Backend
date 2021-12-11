namespace ShikashiAPI.Model
{
    public class APIKey
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public long ExpirationTime { get; set; }

        public User User { get; set; }

        public string Compose()
        {
            return $"{Id}-{Identifier}";
        }
    }
}
