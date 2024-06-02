using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla_VillaAPI.Models
{
    public class Villa
    {
        [Key]
        
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        
    }
}
