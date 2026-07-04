namespace OpdManager.Domain.Entities;

/// <summary>
/// Key-value settings stored in the database.
/// </summary>
public class AppSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
