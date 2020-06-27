$('[data-standard-id]').each(function () {
    var std_id = $(this).data('standard-id');
    var std_name = $(this).data('standard-name');
    var board_name = $(this).data('board-name');
    $(this).fireModal({
        title: 'Do you want to delete this Standard?',
        body: std_name + " of " + board_name,
        center: true,
        footerClass: 'bg-white',
        autoFocus: false,
        buttons: [
            {
                text: 'Yes',
                class: 'btn btn-danger btn-shadow',
                handler: function (modal) {
                    console.log("Yes Clicked by " + std_id);       
                    $.ajax({
                        url: '/Standard/Delete',
                        type: 'POST',
                        data: JSON.stringify({ id: std_id }),
                        dataType: "json",
                        contentType: "application/json; charset=utf-8",
                    }).done(function (response) {
                        console.log(response);
                        location.reload();
                    }).fail(function () {
                        console.log("error");
                    });
                    $.destroyModal(modal);
                }
            },
            {
                text: 'Cancel',
                class: 'btn btn-secondary',
                handler: function (modal) {
                    $.destroyModal(modal);
                }
            }
        ]
    });

});

