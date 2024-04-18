namespace Pixpil.Assets;

public record struct DayProgress() {
	
	public int Day = 0;

	/// <summary>
	/// Range of 0 to 1 of a Day.
	/// </summary>
	public float DayPercentile = 0;

}
