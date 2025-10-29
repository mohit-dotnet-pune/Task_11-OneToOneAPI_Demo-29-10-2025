using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneToOneAPI_Demo.Models
{
    public class Passport
    {
        [Key,ForeignKey("Person")]
        public int PersonId { get; set; }
        public int PassportNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Person? Person { get; set; }
    }
}
