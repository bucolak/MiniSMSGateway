namespace Maradit.Types
{
    public class Status
    {
        public Status()
        {
            Code = -1;
            Description = "Client error";
        }

        public int Code { get; set; }

        public string Description { get; set; }
    }
}