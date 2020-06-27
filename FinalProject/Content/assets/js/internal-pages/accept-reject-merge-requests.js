console.log("Ajax");

$('[data-merge-request-id]').each(function () {
    var request_id = $(this).data('merge-request-id');
    console.log(request_id);

    $(this).fireModal({
        title: 'Do you want to permit this Merge Request?',
        body: "Accepting the request will cause you to Merge the document of the requesting branch to merge with your document. Click on Accept if you want to you accept this request.",
        center: true,
        footerClass: 'bg-white',
        autoFocus: false,
        buttons: [
            {
                text: 'Accept',
                class: 'btn btn-success btn-shadow',
                handler: function (modal) {
                    console.log("Yes Clicked by " + request_id);
                    $(this).fireModal({
                        title: "Please enter your comments over the merge request",
                        body: $("#modal-form-merge-request-comment"),
                        center: true,
                        footerClass: 'bg-white',
                        autoFocus: false,
                        onFormSubmit: function (modal, e, form) {
                            let form_data = JSON.parse(JSON.stringify($(e.target).serializeArray()));
                            let comment = form_data[0].value;
                            console.log(comment);
                            e.preventDefault();
                            console.log("Yes Clicked by ");
                            $.ajax({
                                url: '/MergeRequest/AcceptMergeRequest',
                                type: 'POST',
                                data: JSON.stringify({ merge_Request_id: request_id, comment: comment }),
                                dataType: "json",
                                contentType: "application/json; charset=utf-8",
                            }).done(function (response) {
                                console.log(response);
                                location.reload();
                            }).fail(function () {
                                console.log("error");
                            });
                        },
                        buttons: [
                            {
                                text: 'Submit',
                                submit: true,
                                class: 'btn btn-success btn-shadow',
                                handler: function (modal) {
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
                    $.destroyModal(modal);
                }
            },
            {
                text: 'Reject',
                class: 'btn btn-danger',
                handler: function (modal) {
                    $(this).fireModal({
                        title: "Please enter your comments over the merge request",
                        body: $("#modal-form-merge-request-comment"),
                        center: true,
                        footerClass: 'bg-white',
                        autoFocus: false,
                        onFormSubmit: function (modal, e, form) {
                            let form_data = JSON.parse(JSON.stringify($(e.target).serializeArray()));
                            let comment = form_data[0].value;
                            console.log(comment);
                            e.preventDefault();
                            console.log("Yes Clicked by ");
                            $.ajax({
                                url: '/MergeRequest/RejectMergeRequest',
                                type: 'POST',
                                data: JSON.stringify({ merge_Request_id: request_id, comment: comment }),
                                dataType: "json",
                                contentType: "application/json; charset=utf-8",
                            }).done(function (response) {
                                console.log(response);
                                window.location.href = "/Dashboard/";
                            }).fail(function () {
                                console.log("error");
                            });
                        },
                        buttons: [
                            {
                                text: 'Submit',
                                submit: true,
                                class: 'btn btn-success btn-shadow',
                                handler: function (modal) {
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