namespace DevPocketCMS.Core.Models;

public class DashboardStats
{
    public int TotalArticles       { get; set; }
    public int ReadArticles        { get; set; }
    public int UnreadArticles      { get; set; }
    public int FavoriteArticles    { get; set; }
    public int TotalLists          { get; set; }
    public int TotalTags           { get; set; }
    public int PocketSavedArticles { get; set; }

    public int             SavedStreak    { get; set; }
    public List<DayActivity> WeeklyActivity { get; set; } = new();

    public List<ListStat> ArticlesPerList { get; set; } = new();
    public List<TagStat>  TopTags        { get; set; } = new();
}

public class DayActivity
{
    public string DayLabel { get; set; } = string.Empty;
    public int    Count    { get; set; }
    public bool   IsToday  { get; set; }
}

public class ListStat
{
    public string ListName  { get; set; } = string.Empty;
    public string ListColor { get; set; } = "#4FFFB0";
    public int    Count     { get; set; }
}

public class TagStat
{
    public string TagName  { get; set; } = string.Empty;
    public string TagColor { get; set; } = "#8888BB";
    public int    Count    { get; set; }
}
