namespace WhoAmI.Models;

/// <summary>
/// 16 Personalities / MBTI type.
/// </summary>
public enum MBTIType
{
    INTJ, // Architect
    INTP, // Logician
    ENTJ, // Commander
    ENTP, // Debater
    INFJ, // Advocate
    INFP, // Mediator
    ENFJ, // Protagonist
    ENFP, // Campaigner
    ISTJ, // Logistician
    ISFJ, // Defender
    ESTJ, // Executive
    ESFJ, // Consul
    ISTP, // Virtuoso
    ISFP, // Adventurer
    ESTP, // Entrepreneur
    ESFP  // Entertainer
}

/// <summary>
/// Represents a 16 Personalities type with nickname and description.
/// </summary>
public record MBTIPersonality(
    MBTIType Type,
    string Nickname,
    string Description
)
{
    public static readonly Dictionary<MBTIType, MBTIPersonality> Definitions = new()
    {
        [MBTIType.INTJ] = new(MBTIType.INTJ, "Architect", "Strategic, analytical, independent thinker"),
        [MBTIType.INTP] = new(MBTIType.INTP, "Logician", "Innovative inventor with thirst for knowledge"),
        [MBTIType.ENTJ] = new(MBTIType.ENTJ, "Commander", "Bold, imaginative, strong-willed leader"),
        [MBTIType.ENTP] = new(MBTIType.ENTP, "Debater", "Smart, curious thinker who loves intellectual challenge"),
        [MBTIType.INFJ] = new(MBTIType.INFJ, "Advocate", "Quiet, mystical, inspiring idealist"),
        [MBTIType.INFP] = new(MBTIType.INFP, "Mediator", "Poetic, kind, altruistic, always helping"),
        [MBTIType.ENFJ] = new(MBTIType.ENFJ, "Protagonist", "Charismatic, inspiring leader who motivates"),
        [MBTIType.ENFP] = new(MBTIType.ENFP, "Campaigner", "Enthusiastic, creative, sociable free spirit"),
        [MBTIType.ISTJ] = new(MBTIType.ISTJ, "Logistician", "Practical, fact-minded, reliable individual"),
        [MBTIType.ISFJ] = new(MBTIType.ISFJ, "Defender", "Dedicated, warm protector, always ready to defend"),
        [MBTIType.ESTJ] = new(MBTIType.ESTJ, "Executive", "Administrator managing things and people"),
        [MBTIType.ESFJ] = new(MBTIType.ESFJ, "Consul", "Caring, social, popular person, always eager to help"),
        [MBTIType.ISTP] = new(MBTIType.ISTP, "Virtuoso", "Bold, practical experimenter and master of tools"),
        [MBTIType.ISFP] = new(MBTIType.ISFP, "Adventurer", "Flexible, charming artist ready to explore"),
        [MBTIType.ESTP] = new(MBTIType.ESTP, "Entrepreneur", "Smart, energetic, perceptive who lives on the edge"),
        [MBTIType.ESFP] = new(MBTIType.ESFP, "Entertainer", "Spontaneous, energetic, enthusiastic entertainer")
    };
}
