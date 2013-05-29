#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Chraft.Commands
{
    public class MultipleCommandsMatchException : Exception
    {
        public string[] Commands { get; private set; }

        public MultipleCommandsMatchException(string[] commands) : base("Multiple commands has been found")
        {
            Commands = commands;
        }

        protected MultipleCommandsMatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
                Commands = (string[])info.GetValue("Commands", typeof(string[]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
                info.AddValue("Commands", Commands);
        }
    }
}
