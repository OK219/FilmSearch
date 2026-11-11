namespace FilmSearchApi.Filters;

public class FailureSettings
{
    public bool Enabled { get; set; }
    public double ErrorChance { get; set; }
    public int Latency { get; set; }
}
