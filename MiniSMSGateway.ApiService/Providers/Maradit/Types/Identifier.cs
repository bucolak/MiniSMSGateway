namespace Maradit.Types
{
    public class Identifier
    {
        public Identifier()
        {
            OwnerId = -1;
        }

        public int Id { get; set; }

        public int UserId { get; set; }

        public int OwnerId { get; set; }
    }
}