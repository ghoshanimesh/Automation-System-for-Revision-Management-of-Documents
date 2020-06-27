$('[data-subj-id]').each(function () {
    var subj_id = $(this).data('subj-id');
    var subj_name = $(this).data('subj-name');
    var board_name = $(this).data('board-name');
    var std_name = $(this).data('std-name');

    $(this).fireModal({
        title: 'Do you want to delete this Subject?',
        body: subj_name + " of " + board_name + " &mdash; " + std_name,
        center: true,
        footerClass: 'bg-white',
        autoFocus: false,
        buttons: [
            {
                text: 'Yes',
                class: 'btn btn-danger btn-shadow',
                handler: function (modal) {
                    console.log("Yes Clicked by " + subj_id);       
                    $.ajax({
                        url: '/Subject/Delete',
                        type: 'POST',
                        data: JSON.stringify({ id: subj_id }),
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

