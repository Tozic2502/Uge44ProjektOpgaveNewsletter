using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uge44ProjektOpgaveNewsletter.Service
{
    public static class ResponseExtensionMethods
    {
       public static int WordCount(this string str) =>
       str.Split([' ', '.', '?'], StringSplitOptions.RemoveEmptyEntries).Length;

        public static string FormatGroupResponse(this string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return "No response received.";

            var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5 || parts[0] != "211")
                return response; // Not a standard GROUP response

            string articleCount = parts[1];
            string firstArticle = parts[2];
            string lastArticle = parts[3];
            string groupName = parts[4];

            return $"Group Selected: {groupName}\n" +
                   $"Articles Available: {articleCount}\n" +
                   $"First Article #: {firstArticle}\n" +
                   $"Last Article #: {lastArticle}";
        }
        public static string FormatArticleResponse(this string response)
        {
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return "No article data.";

            StringBuilder sb = new StringBuilder();

            // Skip the first status line (e.g., "220 ...")
            sb.AppendLine("=== ARTICLE HEADER ===");

            int i = 1;
            for (; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    break; // blank line separates headers and body
                sb.AppendLine(lines[i].Trim());
            }

            sb.AppendLine("\n=== ARTICLE BODY ===");

            for (i++; i < lines.Length; i++)
                sb.AppendLine(lines[i].Trim());

            return sb.ToString();
        }
        public static string FormatXoverResponse(this string response)
        {
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1)
                return "No overview data.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== ARTICLE OVERVIEW ===");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("224")) continue; // skip status line
                if (line.Trim() == ".") break;

                var parts = line.Split('\t');
                if (parts.Length >= 4)
                {
                    sb.AppendLine($"Article #{parts[0]}");
                    sb.AppendLine($"Subject: {parts[1]}");
                    sb.AppendLine($"From: {parts[2]}");
                    sb.AppendLine($"Date: {parts[3]}");
                    sb.AppendLine("------------------------------");
                }
                else
                {
                    sb.AppendLine(line); // fallback for malformed lines
                }
            }

            return sb.ToString();
        }


    }
}
    