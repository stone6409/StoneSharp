/// <summary>
/// 技能匹配结果
/// </summary>

namespace StoneSharp.Core.Skills
{
    public class MatchResult
    {
        public Skill Skill { get; set; }
        public int Score { get; set; }
        public string Reason { get; set; }
        public bool IsExactMatch { get; set; }
    }
}
