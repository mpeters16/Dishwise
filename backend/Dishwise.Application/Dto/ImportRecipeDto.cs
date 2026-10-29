
namespace Dishwise.Application.Dto;
public class ImportRecipeDto
{
    public string Url { get; set; } = string.Empty;

    public Guid UserId { get; set; }= default!;

    public Guid CategoryId { get; set; }=   default!;
}