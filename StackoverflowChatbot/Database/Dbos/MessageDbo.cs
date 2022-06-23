using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace StackoverflowChatbot.Database.Dbos;

[Index(nameof(OriginMessageId), nameof(OriginPlatform))]
[Index(nameof(DestinationMessageId), nameof(DestinationPlatform))]
public class MessageDbo
{
	[Key]
	public int InternalId { get; set; }

	[Required]
	public MessageOriginDestination OriginPlatform { get; set; }

	[Required]
	public string OriginMessageId { get; set; }

	[Required]
	public string OriginAuthor { get; set; }

	public string MessageBody { get; set; }

	//May contain discord attachment info, for example
	public string? MessageAdditionalDataJson { get; set; }

	public MessageOriginDestination DestinationPlatform { get; set; } = MessageOriginDestination.Unspecified;

	public string? DestinationMessageId { get; set; }

	public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

	public bool IsEdited { get; set; } 

}