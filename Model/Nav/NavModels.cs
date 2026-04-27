namespace FinanceTracker.Models.Nav
{
    public class NavItem
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}