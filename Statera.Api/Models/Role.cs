namespace Statera.Api.Models
{
    public class Role
    {
        public short Id { get; set; }            // smallint is plenty
        public string Position { get; set; } = "";   // e.g., "RN", "LPN", "CNA", "Scheduler"
        public bool IsClinical { get; set; }     // quick filter for staffing logic

        public List<Staff> Staff { get; set; } = new(); // backref
    }
}
