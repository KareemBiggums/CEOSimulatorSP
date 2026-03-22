namespace ExecutiveTycoon.Models;

public sealed class DirectiveState
{
    public DirectiveType Type { get; set; }
    public int RemainingBusinessDays { get; set; }
    public float Intensity { get; set; }
}
