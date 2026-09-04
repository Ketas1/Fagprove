using System.ComponentModel.DataAnnotations;

namespace SportForAlle.Api.Dtos.Equipment;

public record UpdateEquipmentRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(global::SportForAlle.Api.Models.Equipment.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Kategori er påkrevd.")]
    public required Guid CategoryId { get; init; }
}
