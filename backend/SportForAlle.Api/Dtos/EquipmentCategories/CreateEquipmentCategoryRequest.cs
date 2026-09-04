using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.EquipmentCategories;

public record CreateEquipmentCategoryRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(EquipmentCategory.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }
}
