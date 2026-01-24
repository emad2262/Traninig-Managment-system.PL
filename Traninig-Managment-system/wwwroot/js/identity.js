$(document).ready(function () {
    $('form').each(function () {
        $(this).find('input, select, textarea').each(function () {
            $(this).on('blur keyup change', function () {
                if ($(this).valid()) {
                    $(this).removeClass('is-invalid').addClass('is-valid');
                } else {
                    $(this).removeClass('is-valid').addClass('is-invalid');
                }
            });
        });
    });
});