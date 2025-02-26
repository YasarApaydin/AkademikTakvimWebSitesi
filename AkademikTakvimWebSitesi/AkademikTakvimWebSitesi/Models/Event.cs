﻿namespace AkademikTakvimWebSitesi.Models
{
    public class Event
    {


        public int Id { get; set; }
        public int YoneticiId { get; set; }
        public User Yonetici { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
    }
}
