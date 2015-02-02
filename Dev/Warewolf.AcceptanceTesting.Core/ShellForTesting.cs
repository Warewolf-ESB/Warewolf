using Dev2.Common.Interfaces;
using Warewolf.Studio;

namespace Warewolf.AcceptanceTesting.Core
{
    public class ShellForTesting : Shell
    {
        public ShellForTesting(IShellViewModel shellViewModel) : base(shellViewModel)
        {
        }

        protected override void LoadShellViewModel()
        {            
        }
    }
}