using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Annstore.Core.Common
{
    public sealed class AssemblyHelper : IAssemblyHelper
    {
        private const string APP_ASSEMBLY_BASENAME = "ANNSTORE";

        public IEnumerable<Assembly> GetAppOwnAssemblies()
        {
            var annstoreAssemblies = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     where assembly.FullName.ToUpper().Contains(APP_ASSEMBLY_BASENAME)
                                     select assembly;

            return annstoreAssemblies;
        }
    }
}
