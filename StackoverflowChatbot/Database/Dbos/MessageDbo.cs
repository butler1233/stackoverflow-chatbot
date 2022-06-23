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

	/// <summary>
	/// The room ID on Stack, or the discord channel ID
	/// </summary>
	[Required] 
	public string OriginChannelId { get; set; }

	[Required]
	public string OriginAuthor { get; set; }

	public string MessageBody { get; set; }

	/// <summary>
	/// May contain discord attachment info, for example
	/// </summary>
	public string? MessageAdditionalDataJson { get; set; }

	[Required]
	public MessageOriginDestination DestinationPlatform { get; set; }

	[Required]
	public string DestinationMessageId { get; set; }

	[Required]
	public string DestinationChannelId { get; set; }

	public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

	public bool IsEdited { get; set; } 

}