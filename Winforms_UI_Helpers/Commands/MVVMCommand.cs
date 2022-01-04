using System;

namespace CardPlay.Views.New_File_Dialog
{
    public class MVVMCommand : IMVVMCommand
    {
        Action<EventArgs> action;

        public MVVMCommand(Action<EventArgs> action)
        {
            this.action = action;
        }

        public MVVMCommand(string name, Action<EventArgs> action)
            : this(action)
        {
            Name = name;
        }

        public string Name { get; protected set; }

        public void Execute(EventArgs e)
        {
            action.Invoke(e);
        }

        public static implicit operator MVVMCommand(Action<EventArgs> action)
        {
            return new MVVMCommand(action);
        }
    }
}
