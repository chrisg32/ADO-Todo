using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using ADOTodo.Models;

namespace ADOTodo.Util
{
    public static class Extensions
    {
        private static readonly Regex MentionRegex =
            new Regex(@"@<(?<UserId>[{(]?[0-9a-fA-F]{8}[-]?(?:[0-9a-fA-F]{4}[-]?){3}[0-9a-fA-F]{12}[)}]?)>", RegexOptions.Compiled);
        
        public static bool MentionsUser(this GitPullRequestCommentThread thread, Guid userId)
        {
            return thread.Comments.Any(c => MentionRegex.Matches(c.Content).Any(m => Guid.Parse(m.Groups["UserId"].Value) == userId));
        }

        public static IEnumerable<Guid> GetMentions(this TodoItem todo)
        {
            return todo.CommentText == null ? Enumerable.Empty<Guid>() : MentionRegex.Matches(todo.CommentText).Select(m => Guid.Parse(m.Groups["UserId"].Value));
        }
        
        public static void UpdateCommentText(this TodoItem todo, Dictionary<Guid, string?> userMap)
        {
            if(todo.CommentText == null) return;
            var result = todo.CommentText;
            var offset = 0;
            foreach (Match match in MentionRegex.Matches(todo.CommentText).Where(m => m.Success))
            {
                var userId = Guid.Parse(match.Groups["UserId"].Value);
                var displayName = userMap[userId];
                var end = match.Index + match.Length - offset;
                result = result[0..(match.Index-offset)] + displayName + result[end..];
                offset += match.Length - (displayName?.Length ?? 0);
            }

            todo.CommentText = result;
        }
        
        public static string? BlockTruncate(this string source, int length)
        {
            source = source.Trim();
            using var reader = new StringReader(source);
            return (reader.ReadLine()).Truncate(length);
        }
        public static string? Truncate(this string? source, int length)
        {
            if (source != null && source.Length > length)
            {
                source = source.Substring(0, length) + "...";
            }
            return source;
        }
    }
}