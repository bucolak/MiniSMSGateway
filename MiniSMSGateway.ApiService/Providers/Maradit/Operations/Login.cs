using Maradit.Types;

namespace Maradit.Operations
{
    /// <summary>
    /// In
    /// </summary>
    public class Login
    {
        public Credential Credential { get; set; }
    }

    /// <summary>
    /// Response
    /// </summary>
    public class LoginResponse
    {
        public LoginInfo Response { get; set; }
    }

    /// <summary>
    /// Out
    /// </summary>
    public class LoginInfo
    {
        public LoginInfo()
        {
            Identifier = new Identifier();
            Status = new Status();
        }

        public Identifier Identifier { get; set; }

        public Status Status { get; set; }
    }
}