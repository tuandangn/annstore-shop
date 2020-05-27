using System;

namespace Annstore.Application.Models.Admin.Common
{
    [Serializable]
    public class NullableModel<TModel> where TModel : new()
    {
        public static TModel NullModel => new TModel();
    }
}
