using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string Shortcut { get; }
        CommandType Type { get; }
        string Permission { get; }
    }
}
