
$(function () {
    /*$(".create").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        })
    });*/

    $(".details").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        })
    });

    /*$(".edit").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        })
    });*/

    $(".delete").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        })
    });

    $(".active").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        })
    });
});