using System;

namespace CardPlay.Views.New_File_Dialog
{
    public interface IMVVMCommand
    {
        void Execute(EventArgs e);
        string Name { get; }
    }
}
