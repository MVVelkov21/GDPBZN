using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class ChatMessage
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    public int? SenderEmployeeId { get; set; }
    public Employee? SenderEmployee { get; set; }

    [MaxLength(2000)]
    public string Text { get; set; } = default!;

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

    public string? AttachmentUrl { get; set; } // снимка/файл (ако имаш storage)
}

public class MessageTemplate
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Title { get; set; } = default!;

    [MaxLength(400)]
    public string Text { get; set; } = default!;
}