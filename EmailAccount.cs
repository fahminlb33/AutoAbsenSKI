using Newtonsoft.Json;

namespace AutoAbsenSKI
{
    public class EmailAccount
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
    }
}
