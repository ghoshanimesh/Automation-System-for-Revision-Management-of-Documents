$('[data-book-id]').each(function () {
    var book_name = $(this).data('book-name');
    var doc_id = $(this).data('book-id');
    var sll_id = $(this).data('sll-id');

    $(this).fireModal({
        title: 'Do you want to delete this Book?',
        body: "Deleting this book will result you to loose potential information about the book " + book_name + ". Click on Yes if you want to Continue.",
        center: true,
        footerClass: 'bg-white',
        autoFocus: false,
        buttons: [
            {
                text: 'Yes',
                class: 'btn btn-danger btn-shadow',
                handler: function (modal) {
                    console.log("Yes Clicked by " + doc_id);       
                    $.ajax({
                        url: '/Book/Delete',
                        type: 'POST',
                        data: JSON.stringify({ doc_id: doc_id, sll_id: sll_id }),
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

