
using System.Text.Json.Serialization;
using UCMapsShared.Models;

public class UCMarker
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public UserSession User { get; set; }
    public int StillThereCount { get; set; } = 0;
    public int NotThereCount { get; set; } = 0;
}
