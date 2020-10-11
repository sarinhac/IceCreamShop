
$(function () {

    $(".details").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        })
    });

    $(".delete").on('click',function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal({ show: true });
        })
    });

    $(".active").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        })
    });
});

