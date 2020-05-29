using System;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Game.Data
{
    /// <summary>
    /// A JSON naming policy that converts CamelCase names to snake_case.
    /// </summary>
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        /// <summary>
        /// Represents the different state machine states the <see cref="SnakeCaseNamingPolicy.ConvertName"/> method
        /// can be in
        /// </summary>
        private enum SnakeCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }
        
        /// <summary>
        /// Convert given CamelCase name to snake_case.
        /// </summary>
        public override string ConvertName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return name;

            var builder = new StringBuilder();
            var state = SnakeCaseState.Start;

            for (var ix = 0; ix < name.Length; ++ix)
            {
                if (name[ix] == ' ')
                {
                    if (state != SnakeCaseState.Start)
                        state = SnakeCaseState.NewWord;
                }
                else if (Char.IsUpper(name[ix]))
                {
                    switch (state)
                    {
                        case SnakeCaseState.Upper:
                            var hasNext = (ix + 1 < name.Length);
                            if (ix > 0 && hasNext)
                            {
                                var next = name[ix + 1];
                                if (!Char.IsUpper(next) && next != '_')
                                    builder.Append('_');
                            }
                            break;
                        case SnakeCaseState.NewWord:
                        case SnakeCaseState.Lower:
                            builder.Append('_');
                            break;
                    }

                    var c = Char.ToLower(name[ix], CultureInfo.InvariantCulture);
                    builder.Append(c);

                    state = SnakeCaseState.Upper;
                }
                else if (name[ix] == '_')
                {
                    builder.Append('_');
                    state = SnakeCaseState.Start;
                }
                else
                {
                    if (state == SnakeCaseState.NewWord)
                        builder.Append('_');

                    builder.Append(name[ix]);
                    state = SnakeCaseState.Lower;
                }
            }

            return builder.ToString();
        }
    }
}