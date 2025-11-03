using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Auth
{
    public record Resend2FaRequest(
        [Required] string SessionId
    );
}