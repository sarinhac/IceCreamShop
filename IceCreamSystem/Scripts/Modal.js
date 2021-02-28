
$(function () {

   /* $('#modal').on('show.bs.modal', function (e) {
        var button = $(e.relatedTarget);
        var modal = $(this);
        modal.find('.modal-body').load(button.data("remote"),
            function (responseText, textStatus, req) {
                if (textStatus == "error") {
                    modal.find('.modal-body').text("Sorry, we were unable to load the requested page (" + req.status + ").");
                }
            });
        $('#modal').modal('handleUpdate')
    });*/

    $(".details").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal("show");
        });
        //$('#modal').modal('handleUpdate');
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
            $("#modal").modal('show');
        })
    });

    $(".pay").click(function () {
        var url = $(this).data('url');
        $("#modal").load(url, function () {
            $("#modal").modal('show');
        })
    });
});

