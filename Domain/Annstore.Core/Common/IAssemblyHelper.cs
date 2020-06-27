using System.Collections.Generic;
using System.Reflection;

namespace Annstore.Core.Common
{
    public interface IAssemblyHelper
    {
        IEnumerable<Assembly> GetAppOwnAssemblies();
    }
}
