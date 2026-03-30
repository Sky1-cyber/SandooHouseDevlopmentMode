using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sandoohouse.Models;

public class LoginAttempt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string IPAddress { get; set; }

    public int AttemptCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
}