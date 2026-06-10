const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer)
        toast.addEventListener('mouseleave', Swal.resumeTimer)
    }
});

function parseErrorResponse(xhr) {
    console.log("xhr", xhr);
    let msgs = [];

    try {
        let errObj = xhr.responseJSON || JSON.parse(xhr.responseText);

        if (errObj.Errors && Array.isArray(errObj.Errors)) {
            errObj.Errors.forEach(e => {
                if (Array.isArray(e.Errors99)) {
                    msgs = msgs.concat(e.Errors99);
                }
            });
        }
        else if (errObj.detail && errObj.status != 500) {
            msgs.push(errObj.detail);
            console.log("errObj.detail:", errObj.detail);
        }
        else if (typeof errObj === "string") {
            //msgs.push(errObj);
            console.log("errObj:", errObj);
        }
    }
    catch (error) {
        console.log("error:",error);
        //msgs.push("Bilinmeyen bir hata oluştu.");

        Toast.fire({
            icon: 'error',
            title: 'Hata',
            html: `Bilinmeyen bir hata oluştu.`
        });
    }

    // Hataları alt alta HTML olarak bastır
    let html = msgs.map(m => `<li>${m}</li>`).join("");
    //console.log("msgs", msgs);
    //console.log("html", html);
    if (html) {
        Toast.fire({
            icon: 'error',
            title: 'Hata',
            html: `<ul style="margin:0;padding-left:18px;">${html}</ul>`
        });
    }
    else {
        Toast.fire({
            icon: 'error',
            title: 'Hata',
            html: `Bilinmeyen bir hata oluştu.`
        });
    }
}