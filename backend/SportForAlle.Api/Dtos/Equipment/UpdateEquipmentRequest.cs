using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Equipment;

public record UpdateEquipmentRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(global::SportForAlle.Api.Models.Equipment.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Kategori er påkrevd.")]
    public required Guid CategoryId { get; init; }

    [Required(ErrorMessage = "Serienummer er påkrevd.")]
    [MaxLength(global::SportForAlle.Api.Models.Equipment.SerialNumberMaxLength, ErrorMessage = "Serienummer er for langt.")]
    public required string SerialNumber { get; init; }

    /// <summary>
    /// Status is not editable here - it is owned by the Utstyrstatus state
    /// machine, see Equipment.ChangeCondition.
    /// </summary>
    [Required(ErrorMessage = "Tilstand er påkrevd.")]
    public required EquipmentCondition Condition { get; init; }
}

