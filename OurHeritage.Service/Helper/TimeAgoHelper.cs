namespace OurHeritage.Service.Helper
{
    public static class TimeAgoHelper
    {
        public static string GetTimeAgo(DateTime dateCreated)
        {
            TimeSpan timeDifference = DateTime.UtcNow - dateCreated;

            if (timeDifference.TotalSeconds < 60)
                return "Just now";
            if (timeDifference.TotalMinutes < 60)
                return $"{(int)timeDifference.TotalMinutes} minutes ago";
            if (timeDifference.TotalHours < 24)
                return $"{(int)timeDifference.TotalHours} hours ago";
            if (timeDifference.TotalDays == 1)
                return "Yesterday";
            if (timeDifference.TotalDays < 30)
                return $"{(int)timeDifference.TotalDays} days ago";
            if (timeDifference.TotalDays < 365)
                return $"{(int)(timeDifference.TotalDays / 30)} months ago";

            return $"{(int)(timeDifference.TotalDays / 365)} years ago";
        }
    }

}
