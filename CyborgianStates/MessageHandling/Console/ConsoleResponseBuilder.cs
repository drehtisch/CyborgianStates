using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyborgianStates.CommandHandling;
using CyborgianStates.Interfaces;

namespace CyborgianStates.MessageHandling
{
    public class ConsoleResponseBuilder : IResponseBuilder
    {
        private readonly CommandResponse _response;
        private readonly Dictionary<FieldKey, string> _properties = new();
        private readonly Dictionary<string, (string, bool)> _fields = new();
        public ConsoleResponseBuilder()
        {
            _response = new CommandResponse();
        }

        public CommandResponse Build()
        {
            var builder = new StringBuilder();
            var contentEmpty = string.IsNullOrWhiteSpace(_response.Content);
            if (!contentEmpty)
            {
                builder.AppendLine(_response.Content);
            }
            if (_properties.Any() || _fields.Any())
            {
                if (!contentEmpty)
                {
                    builder.AppendLine("= = = = =");
                }
                if (_properties.ContainsKey(FieldKey.Title))
                {
                    builder.AppendLine(_properties[FieldKey.Title]);
                    builder.AppendLine();
                }
                if (_properties.ContainsKey(FieldKey.Description))
                {
                    builder.AppendLine(_properties[FieldKey.Description]);
                    builder.AppendLine();
                }
                AddFields(builder);
                if (_properties.ContainsKey(FieldKey.Footer))
                {
                    builder.AppendLine(_properties[FieldKey.Footer]);
                }
            }
            return new CommandResponse(_response.Status, builder.ToString());
        }

        private void AddFields(StringBuilder builder)
        {
            var prevInlineNameLine = "";
            var prevInlineValueLine = "";
            foreach (var field in _fields)
            {
                var value = field.Value.Item1;
                var isInline = field.Value.Item2;
                var length = value.Length + 2;
                if (length > field.Key.Length && isInline)
                {
                    var fieldName = field.Key.PadRight(length);
                    value = value.PadRight(length);
                    prevInlineNameLine += fieldName;
                    prevInlineValueLine += value;
                }
                else
                {
                    if (!string.IsNullOrEmpty(prevInlineNameLine) && !string.IsNullOrEmpty(prevInlineValueLine))
                    {
                        builder.AppendLine(prevInlineNameLine);
                        builder.AppendLine(prevInlineValueLine);
                        builder.AppendLine();
                        prevInlineNameLine = string.Empty;
                        prevInlineValueLine = string.Empty;
                    }
                    builder.AppendLine(field.Key);
                    builder.AppendLine(value);
                    builder.AppendLine();
                }
            }
        }

        public IResponseBuilder Failed(string reason)
        {
            _response.Status = CommandStatus.Error;
            _response.Content = reason;
            return this;
        }

        public IResponseBuilder Success()
        {
            _response.Status = CommandStatus.Success;
            return this;
        }

        public IResponseBuilder WithContent(string content)
        {
            _response.Content = content;
            return this;
        }

        public IResponseBuilder WithCustomProperty(FieldKey key, string value)
        {
            _properties[key] = value;
            return this;
        }

        public IResponseBuilder WithField(string key, string value, bool isInline = false)
        {
            _fields[key] = (value, isInline);
            return this;
        }
    }
}
