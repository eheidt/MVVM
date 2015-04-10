using MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Application.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public int Number
        {
            get
            {
                return base.GetValue(() => this.Number, () => 0);
            }
            set
            {
                base.SetValue(() => this.Number, value,
                    v =>
                    {
                        if (v > 100)
                            return "number > 100";
                        else
                            return string.Empty;
                    });
            }
        }

        public string Text
        {
            get
            {
                return base.GetValue(() => this.Text, () => "Initial Text String");
            }
            set
            {
                base.SetValue(() => this.Text, value,
                    v =>
                    {
                        if (v.Length > 20)
                            return "Text is too long";
                        else
                            return string.Empty;
                    });
            }
        }

        public Command CmdAddRandomText
        {
            get
            {
                return base.GetCommand("Add random Text",
                () =>
                {
                    this.Text += Guid.NewGuid().ToString().Trim('{', '}');
                },
                () => this.Errors.Any() == false);
            }
        }

        public Command CmdReverseText
        {
            get
            {
                return base.GetCommand("Reverse Text", () =>
                {
                    this.Text = new string(this.Text.Reverse().ToArray());
                },
                () =>
                {
                    return string.IsNullOrEmpty(this.Text) == false;
                },
                () => this.Text);
            }
        }

        public Command CmdClearText
        {
            get
            {
                return base.GetCommand("Clear Text", () =>
                {
                    this.Text = string.Empty;
                },
                () =>
                {
                    return string.IsNullOrEmpty(this.Text) == false;
                },
                () => this.Text);
            }
        }

        public IEnumerable<Command> ContextCommands
        {
            get
            {
                yield return this.CmdAddRandomText;
                yield return this.CmdReverseText;
                yield return this.CmdClearText;
            }
        }
    }
}
