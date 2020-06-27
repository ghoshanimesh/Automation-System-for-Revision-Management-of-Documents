$('[data-board-id]').each(function () {
    var board_id = $(this).data('board-id');
    var board_name = $(this).data('board-name');
    $(this).fireModal({
        title: 'Do you want to delete this Board?',
        body: board_name,
        center: true,
        footerClass: 'bg-white',
        autoFocus: false,
        buttons: [
            {
                text: 'Yes',
                class: 'btn btn-danger btn-shadow',
                handler: function (modal) {
                    console.log("Yes Clicked by " + board_id);       
                    $.ajax({
                        url: '/Board/Delete',
                        type: 'POST',
                        data: JSON.stringify({ id: board_id }),
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

