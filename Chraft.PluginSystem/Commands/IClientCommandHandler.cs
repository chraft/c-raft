using Chraft.PluginSystem.Commands;

namespace Chraft.PluginSystem.Commands
{
    public interface IClientCommandHandler
    {
        /// <summary>
        /// Finds a command and returns it for use.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="command">The name of the command to find.</param>
        /// <returns>A command with the given name.</returns>
        ICommand Find(string command);

        /// <summary>
        /// Finds a command and returns it for use.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="Shortcut">The shortcut of the command to find.</param>
        /// <returns>A command with the given shortcut.</returns>
        ICommand FindShort(string Shortcut);

        /// <summary>
        /// Registers a command with the server.
        /// Exceptions:
        /// <exception cref="CommandAlreadyExistsException">CommandAlreadyExistsException</exception>
        /// </summary>
        /// <param name="command">The <see cref="IClientCommand">Command</see> to register.</param>
        /// <param name="plugin"></param>
        void RegisterCommand(ICommand command);

        /// <summary>
        /// Removes a command from the server.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="command">The <see cref="IClientCommand">Command</see> to remove.</param>
        void UnregisterCommand(ICommand command);

        /// <summary>
        /// Gets an array of all of the commands registerd.
        /// </summary>
        /// <returns>Array of <see cref="IClientCommand"/></returns>
        ICommand[] GetCommands();
    }
}