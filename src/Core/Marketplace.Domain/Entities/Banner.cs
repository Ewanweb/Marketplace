namespace Marketplace.Domain.Entities
{
    public enum BannerPosition
    {
        MainSlider = 0,
        SecondaryBanner1 = 1,
        SecondaryBanner2 = 2
    }

    public class Banner
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string ImageUrl { get; private set; } = string.Empty;
        public string LinkUrl { get; private set; } = string.Empty;
        public BannerPosition Position { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public bool IsActive { get; private set; }

        private Banner() { }

        public static Banner Create(string title, string imageUrl, string linkUrl, BannerPosition position, DateTime? startDate, DateTime? endDate)
        {
            return new Banner
            {
                Id = Guid.NewGuid(),
                Title = title,
                ImageUrl = imageUrl,
                LinkUrl = linkUrl,
                Position = position,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true
            };
        }

        public void Update(string title, string imageUrl, string linkUrl, BannerPosition position, DateTime? startDate, DateTime? endDate)
        {
            Title = title;
            ImageUrl = imageUrl;
            LinkUrl = linkUrl;
            Position = position;
            StartDate = startDate;
            EndDate = endDate;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }
}
