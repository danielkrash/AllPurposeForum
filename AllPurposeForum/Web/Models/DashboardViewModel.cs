namespace AllPurposeForum.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTopics { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }

        // For chart data, we can pass simple arrays or lists
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartData { get; set; } = new List<int>();
    }
}
