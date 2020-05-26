namespace Annstore.Application.Infrastructure.Messages.Messages
{
    public static class AdminMessages
    {
        public static class Category
        {
            public const string CategoryIsNotFound = "Không tìm thấy danh mục";

            public const string CreateCategoryError = "Thêm danh mục mới thất bại";

            public const string UpdateCategoryError = "Chỉnh sửa danh mục thất bại";

            public const string DeleteCategoryError = "Xóa danh mục thất bại";

            public const string CreateCategorySuccess = "Thêm danh mục mới thành công";

            public const string UpdateCategorySuccess = "Chỉnh sửa danh mục thành công";

            public const string DeleteCategorySuccess = "Xóa danh mục thành công";
        }

        public static class Account
        {
            public const string AccountIsNotFound = "Không tìm thấy tài khoản";

            public const string AccountIsEmpty = "Không có tài khoản nào";

            public const string CreateAccountError = "Thêm tài khoản mới thất bại";

            public const string UpdateAccountError = "Chỉnh sửa tài khoản thất bại";

            public const string DeleteAccountError = "Xóa tài khoản thất bại";

            public const string CreateAccountSuccess = "Thêm tài khoản mới thành công";

            public const string UpdateAccountSuccess = "Chỉnh sửa tài khoản thành công";

            public const string DeleteAccountSuccess = "Xóa tài khoản thành công";
        }

        public static class Customer
        {
            public const string CustomerIsNotFound = "Không tìm thấy khách hàng";

            public const string CreateCustomerError = "Thêm khách hàng mới thất bại";

            public const string UpdateCustomerError = "Chỉnh sửa khách hàng thất bại";

            public const string DeleteCustomerError = "Xóa khách hàng thất bại";

            public const string CreateCustomerSuccess = "Thêm khách hàng mới thành công";

            public const string UpdateCustomerSuccess = "Chỉnh sửa khách hàng thành công";

            public const string DeleteCustomerSuccess = "Xóa khách hàng thành công";
        }
    }
}
