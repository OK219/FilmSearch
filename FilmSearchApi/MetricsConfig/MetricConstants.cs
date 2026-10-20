namespace FilmSearchApi.MetricsConfig;

public class MetricConstants
{
    public const string UserFilmActivityCounterName = "UserFilmActivity";
    public static readonly string[] UserFilmActivityCounterLabels = ["activity"];
    
    public const string FilmOrderFrequencyCounterName = "FilmOrderFrequency";
    public static readonly string[] FilmOrderFrequencyCounterLabels = ["order"];
    
    public const string UserAuthActivityCounterName = "UserAuthActivity";
    public static readonly string[] UserAuthActivityCounterLabels = ["activity"];
    
    public const string FilmSearchHistogramName = "FilmSearchHistogram";
    public static readonly double[] FilmSearchHistogramBuckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];
    
    //Film Action names
    public const string FilmAdd = "FilmAdd";
    public const string FilmDelete = "FilmDelete";
    public const string FilmDetail = "FilmDetail";
    public const string ReviewAdd = "ReviewAdd";
    public const string ReviewDelete = "ReviewDelete";
    public const string ReviewUpdate = "ReviewUpdate";
    
    //Auth Action names
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string Register = "Register";
}