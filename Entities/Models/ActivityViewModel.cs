namespace Entities.Models
{
    public class ActivityViewModel
    {
        public string Type { get; set; } // Order, Product, User
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;

                if (timeSpan.TotalMinutes < 1)
                    return "Az önce";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} dakika önce";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} saat önce";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} gün önce";

                return CreatedAt.ToString("dd MMM yyyy");
            }
        }
    }
}