using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs;

public class MoodUpdateRequest
{
    [Required(ErrorMessage = "Mood is required")]
    [EnumDataType(typeof(Mood), ErrorMessage = "Invalid mood value")]
    public Mood Mood { get; set; }
}
