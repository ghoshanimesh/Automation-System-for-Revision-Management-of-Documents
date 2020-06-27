var doc_id = $("#commit_new_file").data('doc_id');
$("#commit_new_file").fireModal({
    title: 'Do you want to Commit this version?',
    body: $("#modal-form-part"),
    center: true,
    footerClass: 'bg-white',
    autoFocus: false,
    onFormSubmit: function (modal, e, form) {
        let form_data = JSON.parse(JSON.stringify($(e.target).serializeArray()));
        console.log(form_data);
        e.preventDefault();
        console.log("Yes Clicked by ");
        $.ajax({
            url: '/DocumentStream/CommitFile',
            type: 'POST',
            data: JSON.stringify({ doc_id: doc_id, comment: form_data[0].value, version_value: form_data[1].value }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
        }).done(function (response) {
            console.log(response.result);
            if (response.result == "success") {
                console.log("Success");
                $("#route_final").submit();
                console.log("submitted");
            } else if (response.result == "error") {
                window.location.href = "/DocumentStream/UploadNewDocument?route=error&doc_id=" + doc_id
            }
        }).fail(function () {
            console.log("error");
        });
    },
    buttons: [
        {
            text: 'Yes',
            submit: true,
            class: 'btn btn-danger btn-shadow',
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
