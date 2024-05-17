using GoogleMapsComponents.Maps;
using OneOf;
using System.Text.Json.Serialization;

public class UCMarker
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    //public OneOf<string, MarkerLabel>? Label { get; set; }
}
