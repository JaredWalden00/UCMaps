namespace UCMapsShared.Models.DTO
{
    public class PostMarkerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int UserId { get; set; }
        public int StillThereCount { get; set; } = 0;
        public int NotThereCount { get; set; } = 0;
    }
}
