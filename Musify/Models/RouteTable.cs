using SQLite.Net.Attributes;

namespace Musify.Models
{
    public class RouteTable
    {
        [PrimaryKey]
        public int Id { get; set; }
      
        public int RouteId { get; set; }

        public int Order { get; set; }

        public string DisplayName { get; set; }
    }
}
