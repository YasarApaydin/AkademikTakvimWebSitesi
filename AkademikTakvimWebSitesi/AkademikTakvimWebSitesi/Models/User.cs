namespace AkademikTakvimWebSitesi.Models
{
    public class User
    {
        public int Id { get; set; } 
        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 

        public string Password { get; set; } = string.Empty;
        public string GsmNo { get; set; } = string.Empty;

        public ICollection<Event>? Events { get; set; }
    }
}
