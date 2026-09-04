using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Equipment;

public record CreateEquipmentRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(global::SportForAlle.Api.Models.Equipment.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Serienummer er påkrevd.")]
    [MaxLength(global::SportForAlle.Api.Models.Equipment.SerialNumberMaxLength, ErrorMessage = "Serienummer er for langt.")]
    public required string SerialNumber { get; init; }

    [Required(ErrorMessage = "Kategori er påkrevd.")]
    public required Guid CategoryId { get; init; }

    /// <summary>Defaults to <see cref="EquipmentCondition.New"/> if omitted.</summary>
    public EquipmentCondition Condition { get; init; } = EquipmentCondition.New;
}
