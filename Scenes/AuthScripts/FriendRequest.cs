using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FriendRequest
{
	[JsonPropertyName("fromUserId")]
	public string FromUserId { get; set; }

	[JsonPropertyName("fromUsername")]
	public string FromUsername { get; set; }

	[JsonPropertyName("toUserId")]
	public string ToUserId { get; set; }

	[JsonPropertyName("toUsername")]
	public string ToUsername { get; set; }

	[JsonPropertyName("status")]
	public string Status { get; set; }

	[JsonPropertyName("timestamp")]
	public long Timestamp { get; set; }

	public static FriendRequest Create(string fromUserId, string fromUsername, string toUserId, string toUsername)
	{
		return new FriendRequest
		{
			FromUserId = fromUserId,
			FromUsername = fromUsername,
			ToUserId = toUserId,
			ToUsername = toUsername,
			Status = "pending",
			Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
		};
	}
}