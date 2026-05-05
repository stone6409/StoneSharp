using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StoneSharp.Core.Skills
{
    /// <summary>
    /// 技能匹配器 - 用于计算技能匹配分数的静态类
    /// </summary>
    public static class SkillMatcher
    {
        /// <summary>
        /// 计算用户输入与技能的匹配分数
        /// </summary>
        public static int CalculateMatchScore(string userInput, Skill skill)
        {
            if (string.IsNullOrWhiteSpace(userInput) || skill == null)
                return 0;

            var score = 0;
            var input = userInput.ToLowerInvariant().Trim();
            var skillName = skill.Name?.ToLowerInvariant() ?? string.Empty;

            // 1. 精确匹配技能名（最高优先级）
            if (input.Equals(skillName, StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }

            // 2. 技能名作为单词边界匹配
            if (Regex.IsMatch(input, $@"\b{Regex.Escape(skillName)}\b", RegexOptions.IgnoreCase))
            {
                score += 80;
            }

            // 3. 技能名包含在输入中
            if (input.Contains(skillName, StringComparison.OrdinalIgnoreCase))
            {
                score += 60;
            }

            // 4. 字符串相似度匹配
            var similarity = CalculateStringSimilarity(input, skillName);
            if (similarity > 0.7) // 相似度超过70%
            {
                score += (int)(similarity * 50);
            }

            // 5. 标签匹配
            if (skill.Tags != null && skill.Tags.Any())
            {
                score += CalculateTagMatchScore(input, skill.Tags);
            }

            // 6. 技能描述匹配
            if (!string.IsNullOrEmpty(skill.Description))
            {
                score += CalculateDescriptionMatchScore(input, skill.Description);
            }

            // 7. 类别匹配
            if (!string.IsNullOrEmpty(skill.Category))
            {
                score += CalculateCategoryMatchScore(input, skill.Category);
            }

            return score;
        }

        /// <summary>
        /// 计算标签匹配分数
        /// </summary>
        private static int CalculateTagMatchScore(string input, List<string> tags)
        {
            var score = 0;

            foreach (var tag in tags)
            {
                var tagLower = tag?.ToLowerInvariant() ?? string.Empty;
                if (string.IsNullOrEmpty(tagLower))
                    continue;

                // 标签精确匹配
                if (input.Equals(tagLower, StringComparison.OrdinalIgnoreCase))
                {
                    score += 70;
                }
                // 标签作为单词边界匹配
                else if (Regex.IsMatch(input, $@"\b{Regex.Escape(tagLower)}\b", RegexOptions.IgnoreCase))
                {
                    score += 50;
                }
                // 标签包含在输入中
                else if (input.Contains(tagLower, StringComparison.OrdinalIgnoreCase))
                {
                    score += 30;
                }
            }

            return score;
        }

        /// <summary>
        /// 计算描述匹配分数
        /// </summary>
        private static int CalculateDescriptionMatchScore(string input, string description)
        {
            var score = 0;
            var descriptionLower = description.ToLowerInvariant();

            // 检查描述中的关键词是否出现在输入中
            // 包含中英文标点符号作为分隔符
            var descriptionWords = descriptionLower
                .Split(new[] {
                    ' ', ',', '.', ';', '!', '?',    // 英文标点
                    '，', '。', '；', '！', '？',     // 中文标点
                    '、', '：', '「', '」', '《', '》', // 更多中文标点
                    '\n', '\r', '\t'                  // 换行和制表符
                }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .Distinct();

            foreach (var word in descriptionWords)
            {
                if (input.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    score += 30;
                }
            }

            return score;
        }

        /// <summary>
        /// 计算类别匹配分数
        /// </summary>
        private static int CalculateCategoryMatchScore(string input, string category)
        {
            var categoryLower = category.ToLowerInvariant();
            
            // 类别精确匹配
            if (input.Equals(categoryLower, StringComparison.OrdinalIgnoreCase))
            {
                return 50;
            }
            // 类别作为单词边界匹配
            else if (Regex.IsMatch(input, $@"\b{Regex.Escape(categoryLower)}\b", RegexOptions.IgnoreCase))
            {
                return 40;
            }
            // 类别包含在输入中
            else if (input.Contains(categoryLower, StringComparison.OrdinalIgnoreCase))
            {
                return 30;
            }

            return 0;
        }

        /// <summary>
        /// 计算字符串相似度（Levenshtein距离）
        /// </summary>
        private static double CalculateStringSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;

            s1 = s1.ToLowerInvariant();
            s2 = s2.ToLowerInvariant();

            int n = s1.Length;
            int m = s2.Length;

            // 如果字符串长度差异太大，直接返回0
            if (Math.Abs(n - m) > Math.Max(n, m) * 0.5)
                return 0;

            int[,] d = new int[n + 1, m + 1];

            // 初始化矩阵
            if (n == 0)
                return m == 0 ? 1.0 : 0.0;
            if (m == 0)
                return 0.0;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            // 计算Levenshtein距离
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            // 计算相似度
            int maxLength = Math.Max(n, m);
            if (maxLength == 0)
                return 1.0;

            double similarity = 1.0 - (double)d[n, m] / maxLength;
            return similarity;
        }

        /// <summary>
        /// 获取所有匹配的技能及其分数
        /// </summary>
        public static List<MatchResult> GetMatchedSkills(string userInput, List<Skill> skills)
        {
            if (string.IsNullOrWhiteSpace(userInput) || skills == null || !skills.Any())
                return new List<MatchResult>();

            var matchedSkills = new List<MatchResult>();

            foreach (var skill in skills)
            {
                var score = CalculateMatchScore(userInput, skill);
                if (score >= 60)
                {
                    matchedSkills.Add(new MatchResult
                    {
                        Skill = skill,
                        Score = score,
                        IsExactMatch = userInput.Trim().Equals(skill.Name, StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            return matchedSkills.OrderByDescending(x => x.Score).ToList();
        }

        /// <summary>
        /// 获取最佳匹配的技能
        /// </summary>
        public static Skill GetBestMatch(string userInput, List<Skill> skills)
        {
            var matchedSkills = GetMatchedSkills(userInput, skills);
            return matchedSkills.FirstOrDefault()?.Skill;
        }

        /// <summary>
        /// 获取最佳匹配的技能名称
        /// </summary>
        public static string GetBestMatchSkillName(string userInput, List<Skill> skills)
        {
            var bestMatch = GetBestMatch(userInput, skills);
            return bestMatch?.Name ?? string.Empty;
        }
    }
}