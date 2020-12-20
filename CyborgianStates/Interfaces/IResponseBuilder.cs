using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyborgianStates.CommandHandling;
using Discord;

namespace CyborgianStates.Interfaces
{
    public interface IResponseBuilder
    {
        IResponseBuilder Failed(string reason);
        IResponseBuilder Success();
        IResponseBuilder WithContent(string content);
        IResponseBuilder WithCustomProperty(FieldKey key, string value);
        IResponseBuilder WithField(string key, string value, bool isInline = false);
        CommandResponse Build();
    }

    public enum FieldKey
    {
        Title,
        Description,
        Footer,
        ThumbnailUrl,
        Color
    }

    public static class ResponseBuilderExtensions
    {
        public static IResponseBuilder WithTitle(this IResponseBuilder builder, string value)
        {
            return builder.WithCustomProperty(FieldKey.Title, value);
        }

        public static IResponseBuilder WithDescription(this IResponseBuilder builder, string value)
        {
            return builder.WithCustomProperty(FieldKey.Description, value);
        }

        public static IResponseBuilder WithFooter(this IResponseBuilder builder, string value)
        {
            return builder.WithCustomProperty(FieldKey.Footer, value);
        }

        public static IResponseBuilder WithThumbnailUrl(this IResponseBuilder builder, string value)
        {
            return builder.WithCustomProperty(FieldKey.ThumbnailUrl, value);
        }

        public static IResponseBuilder WithColor(this IResponseBuilder builder, Color color)
        {
            return builder.WithCustomProperty(FieldKey.Color, color.ToString());
        }
    }
}
