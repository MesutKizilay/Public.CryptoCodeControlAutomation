$(function () {
    //$('#btnLogin').on('click', function () {
    function doLogin() {
        const userForLoginDto = {
            userName: $('#email').val(),
            passwordHash: $('#password').val()
        };

        $.ajax({
            url: '/Auth/Login',
            method: 'POST',
            data: userForLoginDto,
            //headers: { 'RequestVerificationToken': token },
            success: function (response) {
                console.log("AuthResponse11", response);

                Toast.fire({ icon: 'success', title: 'Giriş Başarılı' });

                setTimeout(() => { window.location.href = '/SalesOrderItems/SalesOrderItems'; }, 1000);
            },
            error: function (xhr) {
                console.log("AuthResponseError", xhr);
                parseErrorResponse(xhr);
            }
        });
    }
    //});

    // Enter basınca (form submit)
    $('#formAuthentication').on('submit', function (e) {
        e.preventDefault();
        doLogin();
    });

    // Button click
    $('#btnLogin').on('click', function () {
        doLogin();
    });
});