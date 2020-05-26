"use strict";

(function ($, tinymce) {
    $(function () {
        $('.delete-customer-btn').click(function (e) {
            if (!confirm('Bạn muốn xóa người dùng này?')) {
                e.preventDefault();
            }
        });
    });
})(jQuery);