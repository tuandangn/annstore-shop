"use strict";

(function ($, tinymce) {
    $(function () {
        $('.delete-category-btn').click(function (e) {
            if (!confirm('Bạn muốn xóa danh mục này?')) {
                e.preventDefault();
            }
        });
    });

    if (tinymce) {
        tinymce.init({
            selector: 'textarea',
            height: 320
        });
    }
})(jQuery, window.tinymce);